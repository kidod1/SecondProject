using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseAttackSpeed")]
public class IncreaseAttackSpeed : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("������ ���� �ӵ� ���ҷ� (�� ����)")]
    public float[] cooldownReductions; // ���� 1~5
    private Player playerInstance;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("IncreaseAttackSpeed Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;

        // ���� ������ ���� ���� �ӵ� ���� ����
        if (currentLevel < cooldownReductions.Length)
        {
            player.stat.currentShootCooldown -= cooldownReductions[currentLevel];
            // �ּ� ��ٿ� ����
            if (player.stat.currentShootCooldown < 0.1f)
            {
                player.stat.currentShootCooldown = 0.1f;
            }
        }
        else
        {
            Debug.LogWarning($"IncreaseAttackSpeed: currentLevel ({currentLevel})�� cooldownReductions �迭�� ������ ����ų� ������ 1 �����Դϴ�.");
        }
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel�� 5�� ���, currentLevel�� 0~4
        {
            currentLevel++;

            // ���� �� �� ���� �ӵ� ���� ����
            Player player = FindObjectOfType<Player>();
            if (player != null && currentLevel < cooldownReductions.Length)
            {
                player.stat.currentShootCooldown -= cooldownReductions[currentLevel - 1];
                // �ּ� ��ٿ� ����
                if (player.stat.currentShootCooldown < 0.1f)
                {
                    player.stat.currentShootCooldown = 0.1f;
                }
            }
            else
            {
                Debug.LogWarning($"IncreaseAttackSpeed: �÷��̾ ã�� �� ���ų� currentLevel ({currentLevel})�� cooldownReductions �迭�� ������ ������ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("IncreaseAttackSpeed: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ��� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        // �� ���� �ӵ� ���ҷ� ���
        float totalReduction = 0f;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < cooldownReductions.Length)
                totalReduction += cooldownReductions[i];
        }

        if (currentLevel < cooldownReductions.Length)
        {
            float currentReduction = cooldownReductions[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: ���� ��ٿ� ���� {currentReduction}��\n���ݱ��� �� {totalReduction}�� ����";
        }
        else
        {
            float finalReduction = cooldownReductions.Length > 0 ? cooldownReductions[cooldownReductions.Length - 1] : 0f;
            return $"{baseDescription}\nMax Level: ���� ��ٿ� ���� {finalReduction}��\n���ݱ��� �� {totalReduction}�� ����";
        }
    }

    /// <summary>
    /// ���� ���� ������ �ʿ��� ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���� ���� �� �ʿ��� ��</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < cooldownReductions.Length)
        {
            // ��ٿ� ���ҷ��� ������ ��ȯ (�Ҽ��� ����)
            return Mathf.RoundToInt(cooldownReductions[currentLevel]);
        }
        Debug.LogWarning($"IncreaseAttackSpeed: currentLevel ({currentLevel})�� cooldownReductions �迭�� ������ ������ϴ�. �⺻�� 0�� ��ȯ�մϴ�.");
        return 0;
    }
}
