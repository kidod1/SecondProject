using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events; // UnityEvent를 사용하기 위해 추가
using UnityEngine.SceneManagement; // 씬 전환을 위해 추가

public class MonsterSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnInfo
    {
        public GameObject monsterPrefab; // 스폰할 몬스터 프리팹
        public int count; // 스폰할 몬스터 수
        public float spawnInterval = 1.0f; // 몬스터 스폰 간격 (초)
        public Transform[] customSpawnPoints; // 몬스터별 커스텀 스폰 지점 (없을 경우 기본 스폰 지점 사용)
    }

    [System.Serializable]
    public class Wave
    {
        public List<SpawnInfo> spawnInfos; // 각 웨이브의 스폰 정보 리스트
    }

    [Header("Wave Settings")]
    public List<Wave> waves; // 모든 웨이브 리스트
    public Transform[] spawnPoints; // 기본 스폰 지점들
    public bool isSlothArea = false; // 슬로스 지역 여부

    [Header("Spawn Effect")]
    public GameObject spawnEffectPrefab; // 몬스터 스폰 시 재생할 공용 이펙트 프리팹

    [Header("UI Elements")]
    public TextMeshProUGUI waveNumberText; // 웨이브 클리어 및 시작 UI 텍스트
    public Animator waveNumberAnimator; // 웨이브 텍스트의 Animator 컴포넌트
    public GameObject bossPrefab; // 씬에 미리 배치된 보스 오브젝트 참조

    private List<GameObject> spawnedMonsters = new List<GameObject>(); // 현재 활성화된 몬스터 리스트
    private int currentWaveIndex = 0; // 현재 웨이브 인덱스
    private MonoBehaviour bossInstance; // 보스 인스턴스 참조 (MidBoss 또는 LustBoss)

    [Header("Experience Items")]
    public Transform experienceItemsParent; // 경험치 아이템의 공통 부모

    [Header("Boss Activation Cutscene")]
    [SerializeField, Tooltip("보스 활성화 전에 재생할 경고 WWISE 사운드 이벤트")]
    private AK.Wwise.Event warningSoundEvent; // WWISE 사운드 이벤트 변수 추가

    [SerializeField, Tooltip("보스 활성화 후에 재생할 경고 스탑 WWISE 사운드 이벤트")]
    private AK.Wwise.Event warningStopSoundEvent; // WWISE 사운드 이벤트 변수 추가

    [SerializeField, Tooltip("맵 전체를 가리는 패널의 CanvasGroup")]
    private CanvasGroup mapCoverPanel; // CanvasGroup 변수 추가

    [SerializeField]
    private GameObject MapTextPanel;

    [SerializeField, Tooltip("보스 활성화 연출의 총 지속 시간 (초)")]
    private float cutsceneDuration = 5f;

    [Header("Boss Appearance Event")]
    [Tooltip("보스가 등장할 때 호출되는 이벤트")]
    [SerializeField]
    private UnityEvent OnBossAppeared; // 보스 등장 시 호출될 UnityEvent 추가

    private void Start()
    {
        if (waves.Count > 0)
        {
            StartCoroutine(SpawnWaves());
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: 웨이브 설정이 없습니다.");
        }

        // 경험치 아이템 부모 오브젝트 초기화
        InitializeExperienceItemsParent();

        // 씬에 배치된 보스 오브젝트가 할당되어 있는지 확인하고, 초기에는 비활성화 상태로 설정
        if (bossPrefab != null)
        {
            bossPrefab.SetActive(false); // 보스 초기 비활성화

            // MidBoss 컴포넌트 확인
            bossInstance = bossPrefab.GetComponent<MidBoss>();
            if (bossInstance == null)
            {
                // LustBoss 컴포넌트 확인
                bossInstance = bossPrefab.GetComponent<LustBoss>();
                if (bossInstance == null)
                {
                    Debug.LogError("bossPrefab 오브젝트에 MidBoss 또는 LustBoss 컴포넌트가 없습니다.");
                }
            }
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: bossPrefab 오브젝트가 설정되지 않았습니다.");
        }

        // Animator 참조 확인
        if (waveNumberAnimator == null && waveNumberText != null)
        {
            waveNumberAnimator = waveNumberText.GetComponent<Animator>();
            if (waveNumberAnimator == null)
            {
                Debug.LogWarning("waveNumberText에 Animator 컴포넌트가 없습니다. 애니메이션 기능이 비활성화됩니다.");
            }
        }
    }

    private void Update()
    {
        // 활성화된 몬스터 리스트를 업데이트하여 죽은 몬스터를 제거
        spawnedMonsters.RemoveAll(monster => monster == null || !monster.activeInHierarchy);
    }

    /// <summary>
    /// 경험치 아이템의 부모 오브젝트를 초기화합니다.
    /// </summary>
    private void InitializeExperienceItemsParent()
    {
        if (experienceItemsParent == null)
        {
            // 씬에 "ExperienceItems"라는 이름의 오브젝트가 있는지 찾기
            GameObject parentObj = GameObject.Find("ExperienceItems");
            if (parentObj == null)
            {
                // 없다면 새로 생성
                parentObj = new GameObject("ExperienceItems");
                parentObj.transform.SetParent(this.transform);
            }
            experienceItemsParent = parentObj.transform;
        }
    }

    private IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(3f);
        while (currentWaveIndex < waves.Count)
        {
            Wave currentWave = waves[currentWaveIndex];

            // *** 웨이브 시작 메시지 표시 ***
            float messageDisplayDuration = 0.7f; // 메시지 표시 시간 설정 (예: 0.7초)

            if (waveNumberText != null)
            {
                waveNumberText.text = $"웨이브 {currentWaveIndex + 1}";
                waveNumberText.alpha = 1f;

                // Animator를 통해 애니메이션 재생 (필요 시)
                if (waveNumberAnimator != null)
                {
                    waveNumberAnimator.SetTrigger("StartWave"); // "StartWave" 트리거 설정
                }

                StartCoroutine(FadeOutWaveNumber(messageDisplayDuration)); // 메시지 표시 시간 후에 페이드 아웃 시작
            }
            else
            {
                Debug.LogWarning("MonsterSpawner: waveNumberText가 할당되지 않았습니다.");
            }

            // 메시지 표시 시간만큼 대기
            yield return new WaitForSeconds(messageDisplayDuration);

            // 몬스터 스폰 시작
            foreach (var spawnInfo in currentWave.spawnInfos)
            {
                // 개별 몬스터의 스폰 지점 설정
                Transform[] spawnPointsToUse = (spawnInfo.customSpawnPoints != null && spawnInfo.customSpawnPoints.Length > 0) ? spawnInfo.customSpawnPoints : spawnPoints;

                for (int i = 0; i < spawnInfo.count; i++)
                {
                    SpawnMonster(spawnInfo.monsterPrefab, spawnPointsToUse);
                    yield return new WaitForSeconds(spawnInfo.spawnInterval);
                }
            }

            // 모든 몬스터가 처치될 때까지 대기
            yield return new WaitUntil(() => AreAllMonstersDead());

            // 웨이브 클리어 UI 표시
            ShowWaveClearUI();

            currentWaveIndex++;
        }

        // 모든 웨이브가 완료된 후 보스 활성화 전에 연출 실행
        yield return StartCoroutine(BossActivationCutscene());

        // 모든 웨이브가 완료된 후 보스 활성화
        SpawnMidBoss();

        if (isSlothArea && bossInstance != null)
        {
            if (bossInstance is MidBoss midBoss)
            {
                midBoss.SetAttackable(true); // 슬로스 지역일 경우 MidBoss 공격 가능 상태로 설정
            }
            else if (bossInstance is LustBoss lustBoss)
            {
                lustBoss.SetAttackable(true); // 러스트 지역일 경우 LustBoss 공격 가능 상태로 설정
            }
        }
    }

    /// <summary>
    /// 씬에 미리 배치된 보스 오브젝트를 활성화합니다.
    /// </summary>
    private void SpawnMidBoss()
    {
        if (bossPrefab != null)
        {
            bossPrefab.SetActive(true); // 보스 활성화
            Debug.Log("MonsterSpawner: 보스가 활성화되었습니다.");

            // 보스 등장 이벤트 호출
            InvokeBossAppearedEvent();
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: bossPrefab 오브젝트가 설정되지 않았습니다.");
        }
    }

    /// <summary>
    /// 보스 등장 시 호출되는 UnityEvent를 호출합니다.
    /// </summary>
    private void InvokeBossAppearedEvent()
    {
        if (OnBossAppeared != null)
        {
            OnBossAppeared.Invoke();
            Debug.Log("MonsterSpawner: OnBossAppeared 이벤트가 호출되었습니다.");
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: OnBossAppeared 이벤트가 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// 지정된 스폰 지점에서 몬스터를 스폰(활성화)하고, 스폰 이펙트를 재생합니다.
    /// </summary>
    /// <param name="monsterPrefab">스폰할 몬스터 프리팹</param>
    /// <param name="spawnPointsToUse">사용할 스폰 지점 배열</param>
    private void SpawnMonster(GameObject monsterPrefab, Transform[] spawnPointsToUse)
    {
        if (spawnPointsToUse.Length == 0)
        {
            Debug.LogWarning("MonsterSpawner: 스폰 지점이 설정되지 않았습니다.");
            return;
        }

        int spawnPointIndex = Random.Range(0, spawnPointsToUse.Length);
        Transform spawnPoint = spawnPointsToUse[spawnPointIndex];

        // 몬스터 스폰
        GameObject monster = Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation, this.transform);
        spawnedMonsters.Add(monster);

        // 스폰 이펙트 재생
        if (spawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(spawnEffectPrefab, spawnPoint.position, Quaternion.identity);
            // 이펙트가 자동으로 파괴되도록 설정
            AutoDestroyEffect autoDestroy = effect.GetComponent<AutoDestroyEffect>();
            if (autoDestroy == null)
            {
                effect.AddComponent<AutoDestroyEffect>();
            }
        }

        Debug.Log($"MonsterSpawner: 몬스터 스폰됨. 프리팹: {monsterPrefab.name}, 위치: {spawnPoint.position}");
    }

    /// <summary>
    /// 현재 활성화된 모든 몬스터가 처치되었는지 확인합니다.
    /// </summary>
    /// <returns>모든 몬스터가 처치되었으면 true, 아니면 false</returns>
    private bool AreAllMonstersDead()
    {
        return spawnedMonsters.Count == 0;
    }

    private IEnumerator BossActivationCutscene()
    {
        Debug.Log("BossActivationCutscene 시작");

        // 1. 경고 WWISE 사운드 재생
        if (warningSoundEvent != null)
        {
            warningSoundEvent.Post(gameObject);
            Debug.Log("BossActivationCutscene: 경고 사운드 재생됨.");
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: warningSoundEvent가 할당되지 않았습니다.");
        }

        // 2. 맵 전체를 가리는 패널 활성화 및 오퍼시티 조절
        if (mapCoverPanel != null)
        {
            mapCoverPanel.gameObject.SetActive(true);
            MapTextPanel.gameObject.SetActive(true);
            float elapsedTime = 0f;
            float oscillationSpeed = 5f; // 오퍼시티 변화 속도
            float minAlpha = 0.2f;
            float maxAlpha = 1f;

            while (elapsedTime < cutsceneDuration)
            {
                // 오퍼시티를 sin 함수를 이용해 최대과 최소 사이로 변화
                mapCoverPanel.alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * oscillationSpeed) + 1f) / 2f);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 연출 종료 후 패널을 완전히 투명하게 하고 비활성화
            mapCoverPanel.alpha = 0f;
            mapCoverPanel.gameObject.SetActive(false);
            MapTextPanel.gameObject.SetActive(false);
            if (warningStopSoundEvent != null)
            {
                warningStopSoundEvent.Post(gameObject);
                Debug.Log("BossActivationCutscene: 경고 사운드 정지됨.");
            }
            else
            {
                Debug.LogWarning("MonsterSpawner: warningSoundEvent가 할당되지 않았습니다.");
            }
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: mapCoverPanel이 할당되지 않았습니다.");
        }

        Debug.Log("BossActivationCutscene 종료");
    }

    /// <summary>
    /// 웨이브 클리어 UI를 표시하고 서서히 페이드 아웃합니다.
    /// </summary>
    private void ShowWaveClearUI()
    {
        if (waveNumberText != null)
        {
            waveNumberText.text = $"웨이브 {currentWaveIndex + 1} 클리어!";
            waveNumberText.alpha = 1f; // 텍스트를 완전히 불투명하게 설정

            // Animator를 통해 클리어 애니메이션 재생 (필요 시)
            if (waveNumberAnimator != null)
            {
                waveNumberAnimator.SetTrigger("Clear"); // "Clear" 트리거 설정
                Debug.Log("ShowWaveClearUI: 'Clear' 트리거 발동.");
            }

            StartCoroutine(FadeOutWaveNumber(3f)); // 3초 후에 페이드 아웃 시작
            Debug.Log($"ShowWaveClearUI: 웨이브 {currentWaveIndex + 1} 클리어 UI 표시.");
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: waveNumberText가 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// 지정된 시간 동안 UI 텍스트를 서서히 페이드 아웃합니다.
    /// </summary>
    /// <param name="duration">페이드 아웃 시간 (초)</param>
    /// <returns></returns>
    private IEnumerator FadeOutWaveNumber(float duration)
    {
        if (waveNumberText == null)
        {
            yield break;
        }

        float elapsedTime = 0f;
        Color originalColor = waveNumberText.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration); // 점점 투명해지게 설정
            waveNumberText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        waveNumberText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // 페이드 아웃이 끝나면 완전히 투명하게 설정
        Debug.Log("FadeOutWaveNumber: 웨이브 클리어 UI 페이드 아웃 완료.");
    }
}
