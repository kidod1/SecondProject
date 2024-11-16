using UnityEngine;
using UnityEngine.Events;
using AK.Wwise; // WWISE 네임스페이스 추가

[CreateAssetMenu(menuName = "Abilities/FieryBloodToastAbility")]
public class FieryBloodToastAbility : Ability
{
    [Header("Damage Multiplier Parameters")]
    [Tooltip("레벨별 최대 공격력 배율")]
    public float[] damageMultipliers = { 1.5f, 1.75f, 2.0f }; // 예: 레벨 1~3

    [Header("WWISE Sound Events")]
    [Tooltip("FieryBloodToastAbility 능력 발동 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event activateSound;

    private PlayerData playerData;
    private string buffIdentifier;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("FieryBloodToastAbility Apply: player 인스턴스가 null입니다.");
            return;
        }

        if (currentLevel == 0)
        {
            playerData = player.stat; // PlayerData 참조
            buffIdentifier = this.name;

            player.OnTakeDamage.AddListener(UpdateDamage);
            player.OnHeal.AddListener(UpdateDamage); // 회복 시에도 업데이트

            // 능력 적용 시 즉시 공격력 업데이트
            UpdateDamage();

            // FieryBloodToastAbility 능력 발동 시 WWISE 사운드 재생
            if (activateSound != null)
            {
                activateSound.Post(player.gameObject);
            }
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            Debug.Log($"FieryBloodToastAbility 업그레이드: 현재 레벨 {currentLevel + 1}");
            UpdateDamage();

            // 업그레이드 시 WWISE 사운드 재생
            if (activateSound != null)
            {
                activateSound.Post(PlayManager.I.GetPlayer().gameObject);
            }
        }
        else
        {
            Debug.LogWarning("FieryBloodToastAbility: 이미 최대 레벨에 도달했습니다.");
        }
    }

    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";

        description += $"Lv {currentLevel + 1}:\n";
        description += $"체력이 낮을수록 공격력 증가 (배율: x{damageMultipliers[currentLevel]:F2})\n";

        return description;
    }

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
    /// 현재 체력에 따라 데미지 배율을 반환합니다.
    /// </summary>
    public float GetDamageMultiplier()
    {
        if (playerData == null)
            return 1f;

        float healthPercentage = (float)playerData.currentHP / playerData.currentMaxHP;
        // 체력이 낮을수록 높은 배율을 적용
        float maxMultiplier = damageMultipliers[Mathf.Min(currentLevel, damageMultipliers.Length - 1)];
        float damageMultiplier = Mathf.Lerp(1f, maxMultiplier, 1f - healthPercentage);
        return damageMultiplier;
    }

    private void UpdateDamage()
    {
        if (playerData == null)
            return;

        float damageMultiplier = GetDamageMultiplier();
        float additionalDamage = playerData.defaultPlayerDamage * (damageMultiplier - 1f);

        // 기존 버프 제거
        playerData.RemoveBuff(buffIdentifier, BuffType.AttackDamage);

        // 새로운 버프 적용
        playerData.AddBuff(buffIdentifier, BuffType.AttackDamage, additionalDamage);
    }

    public void RemoveAbility()
    {
        if (playerData != null)
        {
            playerData.RemoveBuff(buffIdentifier, BuffType.AttackDamage);
        }
        currentLevel = 0;
    }
    public override void ResetLevel()
    {
        currentLevel = 0;
    }
}
