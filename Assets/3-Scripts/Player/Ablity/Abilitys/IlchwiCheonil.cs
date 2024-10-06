using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/IlchwiCheonil")]
public class IlchwiCheonil : Ability
{
    [Header("Ability Parameters")]
    public float poisonDamage = 10f;         // �������� ���ط� (�ʴ� ���ط�)
    public float poisonRange = 5f;           // �������� ���� (������)
    public float poisonDuration = 5f;        // �������� ���� �ð�
    public float spawnInterval = 10f;        // ������ ���� �ֱ�
    public GameObject poisonCloudPrefab;     // ������ ������

    private Player playerInstance;
    private Coroutine poisonCloudCoroutine;

    public override void Apply(Player player)
    {
        playerInstance = player;

        // ������ ���� �ڷ�ƾ ����
        if (poisonCloudCoroutine == null)
        {
            poisonCloudCoroutine = player.StartCoroutine(SpawnPoisonCloudRoutine());
        }
    }

    public void Remove(Player player)
    {
        if (poisonCloudCoroutine != null)
        {
            player.StopCoroutine(poisonCloudCoroutine);
            poisonCloudCoroutine = null;
        }
    }

    private IEnumerator SpawnPoisonCloudRoutine()
    {
        while (true)
        {
            SpawnPoisonCloud();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnPoisonCloud()
    {
        if (poisonCloudPrefab == null)
        {
            Debug.LogError("������ �������� �������� �ʾҽ��ϴ�.");
            return;
        }

        Vector3 spawnPosition = playerInstance.transform.position;

        GameObject poisonCloud = Instantiate(poisonCloudPrefab, spawnPosition, Quaternion.identity);
        PoisonCloud poisonCloudScript = poisonCloud.GetComponent<PoisonCloud>();

        if (poisonCloudScript != null)
        {
            poisonCloudScript.Initialize(poisonDamage, poisonRange, poisonDuration);
        }
        else
        {
            Debug.LogError("PoisonCloud ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        // �ʿ� �� ���� �ʱ�ȭ ���� �߰�
    }

    protected override int GetNextLevelIncrease()
    {
        return 1;
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            poisonDamage += 5f;
            poisonRange += 0.5f;
        }
    }
}
