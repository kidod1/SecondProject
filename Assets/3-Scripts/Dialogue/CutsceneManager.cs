using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Fade 애니메이션을 제어하기 위한 트리거 타입
/// </summary>
public enum FadeTrigger
{
    None,    // 트리거 없음
    FadeIn,  // FadeIn 트리거 실행
    FadeOut  // FadeOut 트리거 실행
}

/// <summary>
/// 컷신 다이얼로그를 관리하는 매니저 클래스
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneDialogue
    {
        [System.Serializable]
        public struct DialogueEntry
        {
            public string characterName; // 캐릭터 이름
            public GameObject characterImage; // 캐릭터 이미지 오브젝트
            public int hideAfterSentence; // 이 대사 이후에 캐릭터 이미지를 비활성화할 인덱스
        }

        [TextArea(3, 10)]
        public string[] sentences;
        public DialogueEntry[] dialogueEntries; // 대사와 함께 캐릭터 정보를 저장하는 구조체 배열
        public int[] characterImageTogglePoints; // 특정 대사에서 캐릭터 이미지를 활성화할 인덱스
        public FadeTrigger[] fadeTriggers; // 각 대사에 대한 Fade 트리거 배열

        // New fields for 임프
        [Header("Imp Configuration")]
        public int impAppearAfterSentenceIndex = -1; // 대화 인덱스 이후에 임프가 등장
        public int impFadeOutAfterDialogueCount = 2; // 등장 후 몇 번째 대화에서 FadeOut 할지
    }

    public CutsceneDialogue cutsceneDialogue;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image animationImage;
    public int nextSceneIndex = -1;
    public float textAnimationSpeed = 0.05f; // 텍스트 애니메이션 속도

    [Header("UI Elements")]
    public GameObject speechBubble; // 말풍선 UI GameObject
    public GameObject speechBubble2; // 말풍선 UI GameObject
    [SerializeField]
    public Animator speechBubbleAnimator; // 말풍선의 Animator 컴포넌트

    // New fields for 임프
    [Header("Imp Specific Configuration")]
    public GameObject impCharacterImage; // 임프의 캐릭터 이미지 오브젝트
    public string impCharacterName = "Imp"; // 임프의 이름

    private Queue<string> sentences;
    private int currentSentenceIndex = 0;
    private Coroutine textAnimationCoroutine;
    private bool canProceed = true;
    private bool isAnimating = false; // 텍스트 애니메이션 중인지 확인하는 플래그
    private bool cutsceneEnded = false;
    private string currentSentence = ""; // 현재 대사 저장

    private int impFadeOutSentenceIndex = -1; // 임프가 FadeOut 될 대화 인덱스
    private void Start()
    {
        sentences = new Queue<string>();

        // 시작할 때 모든 캐릭터 이미지를 비활성화
        foreach (var entry in cutsceneDialogue.dialogueEntries)
        {
            if (entry.characterImage != null)
            {
                entry.characterImage.SetActive(false);
            }
        }

        // Initialize 임프's image
        if (impCharacterImage != null)
        {
            impCharacterImage.SetActive(false);
        }

        if (speechBubble != null)
        {
            speechBubble.SetActive(true); // 말풍선을 활성화
        }

        if (speechBubbleAnimator != null)
        {
            speechBubbleAnimator.ResetTrigger("FadeOut"); // 초기화 시 트리거 리셋
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
        sentences.Clear();

        foreach (string sentence in cutsceneDialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (!canProceed) return;

        if (sentences.Count == 0)
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

        currentSentence = sentences.Dequeue();
        currentSentenceIndex++;

        ToggleCharacterImagesAndNames();

        // Handle 임프's appearance and fade triggers
        HandleImpAppearanceAndFade();

        // 현재 대사에 대한 Fade 트리거 확인 및 실행
        ExecuteFadeTrigger(currentSentenceIndex - 1); // 배열은 0부터 시작

        // 첫 번째 대사에만 딜레이 추가
        if (currentSentenceIndex == 1)
        {
            StartCoroutine(DisplayNameAndSentenceWithDelay(currentSentence, 3.3f)); // 딜레이 추가
        }
        else
        {
            DisplayNameAndSentence(currentSentence);
        }
    }

    private void HandleImpAppearanceAndFade()
    {
        // Check if 임프 should appear after the previous sentence
        if (cutsceneDialogue.impAppearAfterSentenceIndex != -1 &&
            currentSentenceIndex - 1 == cutsceneDialogue.impAppearAfterSentenceIndex)
        {
            // Activate 임프's image
            if (impCharacterImage != null)
            {
                impCharacterImage.SetActive(true);
            }

            // Set 임프's name
            if (nameText != null)
            {
                nameText.text = impCharacterName;
            }

            // Trigger FadeIn for 임프
            if (speechBubbleAnimator != null)
            {
                speechBubbleAnimator.SetTrigger("FadeIn");
            }

            // Calculate when to fade out 임프
            impFadeOutSentenceIndex = currentSentenceIndex + cutsceneDialogue.impFadeOutAfterDialogueCount;
        }

        // Check if it's time to fade out 임프
        if (impFadeOutSentenceIndex != -1 && currentSentenceIndex == impFadeOutSentenceIndex)
        {
            // Trigger FadeOut for 임프
            if (speechBubbleAnimator != null)
            {
                speechBubbleAnimator.SetTrigger("FadeOut");
            }

            // Deactivate 임프's image after fade out (assuming fade out takes some time, adjust as needed)
            StartCoroutine(DeactivateImpAfterFadeOut());
        }
    }

    private IEnumerator DeactivateImpAfterFadeOut()
    {
        // Wait for the duration of the fade-out animation
        // Replace 1.0f with your actual fade-out animation duration
        yield return new WaitForSeconds(1.0f);

        if (impCharacterImage != null)
        {
            impCharacterImage.SetActive(false);
        }

        impFadeOutSentenceIndex = -1; // Reset
    }

    private void ExecuteFadeTrigger(int sentenceIndex)
    {
        if (cutsceneDialogue.fadeTriggers == null || sentenceIndex >= cutsceneDialogue.fadeTriggers.Length)
        {
            Debug.LogWarning("FadeTriggers 배열이 설정되지 않았거나, 인덱스가 범위를 벗어났습니다.");
            return;
        }

        FadeTrigger trigger = cutsceneDialogue.fadeTriggers[sentenceIndex];

        switch (trigger)
        {
            case FadeTrigger.FadeIn:
                if (speechBubbleAnimator != null)
                {
                    speechBubbleAnimator.SetTrigger("FadeIn");
                }
                break;
            case FadeTrigger.FadeOut:
                if (speechBubbleAnimator != null)
                {
                    speechBubbleAnimator.SetTrigger("FadeOut");
                }
                break;
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

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(textAnimationSpeed);
        }

        isAnimating = false; // 애니메이션 종료

        // 애니메이션이 끝나면 이미지를 활성화하고, 진행 가능하도록 설정
        animationImage.gameObject.SetActive(true);
        canProceed = true;
    }

    private void ToggleCharacterImagesAndNames()
    {
        // 특정 인덱스에서 캐릭터 이미지를 활성화
        for (int i = 0; i < cutsceneDialogue.characterImageTogglePoints.Length; i++)
        {
            if (cutsceneDialogue.characterImageTogglePoints[i] == currentSentenceIndex)
            {
                if (i < cutsceneDialogue.dialogueEntries.Length)
                {
                    var entry = cutsceneDialogue.dialogueEntries[i];
                    if (entry.characterImage != null)
                    {
                        entry.characterImage.SetActive(true);
                    }
                    if (!string.IsNullOrEmpty(entry.characterName))
                    {
                        nameText.text = entry.characterName;
                    }
                }
            }
        }

        // 현재 대사 인덱스보다 hideAfterSentence 값을 가진 캐릭터 이미지를 비활성화
        foreach (var entry in cutsceneDialogue.dialogueEntries)
        {
            if (entry.characterImage != null && currentSentenceIndex > entry.hideAfterSentence)
            {
                entry.characterImage.SetActive(false);
            }
        }
    }

    public void LoadNextScene()
    {
        if (nextSceneIndex >= 0 && nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogError("다음 씬의 인덱스가 유효하지 않습니다.");
        }
    }

    private void EndCutscene()
    {
        cutsceneEnded = true;
        // 필요 시, FadeOut 트리거를 추가로 실행
        if (speechBubbleAnimator != null)
        {
            speechBubbleAnimator.SetTrigger("FadeOut");
        }

        // 다음 씬 로드 (옵션)
        // LoadNextScene();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
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
            }
            else if (canProceed)
            {
                DisplayNextSentence();
            }
        }
    }
}
