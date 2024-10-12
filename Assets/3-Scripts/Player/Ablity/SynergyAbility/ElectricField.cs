using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/ElectricField")]
public class ElectricField : SynergyAbility
{
    [Header("Electric Field Parameters")]
    public GameObject damageFieldPrefab;      // 전기장 프리팹
    public float damageFieldDuration = 1f;    // 전기장 지속 시간
    public float damageInterval = 0.25f;      // 데미지 간격
    public int damageAmount = 20;             // 데미지 양
    public float abilityCooldown = 5f;        // 이 능력의 쿨다운 시간 설정
    public Sprite warningSprite;              // 전기장 경고용 스프라이트 (선택 사항)

    private Player playerInstance;
    private GameObject activeDamageField;
    private ParticleSystem particleSystem;    // 파티클 시스템 추가

    private void OnEnable()
    {
        // 쿨다운 시간을 설정
        cooldownDuration = abilityCooldown;
    }

    public override void Apply(Player player)
    {
        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        // 전기장 생성 로직 실행
        SpawnElectricField();
    }

    private void SpawnElectricField()
    {
        if (activeDamageField == null)
        {
            // 플레이어 위치에 전기장 경고 표시 생성 (선택 사항)
            // GameObject warning = CreateWarningCircle(playerInstance.transform.position, 2f);
            // playerInstance.StartCoroutine(ElectricFieldSpawnAfterWarning(warning, playerInstance.transform.position));

            // 경고 표시 없이 즉시 전기장 생성
            CreateDamageField(playerInstance.transform.position);
        }
    }

    // 경고 표시 후 전기장 생성 (선택 사항)
    private IEnumerator ElectricFieldSpawnAfterWarning(GameObject warning, Vector2 position)
    {
        yield return new WaitForSeconds(0f); // warningDuration 변수를 추가하거나 적절한 값 사용

        // 경고 표시 제거
        Destroy(warning);

        // 전기장 생성
        CreateDamageField(position);
    }

    private void CreateDamageField(Vector2 position)
    {
        activeDamageField = Instantiate(damageFieldPrefab, position, Quaternion.identity);

        // 생성 후 부모 설정
        activeDamageField.transform.SetParent(playerInstance.transform);

        // 전기장 스크립트 초기화
        DamageField damageFieldScript = activeDamageField.GetComponent<DamageField>();
        if (damageFieldScript != null)
        {
            damageFieldScript.Initialize(this);
            activeDamageField.SetActive(true);

            // 파티클 시스템 초기화 및 재생
            particleSystem = activeDamageField.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Play(); // 파티클 재생
            }

            // 데미지 주기 시작
            playerInstance.StartCoroutine(DamageOverTime(damageFieldScript));
        }
        else
        {
            Debug.LogError("DamageField component is missing on the prefab.");
        }
    }

    private IEnumerator DamageOverTime(DamageField damageFieldScript)
    {
        // 필드 활성화
        damageFieldScript.Activate();

        float elapsedTime = 0f;

        while (elapsedTime < damageFieldDuration)
        {
            damageFieldScript.DealDamage(damageAmount);
            elapsedTime += damageInterval;
            yield return new WaitForSeconds(damageInterval);
        }

        // 필드 비활성화 및 제거
        DeactivateDamageField();
    }

    private void DeactivateDamageField()
    {
        if (activeDamageField != null)
        {
            // 파티클 시스템이 있다면 중지
            DamageField damageFieldScript = activeDamageField.GetComponent<DamageField>();
            if (damageFieldScript != null)
            {
                damageFieldScript.Deactivate();
            }

            Destroy(activeDamageField);
            activeDamageField = null;
        }
    }

    // 경고 표시 생성 메서드 (선택 사항)
    private GameObject CreateWarningCircle(Vector2 position, float radius)
    {
        GameObject warning = new GameObject("ElectricFieldWarning");
        warning.transform.position = position;

        // SpriteRenderer를 사용하여 원형 경고 표시 생성
        SpriteRenderer renderer = warning.AddComponent<SpriteRenderer>();
        renderer.sprite = warningSprite;
        renderer.color = new Color(0f, 0f, 1f, 0.5f); // 파란색 반투명
        renderer.sortingLayerName = "Effect";

        warning.transform.localScale = new Vector3(radius, radius, 1f);

        return warning;
    }

    public override void Upgrade()
    {
        // 업그레이드 논리를 여기에 추가할 수 있음
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
    }
}
