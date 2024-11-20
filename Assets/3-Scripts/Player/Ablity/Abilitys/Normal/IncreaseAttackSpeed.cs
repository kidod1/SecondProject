using UnityEngine;
using TMPro;

[CreateAssetMenu(menuName = "Abilities/IncreaseAttackSpeed")]
public class IncreaseAttackSpeed : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("레벨별 공격 속도 증가량 (초당 공격 횟수 증가)")]
    public float[] attackSpeedIncrements;

    private Player playerInstance; // 플레이어 인스턴스 참조
    private float previousTotalIncrease = 0f; // 이전에 적용된 총 공격 속도 증가량

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        Debug.Log("IncreaseAttackSpeed Apply");
        if (player == null)
        {
            Debug.LogError("IncreaseAttackSpeed Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;

        // 현재 레벨까지의 총 공격 속도 증가량 계산
        float totalIncrease = GetTotalAttackSpeedIncrease();

        // 이전에 적용된 공격 속도 증가량을 제거
        player.stat.defaultAttackSpeed -= previousTotalIncrease;

        // 새로운 총 공격 속도 증가량을 적용
        player.stat.defaultAttackSpeed += totalIncrease;
        player.stat.currentAttackSpeed = player.stat.defaultAttackSpeed;

        // 이전 총 공격 속도 증가량 업데이트
        previousTotalIncrease = totalIncrease;
        Debug.Log($"IncreaseAttackSpeed: Applied total attack speed increase of {totalIncrease} at level {currentLevel}");
    }

    /// <summary>
    /// 능력을 업그레이드합니다.
    /// </summary>
    public override void Upgrade()
    {
        Debug.Log("IncreaseAttackSpeed Upgrade");

        if (playerInstance == null)
        {
            Debug.LogWarning("IncreaseAttackSpeed Upgrade: playerInstance가 null입니다.");
            return;
        }

        // 현재 레벨까지의 총 공격 속도 증가량 계산
        float totalIncrease = GetTotalAttackSpeedIncrease();

        // 이전에 적용된 공격 속도 증가량을 제거
        playerInstance.stat.defaultAttackSpeed -= previousTotalIncrease;

        // 새로운 총 공격 속도 증가량을 적용
        playerInstance.stat.defaultAttackSpeed += totalIncrease;
        playerInstance.stat.currentAttackSpeed = playerInstance.stat.defaultAttackSpeed;

        // 이전 총 공격 속도 증가량 업데이트
        previousTotalIncrease = totalIncrease;
    }

    /// <summary>
    /// 능력의 설명을 반환합니다.
    /// </summary>
    /// <returns>능력의 설명 문자열</returns>
    public override string GetDescription()
    {
        // 총 공격 속도 증가량 계산
        float totalIncrease = GetTotalAttackSpeedIncrease();

        if (currentLevel < attackSpeedIncrements.Length)
        {
            float nextIncrease = attackSpeedIncrements[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}:\n공격 속도 +{nextIncrease} 공격/초\n다음 레벨: 공격 속도 +{nextIncrease}";
        }
        else
        {
            float finalIncrease = attackSpeedIncrements.Length > 0 ? attackSpeedIncrements[attackSpeedIncrements.Length - 1] : 0f;
            return $"{baseDescription}\n최대 레벨 도달\n공격 속도 +{finalIncrease} 공격/초\n총 공격 속도 증가량: +{totalIncrease} 공격/초";
        }
    }

    /// <summary>
    /// 다음 레벨 증가에 필요한 값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨 증가 시 필요한 값</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < attackSpeedIncrements.Length)
        {
            return Mathf.RoundToInt(attackSpeedIncrements[currentLevel]);
        }
        else
        {
            Debug.LogWarning($"IncreaseAttackSpeed: currentLevel ({currentLevel})이 attackSpeedIncrements 배열의 범위를 벗어났습니다. 기본값 0을 반환합니다.");
            return 0;
        }
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        if (playerInstance != null)
        {
            playerInstance.stat.defaultAttackSpeed -= GetTotalAttackSpeedIncrease();
            playerInstance.stat.currentAttackSpeed = playerInstance.stat.defaultAttackSpeed;
        }
        currentLevel = 0;
        previousTotalIncrease = 0f;
    }

    /// <summary>
    /// 현재 레벨까지의 총 공격 속도 증가량을 반환합니다.
    /// </summary>
    /// <returns>총 공격 속도 증가량</returns>
    private float GetTotalAttackSpeedIncrease()
    {
        float totalIncrease = 0f;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < attackSpeedIncrements.Length)
                totalIncrease += attackSpeedIncrements[i];
        }
        return totalIncrease;
    }
}
