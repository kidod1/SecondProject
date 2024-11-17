using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using Spine.Unity; // Spine.Unity ���ӽ����̽� �߰�

/// <summary>
/// �ƽ� ���̾�α׸� �����ϴ� �Ŵ��� Ŭ����
/// </summary>
public class LustCutSceneManager : MonoBehaviour
{
    [System.Serializable]
    public class AnimationName
    {
        [Spine.Unity.SpineAnimation]
        public string animationName; // ����� �ִϸ��̼��� �̸�
    }

    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 10)]
        public string sentence; // ���
        public string characterName; // ĳ���� �̸�
        public GameObject characterImage; // ĳ���� �̹��� ������Ʈ
        public List<AnimationName> animationNames; // ���������� ����� �ִϸ��̼� �̸� ����Ʈ
    }

    [System.Serializable]
    public class CutsceneDialogue
    {
        public List<DialogueLine> dialogueLines; // ��ȭ ���� ����Ʈ
    }

    public CutsceneDialogue cutsceneDialogue;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image animationImage;
    public string nextSceneName;
    public float textAnimationSpeed = 0.05f; // �ؽ�Ʈ �ִϸ��̼� �ӵ�

    [Header("Cutscene Events")]
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

    // �߰��� �κ�: Ÿ���� ���尡 ��� ������ Ȯ���ϴ� ����
    private uint typingSoundPlayingID = 0;

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

        animationImage.gameObject.SetActive(false); // �ʱ�ȭ�� �� �ִϸ��̼� �̹����� ��Ȱ��ȭ

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
            StopCoroutine(textAnimationCoroutine); // ���� �ִϸ��̼� ����
        }

        // ���ο� ��簡 ��µǱ� ���� �̹����� ��Ȱ��ȭ
        animationImage.gameObject.SetActive(false);

        DialogueLine currentLine = dialogueQueue.Dequeue();
        currentSentenceIndex++;
        currentSentence = currentLine.sentence; // ���� ��� ����

        ToggleCharacterImagesAndNames(currentLine);

        // Spine �ִϸ��̼� ����� ActivateSkeletonGraphic���� ó��
        ActivateSkeletonGraphic(currentLine);

        // ù ��° ��翡�� ������ �߰�
        if (currentSentenceIndex == 1)
        {
            StartCoroutine(DisplayNameAndSentenceWithDelay(currentLine.sentence, 3.3f)); // ������ �߰�
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
                        // ������ �ִϸ��̼��� AddAnimation���� ť�� �߰� (Ʈ�� 0 ���)
                        skeletonGraphic.AnimationState.SetAnimation(0, animName, false);

                        // ���� Ʈ�� 0�� �ִϸ��̼� ���� �α� ���
                        var currentAnim = skeletonGraphic.AnimationState.GetCurrent(0);
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

    private void ExecuteFadeTrigger(FadeTrigger trigger)
    {
        switch (trigger)
        {
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

        isAnimating = false; // �ִϸ��̼� ����

        // Ÿ���� ���� ����
        if (typingSoundPlayingID != 0)
        {
            AkSoundEngine.StopPlayingID(typingSoundPlayingID);
            typingSoundPlayingID = 0;
        }

        // �ؽ�Ʈ ��� �Ϸ� ���� �̺�Ʈ ȣ��
        OnTextFinished?.Invoke();

        // �ִϸ��̼��� ������ �̹����� Ȱ��ȭ�ϰ�, ���� �����ϵ��� ����
        animationImage.gameObject.SetActive(true);
        canProceed = true; // ���� ���� �Ѿ �� �ֵ��� ����
    }

    private void ToggleCharacterImagesAndNames(DialogueLine currentLine)
    {
        // ���� ��翡 �ش��ϴ� ĳ���� �̹��� Ȱ��ȭ
        if (currentLine.characterImage != null)
        {
            currentLine.characterImage.SetActive(true);
        }

        // ĳ���� �̸� ����
        if (!string.IsNullOrEmpty(currentLine.characterName))
        {
            // ���� ĳ���� �̸��� �ٸ� ��쿡�� �ִϸ��̼� ���
            if (nameText.text != currentLine.characterName)
            {
                nameText.text = currentLine.characterName;
                ActivateSkeletonGraphic(currentLine); // �ִϸ��̼� ���
            }
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
            // Ŭ�� ���� �̺�Ʈ ȣ��
            OnClickSound?.Invoke();

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
