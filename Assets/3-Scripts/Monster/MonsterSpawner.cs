using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonsterSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnInfo
    {
        [Tooltip("�̸� ��ġ�� ���� ������Ʈ��")]
        public List<GameObject> monstersToSpawn; // ������ ��� �̸� ��ġ�� ���� ����Ʈ
        public float spawnInterval = 1.0f; // ���� ���� ���� ����
    }

    [System.Serializable]
    public class Wave
    {
        public List<SpawnInfo> spawnInfos;
        public Transform[] customSpawnPoints; // ���̺꺰 Ŀ���� ���� ����
    }

    [Header("Wave Settings")]
    public List<Wave> waves;
    public Transform[] spawnPoints; // �⺻ ���� ����
    public bool isSlothArea = false; // ���ν� ���� ����

    [Header("UI Elements")]
    public TextMeshProUGUI waveNumberText;

    [Header("Mid-Boss Settings")]
    [Tooltip("���� �̸� ��ġ�� �߰� ���� ������Ʈ")]
    public MidBoss midBossInstance; // ���� �̸� ��ġ�� MidBoss ������Ʈ

    private int currentWaveIndex = 0;

    private void Start()
    {
        if (waves.Count > 0)
        {
            StartCoroutine(SpawnWaves());
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: ���̺� ������ �����ϴ�.");
        }

        if (midBossInstance != null)
        {
            midBossInstance.gameObject.SetActive(false); // �ʱ⿡�� ��Ȱ��ȭ
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: midBossInstance�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    private IEnumerator SpawnWaves()
    {
        while (currentWaveIndex < waves.Count)
        {
            Wave currentWave = waves[currentWaveIndex];
            Transform[] spawnPointsToUse = currentWave.customSpawnPoints.Length > 0 ? currentWave.customSpawnPoints : spawnPoints;

            foreach (var spawnInfo in currentWave.spawnInfos)
            {
                foreach (var monster in spawnInfo.monstersToSpawn)
                {
                    SpawnMonster(monster, spawnPointsToUse);
                    yield return new WaitForSeconds(spawnInfo.spawnInterval);
                }
            }

            yield return new WaitUntil(() => AreAllMonstersDead());

            ShowWaveNumberUI();

            currentWaveIndex++;
        }

        SpawnMidBoss();

        if (isSlothArea && midBossInstance != null)
        {
            midBossInstance.SetAttackable(true);
        }
    }

    /// <summary>
    /// �̸� ��ġ�� ���͸� Ȱ��ȭ�ϰ� ���� ������ ��ġ�մϴ�.
    /// </summary>
    /// <param name="monster">Ȱ��ȭ�� ���� ������Ʈ</param>
    /// <param name="spawnPointsToUse">����� ���� ���� �迭</param>
    private void SpawnMonster(GameObject monster, Transform[] spawnPointsToUse)
    {
        if (spawnPointsToUse.Length == 0)
        {
            Debug.LogWarning("MonsterSpawner: ���� ������ �������� �ʾҽ��ϴ�.");
            return;
        }

        // ��Ȱ��ȭ�� ���͸� ã�� Ȱ��ȭ
        if (monster.activeInHierarchy)
        {
            Debug.LogWarning($"MonsterSpawner: {monster.name}��(��) �̹� Ȱ��ȭ�Ǿ� �ֽ��ϴ�.");
            return;
        }

        // ���� ���� ���� ����
        int spawnPointIndex = Random.Range(0, spawnPointsToUse.Length);
        Transform spawnPoint = spawnPointsToUse[spawnPointIndex];

        // ���� ��ġ �� ȸ�� ����
        monster.transform.position = spawnPoint.position;
        monster.transform.rotation = spawnPoint.rotation;

        // ���� Ȱ��ȭ
        monster.SetActive(true);

        Debug.Log($"MonsterSpawner: {monster.name}��(��) ���� ���� {spawnPoint.name}�� �����߽��ϴ�.");
    }

    private void SpawnMidBoss()
    {
        if (midBossInstance != null)
        {
            // �̸� ��ġ�� MidBoss�� ��ġ �� ȸ�� ���� (�ʿ� �� ����)
            // ���� ���, Ư�� ��ġ�� �̵��ϰų� ȸ���� ������ �� �ֽ��ϴ�.
            // midBossInstance.transform.position = midBossSpawnPoint.position;
            // midBossInstance.transform.rotation = midBossSpawnPoint.rotation;

            midBossInstance.gameObject.SetActive(true);
            Debug.Log("MonsterSpawner: �߰� ������ Ȱ��ȭ�Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: midBossInstance�� �������� �ʾҽ��ϴ�.");
        }
    }

    private bool AreAllMonstersDead()
    {
        // ���� �ִ� ��� ���͸� ã���ϴ�. (�±׸� "Monster"�� ����)
        GameObject[] activeMonsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (var monster in activeMonsters)
        {
            if (monster.activeInHierarchy)
            {
                return false;
            }
        }
        return true;
    }

    private void ShowWaveNumberUI()
    {
        if (waveNumberText != null)
        {
            waveNumberText.text = $"���̺� {currentWaveIndex} Ŭ����!";
            waveNumberText.alpha = 1f; // �ؽ�Ʈ�� ������ �������ϰ� ����
            StartCoroutine(FadeOutWaveNumber(3f)); // 3�� �Ŀ� ���̵� �ƿ� ����
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: waveNumberText�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    private IEnumerator FadeOutWaveNumber(float duration)
    {
        float elapsedTime = 0f;
        Color originalColor = waveNumberText.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration); // ���� ���������� ����
            waveNumberText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        waveNumberText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // ���̵� �ƿ��� ������ ������ �����ϰ� ����
    }
}
