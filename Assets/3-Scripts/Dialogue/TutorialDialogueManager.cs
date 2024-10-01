using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Ʃ�丮�� ���̾�α��� ���� Ÿ���� �����ϴ� ������
/// </summary>
public enum ConditionType
{
    None,                // ���� ����
    PressWASD,           // WASD Ű �Է�
    PressR,              // R Ű �Է�
    DefeatAllMonsters    // ��� ���� óġ
}

/// <summary>
/// �� ��ȭ �����͸� �����ϴ� Ŭ����
/// </summary>
[System.Serializable]
public class DialogueData
{
    [TextArea(3, 10)]
    public string sentence; // ��� ����
    public bool isConditional; // ���Ǻ� ������� ����
    public ConditionType conditionType; // ���� Ÿ��
    public GameObject associatedImage; // ���� �Բ� ������ �̹��� (�ɼ�)
}

/// <summary>
/// Ʃ�丮�� ���̾�α׸� �����ϴ� �Ŵ��� Ŭ����
/// </summary>
public class TutorialDialogueManager : MonoBehaviour
{
    [Header("��ȭ ���")]
    public List<DialogueData> dialogues; // ��� ���

    [Header("UI ���")]
    public TMP_Text dialogueText; // ��� �ؽ�Ʈ UI
    public Image dialogueImage; // ��� �̹��� UI

    [Header("�ִϸ��̼� �� �̹��� ����")]
    public float textAnimationSpeed = 0.05f; // �ؽ�Ʈ �ִϸ��̼� �ӵ�
    public float conditionTimeout = 10f; // ���� ���� ��� �ð�
    public float autoAdvanceDuration = 3f; // �Ϲ� ��� �ڵ� ���� �ð�

    private int currentDialogueIndex = 0; // ���� ��� �ε���
    private Coroutine textAnimationCoroutine; // �ؽ�Ʈ �ִϸ��̼� �ڷ�ƾ
    private Coroutine conditionCoroutine; // ���� ��� �ڷ�ƾ

    private bool conditionMet = false; // ���� ���� ����

    // �÷��̾� �Է� �� ���� üũ�� ���� ������
    private bool hasMoved = false; // �÷��̾ WASD�� ��������
    private bool hasUsedAbility = false; // �÷��̾ R Ű�� ���� �ɷ��� ����ߴ���
    private bool allMonstersDefeated = false; // ��� ���͸� �����ƴ���

    private void Start()
    {
        // ���� �� ù ��° ��縦 ���
        DisplayNextDialogue();
    }

    private void Update()
    {
        // �÷��̾��� ������ üũ (WASD �Է�)
        if (!hasMoved && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                          Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)))
        {
            hasMoved = true;
        }

        // �÷��̾��� �ɷ� ��� üũ (R Ű �Է�)
        if (!hasUsedAbility && Input.GetKeyDown(KeyCode.R))
        {
            hasUsedAbility = true;
        }

        // ���� óġ ���� üũ
        if (!allMonstersDefeated && AreAllMonstersDefeated())
        {
            allMonstersDefeated = true;
        }
    }

    /// <summary>
    /// �� ���� ��� ���Ͱ� óġ�Ǿ����� Ȯ���ϴ� �޼���
    /// </summary>
    /// <returns>��� ���Ͱ� óġ�Ǿ����� true, �ƴϸ� false</returns>
    private bool AreAllMonstersDefeated()
    {
        Monster[] monsters = FindObjectsOfType<Monster>();
        return monsters.Length == 0;
    }

    /// <summary>
    /// ���� ��縦 ȭ�鿡 ǥ���ϴ� �޼���
    /// </summary>
    public void DisplayNextDialogue()
    {
        if (currentDialogueIndex >= dialogues.Count)
        {
            EndDialogue();
            return;
        }

        DialogueData dialogue = dialogues[currentDialogueIndex];

        // ���� �ؽ�Ʈ �ִϸ��̼� ����
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
        }

        // ���� �̹��� ����
        if (dialogueImage != null)
        {
            dialogueImage.sprite = dialogue.associatedImage != null ? dialogue.associatedImage.GetComponent<SpriteRenderer>().sprite : null;
            dialogueImage.gameObject.SetActive(dialogue.associatedImage != null);
        }

        // �ؽ�Ʈ �ִϸ��̼� ����
        textAnimationCoroutine = StartCoroutine(AnimateText(dialogue.sentence));

        if (dialogue.isConditional)
        {
            // ���Ǻ� ����� ��� ���� ��� �ڷ�ƾ ����
            if (conditionCoroutine != null)
            {
                StopCoroutine(conditionCoroutine);
            }
            conditionCoroutine = StartCoroutine(WaitForCondition(dialogue.conditionType));
        }
        else
        {
            // �Ϲ� ����� ��� �ڵ� ���� �ڷ�ƾ ����
            StartCoroutine(WaitForAutoAdvance());
        }
    }

    /// <summary>
    /// �ؽ�Ʈ�� �� ���ھ� ǥ���ϴ� �ִϸ��̼� �ڷ�ƾ
    /// </summary>
    /// <param name="sentence">ǥ���� ���</param>
    /// <returns></returns>
    private IEnumerator AnimateText(string sentence)
    {
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(textAnimationSpeed);
        }
    }

    /// <summary>
    /// �Ϲ� ��縦 ���� �ð� �� �ڵ����� ���� ���� �����ϴ� �ڷ�ƾ
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForAutoAdvance()
    {
        yield return new WaitForSecondsRealtime(autoAdvanceDuration);
        currentDialogueIndex++;
        DisplayNextDialogue();
    }

    /// <summary>
    /// ���Ǻ� ��縦 ���� ���� ������ ��ٸ��� �ڷ�ƾ
    /// </summary>
    /// <param name="conditionType">����� ���� Ÿ��</param>
    /// <returns></returns>
    private IEnumerator WaitForCondition(ConditionType conditionType)
    {
        float elapsedTime = 0f;
        conditionMet = false;

        while (elapsedTime < conditionTimeout)
        {
            if (CheckConditionMet(conditionType))
            {
                conditionMet = true;
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (conditionMet)
        {
            currentDialogueIndex++;
            DisplayNextDialogue();
        }
        else
        {
            // ���� ������ �� ���� �̹��� �����
            DisplayNextDialogue();
        }
    }

    /// <summary>
    /// �־��� ���� Ÿ���� �����Ǿ����� Ȯ���ϴ� �޼���
    /// </summary>
    /// <param name="conditionType">Ȯ���� ���� Ÿ��</param>
    /// <returns>������ �����Ǿ����� true, �ƴϸ� false</returns>
    private bool CheckConditionMet(ConditionType conditionType)
    {
        switch (conditionType)
        {
            case ConditionType.PressWASD:
                return hasMoved;
            case ConditionType.PressR:
                return hasUsedAbility;
            case ConditionType.DefeatAllMonsters:
                return allMonstersDefeated;
            default:
                return false;
        }
    }

    /// <summary>
    /// ��ȭ�� ��� ������ �� ȣ��Ǵ� �޼���
    /// </summary>
    private void EndDialogue()
    {
        // Ʃ�丮�� ���� �Ǵ� ���� ������ �̵� ���� ó��
        Debug.Log("Ʃ�丮�� ����");
        // ��: SceneManager.LoadScene("NextSceneName");
    }
}
