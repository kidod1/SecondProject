using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseSpeed")]
public class IncreaseSpeed : Ability
{
    [Tooltip("각 레벨에서의 이동 속도 증가량 (예: 0.5f = 0.5 단위 증가)")]
    public float[] speedIncreases;

    // 플레이어에 어빌리티 적용
    public override void Apply(Player player)
    {
        if (currentLevel < speedIncreases.Length)
        {
            PlayManager.I.GetPlayer().stat.currentPlayerSpeed += speedIncreases[currentLevel];
        }
    }
    // 어빌리티 업그레이드
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel이 5일 경우, currentLevel은 0~4
        {
            currentLevel++;

            // 레벨 업 시 이동 속도 증가 적용
            Player player = FindObjectOfType<Player>();
            if (player != null && currentLevel < speedIncreases.Length)
            {
                player.stat.currentPlayerSpeed += speedIncreases[currentLevel - 1];
            }
        }
        else
        {
            Debug.LogWarning("IncreaseSpeed: 이미 최대 레벨에 도달했습니다.");
        }
    }

    // 다음 레벨 증가값 반환
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel && currentLevel < speedIncreases.Length)
        {
            return Mathf.RoundToInt(speedIncreases[currentLevel] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    // 능력의 설명을 반환
    public override string GetDescription()
    {
        // 총 이동 속도 증가량 계산
        float totalIncrease = 0f;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < speedIncreases.Length)
                totalIncrease += speedIncreases[i];
        }
        float totalSpeedIncrease = Mathf.Round(totalIncrease * 100f) / 100f; // 소수점 두 자리까지 반올림

        if (currentLevel < maxLevel && currentLevel < speedIncreases.Length)
        {
            float speedIncrease = speedIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 이동 속도 +{speedIncrease} 단위\n지금까지 총 {totalSpeedIncrease} 단위 상승";
        }
        else if (currentLevel == maxLevel && currentLevel < speedIncreases.Length)
        {
            float speedIncrease = speedIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 이동 속도 +{speedIncrease} 단위\n지금까지 총 {totalSpeedIncrease} 단위 상승\n최대 레벨 도달";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달\n총 {totalSpeedIncrease} 단위 상승";
        }
    }

    // 어빌리티 레벨 초기화
    public override void ResetLevel()
    {
        base.ResetLevel();
    }
}
