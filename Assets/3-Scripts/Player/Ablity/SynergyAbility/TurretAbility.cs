using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/SentryAbility")]
public class SentryAbility : SynergyAbility
{
    [Header("Sentry Parameters")]
    public GameObject sentryPrefab;         // 센트리 프리팹
    public float abilityDuration = 10f;     // 센트리의 지속 시간
    public float abilityCooldown = 15f;     // 능력의 쿨다운 시간
    public int sentryDamage = 20;           // 센트리의 공격력
    public float sentryAttackSpeed = 1f;    // 센트리의 공격 속도

    private Player playerInstance;

    private void OnEnable()
    {
        // 쿨다운 시간을 설정
        cooldownDuration = abilityCooldown;
    }

    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        // 센트리 생성
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


        // 센트리 생성
        GameObject sentryObject = Instantiate(sentryPrefab, spawnPosition, Quaternion.identity);

        // 센트리 스크립트 초기화
        Sentry sentryScript = sentryObject.GetComponent<Sentry>();
        if (sentryScript != null)
        {
            // PlayerData와 Player 인스턴스를 전달
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
        // 예: 피해량 증가, 공격 속도 증가 등
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
    }
}
