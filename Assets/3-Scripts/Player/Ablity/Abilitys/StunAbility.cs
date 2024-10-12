using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/StunAbility")]
public class StunAbility : Ability
{
    [Tooltip("������ ���� Ȯ�� (0.0f ~ 1.0f)")]
    [Range(0f, 1f)]
    public float[] stunChances;   // ������ ���� Ȯ�� �迭

    public float stunDuration = 2f;   // ���� ���� �ð�

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
    /// ���Ϳ��� ������ �õ��մϴ�.
    /// </summary>
    /// <param name="monster">������ �õ��� ����</param>
    public void TryStun(Monster monster)
    {
        float currentStunChance = GetCurrentStunChance();
        float randomValue = Random.value;
        if (randomValue < currentStunChance) // ���� Ȯ�� üũ
        {
            monster.Stun(stunDuration); // ���� ������Ű��
        }
        else
        {
            Debug.Log($"{monster.name} resisted the stun.");
        }
    }

    /// <summary>
    /// ���� ������ ���� ���� Ȯ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ���� Ȯ�� (0.0f ~ 1.0f)</returns>
    private float GetCurrentStunChance()
    {
        if (currentLevel < stunChances.Length)
        {
            return stunChances[currentLevel];
        }
        else
        {
            Debug.LogWarning($"StunAbility: currentLevel ({currentLevel}) exceeds stunChances �迭 ����. ������ ������ ���� Ȯ���� ����մϴ�.");
            return stunChances[stunChances.Length - 1];
        }
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ���� Ȯ���� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel�� 5��� currentLevel�� 0~4
        {
            currentLevel++;
            Debug.Log($"StunAbility upgraded to Level {currentLevel + 1}. ���� Ȯ��: {stunChances[currentLevel] * 100}%");
        }
        else
        {
            Debug.LogWarning("StunAbility: Already at max level.");
        }
    }

    /// <summary>
    /// ���� ������ ���� Ȯ�� �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���������� ���� Ȯ�� ������ (�ۼ�Ʈ)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < stunChances.Length)
        {
            return Mathf.RoundToInt(stunChances[currentLevel + 1] * 100); // �ۼ�Ʈ�� ��ȯ
        }
        return 0;
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        currentLevel = 0;
    }

    /// <summary>
    /// �ɷ��� ���� ���¿� ȿ���� �����ϴ� ���ڿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {

        if (currentLevel < stunChances.Length && currentLevel >= 0)
        {
            float stunChancePercent = stunChances[currentLevel] * 100f;
            return $"{baseDescription}\nLv {currentLevel + 1}: ���� ������ �� {stunChancePercent}% Ȯ���� {stunDuration}�� ���� ����";
        }
        else if (currentLevel >= stunChances.Length)
        {
            float maxStunChancePercent = stunChances[stunChances.Length - 1] * 100f;
            return $"{baseDescription}\n�ִ� ���� ����: ���� ������ �� {maxStunChancePercent}% Ȯ���� {stunDuration}�� ���� ����";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����.";
        }
    }
}
