using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseExperience")]
public class IncreaseExperience : Ability
{
    [Tooltip("각 레벨에서 경험치 획득량 증가 배수 (예: 0.1 = 10% 증가)")]
    public float[] experienceMultipliers;

    public override void Apply(Player player)
    {
        if (currentLevel < experienceMultipliers.Length)
        {
            player.stat.experienceMultiplier += experienceMultipliers[currentLevel];
        }
        if (player == null) return;
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;

            Player player = FindObjectOfType<Player>();
            if (player != null && currentLevel < experienceMultipliers.Length)
            {
                player.stat.experienceMultiplier += experienceMultipliers[currentLevel - 1];
            }
        }
        else
        {
            Debug.LogWarning("IncreaseExperience: 이미 최대 레벨에 도달했습니다.");
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
        float totalIncrease = 0f;
        for (int i = 0; i < currentLevel; i++)
        {
            totalIncrease += experienceMultipliers[i];
        }
        int totalPercentIncrease = Mathf.RoundToInt(totalIncrease * 100);

        if (currentLevel < maxLevel && currentLevel < experienceMultipliers.Length)
        {
            int percentIncrease = Mathf.RoundToInt(experienceMultipliers[currentLevel] * 100);
            return $"{baseDescription}\n레벨 {currentLevel + 1}: 경험치 획득 +{percentIncrease}%\n현재까지 총 {totalPercentIncrease}% 상승";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달\n총 {totalPercentIncrease}% 상승";
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        // 필요 시 플레이어의 경험치 배수 초기화 로직 추가
    }
}
