using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/SynergyAbility")]
public class SynergyAbility : Ability
{
    public float cooldownDuration;
    private float lastUsedTime = 0;

    public bool IsReady => Time.time >= lastUsedTime + cooldownDuration;

    public virtual void Activate(Player player)
    {
        Debug.Log(IsReady);
        Debug.Log(lastUsedTime);
        Debug.Log(cooldownDuration);
        if (IsReady)
        {
            lastUsedTime = Time.time; // 쿨타임 시작
            Apply(player);
            Debug.Log("능력 적용 완료, 쿨타임 시작");
        }
        else
        {
            float remainingCooldown = (lastUsedTime + cooldownDuration) - Time.time;
            Debug.Log($"Ability {abilityName} is on cooldown. Remaining: {remainingCooldown:F2} seconds.");
        }
    }
    public override void Apply(Player player)
    {
    }

    public override void Upgrade()
    {
    }

    public override string GetDescription()
    {
        return baseDescription;
    }

    protected override int GetNextLevelIncrease()
    {
        return 0;
    }
}
