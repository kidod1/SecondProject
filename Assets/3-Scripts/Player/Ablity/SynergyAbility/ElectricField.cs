using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "SynergyAbilities/ElectricField")]
public class ElectricField : SynergyAbility
{
    public GameObject damageFieldPrefab;
    public float damageFieldDuration = 1f; // ������ ���� �ð�
    public float damageInterval = 0.25f; // ������ ����
    public int damageAmount = 10; // ������ ��
    public float effectRadius = 5f; // ȿ�� �ݰ�
    public float cooldownDuration = 10f; // ��ٿ� �ð�

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
        Debug.Log("Electric Field ability applied to player."); // ����� �޽��� �߰�

        if (activeDamageField == null)
        {
            // ������ ������Ʈ�� �ν��Ͻ�ȭ�ϰ� �÷��̾�� ��ġ
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
            damageFieldScript.enabled = true; // ������ ��� Ȱ��ȭ
            particleSystem?.Play(); // �ð��� ȿ�� Ȱ��ȭ

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
        yield return new WaitForSeconds(damageFieldDuration); // �������� ���ӵ� ��

        if (activeDamageField != null)
        {
            particleSystem?.Stop();
            damageFieldScript.enabled = false; // ������ ��� ��Ȱ��ȭ
        }

        isCooldown = true;
        yield return new WaitForSeconds(cooldownDuration); // ��ٿ� ���
        isCooldown = false;
    }

    public override void Upgrade()
    {
        // �ó��� �ɷ��� ���׷��̵� ����
    }
}
