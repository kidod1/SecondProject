using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseExperience")]
public class IncreaseExperience : Ability
{
    [Tooltip("�� �������� ����ġ ȹ�淮 ���� ��� (��: 0.1 = 10% ����)")]
    public float[] experienceMultipliers;

    public override void Apply(Player player)
    {
        if (currentLevel > 0)
        {
            player.stat.experienceMultiplier += experienceMultipliers[currentLevel];
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel)
        {
            return Mathf.RoundToInt(experienceMultipliers[currentLevel] * 100); // �ۼ�Ʈ�� ��ȯ
        }
        return 0;
    }

    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            return $"{baseDescription}{Environment.NewLine}(Level {currentLevel + 1}: +{Mathf.RoundToInt(experienceMultipliers[currentLevel] * 100)}% ����ġ ȹ��)";
        }
        else
        {
            return $"{baseDescription}{Environment.NewLine}(Max Level)";
        }
    }
}
