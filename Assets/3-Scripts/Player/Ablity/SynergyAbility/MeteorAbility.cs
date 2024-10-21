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
    [InspectorName("��ٿ� �ð�")] public float MeteorCooldownDurations = 5f;  // ��ٿ� �ð�

    [Header("��� ������")]
    [InspectorName("��� ������")] public GameObject warningPrefab;       // ��� ǥ�� ������

    private Player playerInstance;

    private void OnEnable()
    {
        cooldownDuration = MeteorCooldownDurations; // ��ٿ� �ð� ����
    }

    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        // ���׿� ���� ���� ����
        SpawnMeteors();
    }

    private void SpawnMeteors()
    {
        for (int i = 0; i < meteorCount; i++)
        {
            Vector2 spawnPosition = (Vector2)playerInstance.transform.position + Random.insideUnitCircle * spawnRadius;

            // ��� ǥ�� ����
            GameObject warning = CreateWarningPrefab(spawnPosition);

            if (warning != null)
            {
                // ��� �� ���׿� ����
                playerInstance.StartCoroutine(MeteorSpawnAfterWarning(warning, spawnPosition));
            }
        }
    }

    private IEnumerator MeteorSpawnAfterWarning(GameObject warning, Vector2 spawnPosition)
    {
        yield return new WaitForSeconds(warningDuration);

        // ��� ǥ�� ����
        Destroy(warning);

        // ���׿� ���� ��ġ ���
        float spawnDistance = 10f;
        float angle = 45 * Mathf.Deg2Rad;
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

    private GameObject CreateWarningPrefab(Vector2 position)
    {
        if (warningPrefab != null)
        {
            // -45�� ȸ���� ���·� ��� ������ �ν��Ͻ�ȭ
            Quaternion rotation = Quaternion.Euler(-45f, 0f, 0f);
            GameObject warning = Instantiate(warningPrefab, position, rotation);
            return warning;
        }
        else
        {
            Debug.LogError("��� �������� �Ҵ���� �ʾҽ��ϴ�.");
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
