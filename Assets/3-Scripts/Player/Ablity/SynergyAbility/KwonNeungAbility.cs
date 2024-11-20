using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/KwonNeungAbility")]
public class KwonNeungAbility : SynergyAbility
{
    [Header("권능 파라미터")]
    [InspectorName("스턴 시간")]
    public float stunDuration = 5f;

    [Header("소환할 이펙트 프리팹")]
    [Tooltip("능력 사용 시 소환될 게임 오브젝트 프리팹")]
    public GameObject effectPrefab;

    private Player playerInstance;

    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;

        // 이펙트 소환
        SpawnEffect();

        // 몬스터 스턴
        StunMonsters();
    }

    private void SpawnEffect()
    {
        if (effectPrefab != null && playerInstance != null)
        {
            // 플레이어 위치에 이펙트 생성
            GameObject effectInstance = Instantiate(effectPrefab, playerInstance.transform.position, Quaternion.identity);

            // 3초 후에 이펙트 파괴
            Destroy(effectInstance, 3f);
        }
        else
        {
            Debug.LogWarning("Effect Prefab이 할당되지 않았거나, playerInstance가 없습니다.");
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
        // 업그레이드 로직 추가
    }

    public override void ResetLevel()
    {
        lastUsedTime = 0;
    }
}
