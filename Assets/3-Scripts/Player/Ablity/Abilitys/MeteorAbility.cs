using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/MeteorSynergyAbility")]
public class MeteorSynergyAbility : SynergyAbility
{
    [Header("Meteor Parameters")]
    public GameObject meteorPrefab;       // ���׿� ������
    public float meteorDamage = 50f;      // ���׿� ���ط�
    public float meteorRadius = 2f;       // ���׿� �浹 �ݰ�
    public float warningDuration = 1.5f;  // ���׿� ��� ǥ�� �ð�
    public float fallSpeed = 10f;         // ���׿� ���� �ӵ�
    public int meteorCount = 3;           // ���׿� ����
    public float spawnRadius = 5f;        // �÷��̾� �ֺ� ���׿� ���� �ݰ�
    public float MeteorCooldownDurations = 5f;  // ��ٿ� �ð�

    public Sprite warningSprite;          // ��� ǥ�ÿ� ��������Ʈ

    private Player playerInstance;

    private void OnEnable()
    {
        cooldownDuration = MeteorCooldownDurations; // ��ٿ� �ð� ����
    }

    public override void Apply(Player player)
    {
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
            GameObject warning = CreateWarningCircle(spawnPosition, meteorRadius);

            // ��� �� ���׿� ����
            playerInstance.StartCoroutine(MeteorSpawnAfterWarning(warning, spawnPosition));
        }
    }

    private IEnumerator MeteorSpawnAfterWarning(GameObject warning, Vector2 spawnPosition)
    {
        yield return new WaitForSeconds(warningDuration);

        // ��� ǥ�� ����
        Destroy(warning);

        // ���׿� ���� ��ġ ���
        float spawnDistance = 10f;
        float angle = Random.Range(0f, 180f) * Mathf.Deg2Rad; // ��ü ������ ����
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

    private GameObject CreateWarningCircle(Vector2 position, float radius)
    {
        GameObject warning = new GameObject("MeteorWarning");
        warning.transform.position = position;

        // SpriteRenderer�� ����Ͽ� ���� ��� ǥ�� ����
        SpriteRenderer renderer = warning.AddComponent<SpriteRenderer>();
        renderer.sprite = warningSprite;
        renderer.color = new Color(1f, 0f, 0f, 0.5f);

        // Sorting Layer�� "Effect"�� ����
        renderer.sortingLayerName = "Effect";

        warning.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        return warning;
    }

    public override void Upgrade()
    {
        // ���׷��̵� ���� �߰� ����
    }
}
