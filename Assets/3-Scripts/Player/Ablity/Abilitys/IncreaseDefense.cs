using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseDefense")]
public class IncreaseDefense : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("������ ���� ������ (���� 1���� ����)")]
    public int[] defenseIncreases; // ���� 1~5
    private Player playerInstance;
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

        // ���� ������ ���� ���� ���� ����
        if (currentLevel < defenseIncreases.Length && currentLevel > 0)
        {
            player.stat.currentDefense += defenseIncreases[currentLevel];
            Debug.Log($"IncreaseDefense�� ����Ǿ����ϴ�. ���� ���� Lv: {currentLevel + 1}, ���� ����: {defenseIncreases[currentLevel]}");
        }
        else
        {
            Debug.LogWarning($"IncreaseDefense: currentLevel ({currentLevel})�� defenseIncreases �迭�� ������ ����ų� ������ 1 �����Դϴ�.");
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
            Debug.Log($"IncreaseDefense ���׷��̵�: ���� ���� {currentLevel + 1}");

            // ���� �� �� ���� ���� ����
            Player player = FindObjectOfType<Player>();
            if (player != null && currentLevel < defenseIncreases.Length)
            {
                player.stat.currentDefense += defenseIncreases[currentLevel];
                Debug.Log($"IncreaseDefense ���� {currentLevel + 1}���� ���� ����: {defenseIncreases[currentLevel]}");
            }
            else
            {
                Debug.LogWarning($"IncreaseDefense: �÷��̾ ã�� �� ���ų� currentLevel ({currentLevel})�� defenseIncreases �迭�� ������ ������ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("IncreaseDefense: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ��� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < defenseIncreases.Length)
        {
            int currentIncrease = defenseIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: ���� +{currentIncrease}";
        }
        else
        {
            Debug.LogWarning($"IncreaseDefense: currentLevel ({currentLevel})�� defenseIncreases �迭�� ������ ������ϴ�. �ִ� ���� ������ ��ȯ�մϴ�.");
            int finalIncrease = defenseIncreases[defenseIncreases.Length - 1];
            return $"{baseDescription}\nMax Level: ���� +{finalIncrease}";
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
        Debug.LogWarning($"IncreaseDefense: currentLevel ({currentLevel})�� defenseIncreases �迭�� ������ ������ϴ�. �⺻�� 0�� ��ȯ�մϴ�.");
        return 0;
    }
}
