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
    // �ٸ� ��͵��� ���� Bool ���� �߰�
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
        // �ٸ� ��͵��� ���⿡ �߰�
        // if (gimmick2) { ExecuteGimmick2(); }
        // if (gimmick3) { ExecuteGimmick3(); }
        // ...
    }

    private void ExecuteSlothMapGimmick()
    {
        // ���� �� ��� ���� ����
        // �ð�������� ���� �������� �ݽð�������� ������ ���� ����� ����
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
            Debug.LogWarning("���� ������ �������� �ʾҽ��ϴ�.");
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
