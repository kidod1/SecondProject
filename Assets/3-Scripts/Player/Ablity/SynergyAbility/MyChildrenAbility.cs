using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/MyChildrenAbility")]
public class MyChildrenAbility : SynergyAbility
{
    [Header("My Children Parameters")]
    public GameObject beePrefab;               // 벌 프리팹
    public int numberOfBees = 5;               // 생성할 벌의 수
    public float spawnInterval = 0.5f;         // 벌 생성 간격
    public float hoverDuration = 3f;           // 플레이어 주변에서 맴도는 시간
    public float abilityCooldown = 15f;        // 능력의 쿨다운 시간
    public float beeSpeed = 5f;                // 벌의 이동 속도
    public int beeDamage = 10;                 // 벌의 피해량
    public float beeLifetime = 10f;            // 벌의 생존 시간
    public float attackRange = 0.5f;           // 공격 범위

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

        // 벌 생성 시작
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

        // 플레이어 위치에 벌 생성
        GameObject beeObject = Instantiate(beePrefab, playerInstance.transform.position, Quaternion.identity);

        // Bee 스크립트 초기화
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
        // 업그레이드 로직을 여기에 추가할 수 있습니다.
        // 예: 벌의 수 증가, 피해량 증가 등
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
    }
}
