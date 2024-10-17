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
    [InspectorName("쿨다운 시간")] public float MeteorCooldownDurations = 5f;  // 쿨다운 시간

    [Header("경고 프리팹")]
    [InspectorName("경고 프리팹")] public GameObject warningPrefab;       // 경고 표시 프리팹

    private Player playerInstance;

    private void OnEnable()
    {
        cooldownDuration = MeteorCooldownDurations; // 쿨다운 시간 설정
    }

    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        // 메테오 생성 로직 실행
        SpawnMeteors();
    }

    private void SpawnMeteors()
    {
        for (int i = 0; i < meteorCount; i++)
        {
            Vector2 spawnPosition = (Vector2)playerInstance.transform.position + Random.insideUnitCircle * spawnRadius;

            // 경고 표시 생성
            GameObject warning = CreateWarningPrefab(spawnPosition);

            if (warning != null)
            {
                // 경고 후 메테오 생성
                playerInstance.StartCoroutine(MeteorSpawnAfterWarning(warning, spawnPosition));
            }
        }
    }

    private IEnumerator MeteorSpawnAfterWarning(GameObject warning, Vector2 spawnPosition)
    {
        yield return new WaitForSeconds(warningDuration);

        // 경고 표시 제거
        Destroy(warning);

        // 메테오 시작 위치 계산
        float spawnDistance = 10f;
        float angle = 45 * Mathf.Deg2Rad;
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

    private GameObject CreateWarningPrefab(Vector2 position)
    {
        if (warningPrefab != null)
        {
            // -45도 회전된 상태로 경고 프리팹 인스턴스화
            Quaternion rotation = Quaternion.Euler(-45f, 0f, 0f);
            GameObject warning = Instantiate(warningPrefab, position, rotation);
            return warning;
        }
        else
        {
            Debug.LogError("경고 프리팹이 할당되지 않았습니다.");
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
