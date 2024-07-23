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
    public PlayerInteraction playerInteraction;

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
        }

        if (!string.IsNullOrEmpty(endDialogueName) && playerInteraction != null)
        {
            Debug.Log($"종료 대화명 설정: {endDialogueName}");
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
        spawnedMonsters.RemoveAll(monster => monster == null || !monster.activeInHierarchy);
    }

    private bool AreAllMonstersDead()
    {
        return spawnedMonsters.Count == 0;
    }
}
