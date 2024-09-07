using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Barrier")]
public class Barrier : Ability
{
    public GameObject shieldPrefab;
    public GameObject shieldDeactivateEffectPrefab;

    private GameObject activeShield;
    private Player playerInstance;
    private Coroutine cooldownCoroutine;
    private bool isShieldActive;

    [SerializeField]
    private float[] cooldownTimes = { 30f, 25f, 20f, 15f, 10f };

    public override void Apply(Player player)
    {
        playerInstance = player;

        if (currentLevel == 0)
        {
            ActivateBarrierVisual();
        }
    }

    protected override int GetNextLevelIncrease()
    {
        return currentLevel < maxLevel ? currentLevel + 1 : 0;
    }

    public void ActivateBarrierVisual()
    {
        isShieldActive = true;

        if (shieldPrefab != null)
        {
            if (activeShield != null)
            {
                Destroy(activeShield);
            }

            activeShield = Instantiate(shieldPrefab, playerInstance.transform);
            activeShield.transform.SetParent(playerInstance.transform);
        }
    }

    public void DeactivateBarrierVisual()
    {
        isShieldActive = false;

        if (activeShield != null)
        {
            Destroy(activeShield);

            if (shieldDeactivateEffectPrefab != null)
            {
                // ÀÌÆåÆ® »ý¼º
                GameObject effectInstance = Instantiate(shieldDeactivateEffectPrefab, playerInstance.transform.position, Quaternion.identity);

                Destroy(effectInstance, 1f);
            }
        }
    }

    public bool IsShieldActive()
    {
        return isShieldActive;
    }

    public void StartCooldown()
    {
        if (cooldownCoroutine == null)
        {
            cooldownCoroutine = playerInstance.StartCoroutine(BarrierCooldown());
        }
    }

    private IEnumerator BarrierCooldown()
    {
        if (currentLevel - 1 >= 0 && currentLevel - 1 < cooldownTimes.Length)
        {
            yield return new WaitForSeconds(cooldownTimes[currentLevel - 1]);
        }

        if (playerInstance != null)
        {
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
