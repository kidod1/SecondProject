using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using AK.Wwise; // Wwise ���ӽ����̽� �߰�
using Spine.Unity; // Spine.Unity ���ӽ����̽� �߰�
using Spine; // Spine ���ӽ����̽� �߰� (AnimationState ����� ����)

public enum ConditionType
{
    None,                // ���� ����
    PressWASD,           // WASD Ű �Է�
    PressR,              // R Ű �Է�
    DefeatAllMonsters    // ��� ���� óġ
}

[System.Serializable]
public class DialogueData
{
    [TextArea(3, 10)]
    public string sentence; // ��� ����
    public bool isConditional; // ���Ǻ� ������� ����
    public ConditionType conditionType; // ���� Ÿ��
    public Sprite associatedSprite; // ���� �Բ� ������ Sprite (�ɼ�)
}

public class TutorialDialogueManager : MonoBehaviour
{
    [Header("��ȭ ���")]
    public List<DialogueData> dialogues; // ��� ���

    [Header("UI ���")]
    [SerializeField]
    private TMP_Text dialogueText; // ��� �ؽ�Ʈ UI
    [SerializeField]
    private Image dialogueImage; // ��� �̹��� UI
    [SerializeField]
    private Image TutoConditionsImage;
    [SerializeField]
    private TMP_Text conditionDescriptionText; // ���� ���� �ؽ�Ʈ UI
    [SerializeField]
    private Image conditionIconImage; // ���� ������ �̹��� UI
    [SerializeField]
    private Sprite defaultConditionIcon; // �⺻ ������ ��������Ʈ
    [SerializeField]
    private Sprite completedConditionIcon; // ���� �Ϸ� �� ������ ��������Ʈ
    [SerializeField]
    private TMP_Text progressText; // Ʃ�丮�� ���൵ ǥ�ÿ� �ؽ�Ʈ

    [Header("�ִϸ��̼� �� �̹��� ����")]
    public float textAnimationSpeed = 0.05f; // �ؽ�Ʈ �ִϸ��̼� �ӵ�
    public float conditionTimeout = 10f; // ���� ���� ��� �ð�
    public float autoAdvanceDuration = 3f; // �Ϲ� ��� �ڵ� ���� �ð�

    [Header("���� ���� ����")]
    public GameObject monsterPrefab; // ������ ���� ������
    public Transform[] spawnPoints; // ���� ���� ��ġ �迭
    public int monstersToSpawn = 5; // ������ ���� ��

    [Header("��Ż ������Ʈ")]
    [SerializeField]
    private GameObject PortalObject;

    [SerializeField]
    private Sprite diedImpSprite;

    // �߰��� �κ�: Wwise �̺�Ʈ
    [Header("���� �̺�Ʈ")]
    public AK.Wwise.Event typingSoundEvent; // �ؽ�Ʈ Ÿ���� ���� �̺�Ʈ

    // �߰��� �κ�: Spine �ִϸ��̼�
    [Header("Spine �ִϸ��̼�")]
    [SerializeField]
    private SkeletonAnimation doorSkeletonAnimation; // Door ������ �ִϸ��̼�

    private int currentDialogueIndex = 0; // ���� ��� �ε���

    // �÷��̾� �Է� �� ���� üũ�� ���� ������
    private bool hasMoved = false; // �÷��̾ WASD�� ��������
    private bool hasUsedAbility = false; // �÷��̾ R Ű�� ���� �ɷ��� ����ߴ���
    private bool allMonstersDefeated = false; // ��� ���͸� �����ƴ���

    private int lastDefeatAllMonstersDialogueIndex = -1; // ������ DefeatAllMonsters ��� �ε���
    private Player player;

