using System.Collections;
using UnityEngine;
using AK.Wwise; // WWISE 네임스페이스 추가

[CreateAssetMenu(menuName = "Abilities/TooMuchWork")]
public class TooMuchWork : Ability
{
    [Tooltip("레벨별 최대 공격 속도 배율")]
    public float[] maxAttackSpeedMultipliers = { 2.0f, 2.5f, 3.0f, 3.5f, 4.0f };

    [Tooltip("레벨별 최대 공격 속도에 도달하는 시간 (초)")]
    public float[] baseTimeToMaxSpeedLevels = { 5.0f, 4.5f, 4.0f, 3.5f, 3.0f };

    [Tooltip("레벨별 과열 지속 시간 (초)")]
    public float[] overheatDurationsLevels = { 5.0f, 4.5f, 4.0f, 3.5f, 3.0f };

    // WWISE 이벤트 변수들
    [Tooltip("공격 속도 증가 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event attackSpeedIncreaseSound;

    private Coroutine overheatCoroutine;
    private Coroutine attackSpeedCoroutine;
    private Player playerInstance;
    private bool isOverheated = false;
    private bool isListenerRegistered = false;

    // 현재 레벨에 따른 변수들
    private float maxAttackSpeedMultiplier;
    private float baseTimeToMaxSpeed;
    private float overheatDuration;

    public override void Apply(Player player)
    {
        Debug.Log(currentLevel);
        playerInstance = player;

        if (playerInstance == null)
        {
            return;
        }

        player.OnShoot.AddListener(HandleShooting);
        player.OnShootCanceled.AddListener(HandleShootCanceled);
        isListenerRegistered = true;
        ApplyLevelEffects();
    }

    private void ApplyLevelEffects()
    {
        int levelIndex = Mathf.Clamp(currentLevel, 0, maxAttackSpeedMultipliers.Length - 1);

        maxAttackSpeedMultiplier = maxAttackSpeedMultipliers[levelIndex];
        baseTimeToMaxSpeed = baseTimeToMaxSpeedLevels[levelIndex];
        overheatDuration = overheatDurationsLevels[levelIndex];
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            ApplyLevelEffects();

            // 업그레이드 시 공격 속도 증가 사운드 재생
            if (attackSpeedIncreaseSound != null)
            {
                attackSpeedIncreaseSound.Post(playerInstance.gameObject);
            }
        }
    }

    protected override int GetNextLevelIncrease()
    {
        int levelIndex = currentLevel; // 다음 레벨을 미리 보기 위해 currentLevel 사용

        if (levelIndex < maxAttackSpeedMultipliers.Length)
        {
            return Mathf.RoundToInt(maxAttackSpeedMultipliers[levelIndex] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    private void HandleShooting(Vector2 direction, int prefabIndex, GameObject projectile)
    {
        Debug.Log("HandleShooting called.");

        if (isOverheated || playerInstance == null) return;

        if (attackSpeedCoroutine == null)
        {
            attackSpeedCoroutine = playerInstance.StartCoroutine(IncreaseAttackSpeed());
        }
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
            playerInstance.stat.currentAttackSpeed = playerInstance.stat.defaultAttackSpeed;
        }
    }

    private IEnumerator IncreaseAttackSpeed()
    {
        if (playerInstance == null)
        {
            Debug.Log("IncreaseAttackSpeed: playerInstance is null, exiting coroutine.");
            yield break;
        }

        Debug.Log("IncreaseAttackSpeed: Coroutine started.");

        float elapsedTime = 0f;
        float originalAttackSpeed = playerInstance.stat.defaultAttackSpeed;
        float targetAttackSpeed = originalAttackSpeed * maxAttackSpeedMultiplier;

        while (elapsedTime < baseTimeToMaxSpeed)
        {
            if (!playerInstance.isShooting)
            {
                Debug.Log("IncreaseAttackSpeed: isShooting is false, resetting attack speed and exiting coroutine.");
                playerInstance.stat.currentAttackSpeed = originalAttackSpeed;
                attackSpeedCoroutine = null;
                yield break;
            }

            if (playerInstance.IsOverheated)
            {
                Debug.Log("IncreaseAttackSpeed: isOverheated is true, exiting coroutine.");
                attackSpeedCoroutine = null;
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / baseTimeToMaxSpeed;
            float newAttackSpeed = Mathf.Lerp(originalAttackSpeed, targetAttackSpeed, progress);

            playerInstance.stat.currentAttackSpeed = newAttackSpeed;

            yield return null;
        }

        // 최대 공격 속도 도달
        playerInstance.stat.currentAttackSpeed = targetAttackSpeed;
        // 과열 상태 트리거 및 사운드 재생
        TriggerOverheat();

        // 코루틴 참조 초기화
        attackSpeedCoroutine = null;
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

        playerInstance.IsOverheated = true; // 과열 상태 시작

        yield return new WaitForSeconds(overheatDuration);

        if (playerInstance != null)
        {
            playerInstance.IsOverheated = false; // 과열 상태 종료
            playerInstance.stat.currentAttackSpeed = playerInstance.stat.defaultAttackSpeed;

            // 공격이 재개될 수 있도록 nextShootTime 재설정
            playerInstance.ResetNextShootTime();
        }
    }

    public override void ResetLevel()
    {
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

            playerInstance.stat.currentAttackSpeed = playerInstance.stat.defaultAttackSpeed;

            // 이벤트 리스너 제거
            if (isListenerRegistered)
            {
                playerInstance.OnShoot.RemoveListener(HandleShooting);
                playerInstance.OnShootCanceled.RemoveListener(HandleShootCanceled);
                isListenerRegistered = false;
            }
        }
        currentLevel = 0;
        isOverheated = false;
    }

    public override string GetDescription()
    {
        int levelIndex = Mathf.Clamp(currentLevel, 0, maxAttackSpeedMultipliers.Length - 1);

        if (currentLevel < maxAttackSpeedMultipliers.Length && currentLevel >= 0)
        {
            float maxAttackSpeedMultiplierPercent = maxAttackSpeedMultipliers[levelIndex] * 100f;
            float baseTimeToMaxSpeedValue = baseTimeToMaxSpeedLevels[levelIndex];
            float overheatDurationValue = overheatDurationsLevels[levelIndex];

            return $"{baseDescription}\nLv {currentLevel + 1}: 공격 시 최대 공격 속도 배율을 {maxAttackSpeedMultiplierPercent}%까지 증가시키며, 최대 속도에 도달하는 시간이 {baseTimeToMaxSpeedValue}초입니다. 최대 속도 도달 시 과열되어 {overheatDurationValue}초 동안 공격할 수 없습니다.";
        }
        else if (currentLevel > maxAttackSpeedMultipliers.Length)
        {
            float maxAttackSpeedMultiplierPercent = maxAttackSpeedMultipliers[maxAttackSpeedMultipliers.Length - 1] * 100f;
            float baseTimeToMaxSpeedValue = baseTimeToMaxSpeedLevels[baseTimeToMaxSpeedLevels.Length - 1];
            float overheatDurationValue = overheatDurationsLevels[overheatDurationsLevels.Length - 1];

            return $"{baseDescription}\n최대 레벨 도달: 공격 시 최대 공격 속도 배율을 {maxAttackSpeedMultiplierPercent}%까지 증가시키며, 최대 속도에 도달하는 시간이 {baseTimeToMaxSpeedValue}초입니다. 최대 속도 도달 시 과열되어 {overheatDurationValue}초 동안 공격할 수 없습니다.";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달.";
        }
    }
}
