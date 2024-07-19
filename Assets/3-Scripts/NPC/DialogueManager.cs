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
        dialoguePanel.SetActive(false); // 시작할 때 다이얼로그 패널 비활성화
        background.gameObject.SetActive(false); // 시작할 때 배경 이미지 비활성화

        // animator가 null인지 확인하고, null이면 할당
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

        // 처음 로드됐을 때 다이얼로그 트리거
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

        dialoguePanel.SetActive(true); // 패널을 활성화
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

        dialogueAnimator.Rebind(); // Animator를 리셋하여 재생 가능한 상태로 만듭니다.
        dialogueAnimator.Update(0); // Animator 상태를 업데이트합니다.

        // 애니메이터를 Unscaled Time으로 설정
        dialogueAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        if (imageAnimator != null)
        {
            imageAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        dialogueAnimator.SetTrigger("In");
        Debug.Log("Animator trigger set to 'In'.");

        nameText.text = dialogue.characterName;
        dialogueText.text = ""; // 텍스트를 초기화
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
        Time.timeScale = 0f; // 게임 시간을 멈춤
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
        yield return new WaitForSecondsRealtime(1f); // 실제 시간 기준으로 대기
        dialoguePanel.SetActive(false);
        background.gameObject.SetActive(false);
        Debug.Log("End of conversation.");

        isDialogueActive = false;
        Time.timeScale = 1f; // 게임 시간을 다시 흐르게 함

        // 애니메이터 업데이트 모드를 다시 Normal로 설정
        dialogueAnimator.updateMode = AnimatorUpdateMode.Normal;
        if (imageAnimator != null)
        {
            imageAnimator.updateMode = AnimatorUpdateMode.Normal;
        }
    }

    private IEnumerator AutoCloseDialogue()
    {
        yield return new WaitForSecondsRealtime(5); // 실제 시간 기준으로 대기
        EndDialogue();
    }
}
