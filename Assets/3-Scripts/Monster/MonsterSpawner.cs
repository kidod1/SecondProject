using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonsterSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnInfo
    {
        public GameObject monsterPrefab; // ������ ���� ������
        public int count; // ������ ���� ��
        public float spawnInterval = 1.0f; // ���� ���� ���� (��)
        public Transform[] customSpawnPoints; // ���ͺ� Ŀ���� ���� ���� (���� ��� �⺻ ���� ���� ���)
    }

    [System.Serializable]
    public class Wave
    {
        public List<SpawnInfo> spawnInfos; // �� ���̺��� ���� ���� ����Ʈ
    }

    [Header("Wave Settings")]
    public List<Wave> waves; // ��� ���̺� ����Ʈ
    public Transform[] spawnPoints; // �⺻ ���� ������
    public bool isSlothArea = false; // ���ν� ���� ����

    [Header("Spawn Effect")]
    public GameObject spawnEffectPrefab; // ���� ���� �� ����� ���� ����Ʈ ������

    [Header("UI Elements")]
    public TextMeshProUGUI waveNumberText; // ���̺� Ŭ���� �� ���� UI �ؽ�Ʈ
    public Animator waveNumberAnimator; // ���̺� �ؽ�Ʈ�� Animator ������Ʈ
    public GameObject midBossPrefab; // ���� �̸� ��ġ�� �߰� ���� ������Ʈ ����

    private List<GameObject> spawnedMonsters = new List<GameObject>(); // ���� Ȱ��ȭ�� ���� ����Ʈ
    private int currentWaveIndex = 0; // ���� ���̺� �ε���
    private MidBoss midBossInstance; // �߰� ���� �ν��Ͻ� ����

    [Header("Experience Items")]
    public Transform experienceItemsParent; // ����ġ �������� ���� �θ�

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

        // ����ġ ������ �θ� ������Ʈ �ʱ�ȭ
        InitializeExperienceItemsParent();

        // ���� ��ġ�� �߰� ���� ������Ʈ�� �Ҵ�Ǿ� �ִ��� Ȯ���ϰ�, �ʱ⿡�� ��Ȱ��ȭ ���·� ����
        if (midBossPrefab != null)
        {
            midBossPrefab.SetActive(false); // ���� �ʱ� ��Ȱ��ȭ

            // midBossPrefab�� �̹� ���� ��ġ�� ������Ʈ��� Instantiate ��� ������ �����ɴϴ�.
            midBossInstance = midBossPrefab.GetComponent<MidBoss>();
            if (midBossInstance == null)
            {
                Debug.LogError("midBossPrefab ������Ʈ�� MidBoss ������Ʈ�� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: midBossPrefab ������Ʈ�� �������� �ʾҽ��ϴ�.");
        }

        // Animator ���� Ȯ��
        if (waveNumberAnimator == null && waveNumberText != null)
        {
            waveNumberAnimator = waveNumberText.GetComponent<Animator>();
            if (waveNumberAnimator == null)
            {
                Debug.LogWarning("waveNumberText�� Animator ������Ʈ�� �����ϴ�. �ִϸ��̼� ����� ��Ȱ��ȭ�˴ϴ�.");
            }
        }
    }

    /// <summary>
    /// ����ġ �������� �θ� ������Ʈ�� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void InitializeExperienceItemsParent()
    {
        if (experienceItemsParent == null)
        {
            // ���� "ExperienceItems"��� �̸��� ������Ʈ�� �ִ��� ã��
            GameObject parentObj = GameObject.Find("ExperienceItems");
            if (parentObj == null)
            {
                // ���ٸ� ���� ����
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

            // *** ���̺� ���� �޽��� ǥ�� ***
            float messageDisplayDuration = 0.7f; // �޽��� ǥ�� �ð� ���� (��: 0.7��)

            if (waveNumberText != null)
            {
                waveNumberText.text = $"���̺� {currentWaveIndex + 1}";
                waveNumberText.alpha = 1f;

                // Animator�� ���� �ִϸ��̼� ��� (�ʿ� ��)
                if (waveNumberAnimator != null)
                {
                    waveNumberAnimator.SetTrigger("StartWave"); // "StartWave" Ʈ���� ����
                }

                StartCoroutine(FadeOutWaveNumber(messageDisplayDuration)); // �޽��� ǥ�� �ð� �Ŀ� ���̵� �ƿ� ����
            }
            else
            {
                Debug.LogWarning("MonsterSpawner: waveNumberText�� �Ҵ���� �ʾҽ��ϴ�.");
            }
            // *** ���̺� ���� �޽��� ǥ�� �� ***

            // �޽��� ǥ�� �ð���ŭ ���
            yield return new WaitForSeconds(messageDisplayDuration);

            // ���� ���� ����
            foreach (var spawnInfo in currentWave.spawnInfos)
            {
                // ���� ������ ���� ���� ����
                Transform[] spawnPointsToUse = (spawnInfo.customSpawnPoints != null && spawnInfo.customSpawnPoints.Length > 0) ? spawnInfo.customSpawnPoints : spawnPoints;

                for (int i = 0; i < spawnInfo.count; i++)
                {
                    SpawnMonster(spawnInfo.monsterPrefab, spawnPointsToUse);
                    yield return new WaitForSeconds(spawnInfo.spawnInterval);
                }
            }

            // ��� ���Ͱ� óġ�� ������ ���
            yield return new WaitUntil(() => AreAllMonstersDead());

            // ���̺� Ŭ���� UI ǥ��
            ShowWaveClearUI();

            currentWaveIndex++;
        }

        // ��� ���̺갡 �Ϸ�� �� �߰� ���� Ȱ��ȭ
        SpawnMidBoss();

        if (isSlothArea && midBossInstance != null)
        {
            midBossInstance.SetAttackable(true); // ���ν� ������ ��� ���� ���� ���� ���·� ����
        }
    }

    /// <summary>
    /// ���� �̸� ��ġ�� �߰� ���� ������Ʈ�� Ȱ��ȭ�մϴ�.
    /// </summary>
    private void SpawnMidBoss()
    {
        if (midBossPrefab != null)
        {
            midBossPrefab.SetActive(true); // ���� Ȱ��ȭ
            Debug.Log("MonsterSpawner: �߰� ������ Ȱ��ȭ�Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: midBossPrefab ������Ʈ�� �������� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// ������ ���� �������� ���͸� ����(Ȱ��ȭ)�ϰ�, ���� ����Ʈ�� ����մϴ�.
    /// </summary>
    /// <param name="monsterPrefab">������ ���� ������</param>
    /// <param name="spawnPointsToUse">����� ���� ���� �迭</param>
    private void SpawnMonster(GameObject monsterPrefab, Transform[] spawnPointsToUse)
    {
        if (spawnPointsToUse.Length == 0)
        {
            Debug.LogWarning("MonsterSpawner: ���� ������ �������� �ʾҽ��ϴ�.");
            return;
        }

        int spawnPointIndex = Random.Range(0, spawnPointsToUse.Length);
        Transform spawnPoint = spawnPointsToUse[spawnPointIndex];

        // ���� ����
        GameObject monster = Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation, this.transform);
        spawnedMonsters.Add(monster);

        // ���� ����Ʈ ���
        if (spawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(spawnEffectPrefab, spawnPoint.position, Quaternion.identity);
            // ����Ʈ�� �ڵ����� �ı��ǵ��� ����
            AutoDestroyEffect autoDestroy = effect.GetComponent<AutoDestroyEffect>();
            if (autoDestroy == null)
            {
                effect.AddComponent<AutoDestroyEffect>();
            }
        }
    }

    private void Update()
    {
        // Ȱ��ȭ�� ���� ����Ʈ�� ������Ʈ�Ͽ� ���� ���͸� ����
        spawnedMonsters.RemoveAll(monster => monster == null || !monster.activeInHierarchy);
    }

    /// <summary>
    /// ���� Ȱ��ȭ�� ��� ���Ͱ� óġ�Ǿ����� Ȯ���մϴ�.
    /// </summary>
    /// <returns>��� ���Ͱ� óġ�Ǿ����� true, �ƴϸ� false</returns>
    private bool AreAllMonstersDead()
    {
        return spawnedMonsters.Count == 0;
    }

    /// <summary>
    /// ���̺� Ŭ���� UI�� ǥ���ϰ� ������ ���̵� �ƿ��մϴ�.
    /// </summary>
    private void ShowWaveClearUI()
    {
        if (waveNumberText != null)
        {
            waveNumberText.text = $"���̺� {currentWaveIndex + 1} Ŭ����!";
            waveNumberText.alpha = 1f; // �ؽ�Ʈ�� ������ �������ϰ� ����

            // Animator�� ���� Ŭ���� �ִϸ��̼� ��� (�ʿ� ��)
            if (waveNumberAnimator != null)
            {
                waveNumberAnimator.SetTrigger("Clear"); // "Clear" Ʈ���� ����
                Debug.Log("Clear Ʈ���� �ߵ�");
            }

            StartCoroutine(FadeOutWaveNumber(3f)); // 3�� �Ŀ� ���̵� �ƿ� ����
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: waveNumberText�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// ������ �ð� ���� UI �ؽ�Ʈ�� ������ ���̵� �ƿ��մϴ�.
    /// </summary>
    /// <param name="duration">���̵� �ƿ� �ð� (��)</param>
    /// <returns></returns>
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
