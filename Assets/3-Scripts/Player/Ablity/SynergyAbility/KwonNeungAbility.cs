using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/KwonNeungAbility")]
public class KwonNeungAbility : SynergyAbility
{
    [Header("KwonNeung Parameters")]
    public float stunDuration = 5f;       // 몬스터 스턴 시간
    public float abilityCooldown = 15f;   // 능력의 쿨다운 시간

    private Player playerInstance;

    private void OnEnable()
    {
        // 쿨다운 시간을 설정
        cooldownDuration = abilityCooldown;
    }

    public override void Apply(Player player)
    {
        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        // 몬스터들에게 스턴을 걸음
        StunMonsters();
    }

    private void StunMonsters()
    {
        // 모든 몬스터를 찾음
        Monster[] monsters = GameObject.FindObjectsOfType<Monster>();

        foreach (Monster monster in monsters)
        {
            monster.Stun(stunDuration);
        }
    }

    public override void Upgrade()
    {
        // 업그레이드 로직을 여기에 추가할 수 있습니다.
        // 예: 스턴 시간 증가, 쿨다운 감소 등
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
    }
}
