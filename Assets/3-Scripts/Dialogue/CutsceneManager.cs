using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Fade �ִϸ��̼��� �����ϱ� ���� Ʈ���� Ÿ��
/// </summary>
public enum FadeTrigger
{
    None,    // Ʈ���� ����
    FadeIn,  // FadeIn Ʈ���� ����
    FadeOut  // FadeOut Ʈ���� ����
}

/// <summary>
/// �ƽ� ���̾�α׸� �����ϴ� �Ŵ��� Ŭ����
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneDialogue
    {
        [System.Serializable]
        public struct DialogueEntry
        {
            public string characterName; // ĳ���� �̸�
            public GameObject characterImage; // ĳ���� �̹��� ������Ʈ
            public int hideAfterSentence; // �� ��� ���Ŀ� ĳ���� �̹����� ��Ȱ��ȭ�� �ε���
        }

        [TextArea(3, 10)]
        public string[] sentences;
        public DialogueEntry[] dialogueEntries; // ���� �Բ� ĳ���� ������ �����ϴ� ����ü �迭
        public int[] characterImageTogglePoints; // Ư�� ��翡�� ĳ���� �̹����� Ȱ��ȭ�� �ε���
        public FadeTrigger[] fadeTriggers; // �� ��翡 ���� Fade Ʈ���� �迭

        // New fields for ����
        [Header("Imp Configuration")]
        public int impAppearAfterSentenceIndex = -1; // ��ȭ �ε��� ���Ŀ� ������ ����
        public int impFadeOutAfterDialogueCount = 2; // ���� �� �� ��° ��ȭ���� FadeOut ����
    }

    public CutsceneDialogue cutsceneDialogue;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image animationImage;
    public string nextSceneName;
    public float textAnimationSpeed = 0.05f; // �ؽ�Ʈ �ִϸ��̼� �ӵ�

    [Header("UI Elements")]
    public GameObject speechBubble; // ��ǳ�� UI GameObject
    public GameObject speechBubble2; // ��ǳ�� UI GameObject
    [SerializeField]
    public Animator speechBubbleAnimator; // ��ǳ���� Animator ������Ʈ

    // New fields for ����
    [Header("Imp Specific Configuration")]
    public GameObject impCharacterImage; // ������ ĳ���� �̹��� ������Ʈ
    public string impCharacterName = "Imp"; // ������ �̸�

    private Queue<string> sentences;
    private int currentSentenceIndex = 0;
    private Coroutine textAnimationCoroutine;
    private bool canProceed = true;
    private bool isAnimating = false; // �ؽ�Ʈ �ִϸ��̼� ������ Ȯ���ϴ� �÷���
    private bool cutsceneEnded = false;
    private string currentSentence = ""; // ���� ��� ����

    private int impFadeOutSentenceIndex = -1; // ������ FadeOut �� ��ȭ �ε���

    [SerializeField]
    private SceneChangeSkeleton sceneChangeSkeleton;
    private void Start()
    {
        sentences = new Queue<string>();

        // ������ �� ��� ĳ���� �̹����� ��Ȱ��ȭ
        foreach (var entry in cutsceneDialogue.dialogueEntries)
        {
            if (entry.characterImage != null)
            {
                entry.characterImage.SetActive(false);
            }
        }

        // Initialize ����'s image
        if (impCharacterImage != null)
        {
            impCharacterImage.SetActive(false);
        }

        if (speechBubble != null)
        {
            speechBubble.SetActive(true); // ��ǳ���� Ȱ��ȭ
        }

        if (speechBubbleAnimator != null)
        {
            speechBubbleAnimator.ResetTrigger("FadeOut"); // �ʱ�ȭ �� Ʈ���� ����
        }

        animationImage.gameObject.SetActive(false); // �ʱ�ȭ�� �� �ִϸ��̼� �̹����� ��Ȱ��ȭ
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
            StopCoroutine(textAnimationCoroutine); // ���� �ִϸ��̼� ����
        }

        // ���ο� ��簡 ��µǱ� ���� �̹����� ��Ȱ��ȭ
        animationImage.gameObject.SetActive(false);

        currentSentence = sentences.Dequeue();
        currentSentenceIndex++;

        ToggleCharacterImagesAndNames();

        // Handle ����'s appearance and fade triggers
        HandleImpAppearanceAndFade();

        // ���� ��翡 ���� Fade Ʈ���� Ȯ�� �� ����
        ExecuteFadeTrigger(currentSentenceIndex - 1); // �迭�� 0���� ����

        // ù ��° ��翡�� ������ �߰�
        if (currentSentenceIndex == 1)
        {
            StartCoroutine(DisplayNameAndSentenceWithDelay(currentSentence, 3.3f)); // ������ �߰�
        }
        else
        {
            DisplayNameAndSentence(currentSentence);
        }
    }

    private void HandleImpAppearanceAndFade()
    {
        // Check if ���� should appear after the previous sentence
        if (cutsceneDialogue.impAppearAfterSentenceIndex != -1 &&
            currentSentenceIndex - 1 == cutsceneDialogue.impAppearAfterSentenceIndex)
        {
            // Activate ����'s image
            if (impCharacterImage != null)
            {
                impCharacterImage.SetActive(true);
            }

            // Set ����'s name
            if (nameText != null)
            {
                nameText.text = impCharacterName;
            }

            // Trigger FadeIn for ����
            if (speechBubbleAnimator != null)
            {
                speechBubbleAnimator.SetTrigger("FadeIn");
            }

            // Calculate when to fade out ����
            impFadeOutSentenceIndex = currentSentenceIndex + cutsceneDialogue.impFadeOutAfterDialogueCount;
        }

        // Check if it's time to fade out ����
        if (impFadeOutSentenceIndex != -1 && currentSentenceIndex == impFadeOutSentenceIndex)
        {
            // Trigger FadeOut for ����
            if (speechBubbleAnimator != null)
            {
                speechBubbleAnimator.SetTrigger("FadeOut");
            }

            StartCoroutine(DeactivateImpAfterFadeOut());
        }
    }

    private IEnumerator DeactivateImpAfterFadeOut()
    {
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
            Debug.LogWarning("FadeTriggers �迭�� �������� �ʾҰų�, �ε����� ������ ������ϴ�.");
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
                // �ƹ� Ʈ���ŵ� �������� ����
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
        isAnimating = true; // �ִϸ��̼� ����
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(textAnimationSpeed);
        }

        isAnimating = false; // �ִϸ��̼� ����

        // �ִϸ��̼��� ������ �̹����� Ȱ��ȭ�ϰ�, ���� �����ϵ��� ����
        animationImage.gameObject.SetActive(true);
        canProceed = true;
    }

    private void ToggleCharacterImagesAndNames()
    {
        // Ư�� �ε������� ĳ���� �̹����� Ȱ��ȭ
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

        // ���� ��� �ε������� hideAfterSentence ���� ���� ĳ���� �̹����� ��Ȱ��ȭ
        foreach (var entry in cutsceneDialogue.dialogueEntries)
        {
            if (entry.characterImage != null && currentSentenceIndex > entry.hideAfterSentence)
            {
                entry.characterImage.SetActive(false);
            }
        }
    }
    private void EndCutscene()
    {
        cutsceneEnded = true;

        if (speechBubbleAnimator != null)
        {
            speechBubbleAnimator.SetTrigger("FadeOut");
        }

        if (sceneChangeSkeleton != null)
        {
            sceneChangeSkeleton.gameObject.SetActive(true);
            sceneChangeSkeleton.PlayCloseAnimation("7_SlothMap");
        }
        else
        {
            Debug.LogWarning("SceneChangeSkeleton�� �������� �ʽ��ϴ�. �ٷ� ���� ������ �̵��մϴ�.");
        }
    }
    private IEnumerator DelayForEndCutSence(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        sceneChangeSkeleton.gameObject.SetActive(true);
        sceneChangeSkeleton.PlayCloseAnimation("7_SlothMap");
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isAnimating)
            {
                // �ִϸ��̼� ���̸� ��� ��ü ��縦 ǥ��
                if (textAnimationCoroutine != null)
                {
                    StopCoroutine(textAnimationCoroutine);
                }
                dialogueText.text = currentSentence;
                isAnimating = false;
                animationImage.gameObject.SetActive(true); // �ִϸ��̼� �̹����� Ȱ��ȭ
                canProceed = true; // ���� ���� �Ѿ �� �ֵ��� ����
            }
            else if (canProceed)
            {
                DisplayNextSentence();
            }
        }
    }
}
