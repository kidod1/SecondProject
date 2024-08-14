using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Barrier")]
public class Barrier : Ability
{
    public GameObject shieldPrefab; // �踮�� ǥ�ø� ���� ������
    private GameObject activeShield;
    private Player playerInstance;
    private Coroutine cooldownCoroutine;

    private float[] cooldownTimes = { 30f, 25f, 20f, 15f, 10f }; // �� ������ ��Ÿ��

    public override void Apply(Player player)
    {
        playerInstance = player;

        if (currentLevel == 0)
        {
            player.OnTakeDamage.AddListener(ActivateBarrier);
            ActivateBarrierVisual();
        }

        currentLevel++;
    }

    protected override int GetNextLevelIncrease()
    {
        return currentLevel + 1;
    }

    private void ActivateBarrierVisual()
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

    private void DeactivateBarrierVisual()
    {
        if (activeShield != null)
        {
            activeShield.SetActive(false);
        }
    }

    private void ActivateBarrier()
    {
        if (cooldownCoroutine == null)
        {
            playerInstance.SetInvincibility(true); // �÷��̾ ���� ���·� ����
            DeactivateBarrierVisual(); // �踮�� ���־� ��Ȱ��ȭ

            cooldownCoroutine = playerInstance.StartCoroutine(BarrierCooldown());
        }
    }

    private IEnumerator BarrierCooldown()
    {
        yield return new WaitForSeconds(cooldownTimes[currentLevel - 1]);

        playerInstance.SetInvincibility(false); // ���� ���� ����
        ActivateBarrierVisual(); // �踮�� ���־� Ȱ��ȭ
        cooldownCoroutine = null; // ��Ÿ�� �ڷ�ƾ �ʱ�ȭ
    }

    public override void Upgrade()
    {
        Apply(playerInstance);
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        if (cooldownCoroutine != null)
        {
            playerInstance.StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        playerInstance.SetInvincibility(false);
        DeactivateBarrierVisual();
    }
}
