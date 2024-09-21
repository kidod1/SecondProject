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
            lastUsedTime = Time.time; // ��Ÿ�� ����
            Apply(player);
            Debug.Log("�ɷ� ���� �Ϸ�, ��Ÿ�� ����");
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
