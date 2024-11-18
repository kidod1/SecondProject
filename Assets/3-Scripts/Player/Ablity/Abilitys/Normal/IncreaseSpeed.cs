using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseSpeed")]
public class IncreaseSpeed : Ability
{
    [Tooltip("각 레벨에서의 이동 속도 증가량 (예: 0.5f = 0.5 단위 증가)")]
    public float[] speedIncreases;

    private Player playerInstance; // 플레이어 인스턴스 참조
    private float previousTotalIncrease = 0f; // 이전에 적용된 총 속도 증가량

    // 플레이어에 어빌리티 적용
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("IncreaseSpeed Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;

        // 현재 레벨까지의 총 이동 속도 증가량 계산
        float totalIncrease = GetTotalSpeedIncrease();

        // 이전에 적용된 이동 속도 증가량을 제거
        player.stat.defaultPlayerSpeed -= previousTotalIncrease;

        // 새로운 총 이동 속도 증가량을 적용
        player.stat.defaultPlayerSpeed += totalIncrease;
        player.stat.currentPlayerSpeed = player.stat.defaultPlayerSpeed;

        // 이전 총 이동 속도 증가량 업데이트
        previousTotalIncrease = totalIncrease;
    }

    // 어빌리티 업그레이드
    public override void Upgrade()
    {
        if (playerInstance == null)
        {
            Debug.LogWarning("IncreaseSpeed Upgrade: playerInstance가 null입니다.");
            return;
        }

        // 현재 레벨까지의 총 이동 속도 증가량 계산
        float totalIncrease = GetTotalSpeedIncrease();

        // 이전에 적용된 이동 속도 증가량을 제거
        playerInstance.stat.defaultPlayerSpeed -= previousTotalIncrease;

        // 새로운 총 이동 속도 증가량을 적용
        playerInstance.stat.defaultPlayerSpeed += totalIncrease;
        playerInstance.stat.currentPlayerSpeed = playerInstance.stat.defaultPlayerSpeed;

        // 이전 총 이동 속도 증가량 업데이트
        previousTotalIncrease = totalIncrease;
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
        float totalIncrease = GetTotalSpeedIncrease();
        float totalSpeedIncrease = Mathf.Round(totalIncrease * 100f) / 100f; // 소수점 두 자리까지 반올림

        if (currentLevel < maxLevel && currentLevel < speedIncreases.Length)
        {
            float speedIncrease = speedIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 이동 속도 +{speedIncrease}\n지금까지 총 {totalSpeedIncrease} 상승";
        }
        else if (currentLevel == maxLevel && currentLevel < speedIncreases.Length)
        {
            float speedIncrease = speedIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 이동 속도 +{speedIncrease}\n지금까지 총 {totalSpeedIncrease} 상승\n최대 레벨 도달";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달\n총 {totalSpeedIncrease} 단위 상승";
        }
    }

    // 어빌리티 레벨 초기화
    public override void ResetLevel()
    {
        if (playerInstance != null)
        {
            playerInstance.stat.defaultPlayerSpeed -= previousTotalIncrease;
            playerInstance.stat.currentPlayerSpeed = playerInstance.stat.defaultPlayerSpeed;
        }
        currentLevel = 0;
        previousTotalIncrease = 0f;
    }

    /// <summary>
    /// 현재 레벨까지의 총 이동 속도 증가량을 반환합니다.
    /// </summary>
    /// <returns>총 이동 속도 증가량</returns>
    private float GetTotalSpeedIncrease()
    {
        float totalIncrease = 0f;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < speedIncreases.Length)
                totalIncrease += speedIncreases[i];
        }
        return totalIncrease;
    }
}
