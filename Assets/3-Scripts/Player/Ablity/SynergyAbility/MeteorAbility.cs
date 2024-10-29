using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/MeteorSynergyAbility")]
public class MeteorAbility : SynergyAbility
{
    [Header("메테오 파라미터")]
    [InspectorName("메테오 프리팹")] public GameObject meteorPrefab;        // 메테오 프리팹
    [InspectorName("메테오 피해량")] public float meteorDamage = 50f;       // 메테오 피해량
    [InspectorName("메테오 충돌 반경")] public float meteorRadius = 2f;    // 메테오 충돌 반경
    [InspectorName("경고 표시 시간")] public float warningDuration = 1.5f;  // 메테오 경고 표시 시간
    [InspectorName("메테오 낙하 속도")] public float fallSpeed = 10f;      // 메테오 낙하 속도
    [InspectorName("메테오 개수")] public int meteorCount = 3;             // 메테오 개수
    [InspectorName("스폰 반경")] public float spawnRadius = 5f;             // 플레이어 주변 메테오 스폰 반경
    [InspectorName("메테오 스폰 간격")] public float meteorSpawnDelay = 0.5f; // 메테오 스폰 간격 (초)

    [Header("경고 이펙트 프리팹")]
    [InspectorName("경고 시작 이펙트 프리팹")] public GameObject warningStartEffectPrefab;  // 경고 시작 이펙트 프리팹
    [InspectorName("경고 종료 이펙트 프리팹")] public GameObject warningEndEffectPrefab;    // 경고 종료 이펙트 프리팹

    private Player playerInstance;

    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        // 메테오 스폰 코루틴 시작
        playerInstance.StartCoroutine(SpawnMeteorsCoroutine());
    }

    private IEnumerator SpawnMeteorsCoroutine()
    {
        // 첫 번째 메테오는 플레이어 위치에 떨어지도록 설정
        Vector2 spawnPosition = playerInstance.transform.position;

        // 경고 시작 이펙트 생성
        GameObject warningStartEffect = CreateWarningStartEffect(spawnPosition);

        // 경고 시작 이펙트가 2초 후에 삭제되도록 설정
        if (warningStartEffect != null)
        {
            Destroy(warningStartEffect, 2.5f);
        }

        // 경고 후 메테오 생성
        playerInstance.StartCoroutine(MeteorSpawnAfterWarning(spawnPosition));

        // 다음 메테오 스폰 전 대기
        yield return new WaitForSeconds(meteorSpawnDelay);

        // 나머지 메테오들 처리
        for (int i = 1; i < meteorCount; i++)
        {
            spawnPosition = (Vector2)playerInstance.transform.position + Random.insideUnitCircle * spawnRadius;

            // 경고 시작 이펙트 생성
            warningStartEffect = CreateWarningStartEffect(spawnPosition);

            // 경고 시작 이펙트가 2초 후에 삭제되도록 설정
            if (warningStartEffect != null)
            {
                Destroy(warningStartEffect, 2f);
            }

            // 경고 후 메테오 생성
            playerInstance.StartCoroutine(MeteorSpawnAfterWarning(spawnPosition));

            // 다음 메테오 스폰 전 대기
            yield return new WaitForSeconds(meteorSpawnDelay);
        }
    }

    private IEnumerator MeteorSpawnAfterWarning(Vector2 spawnPosition)
    {
        yield return new WaitForSeconds(warningDuration);

        // 경고 종료 이펙트 생성
        GameObject warningEndEffect = CreateWarningEndEffect(spawnPosition);

        // 경고 종료 이펙트가 2초 후에 삭제되도록 설정
        if (warningEndEffect != null)
        {
            Destroy(warningEndEffect, 2f);
        }

        // 메테오 시작 위치 계산
        float spawnDistance = 10f;
        float angle = 45f * Mathf.Deg2Rad; // 각도는 그대로 유지
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnDistance;
        Vector2 meteorStartPosition = spawnPosition + offset;

        // 메테오 생성
        GameObject meteor = Instantiate(meteorPrefab, meteorStartPosition, Quaternion.identity);

        // 메테오 회전 설정
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
            Debug.LogError("MeteorController 스크립트를 찾을 수 없습니다.");
        }
    }

    private GameObject CreateWarningStartEffect(Vector2 position)
    {
        if (warningStartEffectPrefab != null)
        {
            // 경고 시작 이펙트 생성
            GameObject effect = Instantiate(warningStartEffectPrefab, position, Quaternion.identity);
            return effect;
        }
        else
        {
            Debug.LogError("경고 시작 이펙트 프리팹이 할당되지 않았습니다.");
            return null;
        }
    }

    private GameObject CreateWarningEndEffect(Vector2 position)
    {
        if (warningEndEffectPrefab != null)
        {
            // 경고 종료 이펙트 생성
            GameObject effect = Instantiate(warningEndEffectPrefab, position, Quaternion.identity);
            return effect;
        }
        else
        {
            Debug.LogError("경고 종료 이펙트 프리팹이 할당되지 않았습니다.");
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
        // 업그레이드 로직 추가 가능
    }
}
