using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 튜토리얼 다이얼로그의 조건 타입을 정의하는 열거형
/// </summary>
public enum ConditionType
{
    None,                // 조건 없음
    PressWASD,           // WASD 키 입력
    PressR,              // R 키 입력
    DefeatAllMonsters    // 모든 몬스터 처치
}

/// <summary>
/// 각 대화 데이터를 저장하는 클래스
/// </summary>
[System.Serializable]
public class DialogueData
{
    [TextArea(3, 10)]
    public string sentence; // 대사 내용
    public bool isConditional; // 조건부 대사인지 여부
    public ConditionType conditionType; // 조건 타입
    public Sprite associatedSprite; // 대사와 함께 보여줄 Sprite (옵션)
}

/// <summary>
/// 튜토리얼 다이얼로그를 관리하는 매니저 클래스
/// </summary>
public class TutorialDialogueManager : MonoBehaviour
{
    [Header("대화 목록")]
    public List<DialogueData> dialogues; // 대사 목록

    [Header("UI 요소")]
    public TMP_Text dialogueText; // 대사 텍스트 UI
    public Image dialogueImage; // 대사 이미지 UI

    [Header("애니메이션 및 이미지 설정")]
    public float textAnimationSpeed = 0.05f; // 텍스트 애니메이션 속도
    public float conditionTimeout = 10f; // 조건 충족 대기 시간
    public float autoAdvanceDuration = 3f; // 일반 대사 자동 진행 시간

    [Header("몬스터 스폰 설정")]
    public GameObject monsterPrefab; // 스폰할 몬스터 프리팹
    public Transform[] spawnPoints; // 몬스터 스폰 위치 배열
    public int monstersToSpawn = 5; // 스폰할 몬스터 수

    private int currentDialogueIndex = 0; // 현재 대사 인덱스

    [SerializeField]
    private GameObject PortalObject;

    // 플레이어 입력 및 상태 체크를 위한 변수들
    private bool hasMoved = false; // 플레이어가 WASD를 눌렀는지
    private bool hasUsedAbility = false; // 플레이어가 R 키를 눌러 능력을 사용했는지
    private bool allMonstersDefeated = false; // 모든 몬스터를 물리쳤는지

    private int lastDefeatAllMonstersDialogueIndex = -1; // 마지막 DefeatAllMonsters 대사 인덱스

    private void Start()
    {
        // 시작 시 마지막 DefeatAllMonsters 대사 인덱스 찾기
        for (int i = 0; i < dialogues.Count; i++)
        {
            if (dialogues[i].isConditional && dialogues[i].conditionType == ConditionType.DefeatAllMonsters)
            {
                lastDefeatAllMonstersDialogueIndex = i;
            }
        }

        if (lastDefeatAllMonstersDialogueIndex == -1)
        {
            Debug.LogWarning("DefeatAllMonsters 조건을 가진 대사가 없습니다.");
        }
        PortalObject.SetActive(false);
        // 시작 시 대화 시퀀스 시작
        StartCoroutine(DialogueSequence());
    }

    private void Update()
    {
        // 플레이어의 움직임 체크 (WASD 입력)
        if (!hasMoved && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                          Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)))
        {
            hasMoved = true;
        }

        // 플레이어의 능력 사용 체크 (R 키 입력)
        if (!hasUsedAbility && Input.GetKeyDown(KeyCode.R))
        {
            hasUsedAbility = true;
        }

        // 몬스터 처치 여부 체크
        if (!allMonstersDefeated && AreAllMonstersDefeated())
        {
            allMonstersDefeated = true;
            Debug.Log("모두 처치");
        }
    }

    /// <summary>
    /// 씬 내의 모든 몬스터가 처치되었는지 확인하는 메서드
    /// </summary>
    /// <returns>모든 몬스터가 처치되었으면 true, 아니면 false</returns>
    private bool AreAllMonstersDefeated()
    {
        Monster[] monsters = FindObjectsOfType<Monster>();
        return monsters.Length == 0;
    }

    /// <summary>
    /// 대화 시퀀스를 진행하는 코루틴
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
                    // Sprite가 null인 경우 이미지를 비활성화
                    dialogueImage.gameObject.SetActive(false);
                }
            }

            // Set up text
            if (dialogueText != null)
            {
                dialogueText.color = new Color(dialogueText.color.r, dialogueText.color.g, dialogueText.color.b, 1f); // 텍스트가 완전히 보이도록 설정
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
                // DefeatAllMonsters 조건일 경우 몬스터 스폰
                if (dialogue.conditionType == ConditionType.DefeatAllMonsters)
                {
                    SpawnMonsters();
                    allMonstersDefeated = false; // 몬스터가 스폰되었으므로 초기화
                }

                // 조건 충족을 기다림
                bool conditionSatisfied = false;
                while (!conditionSatisfied)
                {
                    // 조건이 충족되었는지 확인
                    if (CheckConditionMet(dialogue.conditionType))
                    {
                        conditionSatisfied = true;
                        currentDialogueIndex++;

                        // 마지막 DefeatAllMonsters 대사가 완료되면 추가 몬스터 스폰 (필요 시)
                        if (currentDialogueIndex - 1 == lastDefeatAllMonstersDialogueIndex)
                        {
                            // 예를 들어, 추가 몬스터 스폰이 필요하다면 여기서 호출
                            // SpawnMonsters();
                        }

                        break;
                    }

                    // 조건이 충족되지 않았을 경우, autoAdvanceDuration 만큼 대기 후 대화 재출력
                    yield return new WaitForSecondsRealtime(autoAdvanceDuration);

                    // 대화 텍스트를 다시 애니메이션으로 표시
                    dialogueText.text = "";
                    foreach (char letter in dialogue.sentence.ToCharArray())
                    {
                        dialogueText.text += letter;
                        yield return new WaitForSecondsRealtime(textAnimationSpeed);
                    }

                    // 다시 autoAdvanceDuration 대기
                    yield return new WaitForSecondsRealtime(autoAdvanceDuration);
                }
            }
            else
            {
                // 조건이 없는 대사는 다음 대사로 자동 진행
                currentDialogueIndex++;
            }
        }

        EndDialogue();
    }

    /// <summary>
    /// 주어진 조건 타입이 충족되었는지 확인하는 메서드
    /// </summary>
    /// <param name="conditionType">확인할 조건 타입</param>
    /// <returns>조건이 충족되었으면 true, 아니면 false</returns>
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
    /// 대화가 모두 끝났을 때 호출되는 메서드
    /// </summary>
    private void EndDialogue()
    {
        PortalObject.SetActive(true);
        Debug.Log("튜토리얼 종료");
    }

    /// <summary>
    /// 몬스터를 스폰하는 메서드
    /// </summary>
    private void SpawnMonsters()
    {
        if (monsterPrefab == null)
        {
            Debug.LogError("Monster Prefab이 할당되지 않았습니다.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Spawn Points가 할당되지 않았거나, 배열이 비어 있습니다.");
            return;
        }

        for (int i = 0; i < monstersToSpawn; i++)
        {
            // 랜덤한 스폰 포인트 선택
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[spawnIndex];

            // 몬스터 스폰
            Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        Debug.Log($"{monstersToSpawn}마리의 몬스터가 스폰되었습니다.");
    }
}
