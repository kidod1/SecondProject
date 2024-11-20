using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/KwonNeungAbility")]
public class KwonNeungAbility : SynergyAbility
{
    [Header("�Ǵ� �Ķ����")]
    [InspectorName("���� �ð�")]
    public float stunDuration = 5f;

    [Header("��ȯ�� ����Ʈ ������")]
    [Tooltip("�ɷ� ��� �� ��ȯ�� ���� ������Ʈ ������")]
    public GameObject effectPrefab;

    private Player playerInstance;

    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;

        // ����Ʈ ��ȯ
        SpawnEffect();

        // ���� ����
        StunMonsters();
    }

    private void SpawnEffect()
    {
        if (effectPrefab != null && playerInstance != null)
        {
            // �÷��̾� ��ġ�� ����Ʈ ����
            GameObject effectInstance = Instantiate(effectPrefab, playerInstance.transform.position, Quaternion.identity);

            // 3�� �Ŀ� ����Ʈ �ı�
            Destroy(effectInstance, 3f);
        }
        else
        {
            Debug.LogWarning("Effect Prefab�� �Ҵ���� �ʾҰų�, playerInstance�� �����ϴ�.");
        }
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
        lastUsedTime = 0;
    }
}
