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
        // Don't use DontDestroyOnLoad; instance will be managed per scene
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy this instance if one already exists
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
                    Debug.LogWarning($"중복 대화명을 찾았습니다: {entry.name}");
                }
            }
        }
    }

    private void Start()
    {
        sentences = new Queue<string>();
        backgrounds = new Queue<Sprite>();
        dialoguePanel.SetActive(false);
        background.gameObject.SetActive(false);

        if (dialogueAnimator == null)
        {
            dialogueAnimator = dialoguePanel.GetComponent<Animator>();
        }

        if (dialogueAnimator == null)
        {
            Debug.LogError("대화판에서 애니메이터 구성요소를 찾을 수 없습니다.");
        }
        else if (dialogueAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("애니메이터에 런타임 애니메이터 컨트롤러가 할당되지 않았습니다");
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
        }
    }

    public void TriggerDialogueByName(string name)
    {
        Debug.Log($"다음 이름을 통해 다이얼로그를 실행하려고 하는 중: {name}");
        if (dialogueDictionary.ContainsKey(name))
        {
            StartDialogue(dialogueDictionary[name]);
        }
        else
        {
            Debug.LogWarning($"{name} 위와 같은 이름의 다이얼로그를 찾을 수 없습니다.");
        }
    }

    public void StartDialogue(Dialogue dialogue, bool autoClose = false)
    {
        Debug.Log("Starting dialogue...");
        if (isDialogueActive)
        {
            Debug.LogWarning("다이얼로그가 이미 실행중입니다.");
            return;
        }

        if (dialogueAnimator == null)
        {
            Debug.LogError("애니메이터가 존재하지 않습니다.");
            return;
        }

        dialoguePanel.SetActive(true);
        Debug.Log("다이얼로그 패널 활성화.");

        if (!dialogueAnimator.isActiveAndEnabled)
        {
            dialogueAnimator.enabled = true;
        }

        if (dialogueAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("애니메이터가 runtimeAnimatorController 를 가지고 있지 않습니다.");
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
        Debug.Log("애니메이션 트리거 In 활성화");

        nameText.text = dialogue.characterName;
        dialogueText.text = ""; // 텍스트 초기화
        Debug.Log($"캐릭터 이름 설정: {dialogue.characterName}");

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
        Time.timeScale = 0f;
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
        yield return new WaitForSecondsRealtime(1f);
        dialoguePanel.SetActive(false);
        background.gameObject.SetActive(false);
        Debug.Log("다이얼로그 종료");

        isDialogueActive = false;
        Time.timeScale = 1f;

        dialogueAnimator.updateMode = AnimatorUpdateMode.Normal;
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
