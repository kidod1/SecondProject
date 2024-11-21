using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using AK.Wwise; // Wwise 네임스페이스 추가
using Spine.Unity; // Spine.Unity 네임스페이스 추가
using Spine; // Spine 네임스페이스 추가 (AnimationState 사용을 위해)

public enum ConditionType
{
    None,                // 조건 없음
    PressWASD,           // WASD 키 입력
    PressR,              // R 키 입력
    DefeatAllMonsters    // 모든 몬스터 처치
}

[System.Serializable]
public class DialogueData
{
    [TextArea(3, 10)]
    public string sentence; // 대사 내용
    public bool isConditional; // 조건부 대사인지 여부
    public ConditionType conditionType; // 조건 타입
    public Sprite associatedSprite; // 대사와 함께 보여줄 Sprite (옵션)
}

public class TutorialDialogueManager : MonoBehaviour
{
    [Header("대화 목록")]
    public List<DialogueData> dialogues; // 대사 목록

    [Header("UI 요소")]
    [SerializeField]
    private TMP_Text dialogueText; // 대사 텍스트 UI
    [SerializeField]
    private Image dialogueImage; // 대사 이미지 UI
    [SerializeField]
    private Image TutoConditionsImage;
    [SerializeField]
    private TMP_Text conditionDescriptionText; // 조건 설명 텍스트 UI
    [SerializeField]
    private Image conditionIconImage; // 조건 아이콘 이미지 UI
    [SerializeField]
    private Sprite defaultConditionIcon; // 기본 아이콘 스프라이트
    [SerializeField]
    private Sprite completedConditionIcon; // 조건 완료 시 아이콘 스프라이트
    [SerializeField]
    private TMP_Text progressText; // 튜토리얼 진행도 표시용 텍스트

    [Header("애니메이션 및 이미지 설정")]
    public float textAnimationSpeed = 0.05f; // 텍스트 애니메이션 속도
    public float conditionTimeout = 10f; // 조건 충족 대기 시간
    public float autoAdvanceDuration = 3f; // 일반 대사 자동 진행 시간

    [Header("몬스터 스폰 설정")]
    public GameObject monsterPrefab; // 스폰할 몬스터 프리팹
    public Transform[] spawnPoints; // 몬스터 스폰 위치 배열
    public int monstersToSpawn = 5; // 스폰할 몬스터 수

    [Header("포탈 오브젝트")]
    [SerializeField]
    private GameObject PortalObject;

    [SerializeField]
    private Sprite diedImpSprite;

    // 추가된 부분: Wwise 이벤트
    [Header("사운드 이벤트")]
    public AK.Wwise.Event typingSoundEvent; // 텍스트 타이핑 사운드 이벤트

    // 추가된 부분: Spine 애니메이션
    [Header("Spine 애니메이션")]
    [SerializeField]
    private SkeletonAnimation doorSkeletonAnimation; // Door 스파인 애니메이션

    private int currentDialogueIndex = 0; // 현재 대사 인덱스

    // 플레이어 입력 및 상태 체크를 위한 변수들
    private bool hasMoved = false; // 플레이어가 WASD를 눌렀는지
    private bool hasUsedAbility = false; // 플레이어가 R 키를 눌러 능력을 사용했는지
    private bool allMonstersDefeated = false; // 모든 몬스터를 물리쳤는지

    private int lastDefeatAllMonstersDialogueIndex = -1; // 마지막 DefeatAllMonsters 대사 인덱스
    private Player player;

    // UnityEvent 선언 (Inspector에 표시됨)
    [Header("이벤트")]
    public UnityEvent OnDialogueStartUnityEvent; // 각 대화 시작 시 호출되는 이벤트
    public UnityEvent OnDialogueEndUnityEvent; // 각 대화 종료 시 호출되는 이벤트
    public UnityEvent OnConditionMetUnityEvent; // 조건 충족 시 호출되는 이벤트
    public UnityEvent<int> OnMonstersSpawnedUnityEvent; // 몬스터 스폰 시 호출되는 이벤트, 스폰된 몬스터 수 전달
    public UnityEvent OnPortalOpenedUnityEvent; // 포탈 열림 시 호출되는 이벤트
    public UnityEvent OnPlayerDiedUnityEvent; // 플레이어 사망 시 호출되는 이벤트

    [Header("조건 이벤트")]
    public UnityEvent<string> OnConditionStartedUnityEvent; // 조건 시작 시 호출되는 이벤트, 조건 설명 전달
    public UnityEvent OnConditionCompletedUnityEvent; // 조건 완료 시 호출되는 이벤트

    private int totalSpecialDialogues = 0; // 총 특수 대화 수
    private int processedSpecialDialogues = 0; // 처리된 특수 대화 수

    private bool firstConditionalDialogueProcessed = false;  // 첫 번째 조건부 대화가 처리되었는지 여부를 추적하는 플래그

