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
            player.stat.currentPlayerSpeed += speedIncreases[currentLevel - 1];
        }
    }

    // 어빌리티 업그레이드
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
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
        if (currentLevel < maxLevel && currentLevel < speedIncreases.Length)
        {
            float speedIncrease = speedIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 이동 속도 +{speedIncrease} 단위";
        }
        else if (currentLevel == maxLevel && currentLevel < speedIncreases.Length)
        {
            float speedIncrease = speedIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 이동 속도 +{speedIncrease} 단위\n최대 레벨 도달";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달";
        }
    }

    // 어빌리티 레벨 초기화
    public override void ResetLevel()
    {
        base.ResetLevel();
        // 필요 시 플레이어의 속도 초기화 로직 추가
        // 예: player.stat.currentPlayerSpeed -= speedIncreases[currentLevel - 1];
    }
}
