using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnInfo
    {
        [Tooltip("������ ���� ������")]
        public GameObject monsterPrefab;

        [Tooltip("������ ���� ��")]
        public int count;
    }

    [System.Serializable]
    public class Wave
    {
        [Tooltip("���� ���� ����Ʈ")]
        public List<SpawnInfo> spawnInfos;

        [Tooltip("���� ���� ����")]
        public float spawnInterval = 1.0f;

        [Tooltip("�� ���̺꿡�� ����� ���� ���� (���� ����)")]
        public Transform[] customSpawnPoints; // ���̺꺰 Ŀ���� ���� ����
    }

    [Tooltip("������ ���̺� ���")]
    public List<Wave> waves;

    [Tooltip("�⺻ ���� ���� ����")]
    public Transform[] spawnPoints;

    [Tooltip("���̺� ���� �� Ʈ���ŵ� ��ȭ �̸�")]
    public string endDialogueName;

    [Tooltip("Ư�� ���̺� ���� �� Ʈ���ŵ� ��ȭ �̸�")]
    public string waveDialogueName;

    [Tooltip("��ȭ�� Ʈ������ ���̺� �ε��� (0���� ����)")]
    public int dialogueTriggerWaveIndex = 1;

    [Tooltip("�÷��̾���� ��ȣ�ۿ��� ���� ����")]
    public PlayerInteraction playerInteraction;

    [Tooltip("���� �� ��� ����")]
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
            var spawnPointsToUse = wave.customSpawnPoints.Length > 0 ? wave.customSpawnPoints : spawnPoints; // ���̺��� Ŀ���� ���� ���� ��� ���� ����

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
                Debug.Log($"���̺� {dialogueTriggerWaveIndex}�� Ŭ����Ǿ����ϴ�. ���̾�α׸� Ʈ�����մϴ�: {waveDialogueName}");
                playerInteraction.SetDialogueNameToTrigger(waveDialogueName);
                ExecuteGimmick();
            }
        }

        if (currentWaveIndex >= waves.Count && !string.IsNullOrEmpty(endDialogueName) && playerInteraction != null)
        {
            Debug.Log($"��� ���̺갡 �������ϴ�. ���� ���̾�α׸� Ʈ�����մϴ�: {endDialogueName}");
            playerInteraction.SetDialogueNameToTrigger(endDialogueName);
        }
    }

    private void ExecuteGimmick()
    {
        if (slothMapGimmick)
        {
            Debug.Log("���� �� ����� ����Ǿ����ϴ�.");
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
}
