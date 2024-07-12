using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SynergyAbility")]
public class SynergyAbility : Ability
{
    public override void Apply(Player player)
    {
        // 시너지 능력 적용 로직
    }

    public override void Upgrade()
    {
        // 시너지 능력은 업그레이드 없음
    }

    public override string GetDescription()
    {
        return baseDescription; // 시너지 능력은 레벨에 따른 설명 변경 없음
    }

    protected override int GetNextLevelIncrease()
    {
        return 0; // 시너지 능력에는 적용되지 않음
    }
}