    // UnityEvent ���� (Inspector�� ǥ�õ�)
    [Header("�̺�Ʈ")]
    public UnityEvent OnDialogueStartUnityEvent; // �� ��ȭ ���� �� ȣ��Ǵ� �̺�Ʈ
    public UnityEvent OnDialogueEndUnityEvent; // �� ��ȭ ���� �� ȣ��Ǵ� �̺�Ʈ
    public UnityEvent OnConditionMetUnityEvent; // ���� ���� �� ȣ��Ǵ� �̺�Ʈ
    public UnityEvent<int> OnMonstersSpawnedUnityEvent; // ���� ���� �� ȣ��Ǵ� �̺�Ʈ, ������ ���� �� ����
    public UnityEvent OnPortalOpenedUnityEvent; // ��Ż ���� �� ȣ��Ǵ� �̺�Ʈ
    public UnityEvent OnPlayerDiedUnityEvent; // �÷��̾� ��� �� ȣ��Ǵ� �̺�Ʈ

    [Header("���� �̺�Ʈ")]
    public UnityEvent<string> OnConditionStartedUnityEvent; // ���� ���� �� ȣ��Ǵ� �̺�Ʈ, ���� ���� ����
    public UnityEvent OnConditionCompletedUnityEvent; // ���� �Ϸ� �� ȣ��Ǵ� �̺�Ʈ

    private int totalSpecialDialogues = 0; // �� Ư�� ��ȭ ��
    private int processedSpecialDialogues = 0; // ó���� Ư�� ��ȭ ��

    private bool firstConditionalDialogueProcessed = false;  // ù ��° ���Ǻ� ��ȭ�� ó���Ǿ����� ���θ� �����ϴ� �÷���

    // �߰��� �κ�: Ÿ���� ���� ��� ID�� �����ϱ� ���� ����
    private uint typingSoundPlayingID = 0;

    // �߰��� �κ�: �� ��ȯ ��� ������ ���� ����
    [Header("�� ��ȯ ����")]
    [SerializeField]
    private bool useCloseAnimation = true; // CloseAnimation�� ������� ����
    [SerializeField]
    private string targetSceneName = "NextScene"; // �ε��� ���� �̸�

    private void Start()
    {
        if (!PlayManager.I.isPlayerDied)
        {
            // PlayManager���� �÷��̾� �ν��Ͻ� ��������
            player = PlayManager.I.GetPlayer();

            if (player != null)
            {
                // Ʃ�丮�� ���� �� �÷��̾� ��Ʈ�� ��Ȱ��ȭ
                player.DisableControls();
                Debug.Log("�÷��̾��� �̵��� �����մϴ�.");
            }
            else
            {
                Debug.LogError("PlayManager���� �÷��̾� �ν��Ͻ��� ã�� �� �����ϴ�.");
            }

            // �� Ư�� ��ȭ �� ���
            for (int i = 0; i < dialogues.Count; i++)
            {
                if (dialogues[i].isConditional && dialogues[i].conditionType != ConditionType.None)
                {
                    totalSpecialDialogues++;
                }

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
            // ��ȭ ������ ����
            StartCoroutine(DialogueSequence());
        }

        if (PlayManager.I.isPlayerDied)
        {
            OpenPortalAndShowNewDialogue();
            // �÷��̾ �׾��� �� �̺�Ʈ ȣ��
            OnPlayerDiedUnityEvent?.Invoke();
        }
    }

    private void Update()
    {
        // �÷��̾��� ������ üũ (WASD �Է�)
        if (firstConditionalDialogueProcessed && !hasMoved && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                          Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)))
        {
            hasMoved = true;
            Debug.Log("�÷��̾ ���������ϴ�.");
        }

        // �÷��̾��� �ɷ� ��� üũ (R Ű �Է�)
        if (!hasUsedAbility && Input.GetKeyDown(KeyCode.R))
        {
            hasUsedAbility = true;
            Debug.Log("�÷��̾ �ɷ��� ����߽��ϴ�.");
        }

