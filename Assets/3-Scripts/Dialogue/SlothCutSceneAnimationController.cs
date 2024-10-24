using UnityEngine;
using Spine.Unity;
using System.Collections;
using Spine;

public class SlothCutSceneAnimationController : MonoBehaviour
{
    [SpineAnimation] public string fadeInAnimation;    // Fade_In 애니메이션
    [SpineAnimation] public string animation1;         // Animation1
    [SpineAnimation] public string fadeOutAnimation;   // Fade_Out 애니메이션
    [SpineAnimation] public string animation2;         // Animation2

    [SerializeField]
    private SkeletonGraphic skeletonGraphic; // UI용 SkeletonGraphic 컴포넌트
    private Spine.AnimationState spineAnimationState;
    private CutsceneManager cutsceneManager;

    private bool hasFadedOut = false;

    void Start()
    {
        // SkeletonGraphic 컴포넌트를 가져옵니다.
        skeletonGraphic = GetComponent<SkeletonGraphic>();
        if (skeletonGraphic == null)
        {
            Debug.LogError("SkeletonGraphic 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        spineAnimationState = skeletonGraphic.AnimationState;

        // CutsceneManager를 찾아서 참조
        cutsceneManager = FindObjectOfType<CutsceneManager>();
        if (cutsceneManager == null)
        {
            Debug.LogError("CutsceneManager를 찾을 수 없습니다.");
        }

        // Animation1을 즉시 실행 (반복 없음)
        spineAnimationState.SetAnimation(0, animation1, false).MixDuration = 0.5f;

        // Coroutine을 시작하여 2초 후에 Fade_In을 실행
        StartCoroutine(PlayFadeInAfterDelay(2f));
    }

    // Fade_In 애니메이션을 지연 후 실행하는 Coroutine
    IEnumerator PlayFadeInAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Fade_In 애니메이션을 트랙 1에 실행 (반복 없음)
        spineAnimationState.SetAnimation(1, fadeInAnimation, false).MixDuration = 0.5f;
    }

    void Update()
    {
        // 컷씬이 종료되었고, 아직 Fade_Out이 실행되지 않았다면
        if (cutsceneManager != null && cutsceneManager.IsCutsceneEnded() && !hasFadedOut)
        {
            // Fade_Out과 Animation2를 각각 다른 트랙에서 동시에 실행 (반복 없음)
            spineAnimationState.SetAnimation(1, fadeOutAnimation, false).MixDuration = 0.5f;
            spineAnimationState.SetAnimation(0, animation2, false).MixDuration = 0.5f;

            // 애니메이션 완료 시 호출될 이벤트 핸들러 등록
            spineAnimationState.Complete += OnAnimationComplete;

            // 말풍선 비활성화 및 캐릭터 이미지의 FadeOut 트리거 활성화
            HandleFadeOutEffects();

            hasFadedOut = true;
        }
    }

    private void HandleFadeOutEffects()
    {
        // 1. 말풍선 애니메이터에 FadeOut 트리거 설정
        if (cutsceneManager.speechBubbleAnimator != null)
        {
            cutsceneManager.speechBubbleAnimator.SetTrigger("FadeOut");
            cutsceneManager.speechBubble.SetActive(false);
            cutsceneManager.speechBubble2.SetActive(false);
        }
        else
        {
            Debug.LogWarning("SpeechBubbleAnimator가 할당되지 않았습니다.");
        }

        // 2. 활성화된 캐릭터 이미지의 Animator에 FadeOut 트리거 설정
        foreach (var entry in cutsceneManager.cutsceneDialogue.dialogueEntries)
        {
            if (entry.characterImage != null && entry.characterImage.activeSelf)
            {
                Animator animator = entry.characterImage.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger("FadeOut");
                }
                else
                {
                    Debug.LogWarning($"캐릭터 이미지 '{entry.characterName}'에 Animator 컴포넌트가 없습니다.");
                }
            }
        }
    }

    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        // Animation2가 종료되었을 때 씬 전환
        if (trackEntry.Animation.Name == animation2)
        {
            // 이벤트 핸들러 제거
            spineAnimationState.Complete -= OnAnimationComplete;

            // 씬 전환
            if (cutsceneManager != null)
            {
                cutsceneManager.LoadNextScene();
            }
        }
    }
}
