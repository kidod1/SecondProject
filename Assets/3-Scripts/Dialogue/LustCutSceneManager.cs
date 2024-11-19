using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using Spine.Unity; // Spine.Unity 네임스페이스 추가

public class LustCutSceneManager : MonoBehaviour
{
    [System.Serializable]
    public class AnimationName
    {
        [SpineAnimation]
        public string animationName; // 재생할 애니메이션의 이름
    }

    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 10)]
        public string sentence; // 대사
        public string characterName; // 캐릭터 이름
        public GameObject characterImage; // 캐릭터 이미지 오브젝트
        public List<AnimationName> animationNames; // 재생할 애니메이션 이름 리스트
        public int dialogueBoxIndex; // 사용할 대화창의 인덱스
    }

    [System.Serializable]
    public class DialogueBox
    {
        public TMP_Text nameText;
        public TMP_Text dialogueText;
        public Image speechBubble;
        public GameObject dialogueBoxObject; // 대화창 전체 오브젝트
    }

    [System.Serializable]
    public class CutsceneDialogue
    {
        public List<DialogueLine> dialogueLines; // 대화 라인 리스트
    }

    public CutsceneDialogue cutsceneDialogue;
    public DialogueBox[] dialogueBoxes; // 대화창 배열

    public Image animationImage; // animationImage는 개별적으로 관리

    public string nextSceneName;
    public float textAnimationSpeed = 0.05f; // 텍스트 애니메이션 속도

    [Header("Cutscene Events")]
    public UnityEvent OnCutsceneStarted;
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

    private DialogueBox currentDialogueBox;

    // 타이핑 사운드가 재생 중인지 확인하는 변수
    private uint typingSoundPlayingID = 0;

    // 이전 캐릭터 이름 저장용 변수
    private string previousCharacterName = "";

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

        // 시작할 때 모든 대화창 비활성화
        foreach (var dialogueBox in dialogueBoxes)
        {
            if (dialogueBox.dialogueBoxObject != null)
            {
                dialogueBox.dialogueBoxObject.SetActive(false);
            }
        }

        // animationImage의 위치를 변경하지 않고 초기화 (필요에 따라 설정)
        if (animationImage != null)
        {
            animationImage.gameObject.SetActive(false); // 처음에는 비활성화
        }

        StartCutscene();
    }

    private void OnEnable()
    {
        OnCutsceneStarted?.Invoke();
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

    private void DisplayNextSentence()
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
            StopCoroutine(textAnimationCoroutine);
        }

        // 이전 대화창 비활성화
        if (currentDialogueBox != null && currentDialogueBox.dialogueBoxObject != null)
        {
            currentDialogueBox.dialogueBoxObject.SetActive(false);
        }

        DialogueLine currentLine = dialogueQueue.Dequeue();
        currentSentenceIndex++;
        currentSentence = currentLine.sentence;

        // 대화창 선택
        if (currentLine.dialogueBoxIndex >= 0 && currentLine.dialogueBoxIndex < dialogueBoxes.Length)
        {
            currentDialogueBox = dialogueBoxes[currentLine.dialogueBoxIndex];
            currentDialogueBox.dialogueBoxObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("유효하지 않은 dialogueBoxIndex입니다.");
            return;
        }

        // 이름과 대사 설정
        currentDialogueBox.nameText.text = currentLine.characterName;
        currentDialogueBox.dialogueText.text = "";

        // 캐릭터 이미지 및 애니메이션 처리
        ToggleCharacterImagesAndNames(currentLine);

        // animationImage 처리
        if (animationImage != null)
        {
            animationImage.gameObject.SetActive(true);
        }

        // 텍스트 애니메이션 시작
        textAnimationCoroutine = StartCoroutine(
            AnimateText(currentDialogueBox.dialogueText, currentLine.sentence)
        );
    }

    private IEnumerator AnimateText(TMP_Text dialogueText, string sentence)
    {
        isAnimating = true;
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

        isAnimating = false;

        // 타이핑 사운드 정지
        if (typingSoundPlayingID != 0)
        {
            AkSoundEngine.StopPlayingID(typingSoundPlayingID);
            typingSoundPlayingID = 0;
        }

        OnTextFinished?.Invoke();

        canProceed = true;
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
                        // 나머지 애니메이션은 SetAnimation으로 (트랙 0 사용)
                        skeletonGraphic.AnimationState.SetAnimation(0, animName, false);
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

    private void ToggleCharacterImagesAndNames(DialogueLine currentLine)
    {
        // 현재 대사에 해당하는 캐릭터 이미지 활성화
        if (currentLine.characterImage != null)
        {
            currentLine.characterImage.SetActive(true);
        }

        // 캐릭터 이름이 이전과 다르면 애니메이션 재생
        if (previousCharacterName != currentLine.characterName)
        {
            ActivateSkeletonGraphic(currentLine);
            previousCharacterName = currentLine.characterName;
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
                currentDialogueBox.dialogueText.text = currentSentence;
                isAnimating = false;
                canProceed = true;

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
