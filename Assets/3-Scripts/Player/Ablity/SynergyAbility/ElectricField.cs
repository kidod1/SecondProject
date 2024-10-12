using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/ElectricField")]
public class ElectricField : SynergyAbility
{
    [Header("Electric Field Parameters")]
    public GameObject damageFieldPrefab;      // ������ ������
    public float damageFieldDuration = 1f;    // ������ ���� �ð�
    public float damageInterval = 0.25f;      // ������ ����
    public int damageAmount = 20;             // ������ ��
    public float abilityCooldown = 5f;        // �� �ɷ��� ��ٿ� �ð� ����
    public Sprite warningSprite;              // ������ ���� ��������Ʈ (���� ����)

    private Player playerInstance;
    private GameObject activeDamageField;
    private ParticleSystem particleSystem;    // ��ƼŬ �ý��� �߰�

    private void OnEnable()
    {
        // ��ٿ� �ð��� ����
        cooldownDuration = abilityCooldown;
    }

    public override void Apply(Player player)
    {
        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        // ������ ���� ���� ����
        SpawnElectricField();
    }

    private void SpawnElectricField()
    {
        if (activeDamageField == null)
        {
            // �÷��̾� ��ġ�� ������ ��� ǥ�� ���� (���� ����)
            // GameObject warning = CreateWarningCircle(playerInstance.transform.position, 2f);
            // playerInstance.StartCoroutine(ElectricFieldSpawnAfterWarning(warning, playerInstance.transform.position));

            // ��� ǥ�� ���� ��� ������ ����
            CreateDamageField(playerInstance.transform.position);
        }
    }

    // ��� ǥ�� �� ������ ���� (���� ����)
    private IEnumerator ElectricFieldSpawnAfterWarning(GameObject warning, Vector2 position)
    {
        yield return new WaitForSeconds(0f); // warningDuration ������ �߰��ϰų� ������ �� ���

        // ��� ǥ�� ����
        Destroy(warning);

        // ������ ����
        CreateDamageField(position);
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

    // ��� ǥ�� ���� �޼��� (���� ����)
    private GameObject CreateWarningCircle(Vector2 position, float radius)
    {
        GameObject warning = new GameObject("ElectricFieldWarning");
        warning.transform.position = position;

        // SpriteRenderer�� ����Ͽ� ���� ��� ǥ�� ����
        SpriteRenderer renderer = warning.AddComponent<SpriteRenderer>();
        renderer.sprite = warningSprite;
        renderer.color = new Color(0f, 0f, 1f, 0.5f); // �Ķ��� ������
        renderer.sortingLayerName = "Effect";

        warning.transform.localScale = new Vector3(radius, radius, 1f);

        return warning;
    }

    public override void Upgrade()
    {
        // ���׷��̵� ���� ���⿡ �߰��� �� ����
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
    }
}
