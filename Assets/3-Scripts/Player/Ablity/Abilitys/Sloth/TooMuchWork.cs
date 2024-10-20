using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/TooMuchWork")]
public class TooMuchWork : Ability
{
    [Tooltip("레벨별 최대 공격 속도 배율")]
    public float[] maxAttackSpeedMultipliers = { 2.0f, 2.5f, 3.0f, 3.5f, 4.0f }; // 레벨별 최대 공격 속도 배율 배열

    [Tooltip("레벨별 최대 공격 속도에 도달하는 시간 (초)")]
    public float[] baseTimeToMaxSpeedLevels = { 5.0f, 4.5f, 4.0f, 3.5f, 3.0f }; // 레벨별 최대 속도 도달 시간 배열

    [Tooltip("레벨별 과열 지속 시간 (초)")]
    public float[] overheatDurationsLevels = { 5.0f, 4.5f, 4.0f, 3.5f, 3.0f }; // 레벨별 과열 지속 시간 배열

    private const float minAttackCooldown = 0.15f; // 최소 공격 속도 (쿨다운)

    private Coroutine overheatCoroutine;
    private Coroutine attackSpeedCoroutine;
    private Player playerInstance;
    private bool isOverheated = false;

    // 현재 레벨에 따른 변수들
    private float maxAttackSpeedMultiplier;
    private float baseTimeToMaxSpeed;
    private float overheatDuration;

    public override void Apply(Player player)
    {
        playerInstance = player;

        if (playerInstance == null)
        {
            return;
        }

        if (currentLevel == 0)
        {
            player.OnShoot.AddListener(HandleShooting);
            player.OnShootCanceled.AddListener(HandleShootCanceled); // 리스너 추가
        }

        ApplyLevelEffects();
    }

    private void ApplyLevelEffects()
    {
        if (currentLevel < maxAttackSpeedMultipliers.Length)
        {
            maxAttackSpeedMultiplier = maxAttackSpeedMultipliers[currentLevel];
            baseTimeToMaxSpeed = baseTimeToMaxSpeedLevels[currentLevel];
            overheatDuration = overheatDurationsLevels[currentLevel];
        }
        else
        {
            maxAttackSpeedMultiplier = maxAttackSpeedMultipliers[maxAttackSpeedMultipliers.Length - 1];
            baseTimeToMaxSpeed = baseTimeToMaxSpeedLevels[baseTimeToMaxSpeedLevels.Length - 1];
            overheatDuration = overheatDurationsLevels[overheatDurationsLevels.Length - 1];
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
            ApplyLevelEffects();
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxAttackSpeedMultipliers.Length)
        {
            return Mathf.RoundToInt(maxAttackSpeedMultipliers[currentLevel + 1] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    private void HandleShooting(Vector2 direction, int prefabIndex, GameObject projectile)
    {
        if (isOverheated || playerInstance == null) return;

        if (attackSpeedCoroutine != null)
        {
            playerInstance.StopCoroutine(attackSpeedCoroutine);
        }

        attackSpeedCoroutine = playerInstance.StartCoroutine(IncreaseAttackSpeed());
    }

    private void HandleShootCanceled()
    {
        if (attackSpeedCoroutine != null && playerInstance != null)
        {
            playerInstance.StopCoroutine(attackSpeedCoroutine);
            attackSpeedCoroutine = null;
        }

        if (playerInstance != null)
        {
            playerInstance.stat.currentShootCooldown = playerInstance.stat.defaultShootCooldown;
        }
    }

    private IEnumerator IncreaseAttackSpeed()
    {
        if (playerInstance == null)
        {
            yield break;
        }

        float elapsedTime = 0f;
        float originalCooldown = playerInstance.stat.currentShootCooldown;

        while (elapsedTime < baseTimeToMaxSpeed)
        {
            if (!playerInstance.isShooting)
            {
                playerInstance.stat.currentShootCooldown = originalCooldown;
                yield break;
            }

            elapsedTime += Time.unscaledDeltaTime;
            float newCooldown = Mathf.Lerp(originalCooldown, originalCooldown / maxAttackSpeedMultiplier, elapsedTime / baseTimeToMaxSpeed);

            if (newCooldown <= minAttackCooldown)
            {
                playerInstance.stat.currentShootCooldown = minAttackCooldown;
                TriggerOverheat();
                yield break;
            }

            playerInstance.stat.currentShootCooldown = newCooldown;

            yield return null;
        }
    }

    private void TriggerOverheat()
    {
        if (playerInstance == null)
        {
            return;
        }

        if (overheatCoroutine != null)
        {
            playerInstance.StopCoroutine(overheatCoroutine);
        }
        overheatCoroutine = playerInstance.StartCoroutine(Overheat());
    }

    private IEnumerator Overheat()
    {
        if (playerInstance == null)
        {
            yield break;
        }

        isOverheated = true;
        playerInstance.stat.currentShootCooldown = Mathf.Infinity;

        yield return new WaitForSeconds(overheatDuration);

        if (playerInstance != null)
        {
            isOverheated = false;
            playerInstance.stat.currentShootCooldown = playerInstance.stat.defaultShootCooldown;
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        if (playerInstance != null)
        {
            if (overheatCoroutine != null)
            {
                playerInstance.StopCoroutine(overheatCoroutine);
                overheatCoroutine = null;
            }
            if (attackSpeedCoroutine != null)
            {
                playerInstance.StopCoroutine(attackSpeedCoroutine);
                attackSpeedCoroutine = null;
            }

            playerInstance.stat.currentShootCooldown = playerInstance.stat.defaultShootCooldown;
        }

        isOverheated = false;
    }

    public override string GetDescription()
    {
        if (currentLevel < maxAttackSpeedMultipliers.Length && currentLevel >= 0)
        {
            float maxAttackSpeedMultiplierPercent = maxAttackSpeedMultipliers[currentLevel] * 100f;
            float baseTimeToMaxSpeedValue = baseTimeToMaxSpeedLevels[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 공격 시 최대 공격 속도 배율을 {maxAttackSpeedMultiplierPercent}%까지 증가시키며, 최대 속도에 도달하는 시간이 {baseTimeToMaxSpeedValue}초로 단축됩니다.";
        }
        else if (currentLevel >= maxAttackSpeedMultipliers.Length)
        {
            float maxAttackSpeedMultiplierPercent = maxAttackSpeedMultipliers[maxAttackSpeedMultipliers.Length - 1] * 100f;
            float baseTimeToMaxSpeedValue = baseTimeToMaxSpeedLevels[baseTimeToMaxSpeedLevels.Length - 1];
            return $"{baseDescription}\n최대 레벨 도달: 공격 시 최대 공격 속도 배율을 {maxAttackSpeedMultiplierPercent}%까지 증가시키며, 최대 속도에 도달하는 시간이 {baseTimeToMaxSpeedValue}초로 단축됩니다.";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달.";
        }
    }
}
