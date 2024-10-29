using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/MeteorSynergyAbility")]
public class MeteorAbility : SynergyAbility
{
    [Header("���׿� �Ķ����")]
    [InspectorName("���׿� ������")] public GameObject meteorPrefab;        // ���׿� ������
    [InspectorName("���׿� ���ط�")] public float meteorDamage = 50f;       // ���׿� ���ط�
    [InspectorName("���׿� �浹 �ݰ�")] public float meteorRadius = 2f;    // ���׿� �浹 �ݰ�
    [InspectorName("��� ǥ�� �ð�")] public float warningDuration = 1.5f;  // ���׿� ��� ǥ�� �ð�
    [InspectorName("���׿� ���� �ӵ�")] public float fallSpeed = 10f;      // ���׿� ���� �ӵ�
    [InspectorName("���׿� ����")] public int meteorCount = 3;             // ���׿� ����
    [InspectorName("���� �ݰ�")] public float spawnRadius = 5f;             // �÷��̾� �ֺ� ���׿� ���� �ݰ�
    [InspectorName("���׿� ���� ����")] public float meteorSpawnDelay = 0.5f; // ���׿� ���� ���� (��)

    [Header("��� ����Ʈ ������")]
    [InspectorName("��� ���� ����Ʈ ������")] public GameObject warningStartEffectPrefab;  // ��� ���� ����Ʈ ������
    [InspectorName("��� ���� ����Ʈ ������")] public GameObject warningEndEffectPrefab;    // ��� ���� ����Ʈ ������

    private Player playerInstance;

    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        // ���׿� ���� �ڷ�ƾ ����
        playerInstance.StartCoroutine(SpawnMeteorsCoroutine());
    }

    private IEnumerator SpawnMeteorsCoroutine()
    {
        // ù ��° ���׿��� �÷��̾� ��ġ�� ���������� ����
        Vector2 spawnPosition = playerInstance.transform.position;

        // ��� ���� ����Ʈ ����
        GameObject warningStartEffect = CreateWarningStartEffect(spawnPosition);

        // ��� ���� ����Ʈ�� 2�� �Ŀ� �����ǵ��� ����
        if (warningStartEffect != null)
        {
            Destroy(warningStartEffect, 2.5f);
        }

        // ��� �� ���׿� ����
        playerInstance.StartCoroutine(MeteorSpawnAfterWarning(spawnPosition));

        // ���� ���׿� ���� �� ���
        yield return new WaitForSeconds(meteorSpawnDelay);

        // ������ ���׿��� ó��
        for (int i = 1; i < meteorCount; i++)
        {
            spawnPosition = (Vector2)playerInstance.transform.position + Random.insideUnitCircle * spawnRadius;

            // ��� ���� ����Ʈ ����
            warningStartEffect = CreateWarningStartEffect(spawnPosition);

            // ��� ���� ����Ʈ�� 2�� �Ŀ� �����ǵ��� ����
            if (warningStartEffect != null)
            {
                Destroy(warningStartEffect, 2f);
            }

            // ��� �� ���׿� ����
            playerInstance.StartCoroutine(MeteorSpawnAfterWarning(spawnPosition));

            // ���� ���׿� ���� �� ���
            yield return new WaitForSeconds(meteorSpawnDelay);
        }
    }

    private IEnumerator MeteorSpawnAfterWarning(Vector2 spawnPosition)
    {
        yield return new WaitForSeconds(warningDuration);

        // ��� ���� ����Ʈ ����
        GameObject warningEndEffect = CreateWarningEndEffect(spawnPosition);

        // ��� ���� ����Ʈ�� 2�� �Ŀ� �����ǵ��� ����
        if (warningEndEffect != null)
        {
            Destroy(warningEndEffect, 2f);
        }

        // ���׿� ���� ��ġ ���
        float spawnDistance = 10f;
        float angle = 45f * Mathf.Deg2Rad; // ������ �״�� ����
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnDistance;
        Vector2 meteorStartPosition = spawnPosition + offset;

        // ���׿� ����
        GameObject meteor = Instantiate(meteorPrefab, meteorStartPosition, Quaternion.identity);

        // ���׿� ȸ�� ����
        Vector2 direction = (spawnPosition - meteorStartPosition).normalized;
        float rotationZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        meteor.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ - 90f);

        MeteorController meteorController = meteor.GetComponent<MeteorController>();

        if (meteorController != null)
        {
            meteorController.Initialize(meteorDamage, meteorRadius, fallSpeed, spawnPosition);
        }
        else
        {
            Debug.LogError("MeteorController ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    private GameObject CreateWarningStartEffect(Vector2 position)
    {
        if (warningStartEffectPrefab != null)
        {
            // ��� ���� ����Ʈ ����
            GameObject effect = Instantiate(warningStartEffectPrefab, position, Quaternion.identity);
            return effect;
        }
        else
        {
            Debug.LogError("��� ���� ����Ʈ �������� �Ҵ���� �ʾҽ��ϴ�.");
            return null;
        }
    }

    private GameObject CreateWarningEndEffect(Vector2 position)
    {
        if (warningEndEffectPrefab != null)
        {
            // ��� ���� ����Ʈ ����
            GameObject effect = Instantiate(warningEndEffectPrefab, position, Quaternion.identity);
            return effect;
        }
        else
        {
            Debug.LogError("��� ���� ����Ʈ �������� �Ҵ���� �ʾҽ��ϴ�.");
            return null;
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
    }

    public override void Upgrade()
    {
        // ���׷��̵� ���� �߰� ����
    }
}
