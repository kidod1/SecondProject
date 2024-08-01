using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnInfo
    {
        public GameObject monsterPrefab;
        public int count;
    }

    [System.Serializable]
    public class Wave
    {
        public List<SpawnInfo> spawnInfos;
        public float spawnInterval = 1.0f;
    }

    public List<Wave> waves;
    public Transform[] spawnPoints;
    public string endDialogueName;
    public string waveDialogueName;
    public int dialogueTriggerWaveIndex = 1;
    public PlayerInteraction playerInteraction;

    public bool slothMapGimmick = false;
    // 다른 기믹들을 위한 Bool 변수 추가
    // public bool gimmick2 = false;
    // public bool gimmick3 = false;
    // ...

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

            foreach (var spawnInfo in wave.spawnInfos)
            {
                for (int i = 0; i < spawnInfo.count; i++)
                {
                    SpawnMonster(spawnInfo.monsterPrefab);
                    yield return new WaitForSeconds(wave.spawnInterval);
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
        // 다른 기믹들을 여기에 추가
        // if (gimmick2) { ExecuteGimmick2(); }
        // if (gimmick3) { ExecuteGimmick3(); }
        // ...
    }

    private void ExecuteSlothMapGimmick()
    {
        // 나태 맵 기믹 실행 로직
        // 시계방향으로 돌던 전기줄을 반시계방향으로 빠르게 돌게 만드는 로직
        ElectricWire[] electricWires = FindObjectsOfType<ElectricWire>();
        foreach (var wire in electricWires)
        {
            wire.ReverseRotation();
            wire.IncreaseSpeed();
        }
    }

    private void SpawnMonster(GameObject monsterPrefab)
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("스폰 지점이 설정되지 않았습니다.");
            return;
        }

        int spawnPointIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnPointIndex];

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
