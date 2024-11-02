using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseAttackSpeed")]
public class IncreaseAttackSpeed : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("������ ���� �ӵ� ������ (�ʴ� ���� Ƚ�� ����)")]
    public float[] attackSpeedIncrements;

    private Player playerInstance;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("IncreaseAttackSpeed Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }
        playerInstance = player;

        ApplyAttackSpeedIncrease();
        // currentLevel ���� �κ� ����
    }

    public override void Upgrade()
    {
        if (currentLevel <= maxLevel)
        {
            ApplyAttackSpeedIncrease();
            // currentLevel ���� �κ� ����
        }
    }

    private void ApplyAttackSpeedIncrease()
    {
        int levelIndex = Mathf.Clamp(currentLevel - 1, 0, attackSpeedIncrements.Length - 1);

        float increment = attackSpeedIncrements[levelIndex];
        playerInstance.stat.defaultAttackSpeed += increment;
        playerInstance.stat.currentAttackSpeed += increment;
    }

    public override string GetDescription()
    {
        // �� ���� �ӵ� ������ ���
        float totalIncrement = 0f;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < attackSpeedIncrements.Length)
                totalIncrement += attackSpeedIncrements[i];
        }

        if (currentLevel < attackSpeedIncrements.Length)
        {
            float currentIncrement = attackSpeedIncrements[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: ���� �ӵ� {currentIncrement} ����\n���ݱ��� �� {totalIncrement} ����";
        }
        else
        {
            float finalIncrement = attackSpeedIncrements.Length > 0 ? attackSpeedIncrements[attackSpeedIncrements.Length - 1] : 0f;
            return $"{baseDescription}\nMax Level: ���� �ӵ� {finalIncrement} ����\n���ݱ��� �� {totalIncrement} ����";
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < attackSpeedIncrements.Length)
        {
            return Mathf.RoundToInt(attackSpeedIncrements[currentLevel]);
        }
        Debug.LogWarning($"IncreaseAttackSpeed: currentLevel ({currentLevel})�� attackSpeedIncrements �迭�� ������ ������ϴ�. �⺻�� 0�� ��ȯ�մϴ�.");
        return 0;
    }
}
