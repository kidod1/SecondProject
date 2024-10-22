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
    public Sprite associatedSprite; // ���� �Բ� ������ Sprite (�ɼ�)
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

    [Header("���� ���� ����")]
    public GameObject monsterPrefab; // ������ ���� ������
    public Transform[] spawnPoints; // ���� ���� ��ġ �迭
    public int monstersToSpawn = 5; // ������ ���� ��

    private int currentDialogueIndex = 0; // ���� ��� �ε���

    [SerializeField]
    private GameObject PortalObject;

    // �÷��̾� �Է� �� ���� üũ�� ���� ������
    private bool hasMoved = false; // �÷��̾ WASD�� ��������
    private bool hasUsedAbility = false; // �÷��̾ R Ű�� ���� �ɷ��� ����ߴ���
    private bool allMonstersDefeated = false; // ��� ���͸� �����ƴ���

    private int lastDefeatAllMonstersDialogueIndex = -1; // ������ DefeatAllMonsters ��� �ε���

    private void Start()
    {
        // ���� �� ������ DefeatAllMonsters ��� �ε��� ã��
        for (int i = 0; i < dialogues.Count; i++)
        {
            if (dialogues[i].isConditional && dialogues[i].conditionType == ConditionType.DefeatAllMonsters)
            {
                lastDefeatAllMonstersDialogueIndex = i;
            }
        }

        if (lastDefeatAllMonstersDialogueIndex == -1)
        {
            Debug.LogWarning("DefeatAllMonsters ������ ���� ��簡 �����ϴ�.");
        }
        PortalObject.SetActive(false);
        // ���� �� ��ȭ ������ ����
        StartCoroutine(DialogueSequence());
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
            Debug.Log("��� óġ");
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
    /// ��ȭ �������� �����ϴ� �ڷ�ƾ
    /// </summary>
    /// <returns></returns>
    private IEnumerator DialogueSequence()
    {
        while (currentDialogueIndex < dialogues.Count)
        {
            DialogueData dialogue = dialogues[currentDialogueIndex];

            // Set up image
            if (dialogueImage != null)
            {
                if (dialogue.associatedSprite != null)
                {
                    dialogueImage.sprite = dialogue.associatedSprite;
                    dialogueImage.gameObject.SetActive(true);
                }
                else
                {
                    // Sprite�� null�� ��� �̹����� ��Ȱ��ȭ
                    dialogueImage.gameObject.SetActive(false);
                }
            }

            // Set up text
            if (dialogueText != null)
            {
                dialogueText.color = new Color(dialogueText.color.r, dialogueText.color.g, dialogueText.color.b, 1f); // �ؽ�Ʈ�� ������ ���̵��� ����
                dialogueText.text = "";
                dialogueText.gameObject.SetActive(true);
            }

            // Animate text
            foreach (char letter in dialogue.sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSecondsRealtime(textAnimationSpeed);
            }

            // Wait autoAdvanceDuration after text animation
            yield return new WaitForSecondsRealtime(autoAdvanceDuration);

            if (dialogue.isConditional)
            {
                // DefeatAllMonsters ������ ��� ���� ����
                if (dialogue.conditionType == ConditionType.DefeatAllMonsters)
                {
                    SpawnMonsters();
                    allMonstersDefeated = false; // ���Ͱ� �����Ǿ����Ƿ� �ʱ�ȭ
                }

                // ���� ������ ��ٸ�
                bool conditionSatisfied = false;
                while (!conditionSatisfied)
                {
                    // ������ �����Ǿ����� Ȯ��
                    if (CheckConditionMet(dialogue.conditionType))
                    {
                        conditionSatisfied = true;
                        currentDialogueIndex++;

                        // ������ DefeatAllMonsters ��簡 �Ϸ�Ǹ� �߰� ���� ���� (�ʿ� ��)
                        if (currentDialogueIndex - 1 == lastDefeatAllMonstersDialogueIndex)
                        {
                            // ���� ���, �߰� ���� ������ �ʿ��ϴٸ� ���⼭ ȣ��
                            // SpawnMonsters();
                        }

                        break;
                    }

                    // ������ �������� �ʾ��� ���, autoAdvanceDuration ��ŭ ��� �� ��ȭ �����
                    yield return new WaitForSecondsRealtime(autoAdvanceDuration);

                    // ��ȭ �ؽ�Ʈ�� �ٽ� �ִϸ��̼����� ǥ��
                    dialogueText.text = "";
                    foreach (char letter in dialogue.sentence.ToCharArray())
                    {
                        dialogueText.text += letter;
                        yield return new WaitForSecondsRealtime(textAnimationSpeed);
                    }

                    // �ٽ� autoAdvanceDuration ���
                    yield return new WaitForSecondsRealtime(autoAdvanceDuration);
                }
            }
            else
            {
                // ������ ���� ���� ���� ���� �ڵ� ����
                currentDialogueIndex++;
            }
        }

        EndDialogue();
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
                Debug.Log("PressWASD");
                return hasMoved;
            case ConditionType.PressR:
                Debug.Log("PressR");
                return hasUsedAbility;
            case ConditionType.DefeatAllMonsters:
                Debug.Log("DefeatAllMonsters");
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
        PortalObject.SetActive(true);
        Debug.Log("Ʃ�丮�� ����");
    }

    /// <summary>
    /// ���͸� �����ϴ� �޼���
    /// </summary>
    private void SpawnMonsters()
    {
        if (monsterPrefab == null)
        {
            Debug.LogError("Monster Prefab�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Spawn Points�� �Ҵ���� �ʾҰų�, �迭�� ��� �ֽ��ϴ�.");
            return;
        }

        for (int i = 0; i < monstersToSpawn; i++)
        {
            // ������ ���� ����Ʈ ����
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[spawnIndex];

            // ���� ����
            Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        Debug.Log($"{monstersToSpawn}������ ���Ͱ� �����Ǿ����ϴ�.");
    }
}
