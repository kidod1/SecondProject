using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/MeteorAbility")]
public class MeteorAbility : Ability
{
    [Header("Meteor Parameters")]
    public GameObject meteorPrefab;       // 메테오 프리팹
    public float meteorDamage = 50f;      // 메테오 피해량
    public float meteorRadius = 2f;       // 메테오 충돌 반경
    public float warningDuration = 1.5f;  // 메테오 경고 표시 시간
    public float fallSpeed = 10f;         // 메테오 낙하 속도
    public int meteorCount = 3;           // 메테오 개수
    public float spawnRadius = 5f;        // 플레이어 주변 메테오 스폰 반경
    public float spawnInterval = 10f;     // 메테오 생성 주기

    public Sprite warningSprite;          // 경고 표시용 스프라이트

    private Player playerInstance;
    private Coroutine meteorCoroutine;

    public override void Apply(Player player)
    {
        playerInstance = player;

        // 메테오 생성 코루틴 시작
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

            // 경고 표시 생성
            GameObject warning = CreateWarningCircle(spawnPosition, meteorRadius);

            // 경고 표시 후 메테오 생성 코루틴 시작
            playerInstance.StartCoroutine(MeteorSpawnAfterWarning(warning, spawnPosition));

            // 메테오 사이 간격 조절 (옵션)
            // yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator MeteorSpawnAfterWarning(GameObject warning, Vector2 spawnPosition)
    {
        yield return new WaitForSeconds(warningDuration);

        // 경고 표시 제거
        Destroy(warning);

        float spawnDistance = 10f;
        float angle = Random.Range(0f, 180f) * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnDistance;
        Vector2 meteorStartPosition = spawnPosition + offset;

        // 메테오 생성
        GameObject meteor = Instantiate(meteorPrefab, meteorStartPosition, Quaternion.identity);

        // 메테오 회전 설정 (옵션)
        Vector2 direction = (spawnPosition - meteorStartPosition).normalized;
        float rotationZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        meteor.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ - 90f); // 메테오 스프라이트가 위를 향한다고 가정

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
        renderer.sprite = warningSprite; // 미리 준비한 스프라이트 사용
        renderer.color = new Color(1f, 0f, 0f, 0.5f); // 반투명한 빨간색

        // Sorting Layer를 "Effect"로 설정
        renderer.sortingLayerName = "Effect";

        // 경고 표시 크기 고정 (0.1로 설정)
        warning.transform.localScale = new Vector3(0.3f, 0.3f, 0.1f);

        return warning;
    }



    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            // 레벨 업 시 피해량 증가 등
            meteorDamage += 20f; // 예: 레벨 당 피해량 20 증가
            meteorCount += 1;    // 예: 레벨 당 메테오 개수 1 증가
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        // 필요 시 레벨 초기화 로직 추가
    }

    protected override int GetNextLevelIncrease()
    {
        // 다음 레벨에서 증가하는 피해량 반환
        return Mathf.RoundToInt(meteorDamage + 20f);
    }
}
