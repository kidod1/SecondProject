using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseAttack")]
public class IncreaseAttack : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("������ ���ݷ� ������ (���� 1���� ����)")]
    public int[] attackIncreases;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("IncreaseAttack Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        // ���� ������ ���� ���ݷ� ���� ����
        if (currentLevel < attackIncreases.Length)
        {
            player.stat.currentPlayerDamage += attackIncreases[currentLevel];
            Debug.Log($"IncreaseAttack�� ����Ǿ����ϴ�. ���� ���� Lv: {currentLevel + 1}, ���ݷ� ����: {attackIncreases[currentLevel]}");
        }
        else
        {
            Debug.LogWarning($"IncreaseAttack: currentLevel ({currentLevel})�� attackIncreases �迭�� ������ ������ϴ�.");
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
            Debug.Log($"IncreaseAttack ���׷��̵�: ���� ���� {currentLevel + 1}");

            // ���� �� �� ���ݷ� ���� ����
            Player player = FindObjectOfType<Player>();
            if (player != null && currentLevel < attackIncreases.Length)
            {
                player.stat.currentPlayerDamage += attackIncreases[currentLevel];
                Debug.Log($"IncreaseAttack ���� {currentLevel + 1}���� ���ݷ� ����: {attackIncreases[currentLevel]}");
            }
        }
        else
        {
            Debug.LogWarning("IncreaseAttack: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ��� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < attackIncreases.Length)
        {
            int currentIncrease = attackIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: ���ݷ� +{currentIncrease}";
        }
        else
        {
            Debug.LogWarning($"IncreaseAttack: currentLevel ({currentLevel})�� attackIncreases �迭�� ������ ������ϴ�. �ִ� ���� ������ ��ȯ�մϴ�.");
            return $"{baseDescription}\nMax Level: ���ݷ� +{attackIncreases[attackIncreases.Length - 1]}";
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
        Debug.LogWarning($"IncreaseAttack: currentLevel ({currentLevel})�� attackIncreases �迭�� ������ ������ϴ�. �⺻�� 0�� ��ȯ�մϴ�.");
        return 0;
    }
}
