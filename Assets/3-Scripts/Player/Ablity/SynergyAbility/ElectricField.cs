using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/ElectricField")]
public class ElectricField : SynergyAbility
{
    [Header("������ �ɷ� ����")]
    [InspectorName("������ ������")]
    public GameObject damageFieldPrefab;      // ������ ������

    [InspectorName("������ ���� �ð�")]
    public float damageFieldDuration = 1f;    // ������ ���� �ð�

    [InspectorName("������ ����")]
    public float damageInterval = 0.25f;      // ������ ����

    [InspectorName("������ ��")]
    public int damageAmount = 20;             // ������ ��

    [InspectorName("���� ��������Ʈ")]
    public Sprite warningSprite;              // ������ ���� ��������Ʈ (���� ����)

    private Player playerInstance;
    private GameObject activeDamageField;
    private ParticleSystem particleSystem;    // ��ƼŬ �ý��� �߰�

    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        // ������ ���� ���� ����
        SpawnElectricField();
    }

    private void SpawnElectricField()
    {
        if (activeDamageField == null)
        {
            // ��� ǥ�� ���� ��� ������ ����
            CreateDamageField(playerInstance.transform.position);
        }
    }

    private void CreateDamageField(Vector2 position)
    {
        activeDamageField = Instantiate(damageFieldPrefab, position, Quaternion.identity);

        // ���� �� �θ� ����
        activeDamageField.transform.SetParent(playerInstance.transform);

        // ������ ��ũ��Ʈ �ʱ�ȭ
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

    public override void Upgrade()
    {
        // ���׷��̵� ���� ���⿡ �߰��� �� ����
    }

    public override void ResetLevel()
    {
        lastUsedTime = 0;
    }
}
