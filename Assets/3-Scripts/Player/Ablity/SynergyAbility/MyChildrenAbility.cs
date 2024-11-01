using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/MyChildrenAbility")]
public class MyChildrenAbility : SynergyAbility
{
    [Header("���� ���̵�� �Ķ����")]
    [InspectorName("�� ������")]
    public GameObject beePrefab;

    [InspectorName("������ ���� ��")]
    public int numberOfBees = 5;

    [InspectorName("�� ���� ����")]
    public float spawnInterval = 0.5f;

    [InspectorName("�÷��̾� �ֺ����� �ɵ��� �ð�")]
    public float hoverDuration = 3f;

    [InspectorName("���� �̵� �ӵ�")]
    public float beeSpeed = 5f;

    [InspectorName("���� ���ط�")]
    public int beeDamage = 10;

    [InspectorName("���� ���� �ð�")]
    public float beeLifetime = 10f;

    [InspectorName("���� ����")]
    public float attackRange = 0.5f;

    private Player playerInstance;
    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

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

        GameObject beeObject = Instantiate(beePrefab, playerInstance.transform.position, Quaternion.identity);

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
    }

    public override void ResetLevel()
    {
        lastUsedTime = 0;
    }
}
