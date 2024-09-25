using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/ElectricField")]
public class ElectricField : SynergyAbility
{
    public GameObject damageFieldPrefab;

    public float damageFieldDuration = 1f; // 전기장 지속 시간
    public float damageInterval = 0.25f;   // 데미지 간격
    public int damageAmount = 20;          // 데미지 양
    public float abilityCooldown = 5f;     // 이 능력의 쿨다운 시간 설정

    private Player playerInstance;
    private GameObject activeDamageField;
    private ParticleSystem particleSystem; // 파티클 시스템 추가

    private void OnEnable()
    {
        // 쿨다운 시간을 설정
        cooldownDuration = abilityCooldown;
    }

    public override void Activate(Player player)
    {
        playerInstance = player;

        Debug.Log($"Activating ElectricField ability for {playerInstance.name}");
        Debug.Log($"CooldownDuration: {cooldownDuration}, LastUsedTime: {lastUsedTime}, Time.time: {Time.time}");

        // 부모 클래스의 Activate 메서드를 호출하여 쿨다운 처리
        base.Activate(player);
    }

    public override void Apply(Player player)
    {
        // 능력 발동 로직
        if (activeDamageField == null)
        {
            // 플레이어 위치에 데미지 필드 생성
            activeDamageField = Instantiate(damageFieldPrefab, playerInstance.transform.position, Quaternion.identity);

            // 생성 후 부모 설정
            activeDamageField.transform.SetParent(playerInstance.transform);

            // 데미지 필드 스크립트 초기화
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
        lastUsedTime = 0;
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

    public override void ResetLevel()
    {
        currentLevel = 0;
    }

    public override void Upgrade()
    {
        // 업그레이드 논리를 여기에 추가할 수 있음
    }
}
