using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    private Dictionary<string, Dialogue> dialogueDictionary = new Dictionary<string, Dialogue>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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
                    Debug.LogWarning($"Duplicate dialogue name found: {entry.name}");
                }
            }
        }
    }

    private void Start()
    {
        sentences = new Queue<string>();
        backgrounds = new Queue<Sprite>();
        dialoguePanel.SetActive(false); // ������ �� ���̾�α� �г� ��Ȱ��ȭ
        background.gameObject.SetActive(false); // ������ �� ��� �̹��� ��Ȱ��ȭ

        // animator�� null���� Ȯ���ϰ�, null�̸� �Ҵ�
        if (dialogueAnimator == null)
        {
            dialogueAnimator = dialoguePanel.GetComponent<Animator>();
        }

        if (dialogueAnimator == null)
        {
            Debug.LogError("Animator component not found on the dialogue panel.");
        }
        else if (dialogueAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("Animator does not have a runtimeAnimatorController assigned.");
        }

        // ó�� �ε���� �� ���̾�α� Ʈ����
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
        }
    }

    public void TriggerDialogueByName(string name)
    {
        Debug.Log($"Attempting to trigger dialogue with name: {name}");
        if (dialogueDictionary.ContainsKey(name))
        {
            StartDialogue(dialogueDictionary[name]);
        }
        else
        {
            Debug.LogWarning($"Dialogue with name {name} not found.");
        }
    }

    public void StartDialogue(Dialogue dialogue, bool autoClose = false)
    {
        Debug.Log("Starting dialogue...");
        if (isDialogueActive)
        {
            Debug.LogWarning("Dialogue is already active!");
            return;
        }

        if (dialogueAnimator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return;
        }

        dialoguePanel.SetActive(true); // �г��� Ȱ��ȭ
        Debug.Log("Dialogue panel activated.");

        if (!dialogueAnimator.isActiveAndEnabled)
        {
            dialogueAnimator.enabled = true;
        }

        if (dialogueAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("Animator does not have a runtimeAnimatorController assigned!");
            return;
        }

        dialogueAnimator.Rebind(); // Animator�� �����Ͽ� ��� ������ ���·� ����ϴ�.
        dialogueAnimator.Update(0); // Animator ���¸� ������Ʈ�մϴ�.

        // �ִϸ����͸� Unscaled Time���� ����
        dialogueAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        if (imageAnimator != null)
        {
            imageAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        dialogueAnimator.SetTrigger("In");
        Debug.Log("Animator trigger set to 'In'.");

        nameText.text = dialogue.characterName;
        dialogueText.text = ""; // �ؽ�Ʈ�� �ʱ�ȭ
        Debug.Log($"Character name set to: {dialogue.characterName}");

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
        Time.timeScale = 0f; // ���� �ð��� ����
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
            if (bg != null)
            {
                background.sprite = bg;
                background.gameObject.SetActive(true);
            }
            else
            {
                background.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    private void EndDialogue()
    {
        imageAnimator.SetTrigger("Out");
        dialogueAnimator.SetTrigger("Out");
        StartCoroutine(DeactivatePanelAfterAnimation());
    }

    private IEnumerator DeactivatePanelAfterAnimation()
    {
        yield return new WaitForSecondsRealtime(1f); // ���� �ð� �������� ���
        dialoguePanel.SetActive(false);
        background.gameObject.SetActive(false);
        Debug.Log("End of conversation.");

        isDialogueActive = false;
        Time.timeScale = 1f; // ���� �ð��� �ٽ� �帣�� ��

        // �ִϸ����� ������Ʈ ��带 �ٽ� Normal�� ����
        dialogueAnimator.updateMode = AnimatorUpdateMode.Normal;
        if (imageAnimator != null)
        {
            imageAnimator.updateMode = AnimatorUpdateMode.Normal;
        }
    }

    private IEnumerator AutoCloseDialogue()
    {
        yield return new WaitForSecondsRealtime(5); // ���� �ð� �������� ���
        EndDialogue();
    }
}
