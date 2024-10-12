using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/KwonNeungAbility")]
public class KwonNeungAbility : SynergyAbility
{
    [Header("KwonNeung Parameters")]
    public float stunDuration = 5f;       // ���� ���� �ð�
    public float abilityCooldown = 15f;   // �ɷ��� ��ٿ� �ð�

    private Player playerInstance;

    private void OnEnable()
    {
        // ��ٿ� �ð��� ����
        cooldownDuration = abilityCooldown;
    }

    public override void Apply(Player player)
    {
        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        // ���͵鿡�� ������ ����
        StunMonsters();
    }

    private void StunMonsters()
    {
        // ��� ���͸� ã��
        Monster[] monsters = GameObject.FindObjectsOfType<Monster>();

        foreach (Monster monster in monsters)
        {
            monster.Stun(stunDuration);
        }
    }

    public override void Upgrade()
    {
        // ���׷��̵� ������ ���⿡ �߰��� �� �ֽ��ϴ�.
        // ��: ���� �ð� ����, ��ٿ� ���� ��
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
    }
}
