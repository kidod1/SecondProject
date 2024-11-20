using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using AK.Wwise;
[CreateAssetMenu(menuName = "ActiveAbilities/SynergyAbility")]
public class SynergyAbility : Ability
{
    public float cooldownDuration;
    public float lastUsedTime = 0;

    public AK.Wwise.Event ActiveSound;
    public bool IsReady => Time.time >= lastUsedTime + cooldownDuration;

    public UnityEvent OnCooldownComplete; // 쿨타임 완료 이벤트

    public virtual void Activate(Player player)
    {
        if (IsReady)
        {
            lastUsedTime = Time.time;
            Apply(player);
            player.StartCoroutine(HandleCooldown());
            ActiveSound.Post(PlayManager.I.GetPlayer().gameObject);
        }
        else
        {
            float remainingCooldown = (lastUsedTime + cooldownDuration) - Time.time;
        }
    }

    private IEnumerator HandleCooldown()
    {
        float elapsed = 0f;
        while (elapsed < cooldownDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 쿨타임 완료 시 이벤트 호출
        OnCooldownComplete?.Invoke();
        Debug.Log($"Ability {abilityName} cooldown complete.");
    }

    public override void Apply(Player player)
    {
    }

    public override void Upgrade()
    {
        // 어빌리티 레벨업 로직 구현
    }

    public override string GetDescription()
    {
        return baseDescription + $" (Cooldown: {cooldownDuration} seconds)";
    }

    protected override int GetNextLevelIncrease()
    {
        return 0;
    }

    public override void ResetLevel()
    {
        lastUsedTime = 0;
    }
}
