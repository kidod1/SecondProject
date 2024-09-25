using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/ElectricField")]
public class ElectricField : SynergyAbility
{
    public GameObject damageFieldPrefab;

    public float damageFieldDuration = 1f; // ������ ���� �ð�
    public float damageInterval = 0.25f;   // ������ ����
    public int damageAmount = 20;          // ������ ��
    public float abilityCooldown = 5f;     // �� �ɷ��� ��ٿ� �ð� ����

    private Player playerInstance;
    private GameObject activeDamageField;
    private ParticleSystem particleSystem; // ��ƼŬ �ý��� �߰�

    private void OnEnable()
    {
        // ��ٿ� �ð��� ����
        cooldownDuration = abilityCooldown;
    }

    public override void Activate(Player player)
    {
        playerInstance = player;

        Debug.Log($"Activating ElectricField ability for {playerInstance.name}");
        Debug.Log($"CooldownDuration: {cooldownDuration}, LastUsedTime: {lastUsedTime}, Time.time: {Time.time}");

        // �θ� Ŭ������ Activate �޼��带 ȣ���Ͽ� ��ٿ� ó��
        base.Activate(player);
    }

    public override void Apply(Player player)
    {
        // �ɷ� �ߵ� ����
        if (activeDamageField == null)
        {
            // �÷��̾� ��ġ�� ������ �ʵ� ����
            activeDamageField = Instantiate(damageFieldPrefab, playerInstance.transform.position, Quaternion.identity);

            // ���� �� �θ� ����
            activeDamageField.transform.SetParent(playerInstance.transform);

            // ������ �ʵ� ��ũ��Ʈ �ʱ�ȭ
            DamageField damageFieldScript = activeDamageField.GetComponent<DamageField>();
            if (damageFieldScript != null)
            {
                damageFieldScript.Initialize(this);
                activeDamageField.SetActive(true);

                // ��ƼŬ �ý��� �ʱ�ȭ �� ���
                particleSystem = activeDamageField.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Play(); // ��ƼŬ ���
                }

                // ������ �ֱ� ����
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
        // �ʵ� Ȱ��ȭ
        damageFieldScript.Activate();

        float elapsedTime = 0f;

        while (elapsedTime < damageFieldDuration)
        {
            damageFieldScript.DealDamage(damageAmount);
            elapsedTime += damageInterval;
            yield return new WaitForSeconds(damageInterval);
        }

        // �ʵ� ��Ȱ��ȭ �� ����
        DeactivateDamageField();
    }

    private void DeactivateDamageField()
    {
        if (activeDamageField != null)
        {
            // ��ƼŬ �ý����� �ִٸ� ����
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
        // ���׷��̵� ���� ���⿡ �߰��� �� ����
    }
}
