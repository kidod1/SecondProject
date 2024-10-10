using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/IlchwiCheonil")]
public class IlchwiCheonil : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("������ �� ���ط�")]
    public float[] poisonDamageLevels = { 10f, 15f, 20f, 25f, 30f }; // ���� 1~5

    [Tooltip("������ �� ����")]
    public float[] poisonRangeLevels = { 5f, 5.5f, 6f, 6.5f, 7f }; // ���� 1~5

    [Tooltip("������ �� ���� �ð�")]
    public float[] poisonDurationLevels = { 5f, 6f, 7f, 8f, 9f }; // ���� 1~5

    [Tooltip("������ �� ���� ��ȯ ���� (��)")]
    public float[] spawnIntervalLevels = { 10f, 9f, 8f, 7f, 6f }; // ���� 1~5

    [Tooltip("�� ���� ������")]
    public GameObject poisonCloudPrefab;

    private Player playerInstance;
    private Coroutine poisonCloudCoroutine;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("IlchwiCheonil Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;

        if (poisonCloudCoroutine == null)
        {
            poisonCloudCoroutine = player.StartCoroutine(SpawnPoisonCloudRoutine());
        }

        Debug.Log($"IlchwiCheonil�� ����Ǿ����ϴ�. ���� ���� Lv: {currentLevel + 1}");
    }

    /// <summary>
    /// �ɷ��� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public void Remove(Player player)
    {
        if (poisonCloudCoroutine != null)
        {
            player.StopCoroutine(poisonCloudCoroutine);
            poisonCloudCoroutine = null;
        }

        Debug.Log("IlchwiCheonil�� ���ŵǾ����ϴ�.");
    }

    /// <summary>
    /// �� ������ �ֱ������� ��ȯ�ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator SpawnPoisonCloudRoutine()
    {
        while (true)
        {
            SpawnPoisonCloud();
            yield return new WaitForSeconds(GetCurrentSpawnInterval());
        }
    }

    /// <summary>
    /// �� ������ ��ȯ�մϴ�.
    /// </summary>
    private void SpawnPoisonCloud()
    {
        if (poisonCloudPrefab == null)
        {
            Debug.LogError("IlchwiCheonil: ������ �������� �������� �ʾҽ��ϴ�.");
            return;
        }

        Vector3 spawnPosition = playerInstance.transform.position;

        GameObject poisonCloud = Instantiate(poisonCloudPrefab, spawnPosition, Quaternion.identity);
        PoisonCloud poisonCloudScript = poisonCloud.GetComponent<PoisonCloud>();

        if (poisonCloudScript != null)
        {
            poisonCloudScript.Initialize(GetCurrentPoisonDamage(), GetCurrentPoisonRange(), GetCurrentPoisonDuration());
            Debug.Log($"IlchwiCheonil: ������ ��ȯ��. ���ط�: {GetCurrentPoisonDamage()}, ����: {GetCurrentPoisonRange()}, ���� �ð�: {GetCurrentPoisonDuration()}��");
        }
        else
        {
            Debug.LogError("IlchwiCheonil: PoisonCloud ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

        if (playerInstance != null)
        {
            if (poisonCloudCoroutine != null)
            {
                playerInstance.StopCoroutine(poisonCloudCoroutine);
                poisonCloudCoroutine = null;
            }

            playerInstance = null;
        }

        Debug.Log("IlchwiCheonil ������ �ʱ�ȭ�Ǿ����ϴ�.");
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ �� ���ط�, ����, ���� �ð�, ��ȯ ������ ������ŵ�ϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel�� 5�� ���, currentLevel�� 0~4
        {
            currentLevel++;
            Debug.Log($"IlchwiCheonil ���׷��̵�: ���� ���� {currentLevel + 1}");

            // ���� �� �� �ʿ��� �߰� ������ �ִٸ� ���⿡ �߰�
        }
        else
        {
            Debug.LogWarning("IlchwiCheonil: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ��� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            float currentDamage = GetCurrentPoisonDamage();
            float currentRange = GetCurrentPoisonRange();
            float currentDuration = GetCurrentPoisonDuration();
            float currentInterval = GetCurrentSpawnInterval();

            return $"{baseDescription}\n(Lv {currentLevel + 1}: �� ���ط� {currentDamage}, �� ���� {currentRange}m, �� ���� �ð� {currentDuration}��, ��ȯ ���� {currentInterval}��)";
        }
        else
        {
            float finalDamage = GetCurrentPoisonDamage();
            float finalRange = GetCurrentPoisonRange();
            float finalDuration = GetCurrentPoisonDuration();
            float finalInterval = GetCurrentSpawnInterval();

            return $"{baseDescription}\nMax Level: �� ���ط� {finalDamage}, �� ���� {finalRange}m, �� ���� �ð� {finalDuration}��, ��ȯ ���� {finalInterval}��";
        }
    }

    /// <summary>
    /// ���� ���� ������ �ʿ��� ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���� ���� �� �ʿ��� ��</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < poisonDamageLevels.Length)
        {
            // ����: ���� ������ �� ���ط��� ��ȯ
            return Mathf.RoundToInt(poisonDamageLevels[currentLevel]);
        }
        Debug.LogWarning($"IlchwiCheonil: currentLevel ({currentLevel})�� poisonDamageLevels �迭�� ������ ������ϴ�. �⺻�� 1�� ��ȯ�մϴ�.");
        return 1;
    }

    /// <summary>
    /// ���� ������ �� ���ط��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ �� ���ط�</returns>
    private float GetCurrentPoisonDamage()
    {
        if (currentLevel < poisonDamageLevels.Length)
        {
            return poisonDamageLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"IlchwiCheonil: currentLevel ({currentLevel})�� poisonDamageLevels �迭�� ������ ������ϴ�. �⺻�� {poisonDamageLevels[poisonDamageLevels.Length - 1]}�� ��ȯ�մϴ�.");
            return poisonDamageLevels[poisonDamageLevels.Length - 1];
        }
    }

    /// <summary>
    /// ���� ������ �� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ �� ����</returns>
    private float GetCurrentPoisonRange()
    {
        if (currentLevel < poisonRangeLevels.Length)
        {
            return poisonRangeLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"IlchwiCheonil: currentLevel ({currentLevel})�� poisonRangeLevels �迭�� ������ ������ϴ�. �⺻�� {poisonRangeLevels[poisonRangeLevels.Length - 1]}�� ��ȯ�մϴ�.");
            return poisonRangeLevels[poisonRangeLevels.Length - 1];
        }
    }

    /// <summary>
    /// ���� ������ �� ���� �ð��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ �� ���� �ð�</returns>
    private float GetCurrentPoisonDuration()
    {
        if (currentLevel < poisonDurationLevels.Length)
        {
            return poisonDurationLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"IlchwiCheonil: currentLevel ({currentLevel})�� poisonDurationLevels �迭�� ������ ������ϴ�. �⺻�� {poisonDurationLevels[poisonDurationLevels.Length - 1]}�� ��ȯ�մϴ�.");
            return poisonDurationLevels[poisonDurationLevels.Length - 1];
        }
    }

    /// <summary>
    /// ���� ������ �� ���� ��ȯ ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ �� ���� ��ȯ ����</returns>
    private float GetCurrentSpawnInterval()
    {
        if (currentLevel < spawnIntervalLevels.Length)
        {
            return spawnIntervalLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"IlchwiCheonil: currentLevel ({currentLevel})�� spawnIntervalLevels �迭�� ������ ������ϴ�. �⺻�� {spawnIntervalLevels[spawnIntervalLevels.Length - 1]}�� ��ȯ�մϴ�.");
            return spawnIntervalLevels[spawnIntervalLevels.Length - 1];
        }
    }
}
