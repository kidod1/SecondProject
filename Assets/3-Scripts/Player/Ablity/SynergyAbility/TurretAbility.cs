using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/SentryAbility")]
public class SentryAbility : SynergyAbility
{
    [Header("포탑 소환 파라미터")]
    [InspectorName("센트리 프리팹")]
    public GameObject sentryPrefab;

    [InspectorName("센트리 지속 시간")]
    public float abilityDuration = 10f;

    [InspectorName("쿨다운 시간")]
    public float abilityCooldown = 15f;

    [InspectorName("센트리 공격력")]
    public int sentryDamage = 20;

    [InspectorName("센트리 공격 속도")]
    public float sentryAttackSpeed = 1f;

    private Player playerInstance;

    private void OnEnable()
    {
        cooldownDuration = abilityCooldown;
    }

    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        SpawnSentry();
    }

    private void SpawnSentry()
    {
        if (sentryPrefab == null)
        {
            Debug.LogError("Sentry prefab is not assigned.");
            return;
        }

        Vector3 forwardDirection = GetPlayerForwardDirection();
        Vector3 spawnPosition = playerInstance.transform.position + forwardDirection * 1.5f;

        GameObject sentryObject = Instantiate(sentryPrefab, spawnPosition, Quaternion.identity);

        Sentry sentryScript = sentryObject.GetComponent<Sentry>();
        if (sentryScript != null)
        {
            sentryScript.Initialize(sentryDamage, sentryAttackSpeed, abilityDuration, playerInstance.stat, playerInstance);
        }
        else
        {
            Debug.LogError("Sentry prefab is missing Sentry component.");
        }
    }

    private Vector3 GetPlayerForwardDirection()
    {
        Vector2 forwardDirection = playerInstance.GetFacingDirection();
        return forwardDirection.normalized;
    }

    public override void Upgrade()
    {
        // 업그레이드 로직을 여기에 추가할 수 있습니다.
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
    }
}
