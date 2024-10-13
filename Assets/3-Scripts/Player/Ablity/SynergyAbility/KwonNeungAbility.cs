using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/KwonNeungAbility")]
public class KwonNeungAbility : SynergyAbility
{
    [Header("�Ǵ� �Ķ����")]
    [InspectorName("���� �ð�")]
    public float stunDuration = 5f;

    [InspectorName("��ٿ� �ð�")]
    public float abilityCooldown = 15f;

    private Player playerInstance;

    private void OnEnable()
    {
        cooldownDuration = abilityCooldown;
    }

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
        // ���׷��̵� ���� �߰�
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
    }
}
