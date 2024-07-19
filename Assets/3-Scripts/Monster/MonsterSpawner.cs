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
        public float spawnInterval = 1.0f; // 몬스터 스폰 간격
    }

    public List<Wave> waves;
    public Transform[] spawnPoints; // 스폰 지점들
    public string endDialogueName; // 종료 다이얼로그 이름
    public PlayerInteraction playerInteraction; // PlayerInteraction 스크립트 참조

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

            // 모든 몬스터가 죽을 때까지 대기
            yield return new WaitUntil(() => AreAllMonstersDead());

            // 다음 웨이브로 이동
            currentWaveIndex++;
        }

        // 모든 웨이브 완료 후 종료 다이얼로그 트리거
        if (!string.IsNullOrEmpty(endDialogueName) && playerInteraction != null)
        {
            Debug.Log($"Setting end dialogue name in PlayerInteraction: {endDialogueName}");
            playerInteraction.SetDialogueNameToTrigger(endDialogueName);
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
        // 모든 몬스터가 죽었는지 확인
        spawnedMonsters.RemoveAll(monster => monster == null || !monster.activeInHierarchy);
    }

    private bool AreAllMonstersDead()
    {
        return spawnedMonsters.Count == 0;
    }
}
