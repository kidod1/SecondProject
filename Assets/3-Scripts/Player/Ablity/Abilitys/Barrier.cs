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
                // 이펙트 생성
                GameObject effectInstance = Instantiate(shieldDeactivateEffectPrefab, playerInstance.transform.position, Quaternion.identity);

                // 일정 시간 후에 이펙트 삭제 (예: 2초 후)
                Destroy(effectInstance, 2f);
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
        // 쿨타임 대기
        if (currentLevel - 1 >= 0 && currentLevel - 1 < cooldownTimes.Length)
        {
            yield return new WaitForSeconds(cooldownTimes[currentLevel - 1]);
        }

        // 쿨타임이 끝난 후 Barrier 다시 활성화
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
