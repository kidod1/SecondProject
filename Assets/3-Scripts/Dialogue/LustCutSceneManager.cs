using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using Spine.Unity; // Spine.Unity 네임스페이스 추가

/// <summary>
/// 컷신 다이얼로그를 관리하는 매니저 클래스
/// </summary>
public class LustCutSceneManager : MonoBehaviour
{
    [System.Serializable]
    public class AnimationName
    {
        [Spine.Unity.SpineAnimation]
        public string animationName; // 재생할 애니메이션의 이름
    }

    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 10)]
        public string sentence; // 대사
        public string characterName; // 캐릭터 이름
        public GameObject characterImage; // 캐릭터 이미지 오브젝트
        public List<AnimationName> animationNames; // 순차적으로 재생할 애니메이션 이름 리스트
    }

    [System.Serializable]
    public class CutsceneDialogue
    {
        public List<DialogueLine> dialogueLines; // 대화 라인 리스트
    }

    public CutsceneDialogue cutsceneDialogue;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image animationImage;
    public string nextSceneName;
    public float textAnimationSpeed = 0.05f; // 텍스트 애니메이션 속도

    [Header("Cutscene Events")]
    public UnityEvent OnCutsceneEnded; // 컷신이 종료될 때 호출되는 이벤트

    [Header("Sound Events")]
    public AK.Wwise.Event typingSoundEvent;   // 텍스트 출력 중 재생될 타이핑 사운드 이벤트
    public UnityEvent OnTextFinished;        // 텍스트 출력이 완료되었을 때 호출되는 이벤트
    public UnityEvent OnClickSound;          // 클릭 시 호출되는 이벤트

    private Queue<DialogueLine> dialogueQueue;
    private int currentSentenceIndex = 0;
    private Coroutine textAnimationCoroutine;
    private bool canProceed = true;
    private bool isAnimating = false; // 텍스트 애니메이션 중인지 확인하는 플래그
    private bool cutsceneEnded = false;
    private string currentSentence = ""; // 현재 대사 저장

    [SerializeField]
    private SceneChangeSkeleton sceneChangeSkeleton;
    [SerializeField]
    private bool lastCut = false;

    // 추가된 부분: 타이핑 사운드가 재생 중인지 확인하는 변수
    private uint typingSoundPlayingID = 0;

    private void Start()
    {
        dialogueQueue = new Queue<DialogueLine>();

        // 시작할 때 모든 캐릭터 이미지는 비활성화
        foreach (var line in cutsceneDialogue.dialogueLines)
        {
            if (line.characterImage != null)
            {
                line.characterImage.SetActive(false);
            }
        }

        animationImage.gameObject.SetActive(false); // 초기화할 때 애니메이션 이미지를 비활성화

        StartCutscene();
    }

    public bool IsCutsceneEnded()
    {
        return cutsceneEnded;
    }

    public void StartCutscene()
    {
        dialogueQueue.Clear();

        foreach (DialogueLine line in cutsceneDialogue.dialogueLines)
        {
            dialogueQueue.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (!canProceed) return;

        if (dialogueQueue.Count == 0)
        {
            EndCutscene();
            return;
        }

        canProceed = false;

        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine); // 이전 애니메이션 중지
        }

        // 새로운 대사가 출력되기 전에 이미지를 비활성화
        animationImage.gameObject.SetActive(false);

        DialogueLine currentLine = dialogueQueue.Dequeue();
        currentSentenceIndex++;
        currentSentence = currentLine.sentence; // 현재 대사 저장

        ToggleCharacterImagesAndNames(currentLine);

        // Spine 애니메이션 재생은 ActivateSkeletonGraphic에서 처리
        ActivateSkeletonGraphic(currentLine);

        // 첫 번째 대사에만 딜레이 추가
        if (currentSentenceIndex == 1)
        {
            StartCoroutine(DisplayNameAndSentenceWithDelay(currentLine.sentence, 3.3f)); // 딜레이 추가
        }
        else
        {
            DisplayNameAndSentence(currentLine.sentence);
        }
    }

    private void ActivateSkeletonGraphic(DialogueLine currentLine)
    {
        if (currentLine.animationNames != null && currentLine.animationNames.Count > 0)
        {
            SkeletonGraphic skeletonGraphic = null;
            if (currentLine.characterImage != null)
            {
                skeletonGraphic = currentLine.characterImage.GetComponent<SkeletonGraphic>();
            }

            if (skeletonGraphic == null)
            {
                Debug.LogWarning("SkeletonGraphic 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            for (int i = 0; i < currentLine.animationNames.Count; i++)
            {
                string animName = currentLine.animationNames[i].animationName;
                if (!string.IsNullOrEmpty(animName))
                {
                    if (i == 0)
                    {
                        // 첫 번째 애니메이션은 SetAnimation (트랙 1 사용)
                        skeletonGraphic.AnimationState.SetAnimation(1, animName, true);
                    }
                    else if (i == 1)
                    {
                        skeletonGraphic.AnimationState.ClearTrack(0);
                        // 나머지 애니메이션은 AddAnimation으로 큐에 추가 (트랙 0 사용)
                        skeletonGraphic.AnimationState.SetAnimation(0, animName, false);

                        // 현재 트랙 0의 애니메이션 상태 로그 출력
                        var currentAnim = skeletonGraphic.AnimationState.GetCurrent(0);
                    }
                    else
                    {
                        // 나머지 애니메이션은 AddAnimation으로 큐에 추가 (트랙 0 사용)
                        skeletonGraphic.AnimationState.AddAnimation(0, animName, false, 0f);
                    }
                }
            }
        }
    }

    private void ExecuteFadeTrigger(FadeTrigger trigger)
    {
        switch (trigger)
        {
            case FadeTrigger.None:
            default:
                // 아무 트리거도 실행하지 않음
                break;
        }
    }

    private void DisplayNameAndSentence(string sentence)
    {
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
        }

        textAnimationCoroutine = StartCoroutine(AnimateText(sentence));
    }

    private IEnumerator DisplayNameAndSentenceWithDelay(string sentence, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        DisplayNameAndSentence(sentence);
    }

    private IEnumerator AnimateText(string sentence)
    {
        isAnimating = true; // 애니메이션 시작
        dialogueText.text = "";

        // 타이핑 사운드 재생
        if (typingSoundEvent != null)
        {
            typingSoundPlayingID = typingSoundEvent.Post(gameObject);
        }

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;

            yield return new WaitForSecondsRealtime(textAnimationSpeed);
        }

        isAnimating = false; // 애니메이션 종료

        // 타이핑 사운드 정지
        if (typingSoundPlayingID != 0)
        {
            AkSoundEngine.StopPlayingID(typingSoundPlayingID);
            typingSoundPlayingID = 0;
        }

        // 텍스트 출력 완료 사운드 이벤트 호출
        OnTextFinished?.Invoke();

        // 애니메이션이 끝나면 이미지를 활성화하고, 진행 가능하도록 설정
        animationImage.gameObject.SetActive(true);
        canProceed = true; // 다음 대사로 넘어갈 수 있도록 설정
    }

    private void ToggleCharacterImagesAndNames(DialogueLine currentLine)
    {
        // 현재 대사에 해당하는 캐릭터 이미지 활성화
        if (currentLine.characterImage != null)
        {
            currentLine.characterImage.SetActive(true);
        }

        // 캐릭터 이름 설정
        if (!string.IsNullOrEmpty(currentLine.characterName))
        {
            // 이전 캐릭터 이름과 다를 경우에만 애니메이션 재생
            if (nameText.text != currentLine.characterName)
            {
                nameText.text = currentLine.characterName;
                ActivateSkeletonGraphic(currentLine); // 애니메이션 재생
            }
        }
    }

    private void EndCutscene()
    {
        cutsceneEnded = true;

        // 컷신 종료 이벤트 호출
        OnCutsceneEnded?.Invoke();

        if (sceneChangeSkeleton != null && !lastCut)
        {
            sceneChangeSkeleton.gameObject.SetActive(true);
            sceneChangeSkeleton.PlayCloseAnimation(nextSceneName);
        }
    }

    private IEnumerator DelayForEndCutSence(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        sceneChangeSkeleton.gameObject.SetActive(true);
        sceneChangeSkeleton.PlayCloseAnimation(nextSceneName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 클릭 사운드 이벤트 호출
            OnClickSound?.Invoke();

            if (isAnimating)
            {
                // 애니메이션 중이면 즉시 전체 대사를 표시
                if (textAnimationCoroutine != null)
                {
                    StopCoroutine(textAnimationCoroutine);
                }
                dialogueText.text = currentSentence;
                isAnimating = false;
                animationImage.gameObject.SetActive(true); // 애니메이션 이미지를 활성화
                canProceed = true; // 다음 대사로 넘어갈 수 있도록 설정

                // 타이핑 사운드 정지
                if (typingSoundPlayingID != 0)
                {
                    AkSoundEngine.StopPlayingID(typingSoundPlayingID);
                    typingSoundPlayingID = 0;
                }

                // 텍스트 출력 완료 사운드 이벤트 호출
                OnTextFinished?.Invoke();
            }
            else if (canProceed)
            {
                DisplayNextSentence();
            }
        }
    }
}
