using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnInfo
    {
        [Tooltip("생성할 몬스터 프리팹")]
        public GameObject monsterPrefab;

        [Tooltip("생성할 몬스터 수")]
        public int count;
    }

    [System.Serializable]
    public class Wave
    {
        [Tooltip("스폰 정보 리스트")]
        public List<SpawnInfo> spawnInfos;

        [Tooltip("몬스터 스폰 간격")]
        public float spawnInterval = 1.0f;

        [Tooltip("이 웨이브에서 사용할 스폰 지점 (선택 사항)")]
        public Transform[] customSpawnPoints; // 웨이브별 커스텀 스폰 지점
    }

    [Tooltip("스폰할 웨이브 목록")]
    public List<Wave> waves;

    [Tooltip("기본 몬스터 스폰 지점")]
    public Transform[] spawnPoints;

    [Tooltip("웨이브 종료 후 트리거될 대화 이름")]
    public string endDialogueName;

    [Tooltip("특정 웨이브 종료 후 트리거될 대화 이름")]
    public string waveDialogueName;

    [Tooltip("대화를 트리거할 웨이브 인덱스 (0부터 시작)")]
    public int dialogueTriggerWaveIndex = 1;

    [Tooltip("플레이어와의 상호작용을 위한 참조")]
    public PlayerInteraction playerInteraction;

    [Tooltip("나태 맵 기믹 여부")]
    public bool slothMapGimmick = false;

    private List<GameObject> spawnedMonsters = new List<GameObject>();
    private int currentWaveIndex = 0;

    private void Start()
    {
        if (waves.Count > 0)
        {
            StartCoroutine(SpawnWaves());
        }
    }

    private IEnumerator SpawnWaves()
    {
        while (currentWaveIndex < waves.Count)
        {
            var wave = waves[currentWaveIndex];
            var spawnPointsToUse = wave.customSpawnPoints.Length > 0 ? wave.customSpawnPoints : spawnPoints; // 웨이브의 커스텀 스폰 지점 사용 여부 결정

            foreach (var spawnInfo in wave.spawnInfos)
            {
                for (int i = 0; i < spawnInfo.count; i++)
                {
                    SpawnMonster(spawnInfo.monsterPrefab, spawnPointsToUse);
                    yield return new WaitForSecondsRealtime(wave.spawnInterval);
                }
            }

            yield return new WaitUntil(() => AreAllMonstersDead());

            currentWaveIndex++;

            if (currentWaveIndex == dialogueTriggerWaveIndex && !string.IsNullOrEmpty(waveDialogueName) && playerInteraction != null)
            {
                Debug.Log($"웨이브 {dialogueTriggerWaveIndex}가 클리어되었습니다. 다이얼로그를 트리거합니다: {waveDialogueName}");
                playerInteraction.SetDialogueNameToTrigger(waveDialogueName);
                ExecuteGimmick();
            }
        }

        if (currentWaveIndex >= waves.Count && !string.IsNullOrEmpty(endDialogueName) && playerInteraction != null)
        {
            Debug.Log($"모든 웨이브가 끝났습니다. 종료 다이얼로그를 트리거합니다: {endDialogueName}");
            playerInteraction.SetDialogueNameToTrigger(endDialogueName);
        }
    }

    private void ExecuteGimmick()
    {
        if (slothMapGimmick)
        {
            Debug.Log("나태 맵 기믹이 실행되었습니다.");
            ExecuteSlothMapGimmick();
        }
    }

    private void ExecuteSlothMapGimmick()
    {
        ElectricWire[] electricWires = FindObjectsOfType<ElectricWire>();
        foreach (var wire in electricWires)
        {
            wire.ReverseRotation();
            wire.IncreaseSpeed();
        }
    }

    private void SpawnMonster(GameObject monsterPrefab, Transform[] spawnPointsToUse)
    {
        if (spawnPointsToUse.Length == 0)
        {
            Debug.LogWarning("스폰 지점이 설정되지 않았습니다.");
            return;
        }

        int spawnPointIndex = Random.Range(0, spawnPointsToUse.Length);
        Transform spawnPoint = spawnPointsToUse[spawnPointIndex];

        GameObject monster = Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedMonsters.Add(monster);
    }

    private void Update()
    {
        spawnedMonsters.RemoveAll(monster => monster == null || !monster.activeInHierarchy);
    }

    private bool AreAllMonstersDead()
    {
        return spawnedMonsters.Count == 0;
    }
}