        // ���� óġ ���� üũ
        if (!allMonstersDefeated && AreAllMonstersDefeated())
        {
            allMonstersDefeated = true;
            Debug.Log("��� ���͸� óġ�߽��ϴ�.");
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

            // �̺�Ʈ: ��ȭ ����
            OnDialogueStartUnityEvent?.Invoke();

            // �ִϸ��̼��� �ڿ������� ����� ���� ���
            yield return new WaitForSeconds(0.5f);

            // �̹��� ����
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

            // �ؽ�Ʈ ����
            if (dialogueText != null)
            {
                dialogueText.color = new Color(dialogueText.color.r, dialogueText.color.g, dialogueText.color.b, 1f);
                dialogueText.text = "";
                dialogueText.gameObject.SetActive(true);
            }

            // �ؽ�Ʈ �ִϸ��̼� �� Ÿ���� ���� ���
            if (typingSoundEvent != null)
            {
                typingSoundPlayingID = typingSoundEvent.Post(gameObject);
            }

            foreach (char letter in dialogue.sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSecondsRealtime(textAnimationSpeed);
            }

            // Ÿ���� ���� ����
            if (typingSoundPlayingID != 0)
            {
                AkSoundEngine.StopPlayingID(typingSoundPlayingID);
                typingSoundPlayingID = 0;
            }

            // �ڵ� ���� �ð� ���
            yield return new WaitForSecondsRealtime(autoAdvanceDuration);

            if (dialogue.isConditional)
            {
                TutoConditionsImage.gameObject.SetActive(true);
                string conditionDescription = GetConditionDescription(dialogue.conditionType);
                OnConditionStartedUnityEvent?.Invoke(conditionDescription);
                firstConditionalDialogueProcessed = true;

                if (player != null && !PlayManager.I.isPlayerDied)
                {
                    player.EnableControls();
                    Debug.Log("�÷��̾� ��Ʈ���� Ȱ��ȭ�Ǿ����ϴ�.");
                }
                else
                {
                    Debug.LogError("�÷��̾� �ν��Ͻ��� null�Դϴ�. ��Ʈ���� Ȱ��ȭ�� �� �����ϴ�.");
                }

                // ���� ���� UI ������Ʈ
                if (conditionDescriptionText != null)
                {
                    conditionDescriptionText.text = conditionDescription;
                }

                // ������ �ʱ�ȭ
                if (conditionIconImage != null && defaultConditionIcon != null)
                {
                    conditionIconImage.sprite = defaultConditionIcon;
                }

                // DefeatAllMonsters ������ ��� ���� ����
                if (dialogue.conditionType == ConditionType.DefeatAllMonsters)
                {
                    SpawnMonsters();
                    allMonstersDefeated = false;
                }

                // ���� ������ ��ٸ�
                bool conditionSatisfied = false;
                while (!conditionSatisfied)
                {
                    // ������ �����Ǿ����� Ȯ��
                    if (CheckConditionMet(dialogue.conditionType))
                    {
                        conditionSatisfied = true;

                        // ���� ���� �̺�Ʈ ȣ��
                        OnConditionMetUnityEvent?.Invoke();

                        // ���� �Ϸ� �̺�Ʈ ȣ��
                        OnConditionCompletedUnityEvent?.Invoke();

                        // ������ ����
                        if (conditionIconImage != null && completedConditionIcon != null)
                        {
                            conditionIconImage.sprite = completedConditionIcon;
                        }

                        // Ư�� ��ȭ ó�� �Ϸ�
                        processedSpecialDialogues++;

                        // ���� ���� ����
                        currentDialogueIndex++;
                        break;
                    }

                    // ���� �����ӱ��� ���
                    yield return null;
                }
            }
            else
            {
                // ������ ���� ���� ���� ���� �ڵ� ����
                currentDialogueIndex++;
            }

            // �̺�Ʈ: ��ȭ ����
            OnDialogueEndUnityEvent?.Invoke();
        }

