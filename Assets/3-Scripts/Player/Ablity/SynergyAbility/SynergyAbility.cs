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

    public UnityEvent OnCooldownComplete; // ��Ÿ�� �Ϸ� �̺�Ʈ

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

        // ��Ÿ�� �Ϸ� �� �̺�Ʈ ȣ��
        OnCooldownComplete?.Invoke();
        Debug.Log($"Ability {abilityName} cooldown complete.");
    }

    public override void Apply(Player player)
    {
    }

    public override void Upgrade()
    {
        // �����Ƽ ������ ���� ����
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
