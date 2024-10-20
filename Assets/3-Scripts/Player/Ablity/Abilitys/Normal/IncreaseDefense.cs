using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseDefense")]
public class IncreaseDefense : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("������ ���� ������ (���� 1���� ����)")]
    public int[] defenseIncreases; // ���� 1~5

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        if (player == null) return;

        // ���� ������ ���� ���� ���� ����
        if (currentLevel < defenseIncreases.Length)
        {
            player.stat.currentDefense += defenseIncreases[currentLevel];
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

            // ���� �� �� ���� ���� ����
            Player player = FindObjectOfType<Player>();
            if (player != null && currentLevel < defenseIncreases.Length)
            {
                player.stat.currentDefense += defenseIncreases[currentLevel - 1];
            }
        }
    }

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ��� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        // �� ���� ������ ���
        int totalIncrease = 0;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < defenseIncreases.Length)
                totalIncrease += defenseIncreases[i];
        }

        if (currentLevel < defenseIncreases.Length)
        {
            int currentIncrease = defenseIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: ���� +{currentIncrease}\n���ݱ��� �� {totalIncrease}��ŭ ���";
        }
        else
        {
            int finalIncrease = defenseIncreases.Length > 0 ? defenseIncreases[defenseIncreases.Length - 1] : 0;
            return $"{baseDescription}\nMax Level: ���� +{finalIncrease}\n���ݱ��� �� {totalIncrease}��ŭ ���";
        }
    }

    /// <summary>
    /// ���� ���� ������ �ʿ��� ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���� ���� �� �ʿ��� ��</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < defenseIncreases.Length)
        {
            return defenseIncreases[currentLevel];
        }
        return 0;
    }
}
