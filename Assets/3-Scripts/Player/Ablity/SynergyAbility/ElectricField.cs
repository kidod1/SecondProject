using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "SynergyAbilities/ElectricField")]
public class ElectricField : SynergyAbility
{
    public GameObject damageFieldPrefab;

    public float damageFieldDuration = 1f; // ������ ���� �ð�
    public float damageInterval = 0.25f; // ������ ����
    public int damageAmount = 20; // ������ ��
    public float cooldownDurations = 20f; // ��ٿ� �ð�

    private Player playerInstance;
    private GameObject activeDamageField;

    public override void Apply(Player player)
    {
        // ������ �Ҵ�� �÷��̾� �ν��Ͻ��� �ִٸ� �̺�Ʈ ����
        if (playerInstance != null)
        {
            // ������ ��ϵ� ������ ���� ������ ���ʿ��ϹǷ� �����մϴ�.
            Debug.Log("Previous player instance was removed.");
        }

        playerInstance = player;
        Debug.Log("Electric Field ability applied to player.");

        // �������� �÷��̾�� ��ȯ
        if (activeDamageField == null)
        {
            activeDamageField = Instantiate(damageFieldPrefab, playerInstance.transform.position, Quaternion.identity);
            activeDamageField.transform.SetParent(playerInstance.transform);

            DamageField damageFieldScript = activeDamageField.GetComponent<DamageField>();
            if (damageFieldScript != null)
            {
                damageFieldScript.Initialize(this, playerInstance); // ������ �ʵ� �ʱ�ȭ
                activeDamageField.SetActive(true); // Ȱ��ȭ ���·� ��ȯ
            }
            else
            {
                Debug.LogError("DamageField component is missing on the prefab.");
            }
        }
    }

    public override void Upgrade()
    {
        // ���׷��̵� ���� ���⿡ �߰��� �� ����
    }
}
