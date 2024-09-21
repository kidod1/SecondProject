using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/ElectricField")]
public class ElectricField : SynergyAbility
{
    public GameObject damageFieldPrefab;

    public float damageFieldDuration = 1f; // ������ ���� �ð�
    public float damageInterval = 0.25f; // ������ ����
    public int damageAmount = 20; // ������ ��
    public float cooldownDurations = 5f; // ��ٿ� �ð� �߰�

    private Player playerInstance;
    private GameObject activeDamageField;
    private ParticleSystem particleSystem; // ��ƼŬ �ý��� �߰�

    private void OnEnable()
    {
        cooldownDuration = cooldownDurations; // ��Ÿ�� ����
    }

    public override void Apply(Player player)
    {
        playerInstance = player;

        Debug.Log($"Applying ElectricField ability to {playerInstance.name}");

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
                damageFieldScript.Initialize(this, playerInstance);
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
    }

    private IEnumerator DamageOverTime(DamageField damageFieldScript)
    {
        float elapsedTime = 0f;

        while (elapsedTime < damageFieldDuration)
        {
            damageFieldScript.DealDamage(damageAmount);
            elapsedTime += damageInterval;
            yield return new WaitForSeconds(damageInterval);
        }

        DeactivateDamageField();
    }

    private void DeactivateDamageField()
    {
        if (activeDamageField != null)
        {
            // ��ƼŬ �ý����� �ִٸ� ����
            if (particleSystem != null)
            {
                particleSystem.Stop(); // ��ƼŬ ����
            }

            Destroy(activeDamageField);
            activeDamageField = null;
        }
    }

    public override void Upgrade()
    {
        // ���׷��̵� ���� ���⿡ �߰��� �� ����
    }
}
