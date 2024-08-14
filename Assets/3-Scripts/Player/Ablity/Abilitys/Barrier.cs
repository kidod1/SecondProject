using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Barrier")]
public class Barrier : Ability
{
    public GameObject shieldPrefab; // �踮�� ǥ�ø� ���� ������
    private GameObject activeShield;
    private Player playerInstance;
    private Coroutine cooldownCoroutine;

    [SerializeField]
    private float[] cooldownTimes = { 30f, 25f, 20f, 15f, 10f }; // �� ������ ��Ÿ��

    public override void Apply(Player player)
    {
        playerInstance = player;

        if (currentLevel == 0)
        {
            player.OnTakeDamage.AddListener(ActivateBarrier);
            ActivateBarrierVisual();
        }
    }

    protected override int GetNextLevelIncrease()
    {
        return currentLevel < maxLevel ? currentLevel + 1 : 0;
    }

    private void ActivateBarrierVisual()
    {
        if (shieldPrefab != null)
        {
            if (activeShield == null)
            {
                activeShield = Instantiate(shieldPrefab, playerInstance.transform);
                activeShield.transform.SetParent(playerInstance.transform);
            }
            else
            {
                activeShield.SetActive(true);
            }
        }
    }

    private void DeactivateBarrierVisual()
    {
        if (activeShield != null)
        {
            activeShield.SetActive(false);
        }
    }

    private void ActivateBarrier()
    {
        if (cooldownCoroutine == null && playerInstance != null)
        {
            playerInstance.SetInvincibility(true); // �÷��̾ ���� ���·� ����
            DeactivateBarrierVisual(); // �踮�� ���־� ��Ȱ��ȭ

            cooldownCoroutine = playerInstance.StartCoroutine(BarrierCooldown());
        }
    }

    private IEnumerator BarrierCooldown()
    {
        if (currentLevel - 1 >= 0 && currentLevel - 1 < cooldownTimes.Length)
        {
            yield return new WaitForSeconds(cooldownTimes[currentLevel - 1]);
        }
        else
        {
            Debug.LogError("Current level is out of bounds for cooldownTimes array.");
            yield break;
        }

        if (playerInstance != null)
        {
            playerInstance.SetInvincibility(false);
            ActivateBarrierVisual();
        }

        cooldownCoroutine = null;
    }

    public override void Upgrade()
    {
        currentLevel++;
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        if (cooldownCoroutine != null && playerInstance != null)
        {
            playerInstance.StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        if (playerInstance != null)
        {
            playerInstance.SetInvincibility(false);
        }

        DeactivateBarrierVisual();
    }
}
