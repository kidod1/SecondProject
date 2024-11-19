using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using Spine.Unity; // Spine.Unity ���ӽ����̽� �߰�

public class LustCutSceneManager : MonoBehaviour
{
    [System.Serializable]
    public class AnimationName
    {
        [SpineAnimation]
        public string animationName; // ����� �ִϸ��̼��� �̸�
    }

    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 10)]
        public string sentence; // ���
        public string characterName; // ĳ���� �̸�
        public GameObject characterImage; // ĳ���� �̹��� ������Ʈ
        public List<AnimationName> animationNames; // ����� �ִϸ��̼� �̸� ����Ʈ
        public int dialogueBoxIndex; // ����� ��ȭâ�� �ε���
    }

    [System.Serializable]
    public class DialogueBox
    {
        public TMP_Text nameText;
        public TMP_Text dialogueText;
        public Image speechBubble;
        public GameObject dialogueBoxObject; // ��ȭâ ��ü ������Ʈ
    }

    [System.Serializable]
    public class CutsceneDialogue
    {
        public List<DialogueLine> dialogueLines; // ��ȭ ���� ����Ʈ
    }

    public CutsceneDialogue cutsceneDialogue;
    public DialogueBox[] dialogueBoxes; // ��ȭâ �迭

    public Image animationImage; // animationImage�� ���������� ����

    public string nextSceneName;
    public float textAnimationSpeed = 0.05f; // �ؽ�Ʈ �ִϸ��̼� �ӵ�

    [Header("Cutscene Events")]
    public UnityEvent OnCutsceneStarted;
    public UnityEvent OnCutsceneEnded; // �ƽ��� ����� �� ȣ��Ǵ� �̺�Ʈ

    [Header("Sound Events")]
    public AK.Wwise.Event typingSoundEvent;   // �ؽ�Ʈ ��� �� ����� Ÿ���� ���� �̺�Ʈ
    public UnityEvent OnTextFinished;        // �ؽ�Ʈ ����� �Ϸ�Ǿ��� �� ȣ��Ǵ� �̺�Ʈ
    public UnityEvent OnClickSound;          // Ŭ�� �� ȣ��Ǵ� �̺�Ʈ

    private Queue<DialogueLine> dialogueQueue;
    private int currentSentenceIndex = 0;
    private Coroutine textAnimationCoroutine;
    private bool canProceed = true;
    private bool isAnimating = false; // �ؽ�Ʈ �ִϸ��̼� ������ Ȯ���ϴ� �÷���
    private bool cutsceneEnded = false;
    private string currentSentence = ""; // ���� ��� ����

    [SerializeField]
    private SceneChangeSkeleton sceneChangeSkeleton;
    [SerializeField]
    private bool lastCut = false;

    private DialogueBox currentDialogueBox;

    // Ÿ���� ���尡 ��� ������ Ȯ���ϴ� ����
    private uint typingSoundPlayingID = 0;

    // ���� ĳ���� �̸� ����� ����
    private string previousCharacterName = "";

    private void Start()
    {
        dialogueQueue = new Queue<DialogueLine>();

        // ������ �� ��� ĳ���� �̹����� ��Ȱ��ȭ
        foreach (var line in cutsceneDialogue.dialogueLines)
        {
            if (line.characterImage != null)
            {
                line.characterImage.SetActive(false);
            }
        }

        // ������ �� ��� ��ȭâ ��Ȱ��ȭ
        foreach (var dialogueBox in dialogueBoxes)
        {
            if (dialogueBox.dialogueBoxObject != null)
            {
                dialogueBox.dialogueBoxObject.SetActive(false);
            }
        }

        // animationImage�� ��ġ�� �������� �ʰ� �ʱ�ȭ (�ʿ信 ���� ����)
        if (animationImage != null)
        {
            animationImage.gameObject.SetActive(false); // ó������ ��Ȱ��ȭ
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

        // ���� ��ȭâ ��Ȱ��ȭ
        if (currentDialogueBox != null && currentDialogueBox.dialogueBoxObject != null)
        {
            currentDialogueBox.dialogueBoxObject.SetActive(false);
        }

        DialogueLine currentLine = dialogueQueue.Dequeue();
        currentSentenceIndex++;
        currentSentence = currentLine.sentence;

        // ��ȭâ ����
        if (currentLine.dialogueBoxIndex >= 0 && currentLine.dialogueBoxIndex < dialogueBoxes.Length)
        {
            currentDialogueBox = dialogueBoxes[currentLine.dialogueBoxIndex];
            currentDialogueBox.dialogueBoxObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("��ȿ���� ���� dialogueBoxIndex�Դϴ�.");
            return;
        }

        // �̸��� ��� ����
        currentDialogueBox.nameText.text = currentLine.characterName;
        currentDialogueBox.dialogueText.text = "";

        // ĳ���� �̹��� �� �ִϸ��̼� ó��
        ToggleCharacterImagesAndNames(currentLine);

        // animationImage ó��
        if (animationImage != null)
        {
            animationImage.gameObject.SetActive(true);
        }

        // �ؽ�Ʈ �ִϸ��̼� ����
        textAnimationCoroutine = StartCoroutine(
            AnimateText(currentDialogueBox.dialogueText, currentLine.sentence)
        );
    }

    private IEnumerator AnimateText(TMP_Text dialogueText, string sentence)
    {
        isAnimating = true;
        dialogueText.text = "";

        // Ÿ���� ���� ���
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

        // Ÿ���� ���� ����
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
                Debug.LogWarning("SkeletonGraphic ������Ʈ�� ã�� �� �����ϴ�.");
                return;
            }

            for (int i = 0; i < currentLine.animationNames.Count; i++)
            {
                string animName = currentLine.animationNames[i].animationName;
                if (!string.IsNullOrEmpty(animName))
                {
                    if (i == 0)
                    {
                        // ù ��° �ִϸ��̼��� SetAnimation (Ʈ�� 1 ���)
                        skeletonGraphic.AnimationState.SetAnimation(1, animName, true);
                    }
                    else if (i == 1)
                    {
                        skeletonGraphic.AnimationState.ClearTrack(0);
                        // ������ �ִϸ��̼��� SetAnimation���� (Ʈ�� 0 ���)
                        skeletonGraphic.AnimationState.SetAnimation(0, animName, false);
                    }
                    else
                    {
                        // ������ �ִϸ��̼��� AddAnimation���� ť�� �߰� (Ʈ�� 0 ���)
                        skeletonGraphic.AnimationState.AddAnimation(0, animName, false, 0f);
                    }
                }
            }
        }
    }

    private void ToggleCharacterImagesAndNames(DialogueLine currentLine)
    {
        // ���� ��翡 �ش��ϴ� ĳ���� �̹��� Ȱ��ȭ
        if (currentLine.characterImage != null)
        {
            currentLine.characterImage.SetActive(true);
        }

        // ĳ���� �̸��� ������ �ٸ��� �ִϸ��̼� ���
        if (previousCharacterName != currentLine.characterName)
        {
            ActivateSkeletonGraphic(currentLine);
            previousCharacterName = currentLine.characterName;
        }
    }

    private void EndCutscene()
    {
        cutsceneEnded = true;

        // �ƽ� ���� �̺�Ʈ ȣ��
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
            // Ŭ�� ���� �̺�Ʈ ȣ��
            OnClickSound?.Invoke();

            if (isAnimating)
            {
                // �ִϸ��̼� ���̸� ��� ��ü ��縦 ǥ��
                if (textAnimationCoroutine != null)
                {
                    StopCoroutine(textAnimationCoroutine);
                }
                currentDialogueBox.dialogueText.text = currentSentence;
                isAnimating = false;
                canProceed = true;

                // Ÿ���� ���� ����
                if (typingSoundPlayingID != 0)
                {
                    AkSoundEngine.StopPlayingID(typingSoundPlayingID);
                    typingSoundPlayingID = 0;
                }

                // �ؽ�Ʈ ��� �Ϸ� ���� �̺�Ʈ ȣ��
                OnTextFinished?.Invoke();
            }
            else if (canProceed)
            {
                DisplayNextSentence();
            }
        }
    }
}
