using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/MyChildrenAbility")]
public class MyChildrenAbility : SynergyAbility
{
    [Header("나의 아이들아 파라미터")]
    [InspectorName("벌 프리팹")]
    public GameObject beePrefab;

    [InspectorName("생성할 벌의 수")]
    public int numberOfBees = 5;

    [InspectorName("벌 생성 간격")]
    public float spawnInterval = 0.5f;

    [InspectorName("플레이어 주변에서 맴도는 시간")]
    public float hoverDuration = 3f;

    [InspectorName("벌의 이동 속도")]
    public float beeSpeed = 5f;

    [InspectorName("벌의 피해량")]
    public int beeDamage = 10;

    [InspectorName("벌의 생존 시간")]
    public float beeLifetime = 10f;

    [InspectorName("공격 범위")]
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
