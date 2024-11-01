using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/ElectricField")]
public class ElectricField : SynergyAbility
{
    [Header("전기장 능력 설정")]
    [InspectorName("전기장 프리팹")]
    public GameObject damageFieldPrefab;      // 전기장 프리팹

    [InspectorName("전기장 지속 시간")]
    public float damageFieldDuration = 1f;    // 전기장 지속 시간

    [InspectorName("데미지 간격")]
    public float damageInterval = 0.25f;      // 데미지 간격

    [InspectorName("데미지 양")]
    public int damageAmount = 20;             // 데미지 양

    [InspectorName("경고용 스프라이트")]
    public Sprite warningSprite;              // 전기장 경고용 스프라이트 (선택 사항)

    private Player playerInstance;
    private GameObject activeDamageField;
    private ParticleSystem particleSystem;    // 파티클 시스템 추가

    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        // 전기장 생성 로직 실행
        SpawnElectricField();
    }

    private void SpawnElectricField()
    {
        if (activeDamageField == null)
        {
            // 경고 표시 없이 즉시 전기장 생성
            CreateDamageField(playerInstance.transform.position);
        }
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

    public override void Upgrade()
    {
        // 업그레이드 논리를 여기에 추가할 수 있음
    }

    public override void ResetLevel()
    {
        lastUsedTime = 0;
    }
}
