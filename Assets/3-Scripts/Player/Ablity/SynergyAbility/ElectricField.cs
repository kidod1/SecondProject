using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ElectricField")]
public class ElectricField : SynergyAbility
{
    public GameObject damageFieldPrefab;
    public float damageFieldDuration = 1f;
    public float damageInterval = 0.25f;
    public int damageAmount = 10;
    public float effectRadius = 5f;
    public float cooldownDuration = 10f;

    private Player playerInstance;
    private Coroutine damageCoroutine;
    private Coroutine cooldownCoroutine;
    private GameObject activeDamageField;
    private ParticleSystem particleSystem;
    private bool isCooldown = false;

    public override void Apply(Player player)
    {
        if (playerInstance != null)
        {
            playerInstance.OnMonsterEnter.RemoveListener(OnMonsterEnter);
        }

        playerInstance = player;
        playerInstance.OnMonsterEnter.AddListener(OnMonsterEnter);
        Debug.Log("어플라이 일렉트로닉 필드");

        if (activeDamageField != null)
        {
            activeDamageField.SetActive(false);
        }
    }

    private void OnMonsterEnter(Collider2D collider)
    {
        if (collider.CompareTag("Monster") && !isCooldown)
        {
            if (damageCoroutine != null)
            {
                playerInstance.StopCoroutine(damageCoroutine);
            }
            damageCoroutine = playerInstance.StartCoroutine(SpawnDamageField());
        }
    }

    private IEnumerator SpawnDamageField()
    {
        if (activeDamageField == null)
        {
            activeDamageField = Instantiate(damageFieldPrefab, playerInstance.transform.position, Quaternion.identity);
            activeDamageField.transform.SetParent(playerInstance.transform);
            DamageField damageFieldScript = activeDamageField.GetComponent<DamageField>();
            damageFieldScript.Initialize(damageAmount, damageInterval);
            particleSystem = activeDamageField.GetComponent<ParticleSystem>();
        }

        activeDamageField.SetActive(true);
        particleSystem?.Play();

        yield return new WaitForSeconds(damageFieldDuration);

        if (activeDamageField != null)
        {
            particleSystem?.Stop();
            activeDamageField.SetActive(false);
        }

        if (cooldownCoroutine != null)
        {
            playerInstance.StopCoroutine(cooldownCoroutine);
        }
        cooldownCoroutine = playerInstance.StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator CooldownCoroutine()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        isCooldown = false;
    }

    public override void Upgrade()
    {
        // 시너지 능력은 업그레이드 없음
    }
}
