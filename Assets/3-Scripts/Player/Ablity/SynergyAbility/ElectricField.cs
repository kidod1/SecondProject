using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "SynergyAbilities/ElectricField")]
public class ElectricField : SynergyAbility
{
    public GameObject damageFieldPrefab;
    public float damageFieldDuration = 1f; // 전기장 지속 시간
    public float damageInterval = 0.25f; // 데미지 간격
    public int damageAmount = 10; // 데미지 양
    public float effectRadius = 5f; // 효과 반경
    public float cooldownDuration = 10f; // 쿨다운 시간

    private Player playerInstance;
    private Coroutine cooldownCoroutine;
    private GameObject activeDamageField;
    private DamageField damageFieldScript;
    private ParticleSystem particleSystem;
    [SerializeField]
    private bool isCooldown = false;

    public override void Apply(Player player)
    {
        if (playerInstance != null)
        {
            playerInstance.OnMonsterEnter.RemoveListener(OnMonsterEnter);
        }

        playerInstance = player;
        playerInstance.OnMonsterEnter.AddListener(OnMonsterEnter);
        Debug.Log("Electric Field ability applied to player."); // 디버그 메시지 추가

        if (activeDamageField == null)
        {
            // 전기장 오브젝트를 인스턴스화하고 플레이어에게 배치
            activeDamageField = Instantiate(damageFieldPrefab, playerInstance.transform.position, Quaternion.identity);
            activeDamageField.transform.SetParent(playerInstance.transform);

            damageFieldScript = activeDamageField.GetComponent<DamageField>();
            damageFieldScript.Initialize(damageAmount, damageInterval);

            particleSystem = activeDamageField.GetComponent<ParticleSystem>();
        }

        particleSystem?.Stop();
        damageFieldScript.enabled = false;
    }

    private void OnMonsterEnter(Collider2D collider)
    {
        if (collider.CompareTag("Monster") && !isCooldown)
        {
            SpawnDamageField();
        }
    }

    private void SpawnDamageField()
    {
        if (activeDamageField != null)
        {
            damageFieldScript.enabled = true; // 데미지 기능 활성화
            particleSystem?.Play(); // 시각적 효과 활성화

            Debug.Log("Electric Field Activated!");

            if (cooldownCoroutine != null)
            {
                playerInstance.StopCoroutine(cooldownCoroutine);
            }
            cooldownCoroutine = playerInstance.StartCoroutine(CooldownCoroutine());
        }
        else
        {
            Debug.LogError("Damage field is not initialized.");
        }
    }

    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(damageFieldDuration); // 전기장이 지속된 후

        if (activeDamageField != null)
        {
            particleSystem?.Stop();
            damageFieldScript.enabled = false; // 데미지 기능 비활성화
        }

        isCooldown = true;
        yield return new WaitForSeconds(cooldownDuration); // 쿨다운 대기
        isCooldown = false;
    }

    public override void Upgrade()
    {
        // 시너지 능력은 업그레이드 없음
    }
}