    // 추가된 부분: 타이핑 사운드 재생 ID를 저장하기 위한 변수
    private uint typingSoundPlayingID = 0;

    // 추가된 부분: 씬 전환 방식 선택을 위한 변수
    [Header("씬 전환 설정")]
    [SerializeField]
    private bool useCloseAnimation = true; // CloseAnimation을 사용할지 여부
    [SerializeField]
    private string targetSceneName = "NextScene"; // 로드할 씬의 이름

    private void Start()
    {
        if (!PlayManager.I.isPlayerDied)
        {
            // PlayManager에서 플레이어 인스턴스 가져오기
            player = PlayManager.I.GetPlayer();

            if (player != null)
            {
                // 튜토리얼 시작 시 플레이어 컨트롤 비활성화
                player.DisableControls();
                Debug.Log("플레이어의 이동을 제어합니다.");
            }
            else
            {
                Debug.LogError("PlayManager에서 플레이어 인스턴스를 찾을 수 없습니다.");
            }

            // 총 특수 대화 수 계산
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
                Debug.LogWarning("DefeatAllMonsters 조건을 가진 대사가 없습니다.");
            }

            PortalObject.SetActive(false);
            // 대화 시퀀스 시작
            StartCoroutine(DialogueSequence());
        }

        if (PlayManager.I.isPlayerDied)
        {
            OpenPortalAndShowNewDialogue();
            // 플레이어가 죽었을 때 이벤트 호출
            OnPlayerDiedUnityEvent?.Invoke();
        }
    }

    private void Update()
    {
        // 플레이어의 움직임 체크 (WASD 입력)
        if (firstConditionalDialogueProcessed && !hasMoved && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                          Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)))
        {
            hasMoved = true;
            Debug.Log("플레이어가 움직였습니다.");
        }

        // 플레이어의 능력 사용 체크 (R 키 입력)
        if (!hasUsedAbility && Input.GetKeyDown(KeyCode.R))
        {
            hasUsedAbility = true;
            Debug.Log("플레이어가 능력을 사용했습니다.");
        }

        // 몬스터 처치 여부 체크
        if (!allMonstersDefeated && AreAllMonstersDefeated())
        {
            allMonstersDefeated = true;
            Debug.Log("모든 몬스터를 처치했습니다.");
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

            // 이벤트: 대화 시작
            OnDialogueStartUnityEvent?.Invoke();

            // 애니메이션을 자연스럽게 만들기 위해 대기
            yield return new WaitForSeconds(0.5f);

            // 이미지 설정
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

            // 텍스트 설정
            if (dialogueText != null)
            {
                dialogueText.color = new Color(dialogueText.color.r, dialogueText.color.g, dialogueText.color.b, 1f);
                dialogueText.text = "";
                dialogueText.gameObject.SetActive(true);
            }

            // 텍스트 애니메이션 및 타이핑 사운드 재생
            if (typingSoundEvent != null)
            {
                typingSoundPlayingID = typingSoundEvent.Post(gameObject);
            }

            foreach (char letter in dialogue.sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSecondsRealtime(textAnimationSpeed);
            }

            // 타이핑 사운드 정지
            if (typingSoundPlayingID != 0)
            {
                AkSoundEngine.StopPlayingID(typingSoundPlayingID);
                typingSoundPlayingID = 0;
            }

            // 자동 진행 시간 대기
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
                    Debug.Log("플레이어 컨트롤이 활성화되었습니다.");
                }
                else
                {
                    Debug.LogError("플레이어 인스턴스가 null입니다. 컨트롤을 활성화할 수 없습니다.");
                }

                // 조건 설명 UI 업데이트
                if (conditionDescriptionText != null)
                {
                    conditionDescriptionText.text = conditionDescription;
                }

                // 아이콘 초기화
                if (conditionIconImage != null && defaultConditionIcon != null)
                {
                    conditionIconImage.sprite = defaultConditionIcon;
                }

                // DefeatAllMonsters 조건일 경우 몬스터 스폰
                if (dialogue.conditionType == ConditionType.DefeatAllMonsters)
                {
                    SpawnMonsters();
                    allMonstersDefeated = false;
                }

                // 조건 충족을 기다림
                bool conditionSatisfied = false;
                while (!conditionSatisfied)
                {
                    // 조건이 충족되었는지 확인
                    if (CheckConditionMet(dialogue.conditionType))
                    {
                        conditionSatisfied = true;

                        // 조건 충족 이벤트 호출
                        OnConditionMetUnityEvent?.Invoke();

                        // 조건 완료 이벤트 호출
                        OnConditionCompletedUnityEvent?.Invoke();

                        // 아이콘 변경
                        if (conditionIconImage != null && completedConditionIcon != null)
                        {
                            conditionIconImage.sprite = completedConditionIcon;
                        }

                        // 특수 대화 처리 완료
                        processedSpecialDialogues++;

                        // 다음 대사로 진행
                        currentDialogueIndex++;
                        break;
                    }

                    // 다음 프레임까지 대기
                    yield return null;
                }
            }
            else
            {
                // 조건이 없는 대사는 다음 대사로 자동 진행
                currentDialogueIndex++;
            }

            // 이벤트: 대화 종료
            OnDialogueEndUnityEvent?.Invoke();
        }

        EndDialogue();
    }

    /// <summary>
    /// 주어진 조건 타입에 대한 설명을 반환하는 메서드
    /// </summary>
    /// <param name="conditionType">조건 타입</param>
    /// <returns>조건 설명 문자열</returns>
    private string GetConditionDescription(ConditionType conditionType)
    {
        switch (conditionType)
        {
            case ConditionType.PressWASD:
                return "WASD 누르기";
            case ConditionType.PressR:
                return "R키 누르기";
            case ConditionType.DefeatAllMonsters:
                return "맵에 있는 모든 적 처치";
            default:
                return "";
        }
    }

    /// <summary>
    /// 튜토리얼 진행도를 업데이트하는 메서드
    /// </summary>
    public void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text = $"튜토리얼 진행 {processedSpecialDialogues + 1}/{totalSpecialDialogues}";
        }
    }

    /// <summary>
    /// 튜토리얼 시작 시 진행도를 초기화하는 메서드
    /// </summary>
    public void InitializeProgressText()
    {
        if (progressText != null)
        {
            progressText.text = $"튜토리얼 진행 {processedSpecialDialogues + 1}/{totalSpecialDialogues}";
        }
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
    /// 대화가 모두 끝났을 때 호출되는 메서드
    /// </summary>
    private void EndDialogue()
    {
        Debug.Log("튜토리얼 종료");

        if (useCloseAnimation)
        {
            // CloseAnimation을 사용하는 경우
            if (doorSkeletonAnimation != null)
            {
                // 애니메이션 완료 시 호출될 이벤트 핸들러 등록
                doorSkeletonAnimation.AnimationState.Complete += OnDoorOpenAnimationComplete;

                // Door_Open 애니메이션 실행
                doorSkeletonAnimation.AnimationState.SetAnimation(0, "open_door", false);
            }
            else
            {
                Debug.LogWarning("Door SkeletonAnimation이 할당되지 않았습니다.");

                // Door 애니메이션이 없을 경우 즉시 포탈 활성화
                PortalObject.SetActive(true);
                OnPortalOpenedUnityEvent?.Invoke();
            }
        }
        else
        {
            // SceneManager.LoadScene을 사용하는 경우
            LoadNextScene();
        }
    }

    /// <summary>
    /// Door_Open 애니메이션이 완료되었을 때 호출되는 메서드
    /// </summary>
    private void OnDoorOpenAnimationComplete(TrackEntry trackEntry)
    {
        // 이벤트 핸들러 제거하여 중복 호출 방지
        doorSkeletonAnimation.AnimationState.Complete -= OnDoorOpenAnimationComplete;

        // 포탈 활성화
        PortalObject.SetActive(true);
        Debug.Log("Door_Open 애니메이션 완료, 포탈 활성화");

        // 포탈 열림 이벤트 호출
        OnPortalOpenedUnityEvent?.Invoke();

        // 선택적으로 다음 씬으로 전환
        LoadNextScene();
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
            int spawnIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[spawnIndex];

            // 몬스터 스폰
            Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        Debug.Log($"{monstersToSpawn}마리의 몬스터가 스폰되었습니다.");

        // 몬스터 스폰 이벤트 호출
        OnMonstersSpawnedUnityEvent?.Invoke(monstersToSpawn);
    }

    /// <summary>
    /// 플레이어 사망 시 포탈을 열고 새로운 다이얼로그를 표시하는 메서드
    /// </summary>
    public void OpenPortalAndShowNewDialogue()
    {
        // 포탈 활성화
        PortalObject.SetActive(true);

        // 새로운 다이얼로그 생성
        DialogueData deathDialogue = new DialogueData
        {
            sentence = "또 죽은거야?. 포탈은 열어뒀어.", // 새로운 다이얼로그 문장
            isConditional = false,
            conditionType = ConditionType.None,
            associatedSprite = diedImpSprite // 필요 시 스프라이트 추가
        };

        // 새로운 다이얼로그를 현재 대화 목록에 추가
        dialogues.Add(deathDialogue);

        // 현재 대화 인덱스를 새로운 대화로 설정
        currentDialogueIndex = dialogues.Count - 1;

        // 새로운 대화를 즉시 표시하기 위해 현재 대화 시퀀스를 다시 시작
        StopAllCoroutines();
        StartCoroutine(DialogueSequence());
    }

    /// <summary>
    /// 다음 씬을 로드하는 메서드
    /// </summary>
    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Target Scene Name이 설정되지 않았습니다.");
            return;
        }

        // 씬 로드
        SceneManager.LoadScene(targetSceneName);
    }
}
