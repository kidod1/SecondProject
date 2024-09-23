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

        if (playerInstance == null)
        {
            Debug.LogError("Player instance is null. Cannot apply EmergencyHealing.");
            return;
        }

        // �÷��̾��� OnTakeDamage �̺�Ʈ�� ������ �߰�
        player.OnTakeDamage.AddListener(CheckAndHealPlayer);
    }

    private void CheckAndHealPlayer()
    {
        if (playerInstance == null)
        {
            Debug.LogError("Player instance is null. Cannot check and heal player.");
            return;
        }

        // ü���� 10% ���Ϸ� ���������� Ȯ��
        if (playerInstance.GetCurrentHP() <= playerInstance.stat.currentMaxHP * 0.1f)
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
        if (playerInstance == null)
        {
            Debug.LogError("Player instance is null. Cannot heal player.");
            return;
        }

        // ü���� 30%�� ȸ��
        int healAmount = Mathf.RoundToInt(playerInstance.stat.currentMaxHP * 0.3f);
        playerInstance.Heal(healAmount);

        // ġ�� ��� ���־� ȿ��
        if (healingDronePrefab != null)
        {
            GameObject healingDrone = Instantiate(healingDronePrefab, playerInstance.transform.position, Quaternion.identity);
            if (healingDrone != null)
            {
                healingDrone.transform.SetParent(playerInstance.transform);
                Destroy(healingDrone, 2f); // ġ�� ����� 2�� �Ŀ� �����
            }
            else
            {
                Debug.LogError("Failed to instantiate healing drone prefab.");
            }
        }
        else
        {
            Debug.LogWarning("Healing drone prefab is null. No visual effect will be created.");
        }

        // ��Ÿ�� ����
        cooldownCoroutine = playerInstance.StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        if (cooldownDuration <= 0f)
        {
            Debug.LogWarning("Cooldown duration is set to 0 or a negative value. Skipping cooldown.");
            yield break;
        }

        yield return new WaitForSecondsRealtime(cooldownDuration);
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
