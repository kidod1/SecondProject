using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseAttack")]
public class IncreaseAttack : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("������ ���ݷ� ������ (���� 1���� ����)")]
    public int[] attackIncreases;

    private Player playerInstance; // �÷��̾� �ν��Ͻ� ����
    private int previousTotalIncrease = 0; // ������ ����� �� ���ݷ� ������

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        Debug.Log("IncreaseAttack Apply");
        if (player == null)
        {
            Debug.LogError("IncreaseAttack Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;

        // ���� ���������� �� ���ݷ� ������ ���
        int totalIncrease = GetTotalAttackIncrease();

        // ������ ����� ���ݷ� �������� ����
        player.stat.defaultPlayerDamage -= previousTotalIncrease;

        // ���ο� �� ���ݷ� �������� ����
        player.stat.defaultPlayerDamage += totalIncrease;
        player.stat.currentPlayerDamage = player.stat.defaultPlayerDamage;

        // ���� �� ���ݷ� ������ ������Ʈ
        previousTotalIncrease = totalIncrease;
        Debug.Log($"IncreaseAttack: Applied total attack increase of {totalIncrease} at level {currentLevel}");
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        Debug.Log("IncreaseAttack Upgrade");

        if (playerInstance == null)
        {
            Debug.LogWarning("IncreaseAttack Upgrade: playerInstance�� null�Դϴ�.");
            return;
        }

        // ���� ���������� �� ���ݷ� ������ ���
        int totalIncrease = GetTotalAttackIncrease();

        // ������ ����� ���ݷ� �������� ����
        playerInstance.stat.defaultPlayerDamage -= previousTotalIncrease;

        // ���ο� �� ���ݷ� �������� ����
        playerInstance.stat.defaultPlayerDamage += totalIncrease;
        playerInstance.stat.currentPlayerDamage = playerInstance.stat.defaultPlayerDamage;

        // ���� �� ���ݷ� ������ ������Ʈ
        previousTotalIncrease = totalIncrease;
    }

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ��� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        // �� ���ݷ� ������ ���
        int totalIncrease = GetTotalAttackIncrease();

        if (currentLevel < attackIncreases.Length)
        {
            int nextIncrease = attackIncreases[currentLevel + 1];
            return $"{baseDescription}\nLv {currentLevel + 1}:\n�� ���ݷ� +{totalIncrease}\n���� ����: ���ݷ� +{nextIncrease}";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����\n�� ���ݷ� +{totalIncrease}";
        }
    }

    /// <summary>
    /// ���� ���� ������ �ʿ��� ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���� ���� �� �ʿ��� ��</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < attackIncreases.Length)
        {
            return attackIncreases[currentLevel];
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        if (playerInstance != null)
        {
            playerInstance.stat.defaultPlayerDamage -= GetTotalAttackIncrease();
            playerInstance.stat.currentPlayerDamage = playerInstance.stat.defaultPlayerDamage;
        }
        currentLevel = 0;
        previousTotalIncrease = 0;
    }

    /// <summary>
    /// ���� ���������� �� ���ݷ� �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�� ���ݷ� ������</returns>
    private int GetTotalAttackIncrease()
    {
        int totalIncrease = 0;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < attackIncreases.Length)
                totalIncrease += attackIncreases[i];
        }
        return totalIncrease;
    }
}
