using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/MyChildrenAbility")]
public class MyChildrenAbility : SynergyAbility
{
    [Header("My Children Parameters")]
    public GameObject beePrefab;               // �� ������
    public int numberOfBees = 5;               // ������ ���� ��
    public float spawnInterval = 0.5f;         // �� ���� ����
    public float hoverDuration = 3f;           // �÷��̾� �ֺ����� �ɵ��� �ð�
    public float abilityCooldown = 15f;        // �ɷ��� ��ٿ� �ð�
    public float beeSpeed = 5f;                // ���� �̵� �ӵ�
    public int beeDamage = 10;                 // ���� ���ط�
    public float beeLifetime = 10f;            // ���� ���� �ð�
    public float attackRange = 0.5f;           // ���� ����

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

        // �� ���� ����
        playerInstance.StartCoroutine(SpawnBees());
    }

    private IEnumerator SpawnBees()
    {
        for (int i = 0; i < numberOfBees; i++)
        {
            SpawnBee();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnBee()
    {
        if (beePrefab == null)
        {
            Debug.LogError("Bee prefab is not assigned.");
            return;
        }

        // �÷��̾� ��ġ�� �� ����
        GameObject beeObject = Instantiate(beePrefab, playerInstance.transform.position, Quaternion.identity);

        // Bee ��ũ��Ʈ �ʱ�ȭ
        Bee beeScript = beeObject.GetComponent<Bee>();
        if (beeScript != null)
        {
            beeScript.Initialize(playerInstance, beeSpeed, beeDamage, hoverDuration, beeLifetime, attackRange);
        }
        else
        {
            Debug.LogError("Bee prefab is missing Bee component.");
        }
    }

    public override void Upgrade()
    {
        // ���׷��̵� ������ ���⿡ �߰��� �� �ֽ��ϴ�.
        // ��: ���� �� ����, ���ط� ���� ��
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
    }
}
