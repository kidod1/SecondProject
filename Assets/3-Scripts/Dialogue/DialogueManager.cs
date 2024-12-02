using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // �� �Ŵ��� ����� ���� �ʿ�

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField]
    private GameObject dialoguePanel;
    [SerializeField]
    private TMP_Text nameText;
    [SerializeField]
    private TMP_Text dialogueText;
    [SerializeField]
    private Animator dialogueAnimator;
    [SerializeField]
    private Animator imageAnimator;
    [SerializeField]
    private Image background;

    private Queue<string> sentences;
    private Queue<Sprite> backgrounds;

    [SerializeField]
    private Sprite defaultBackground;
    [SerializeField]
    private Sprite zoomedBackground;

    private bool isDialogueActive = false;
    private bool firstDialogueShown = false;
    private bool keepPanelActive = false;  // �г��� �������� ����

    private int nextSceneIndex = -1; // ���� �� �ε��� ����

    private Dictionary<string, Dialogue> dialogueDictionary = new Dictionary<string, Dialogue>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeDialogueDictionary();
    }

    private void InitializeDialogueDictionary()
    {
        DialogueTrigger[] triggers = FindObjectsOfType<DialogueTrigger>();
        foreach (DialogueTrigger trigger in triggers)
        {
            foreach (var entry in trigger.dialogues)
            {
                if (!dialogueDictionary.ContainsKey(entry.name))
                {
                    dialogueDictionary.Add(entry.name, entry.dialogue);
                }
                else
                {
                    Debug.LogWarning($"�ߺ� ��ȭ���� ã�ҽ��ϴ�: {entry.name}");
                }
            }
        }
    }

    private void Start()
    {
        sentences = new Queue<string>();
        backgrounds = new Queue<Sprite>();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("dialoguePanel�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (background != null)
        {
            background.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("background �̹����� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (dialogueAnimator == null)
        {
            if (dialoguePanel != null)
            {
                dialogueAnimator = dialoguePanel.GetComponent<Animator>();
                if (dialogueAnimator == null)
                {
                    Debug.LogError("��ȭ�ǿ��� �ִϸ����� ������Ҹ� ã�� �� �����ϴ�.");
                }
            }
            else
            {
                Debug.LogError("dialoguePanel�� �������� �ʾұ� ������ �ִϸ����͸� ã�� �� �����ϴ�.");
            }
        }

        if (dialogueAnimator != null && dialogueAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("�ִϸ����Ϳ� ��Ÿ�� �ִϸ����� ��Ʈ�ѷ��� �Ҵ���� �ʾҽ��ϴ�");
        }

        TriggerInitialDialogue();
    }

    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            DisplayNextSentence();
        }
    }

    private void TriggerInitialDialogue()
    {
        if (!firstDialogueShown)
        {
            var initialDialogueTrigger = FindObjectOfType<DialogueTrigger>();
            if (initialDialogueTrigger != null)
            {
                initialDialogueTrigger.TriggerDialogueByName("StartDialogue");
                firstDialogueShown = true;
            }
            else
            {
                Debug.LogWarning("�ʱ� ��ȭ Ʈ���Ÿ� ã�� �� �����ϴ�.");
            }
        }
    }

    public void TriggerDialogueByName(string name)
    {
        Debug.Log($"���� �̸��� ���� ���̾�α׸� �����Ϸ��� �ϴ� ��: {name}");
        if (dialogueDictionary.ContainsKey(name))
        {
            StartDialogue(dialogueDictionary[name]);
        }
        else
        {
            Debug.LogWarning($"{name} ���� ���� �̸��� ���̾�α׸� ã�� �� �����ϴ�.");
        }
    }

    public void StartDialogue(Dialogue dialogue, bool autoClose = false, bool keepPanel = false, int nextScene = -1)
    {
        Debug.Log("Starting dialogue...");

        if (isDialogueActive)
        {
            Debug.LogWarning("���̾�αװ� �̹� �������Դϴ�.");
            return;
        }

        if (dialogueAnimator == null)
        {
            Debug.LogError("�ִϸ����Ͱ� �������� �ʽ��ϴ�.");
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            Debug.Log("���̾�α� �г� Ȱ��ȭ.");
        }
        else
        {
            Debug.LogWarning("dialoguePanel�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (!dialogueAnimator.isActiveAndEnabled)
        {
            dialogueAnimator.enabled = true;
        }

        if (dialogueAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("�ִϸ����Ͱ� runtimeAnimatorController�� ������ ���� �ʽ��ϴ�.");
            return;
        }

        dialogueAnimator.Rebind();
        dialogueAnimator.Update(0);

        dialogueAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        if (imageAnimator != null)
        {
            imageAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        dialogueAnimator.SetTrigger("In");
        Debug.Log("�ִϸ��̼� Ʈ���� In Ȱ��ȭ");

        if (nameText != null)
        {
            nameText.text = dialogue.characterName;
            Debug.Log($"ĳ���� �̸� ����: {dialogue.characterName}");
        }
        else
        {
            Debug.LogWarning("nameText�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (dialogueText != null)
        {
            dialogueText.text = ""; // �ؽ�Ʈ �ʱ�ȭ
        }
        else
        {
            Debug.LogWarning("dialogueText�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        sentences.Clear();
        backgrounds.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        foreach (Sprite bg in dialogue.backgrounds)
        {
            backgrounds.Enqueue(bg);
        }

        DisplayNextSentence();

        if (autoClose)
        {
            StartCoroutine(AutoCloseDialogue());
        }

        isDialogueActive = true;
        keepPanelActive = keepPanel;
        nextSceneIndex = nextScene; // ���� �� �ε��� ����
        Time.timeScale = 0f;
    }

    public bool IsDialogueActive
    {
        get { return isDialogueActive; }
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));

        if (backgrounds.Count > 0)
        {
            Sprite bg = backgrounds.Dequeue();
            if (bg != null && background != null)
            {
                background.sprite = bg;
                background.gameObject.SetActive(true);
            }
            else if (background != null)
            {
                background.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("background �̹����� �Ҵ���� �ʾҽ��ϴ�.");
            }
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        if (dialogueText == null)
        {
            Debug.LogWarning("dialogueText�� �Ҵ���� �ʾҽ��ϴ�.");
            yield break;
        }

        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    private void EndDialogue()
    {
        if (nextSceneIndex >= 0) // ���� ���� �����Ǿ� �ִ� ��� �ٷ� �� ��ȯ
        {
            PlayManager.I.ChangeScene(nextSceneIndex);
            return;
        }

        if (imageAnimator != null)
        {
            imageAnimator.SetTrigger("Out");
        }
        else
        {
            Debug.LogWarning("imageAnimator�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (dialogueAnimator != null)
        {
            dialogueAnimator.SetTrigger("Out");
        }
        else
        {
            Debug.LogWarning("dialogueAnimator�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        StartCoroutine(DeactivatePanelAfterAnimation());
    }

    private IEnumerator DeactivatePanelAfterAnimation()
    {
        if (nextSceneIndex >= 0)
        {
            // ���� ���� �����Ǿ� ������ �г��� ��Ȱ��ȭ���� �ʰ� �ٷ� ����
            yield break;
        }

        yield return new WaitForSecondsRealtime(1f);

        if (!keepPanelActive)
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
            else
            {
                Debug.LogWarning("dialoguePanel�� �Ҵ���� �ʾҽ��ϴ�.");
            }

            if (background != null)
            {
                background.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("background �̹����� �Ҵ���� �ʾҽ��ϴ�.");
            }
        }

        Debug.Log("���̾�α� ����");

        isDialogueActive = false;
        Time.timeScale = 1f;

        if (dialogueAnimator != null)
        {
            dialogueAnimator.updateMode = AnimatorUpdateMode.Normal;
        }

        if (imageAnimator != null)
        {
            imageAnimator.updateMode = AnimatorUpdateMode.Normal;
        }
    }

    private IEnumerator AutoCloseDialogue()
    {
        yield return new WaitForSecondsRealtime(5);
        EndDialogue();
    }
}
