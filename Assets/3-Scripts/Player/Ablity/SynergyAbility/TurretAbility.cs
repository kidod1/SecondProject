using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/SentryAbility")]
public class SentryAbility : SynergyAbility
{
    [Header("Sentry Parameters")]
    public GameObject sentryPrefab;         // ��Ʈ�� ������
    public float abilityDuration = 10f;     // ��Ʈ���� ���� �ð�
    public float abilityCooldown = 15f;     // �ɷ��� ��ٿ� �ð�
    public int sentryDamage = 20;           // ��Ʈ���� ���ݷ�
    public float sentryAttackSpeed = 1f;    // ��Ʈ���� ���� �ӵ�

    private Player playerInstance;

    private void OnEnable()
    {
        // ��ٿ� �ð��� ����
        cooldownDuration = abilityCooldown;
    }

    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        // ��Ʈ�� ����
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


        // ��Ʈ�� ����
        GameObject sentryObject = Instantiate(sentryPrefab, spawnPosition, Quaternion.identity);

        // ��Ʈ�� ��ũ��Ʈ �ʱ�ȭ
        Sentry sentryScript = sentryObject.GetComponent<Sentry>();
        if (sentryScript != null)
        {
            // PlayerData�� Player �ν��Ͻ��� ����
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
        // ��: ���ط� ����, ���� �ӵ� ���� ��
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
    }
}
