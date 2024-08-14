using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "SynergyAbilities/EmergencyHealing")]
public class EmergencyHealing : SynergyAbility
{
    public GameObject healingDronePrefab; // ġ�� ��� ������
    public float cooldownDuration = 20f; // ��Ÿ�� 20��
    private Player playerInstance;
    private Coroutine cooldownCoroutine;

    public override void Apply(Player player)
    {
        playerInstance = player;

        // �÷��̾��� OnTakeDamage �̺�Ʈ�� ������ �߰�
        player.OnTakeDamage.AddListener(CheckAndHealPlayer);
    }

    private void CheckAndHealPlayer()
    {
        // ü���� 10% ���Ϸ� ���������� Ȯ��
        if (playerInstance.GetCurrentHP() <= playerInstance.stat.maxHP * 0.1f)
        {
            // ��Ÿ���� �ƴ� ���¿����� ȸ��
            if (cooldownCoroutine == null)
            {
                HealPlayer();
            }
        }
    }

    private void HealPlayer()
    {
        // ü���� 30%�� ȸ��
        int healAmount = Mathf.RoundToInt(playerInstance.stat.maxHP * 0.3f);
        playerInstance.Heal(healAmount);

        // ġ�� ��� ���־� ȿ��
        if (healingDronePrefab != null)
        {
            GameObject healingDrone = Instantiate(healingDronePrefab, playerInstance.transform.position, Quaternion.identity);
            healingDrone.transform.SetParent(playerInstance.transform);
            Destroy(healingDrone, 2f); // ġ�� ����� 2�� �Ŀ� �����
        }

        // ��Ÿ�� ����
        cooldownCoroutine = playerInstance.StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldownDuration);
        cooldownCoroutine = null; // ��Ÿ���� ������ �ʱ�ȭ
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        // ������ ����
        if (playerInstance != null)
        {
            playerInstance.OnTakeDamage.RemoveListener(CheckAndHealPlayer);
        }

        // ��Ÿ�� �ߴ�
        if (cooldownCoroutine != null && playerInstance != null)
        {
            playerInstance.StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }
    }
}