        EndDialogue();
    }

    /// <summary>
    /// �־��� ���� Ÿ�Կ� ���� ������ ��ȯ�ϴ� �޼���
    /// </summary>
    /// <param name="conditionType">���� Ÿ��</param>
    /// <returns>���� ���� ���ڿ�</returns>
    private string GetConditionDescription(ConditionType conditionType)
    {
        switch (conditionType)
        {
            case ConditionType.PressWASD:
                return "WASD ������";
            case ConditionType.PressR:
                return "RŰ ������";
            case ConditionType.DefeatAllMonsters:
                return "�ʿ� �ִ� ��� �� óġ";
            default:
                return "";
        }
    }

    /// <summary>
    /// Ʃ�丮�� ���൵�� ������Ʈ�ϴ� �޼���
    /// </summary>
    public void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text = $"Ʃ�丮�� ���� {processedSpecialDialogues + 1}/{totalSpecialDialogues}";
        }
    }

    /// <summary>
    /// Ʃ�丮�� ���� �� ���൵�� �ʱ�ȭ�ϴ� �޼���
    /// </summary>
    public void InitializeProgressText()
    {
        if (progressText != null)
        {
            progressText.text = $"Ʃ�丮�� ���� {processedSpecialDialogues + 1}/{totalSpecialDialogues}";
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
        Debug.Log("Ʃ�丮�� ����");

        if (useCloseAnimation)
        {
            // CloseAnimation�� ����ϴ� ���
            if (doorSkeletonAnimation != null)
            {
                // �ִϸ��̼� �Ϸ� �� ȣ��� �̺�Ʈ �ڵ鷯 ���
                doorSkeletonAnimation.AnimationState.Complete += OnDoorOpenAnimationComplete;

                // Door_Open �ִϸ��̼� ����
                doorSkeletonAnimation.AnimationState.SetAnimation(0, "open_door", false);
            }
            else
            {
                Debug.LogWarning("Door SkeletonAnimation�� �Ҵ���� �ʾҽ��ϴ�.");

                // Door �ִϸ��̼��� ���� ��� ��� ��Ż Ȱ��ȭ
                PortalObject.SetActive(true);
                OnPortalOpenedUnityEvent?.Invoke();
            }
        }
        else
        {
            // SceneManager.LoadScene�� ����ϴ� ���
            LoadNextScene();
        }
    }

    /// <summary>
    /// Door_Open �ִϸ��̼��� �Ϸ�Ǿ��� �� ȣ��Ǵ� �޼���
    /// </summary>
    private void OnDoorOpenAnimationComplete(TrackEntry trackEntry)
    {
        // �̺�Ʈ �ڵ鷯 �����Ͽ� �ߺ� ȣ�� ����
        doorSkeletonAnimation.AnimationState.Complete -= OnDoorOpenAnimationComplete;

        // ��Ż Ȱ��ȭ
        PortalObject.SetActive(true);
        Debug.Log("Door_Open �ִϸ��̼� �Ϸ�, ��Ż Ȱ��ȭ");

        // ��Ż ���� �̺�Ʈ ȣ��
        OnPortalOpenedUnityEvent?.Invoke();

        // ���������� ���� ������ ��ȯ
        LoadNextScene();
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
            int spawnIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[spawnIndex];

            // ���� ����
            Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        Debug.Log($"{monstersToSpawn}������ ���Ͱ� �����Ǿ����ϴ�.");

        // ���� ���� �̺�Ʈ ȣ��
        OnMonstersSpawnedUnityEvent?.Invoke(monstersToSpawn);
    }

    /// <summary>
    /// �÷��̾� ��� �� ��Ż�� ���� ���ο� ���̾�α׸� ǥ���ϴ� �޼���
    /// </summary>
    public void OpenPortalAndShowNewDialogue()
    {
        // ��Ż Ȱ��ȭ
        PortalObject.SetActive(true);

        // ���ο� ���̾�α� ����
        DialogueData deathDialogue = new DialogueData
        {
            sentence = "�� �����ž�?. ��Ż�� ����׾�.", // ���ο� ���̾�α� ����
            isConditional = false,
            conditionType = ConditionType.None,
            associatedSprite = diedImpSprite // �ʿ� �� ��������Ʈ �߰�
        };

        // ���ο� ���̾�α׸� ���� ��ȭ ��Ͽ� �߰�
        dialogues.Add(deathDialogue);

        // ���� ��ȭ �ε����� ���ο� ��ȭ�� ����
        currentDialogueIndex = dialogues.Count - 1;

        // ���ο� ��ȭ�� ��� ǥ���ϱ� ���� ���� ��ȭ �������� �ٽ� ����
        StopAllCoroutines();
        StartCoroutine(DialogueSequence());
    }

    /// <summary>
    /// ���� ���� �ε��ϴ� �޼���
    /// </summary>
    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Target Scene Name�� �������� �ʾҽ��ϴ�.");
            return;
        }

        // �� �ε�
        SceneManager.LoadScene(targetSceneName);
    }
}
