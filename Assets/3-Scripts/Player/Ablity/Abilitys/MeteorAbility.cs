using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/MeteorSynergyAbility")]
public class MeteorSynergyAbility : SynergyAbility
{
    [Header("Meteor Parameters")]
    public GameObject meteorPrefab;       // 메테오 프리팹
    public float meteorDamage = 50f;      // 메테오 피해량
    public float meteorRadius = 2f;       // 메테오 충돌 반경
    public float warningDuration = 1.5f;  // 메테오 경고 표시 시간
    public float fallSpeed = 10f;         // 메테오 낙하 속도
    public int meteorCount = 3;           // 메테오 개수
    public float spawnRadius = 5f;        // 플레이어 주변 메테오 스폰 반경
    public float MeteorCooldownDurations = 5f;  // 쿨다운 시간

    public Sprite warningSprite;          // 경고 표시용 스프라이트

    private Player playerInstance;

    private void OnEnable()
    {
        cooldownDuration = MeteorCooldownDurations; // 쿨다운 시간 설정
    }

    public override void Apply(Player player)
    {
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
            GameObject warning = CreateWarningCircle(spawnPosition, meteorRadius);

            // 경고 후 메테오 생성
            playerInstance.StartCoroutine(MeteorSpawnAfterWarning(warning, spawnPosition));
        }
    }

    private IEnumerator MeteorSpawnAfterWarning(GameObject warning, Vector2 spawnPosition)
    {
        yield return new WaitForSeconds(warningDuration);

        // 경고 표시 제거
        Destroy(warning);

        // 메테오 시작 위치 계산
        float spawnDistance = 10f;
        float angle = Random.Range(0f, 180f) * Mathf.Deg2Rad; // 전체 각도로 수정
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

    private GameObject CreateWarningCircle(Vector2 position, float radius)
    {
        GameObject warning = new GameObject("MeteorWarning");
        warning.transform.position = position;

        // SpriteRenderer를 사용하여 원형 경고 표시 생성
        SpriteRenderer renderer = warning.AddComponent<SpriteRenderer>();
        renderer.sprite = warningSprite;
        renderer.color = new Color(1f, 0f, 0f, 0.5f);

        // Sorting Layer를 "Effect"로 설정
        renderer.sortingLayerName = "Effect";

        warning.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        return warning;
    }

    public override void Upgrade()
    {
        // 업그레이드 로직 추가 가능
    }
}
