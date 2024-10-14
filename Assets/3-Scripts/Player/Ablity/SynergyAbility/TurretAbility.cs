using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/SentryAbility")]
public class SentryAbility : SynergyAbility
{
    [Header("��ž ��ȯ �Ķ����")]
    [InspectorName("��Ʈ�� ������")]
    public GameObject sentryPrefab;

    [InspectorName("��Ʈ�� ���� �ð�")]
    public float abilityDuration = 10f;

    [InspectorName("��ٿ� �ð�")]
    public float abilityCooldown = 15f;

    [InspectorName("��Ʈ�� ���ݷ�")]
    public int sentryDamage = 20;

    [InspectorName("��Ʈ�� ���� �ӵ�")]
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
        // ���׷��̵� ������ ���⿡ �߰��� �� �ֽ��ϴ�.
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
    }
}
