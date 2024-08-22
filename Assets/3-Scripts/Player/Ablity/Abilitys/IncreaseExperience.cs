using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseExperience")]
public class IncreaseExperience : Ability
{
    [Tooltip("각 레벨에서 경험치 획득량 증가 배수 (예: 0.1 = 10% 증가)")]
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
            return Mathf.RoundToInt(experienceMultipliers[currentLevel] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            return $"{baseDescription}{Environment.NewLine}(Level {currentLevel + 1}: +{Mathf.RoundToInt(experienceMultipliers[currentLevel] * 100)}% 경험치 획득)";
        }
        else
        {
            return $"{baseDescription}{Environment.NewLine}(Max Level)";
        }
    }
}
