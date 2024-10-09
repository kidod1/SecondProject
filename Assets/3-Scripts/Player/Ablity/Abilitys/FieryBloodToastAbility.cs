using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/FieryBloodToastAbility")]
public class FieryBloodToastAbility : Ability
{
    [Header("Damage Multiplier Parameters")]
    [Tooltip("레벨별 최대 공격력 배율")]
    public float[] damageMultipliers = { 1.5f, 1.75f, 2.0f }; // 예: 레벨 1~3

    private Player playerInstance;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("FieryBloodToastAbility Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;
        playerInstance.OnTakeDamage.AddListener(UpdateDamage);
        playerInstance.OnHeal.AddListener(UpdateDamage); // 회복 시에도 업데이트

        // 능력 적용 시 즉시 공격력 업데이트
        UpdateDamage();
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 데미지 배율이 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            Debug.Log($"FieryBloodToastAbility 업그레이드: 현재 레벨 {currentLevel}");

            // 레벨 업 시 데미지 배율 업데이트
            UpdateDamage();
        }
        else
        {
            Debug.LogWarning("FieryBloodToastAbility: 이미 최대 레벨에 도달했습니다.");
        }
    }
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            float damageMultiplier = damageMultipliers[currentLevel];
            return $"{baseDescription}{Environment.NewLine}(Level {currentLevel + 1}: x{damageMultiplier} 공격력)";
        }
        else
        {
            float finalDamageMultiplier = damageMultipliers[currentLevel];
            return $"{baseDescription}{Environment.NewLine}(Max Level: x{finalDamageMultiplier} 공격력)";
        }
    }

    /// <summary>
    /// 다음 레벨 증가에 필요한 값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨 증가 시 필요한 값</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < damageMultipliers.Length)
        {
            return Mathf.RoundToInt(damageMultipliers[currentLevel]);
        }
        Debug.LogWarning($"FieryBloodToastAbility: currentLevel ({currentLevel})이 damageMultipliers 배열의 범위를 벗어났습니다. 기본값 1을 반환합니다.");
        return 1;
    }

    /// <summary>
    /// 플레이어의 공격력을 업데이트합니다.
    /// </summary>
    private void UpdateDamage()
    {
        if (playerInstance == null || playerInstance.stat == null)
        {
            Debug.LogWarning("FieryBloodToastAbility UpdateDamage: playerInstance 또는 playerInstance.stat이 null입니다.");
            return;
        }

        float healthPercentage = (float)playerInstance.stat.currentHP / playerInstance.stat.currentMaxHP;
        float damageMultiplier = GetDamageMultiplier();

        // basePlayerDamage가 PlayerData에 정의되어 있어야 합니다.
        playerInstance.stat.currentPlayerDamage = Mathf.RoundToInt(playerInstance.stat.defaultPlayerDamage * damageMultiplier);

        Debug.Log($"FieryBloodToastAbility: 플레이어의 현재 데미지 배율이 x{damageMultiplier}로 업데이트되었습니다.");
    }

    /// <summary>
    /// 플레이어의 현재 체력 비율에 따라 데미지 배율을 반환합니다.
    /// </summary>
    /// <returns>계산된 데미지 배율</returns>
    public float GetDamageMultiplier()
    {
        if (playerInstance == null || playerInstance.stat == null)
        {
            return 1f;
        }

        float healthPercentage = (float)playerInstance.stat.currentHP / playerInstance.stat.currentMaxHP;
        // 체력이 낮을수록 높은 배율을 적용
        float damageMultiplier = Mathf.Lerp(damageMultipliers[0], damageMultipliers[Mathf.Min(currentLevel, damageMultipliers.Length - 1)], 1f - healthPercentage);
        return damageMultiplier;
    }

    /// <summary>
    /// 능력을 제거합니다.
    /// </summary>
    public void RemoveAbility()
    {
        if (playerInstance != null)
        {
            playerInstance.OnTakeDamage.RemoveListener(UpdateDamage);
            playerInstance.OnHeal.RemoveListener(UpdateDamage);
        }
        currentLevel = 0;
    }
    
}
