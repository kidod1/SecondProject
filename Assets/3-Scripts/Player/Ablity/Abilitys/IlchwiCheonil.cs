using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/IlchwiCheonil")]
public class IlchwiCheonil : Ability
{
    [Header("Ability Parameters")]
    public float poisonDamage = 10f;         // 독구름의 피해량 (초당 피해량)
    public float poisonRange = 5f;           // 독구름의 범위 (반지름)
    public float poisonDuration = 5f;        // 독구름의 지속 시간
    public float spawnInterval = 10f;        // 독구름 생성 주기
    public GameObject poisonCloudPrefab;     // 독구름 프리팹

    private Player playerInstance;
    private Coroutine poisonCloudCoroutine;

    public override void Apply(Player player)
    {
        playerInstance = player;

        // 독구름 생성 코루틴 시작
        if (poisonCloudCoroutine == null)
        {
            poisonCloudCoroutine = player.StartCoroutine(SpawnPoisonCloudRoutine());
        }
    }

    public void Remove(Player player)
    {
        if (poisonCloudCoroutine != null)
        {
            player.StopCoroutine(poisonCloudCoroutine);
            poisonCloudCoroutine = null;
        }
    }

    private IEnumerator SpawnPoisonCloudRoutine()
    {
        while (true)
        {
            SpawnPoisonCloud();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnPoisonCloud()
    {
        if (poisonCloudPrefab == null)
        {
            Debug.LogError("독구름 프리팹이 설정되지 않았습니다.");
            return;
        }

        Vector3 spawnPosition = playerInstance.transform.position;

        GameObject poisonCloud = Instantiate(poisonCloudPrefab, spawnPosition, Quaternion.identity);
        PoisonCloud poisonCloudScript = poisonCloud.GetComponent<PoisonCloud>();

        if (poisonCloudScript != null)
        {
            poisonCloudScript.Initialize(poisonDamage, poisonRange, poisonDuration);
        }
        else
        {
            Debug.LogError("PoisonCloud 스크립트를 찾을 수 없습니다.");
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        // 필요 시 레벨 초기화 로직 추가
    }

    protected override int GetNextLevelIncrease()
    {
        return 1;
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            poisonDamage += 5f;
            poisonRange += 0.5f;
        }
    }
}
