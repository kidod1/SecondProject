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
    }

    private void Update()
    {
        if (dialoguePanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(Dialogue dialogue, bool autoClose = false)
    {
        if (dialogueAnimator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return;
        }

        if (!dialogueAnimator.isActiveAndEnabled)
        {
            dialoguePanel.SetActive(true); // 패널을 활성화
            dialogueAnimator.enabled = true;
        }

        if (dialogueAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("Animator does not have a runtimeAnimatorController assigned!");
            return;
        }

        dialogueAnimator.Rebind(); // Animator를 리셋하여 재생 가능한 상태로 만듭니다.
        dialogueAnimator.Update(0); // Animator 상태를 업데이트합니다.

        dialoguePanel.SetActive(true);
        dialogueAnimator.SetTrigger("In");

        nameText.text = dialogue.characterName;

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
        yield return new WaitForSeconds(1f);
        dialoguePanel.SetActive(false);
        background.gameObject.SetActive(false);
        Debug.Log("End of conversation.");
    }

    private IEnumerator AutoCloseDialogue()
    {
        yield return new WaitForSeconds(5);
        EndDialogue();
    }
}
