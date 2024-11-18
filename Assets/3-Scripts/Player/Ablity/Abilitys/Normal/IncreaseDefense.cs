using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseDefense")]
public class IncreaseDefense : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("������ ���� ������ (���� 1���� ����)")]
    public int[] defenseIncreases; // ���� 1~5

    private Player playerInstance; // �÷��̾� �ν��Ͻ� ����
    private int previousTotalIncrease = 0; // ������ ����� �� ���� ������

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("IncreaseDefense Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;

        // ���� ���������� �� ���� ������ ���
        int totalIncrease = GetTotalDefenseIncrease();

        // ������ ����� ���� �������� ����
        player.stat.defaultDefense -= previousTotalIncrease;

        // ���ο� �� ���� �������� ����
        player.stat.defaultDefense += totalIncrease;
        player.stat.currentDefense = player.stat.defaultDefense;

        // ���� �� ���� ������ ������Ʈ
        previousTotalIncrease = totalIncrease;
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (playerInstance == null)
        {
            Debug.LogWarning("IncreaseDefense Upgrade: playerInstance�� null�Դϴ�.");
            return;
        }

        // ���� ���������� �� ���� ������ ���
        int totalIncrease = GetTotalDefenseIncrease();

        // ������ ����� ���� �������� ����
        playerInstance.stat.defaultDefense -= previousTotalIncrease;

        // ���ο� �� ���� �������� ����
        playerInstance.stat.defaultDefense += totalIncrease;
        playerInstance.stat.currentDefense = playerInstance.stat.defaultDefense;

        // ���� �� ���� ������ ������Ʈ
        previousTotalIncrease = totalIncrease;
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

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ��� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        // �� ���� ������ ���
        int totalIncrease = GetTotalDefenseIncrease();

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
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        if (playerInstance != null)
        {
            playerInstance.stat.defaultDefense -= previousTotalIncrease;
            playerInstance.stat.currentDefense = playerInstance.stat.defaultDefense;
        }
        currentLevel = 0;
        previousTotalIncrease = 0;
    }

    /// <summary>
    /// ���� ���������� �� ���� �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�� ���� ������</returns>
    private int GetTotalDefenseIncrease()
    {
        int totalIncrease = 0;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < defenseIncreases.Length)
                totalIncrease += defenseIncreases[i];
        }
        return totalIncrease;
    }
}
