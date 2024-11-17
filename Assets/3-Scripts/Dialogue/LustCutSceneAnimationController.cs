using UnityEngine;
using Spine.Unity;
using System.Collections;
using Spine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LustCutSceneAnimationController : MonoBehaviour
{
    [SpineAnimation] public string fadeInAnimation;    // Fade_In 애니메이션
    [SpineAnimation] public string animation1;         // Animation1
    [SpineAnimation] public string fadeOutAnimation;   // Fade_Out 애니메이션
    [SpineAnimation] public string animation2;         // Animation2

    [SerializeField]
    private SkeletonGraphic skeletonGraphic; // UI용 SkeletonGraphic 컴포넌트
    private Spine.AnimationState spineAnimationState;
    private LustCutSceneManager cutsceneManager;

    private bool hasFadedOut = false;

    // UnityEvent 추가: 애니메이션 완료 시 호출됨
    [SerializeField]
    private UnityEvent onCutsceneEnd;

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
        cutsceneManager = FindObjectOfType<LustCutSceneManager>();
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

            hasFadedOut = true;
        }
    }

    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        // Animation2가 종료되었을 때
        if (trackEntry.Animation.Name == animation2)
        {
            // 이벤트 핸들러 제거
            spineAnimationState.Complete -= OnAnimationComplete;

            // UnityEvent 호출
            onCutsceneEnd?.Invoke();
        }
    }

    // 객체가 파괴될 때 이벤트 핸들러 해제
    private void OnDestroy()
    {
        if (spineAnimationState != null)
        {
            spineAnimationState.Complete -= OnAnimationComplete;
        }
    }

    // 객체가 비활성화될 때 이벤트 핸들러 해제 (선택 사항)
    private void OnDisable()
    {
        if (spineAnimationState != null)
        {
            spineAnimationState.Complete -= OnAnimationComplete;
        }
    }
}
