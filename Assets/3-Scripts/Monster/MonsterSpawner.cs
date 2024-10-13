using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonsterSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnInfo
    {
        public GameObject monsterPrefab;
        public int count;
        public float spawnInterval = 1.0f;
    }

    [System.Serializable]
    public class Wave
    {
        public List<SpawnInfo> spawnInfos;
        public Transform[] customSpawnPoints;
    }

    public List<Wave> waves;
    public Transform[] spawnPoints;
    public bool isSlothArea = false;

    [Header("UI Elements")]
    public TextMeshProUGUI waveNumberText;
    public GameObject midBossPrefab;
    public Transform midBossSpawnPoint;

    private List<GameObject> spawnedMonsters = new List<GameObject>();
    private int currentWaveIndex = 0;
    private MidBoss midBossInstance;

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
            var spawnPointsToUse = wave.customSpawnPoints.Length > 0 ? wave.customSpawnPoints : spawnPoints;

            foreach (var spawnInfo in wave.spawnInfos)
            {
                for (int i = 0; i < spawnInfo.count; i++)
                {
                    SpawnMonster(spawnInfo.monsterPrefab, spawnPointsToUse);
                    yield return new WaitForSeconds(spawnInfo.spawnInterval);
                }
            }

            yield return new WaitUntil(() => AreAllMonstersDead());

            currentWaveIndex++;
            ShowWaveNumberUI();
        }

        SpawnMidBoss();

        if (isSlothArea && midBossInstance != null)
        {
            midBossInstance.SetAttackable(true);

        }
    }

    private void SpawnMidBoss()
    {
        if (midBossPrefab != null && midBossSpawnPoint != null)
        {
            GameObject bossObject = Instantiate(midBossPrefab, midBossSpawnPoint.position, midBossSpawnPoint.rotation);
            midBossInstance = bossObject.GetComponent<MidBoss>();
            Debug.Log("�߰� ������ �����Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("�߰� ���� ������ �Ǵ� ���� ������ �������� �ʾҽ��ϴ�.");
        }
    }

    private void SpawnMonster(GameObject monsterPrefab, Transform[] spawnPointsToUse)
    {
        if (spawnPointsToUse.Length == 0)
        {
            Debug.LogWarning("���� ������ �������� �ʾҽ��ϴ�.");
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

    private void ShowWaveNumberUI()
    {
        if (waveNumberText != null)
        {
            waveNumberText.text = $"���̺� {currentWaveIndex} Ŭ����!";
            waveNumberText.alpha = 1f; // �ؽ�Ʈ�� ������ �������ϰ� ����
            StartCoroutine(FadeOutWaveNumber(3f)); // 3�� �Ŀ� ���̵� �ƿ� ����
        }
    }

    private IEnumerator FadeOutWaveNumber(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration); // ���� ���������� ����
            waveNumberText.alpha = alpha;
            yield return null;
        }
        waveNumberText.alpha = 0f; // ���̵� �ƿ��� ������ ������ �����ϰ� ����
    }
}
