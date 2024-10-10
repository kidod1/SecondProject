using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseExperience")]
public class IncreaseExperience : Ability
{
    [Tooltip("각 레벨에서 경험치 획득량 증가 배수 (예: 0.1 = 10% 증가)")]
    public float[] experienceMultipliers;

    public override void Apply(Player player)
    {
        if (currentLevel > 0 && currentLevel - 1 < experienceMultipliers.Length)
        {
            player.stat.experienceMultiplier += experienceMultipliers[currentLevel - 1];
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            Debug.Log($"IncreaseExperience 업그레이드: 현재 레벨 {currentLevel}");
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel && currentLevel < experienceMultipliers.Length)
        {
            return Mathf.RoundToInt(experienceMultipliers[currentLevel] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    public override string GetDescription()
    {
        if (currentLevel < maxLevel && currentLevel < experienceMultipliers.Length)
        {
            int percentIncrease = Mathf.RoundToInt(experienceMultipliers[currentLevel] * 100);
            return $"{baseDescription}\n레벨 {currentLevel + 1}: 경험치 획득 +{percentIncrease}%";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달";
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        // 필요 시 플레이어의 경험치 배수 초기화 로직 추가
    }
}
