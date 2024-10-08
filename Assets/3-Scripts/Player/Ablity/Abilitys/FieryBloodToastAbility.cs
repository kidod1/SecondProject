using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/FieryBloodToastAbility")]
public class FieryBloodToastAbility : Ability
{
    public float maxDamageMultiplier = 2.0f; // 최대 공격력 배율

    private Player playerInstance;

    public override void Apply(Player player)
    {
        playerInstance = player;
        playerInstance.OnTakeDamage.AddListener(UpdateDamage);
        playerInstance.OnHeal.AddListener(UpdateDamage); // 회복 시에도 업데이트

        // 능력 적용 시 즉시 공격력 업데이트
        UpdateDamage();
    }

    public override void Upgrade()
    {
        // 업그레이드 로직 추가 가능
    }

    public override string GetDescription()
    {
        return "생명력이 적을수록 주는 피해량이 증가한다.";
    }

    protected override int GetNextLevelIncrease()
    {
        // 필요에 따라 구현
        return 0;
    }

    private void UpdateDamage()
    {
        // 내부에서 플레이어의 공격력을 업데이트합니다.
        // 필요에 따라 구현
    }

    public float GetDamageMultiplier()
    {
        // 플레이어의 현재 체력 비율에 따라 데미지 배율을 반환합니다.
        if (playerInstance == null || playerInstance.stat == null)
        {
            return 1f;
        }

        float healthPercentage = (float)playerInstance.stat.currentHP / playerInstance.stat.currentMaxHP;
        float damageMultiplier = Mathf.Lerp(maxDamageMultiplier, 1f, healthPercentage);
        return damageMultiplier;
    }

    public void RemoveAbility()
    {
        if (playerInstance != null)
        {
            playerInstance.OnTakeDamage.RemoveListener(UpdateDamage);
            playerInstance.OnHeal.RemoveListener(UpdateDamage);
        }
    }
}
