using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/HoneyDrop")]
public class HoneyDrop : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("������ ȸ���� (���� 1~5)")]
    public int[] healthRecoveryAmountLevels = { 20, 30, 40, 50, 60 }; // ���� 1~5

    [Tooltip("������ ���� ��� Ȯ��")]
    [Range(0f, 1f)]
    public float[] honeyDropChanceLevels = { 0.3f, 0.35f, 0.4f, 0.45f, 0.5f }; // ���� 1~5

    [Tooltip("���� ������ ������ ���")]
    public string honeyItemPrefabPath = "HoneyItemPrefab";

    private Player playerInstance;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        playerInstance = player;
    }

    /// <summary>
    /// ���Ͱ� �׾��� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="monster">���� ����</param>
    public void OnMonsterDeath(Monster monster)
    {
        if (Random.value <= GetCurrentHoneyDropChance())
        {
            SpawnHoney(monster.transform.position);
        }
    }

    /// <summary>
    /// ���� �������� �����մϴ�.
    /// </summary>
    /// <param name="position">������ ���� ��ġ</param>
    private void SpawnHoney(Vector3 position)
    {
        GameObject honeyPrefab = Resources.Load<GameObject>(honeyItemPrefabPath);

        if (honeyPrefab != null)
        {
            GameObject honeyItem = Instantiate(honeyPrefab, position, Quaternion.identity);
            HoneyItem honeyScript = honeyItem.GetComponent<HoneyItem>();
            if (honeyScript != null)
            {
                honeyScript.ItemData.healAmount = GetCurrentHealthRecoveryAmount();
                Debug.Log($"HoneyDrop: ���� ������ ������. ȸ����: {honeyScript.ItemData.healAmount}");
            }
            else
            {
                Debug.LogWarning("HoneyDrop: HoneyItem ��ũ��Ʈ�� ã�� �� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning($"HoneyDrop: ���� ������ �������� ã�� �� �����ϴ�: {honeyItemPrefabPath}");
        }
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ȸ������ ��� Ȯ���� ������ŵ�ϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel�� 5�� ���, currentLevel�� 0~4
        {
            currentLevel++;
            Debug.Log($"HoneyDrop ���׷��̵�: ���� ���� {currentLevel + 1}");
            // ���� �� �� �ʿ��� �߰� ������ �ִٸ� ���⿡ �߰�
        }
        else
        {
            Debug.LogWarning("HoneyDrop: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
    }

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ��� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            int currentHeal = GetCurrentHealthRecoveryAmount();
            float currentChance = GetCurrentHoneyDropChance();
            return $"{baseDescription}{System.Environment.NewLine}(Lv {currentLevel + 1}: ȸ���� +{currentHeal}, ��� Ȯ�� {currentChance * 100}%)";
        }
        else
        {
            int finalHeal = GetCurrentHealthRecoveryAmount();
            float finalChance = GetCurrentHoneyDropChance();
            return $"{baseDescription}{System.Environment.NewLine}(Max Level: ȸ���� +{finalHeal}, ��� Ȯ�� {finalChance * 100}%)";
        }
    }

    /// <summary>
    /// ���� ���� ������ �ʿ��� ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���� ���� �� �ʿ��� ��</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < healthRecoveryAmountLevels.Length)
        {
            // ���� ������ ȸ������ ��ȯ
            return healthRecoveryAmountLevels[currentLevel];
        }
        Debug.LogWarning($"HoneyDrop: currentLevel ({currentLevel})�� healthRecoveryAmountLevels �迭�� ������ ������ϴ�. �⺻�� 1�� ��ȯ�մϴ�.");
        return 1;
    }

    /// <summary>
    /// ���� ������ ȸ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ȸ����</returns>
    private int GetCurrentHealthRecoveryAmount()
    {
        if (currentLevel < healthRecoveryAmountLevels.Length)
        {
            return healthRecoveryAmountLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"HoneyDrop: currentLevel ({currentLevel})�� healthRecoveryAmountLevels �迭�� ������ ������ϴ�. �⺻�� {healthRecoveryAmountLevels[healthRecoveryAmountLevels.Length - 1]}�� ��ȯ�մϴ�.");
            return healthRecoveryAmountLevels[healthRecoveryAmountLevels.Length - 1];
        }
    }

    /// <summary>
    /// ���� ������ ���� ��� Ȯ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ���� ��� Ȯ��</returns>
    private float GetCurrentHoneyDropChance()
    {
        if (currentLevel < honeyDropChanceLevels.Length)
        {
            return honeyDropChanceLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"HoneyDrop: currentLevel ({currentLevel})�� honeyDropChanceLevels �迭�� ������ ������ϴ�. �⺻�� {honeyDropChanceLevels[honeyDropChanceLevels.Length - 1]}�� ��ȯ�մϴ�.");
            return honeyDropChanceLevels[honeyDropChanceLevels.Length - 1];
        }
    }
}
