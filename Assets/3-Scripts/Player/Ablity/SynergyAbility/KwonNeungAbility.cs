using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/KwonNeungAbility")]
public class KwonNeungAbility : SynergyAbility
{
    [Header("권능 파라미터")]
    [InspectorName("스턴 시간")]
    public float stunDuration = 5f;

    private Player playerInstance;

    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;

        StunMonsters();
    }

    private void StunMonsters()
    {
        Monster[] monsters = GameObject.FindObjectsOfType<Monster>();

        foreach (Monster monster in monsters)
        {
            monster.Stun(stunDuration);
        }
    }

    public override void Upgrade()
    {
        // 업그레이드 로직 추가
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
    }
}
