using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/MeteorAbility")]
public class MeteorAbility : Ability
{
    [Header("Meteor Parameters")]
    public GameObject meteorPrefab;       // ���׿� ������
    public float meteorDamage = 50f;      // ���׿� ���ط�
    public float meteorRadius = 2f;       // ���׿� �浹 �ݰ�
    public float warningDuration = 1.5f;  // ���׿� ��� ǥ�� �ð�
    public float fallSpeed = 10f;         // ���׿� ���� �ӵ�
    public int meteorCount = 3;           // ���׿� ����
    public float spawnRadius = 5f;        // �÷��̾� �ֺ� ���׿� ���� �ݰ�
    public float spawnInterval = 10f;     // ���׿� ���� �ֱ�

    public Sprite warningSprite;          // ��� ǥ�ÿ� ��������Ʈ

    private Player playerInstance;
    private Coroutine meteorCoroutine;

    public override void Apply(Player player)
    {
        playerInstance = player;

        // ���׿� ���� �ڷ�ƾ ����
        if (meteorCoroutine == null)
        {
            meteorCoroutine = player.StartCoroutine(SpawnMeteorRoutine());
        }
    }

    public void Remove(Player player)
    {
        if (meteorCoroutine != null)
        {
            player.StopCoroutine(meteorCoroutine);
            meteorCoroutine = null;
        }
    }

    private IEnumerator SpawnMeteorRoutine()
    {
        while (true)
        {
            SpawnMeteors();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnMeteors()
    {
        for (int i = 0; i < meteorCount; i++)
        {
            Vector2 spawnPosition = (Vector2)playerInstance.transform.position + Random.insideUnitCircle * spawnRadius;

            // ��� ǥ�� ����
            GameObject warning = CreateWarningCircle(spawnPosition, meteorRadius);

            // ��� ǥ�� �� ���׿� ���� �ڷ�ƾ ����
            playerInstance.StartCoroutine(MeteorSpawnAfterWarning(warning, spawnPosition));

            // ���׿� ���� ���� ���� (�ɼ�)
            // yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator MeteorSpawnAfterWarning(GameObject warning, Vector2 spawnPosition)
    {
        yield return new WaitForSeconds(warningDuration);

        // ��� ǥ�� ����
        Destroy(warning);

        float spawnDistance = 10f;
        float angle = Random.Range(0f, 180f) * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnDistance;
        Vector2 meteorStartPosition = spawnPosition + offset;

        // ���׿� ����
        GameObject meteor = Instantiate(meteorPrefab, meteorStartPosition, Quaternion.identity);

        // ���׿� ȸ�� ���� (�ɼ�)
        Vector2 direction = (spawnPosition - meteorStartPosition).normalized;
        float rotationZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        meteor.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ - 90f); // ���׿� ��������Ʈ�� ���� ���Ѵٰ� ����

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
        renderer.sprite = warningSprite; // �̸� �غ��� ��������Ʈ ���
        renderer.color = new Color(1f, 0f, 0f, 0.5f); // �������� ������

        // Sorting Layer�� "Effect"�� ����
        renderer.sortingLayerName = "Effect";

        // ��� ǥ�� ũ�� ���� (0.1�� ����)
        warning.transform.localScale = new Vector3(0.3f, 0.3f, 0.1f);

        return warning;
    }



    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            // ���� �� �� ���ط� ���� ��
            meteorDamage += 20f; // ��: ���� �� ���ط� 20 ����
            meteorCount += 1;    // ��: ���� �� ���׿� ���� 1 ����
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        // �ʿ� �� ���� �ʱ�ȭ ���� �߰�
    }

    protected override int GetNextLevelIncrease()
    {
        // ���� �������� �����ϴ� ���ط� ��ȯ
        return Mathf.RoundToInt(meteorDamage + 20f);
    }
}
