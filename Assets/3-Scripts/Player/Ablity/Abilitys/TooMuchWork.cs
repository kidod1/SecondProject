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

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        playerInstance = player;

        if (playerInstance == null)
        {
            Debug.LogError("Player instance is null. Cannot apply TooMuchWork ability.");
            return;
        }

        if (currentLevel == 0)
        {
            player.OnShoot.AddListener(HandleShooting);
            player.OnShootCanceled.AddListener(HandleShootCanceled); // 리스너 추가
        }

        // 레벨에 따른 효과 적용
        ApplyLevelEffects();
    }

    /// <summary>
    /// 현재 레벨에 따른 공격 속도 배율, 최대 속도 도달 시간, 과열 지속 시간을 적용합니다.
    /// </summary>
    private void ApplyLevelEffects()
    {
        if (currentLevel < maxAttackSpeedMultipliers.Length)
        {
            // 현재 레벨에 따른 변수 설정
            maxAttackSpeedMultiplier = maxAttackSpeedMultipliers[currentLevel];
            baseTimeToMaxSpeed = baseTimeToMaxSpeedLevels[currentLevel];
            overheatDuration = overheatDurationsLevels[currentLevel];
            Debug.Log($"TooMuchWork: Level {currentLevel + 1} 적용 - MaxAttackSpeedMultiplier: {maxAttackSpeedMultiplier}, BaseTimeToMaxSpeed: {baseTimeToMaxSpeed}, OverheatDuration: {overheatDuration}");
        }
        else
        {
            // 배열 범위를 초과할 경우 마지막 레벨의 값을 사용
            maxAttackSpeedMultiplier = maxAttackSpeedMultipliers[maxAttackSpeedMultipliers.Length - 1];
            baseTimeToMaxSpeed = baseTimeToMaxSpeedLevels[baseTimeToMaxSpeedLevels.Length - 1];
            overheatDuration = overheatDurationsLevels[overheatDurationsLevels.Length - 1];
            Debug.LogWarning($"TooMuchWork: currentLevel ({currentLevel + 1}) exceeds 배열 범위. 마지막 레벨의 값을 사용합니다.");
        }
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 공격 속도 배율이 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel이 5라면 currentLevel은 0~4
        {
            currentLevel++;
            Debug.Log($"TooMuchWork 업그레이드: 현재 레벨 {currentLevel + 1}, MaxAttackSpeedMultiplier {maxAttackSpeedMultipliers[currentLevel]}");
            ApplyLevelEffects(); // 업그레이드 후 레벨에 따른 효과 재적용
        }
        else
        {
            Debug.LogWarning("TooMuchWork: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 다음 레벨의 공격 속도 배율을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨의 공격 속도 배율 (퍼센트)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxAttackSpeedMultipliers.Length)
        {
            return Mathf.RoundToInt(maxAttackSpeedMultipliers[currentLevel + 1] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    /// <summary>
    /// 플레이어가 공격할 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="direction">공격 방향</param>
    /// <param name="prefabIndex">공격 프리팹 인덱스</param>
    /// <param name="projectile">발사된 투사체</param>
    private void HandleShooting(Vector2 direction, int prefabIndex, GameObject projectile)
    {
        if (isOverheated || playerInstance == null) return;

        if (attackSpeedCoroutine != null)
        {
            playerInstance.StopCoroutine(attackSpeedCoroutine);
        }

        attackSpeedCoroutine = playerInstance.StartCoroutine(IncreaseAttackSpeed());
    }

    /// <summary>
    /// 공격 취소 시 호출되는 메서드입니다.
    /// </summary>
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

    /// <summary>
    /// 공격 속도를 증가시키는 코루틴입니다.
    /// </summary>
    /// <returns>IEnumerator</returns>
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

            // 공격 속도가 최소값에 도달하면 과열 상태로 전환
            if (newCooldown <= minAttackCooldown)
            {
                playerInstance.stat.currentShootCooldown = minAttackCooldown;
                TriggerOverheat(); // 과열 상태로 전환
                yield break;
            }

            playerInstance.stat.currentShootCooldown = newCooldown;

            yield return null;
        }
    }

    /// <summary>
    /// 과열 상태를 트리거하는 메서드입니다.
    /// </summary>
    private void TriggerOverheat()
    {
        if (playerInstance == null)
        {
            Debug.LogError("Player instance is null. Cannot trigger overheat.");
            return;
        }

        if (overheatCoroutine != null)
        {
            playerInstance.StopCoroutine(overheatCoroutine);
        }
        overheatCoroutine = playerInstance.StartCoroutine(Overheat());
    }

    /// <summary>
    /// 과열 상태를 처리하는 코루틴입니다.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator Overheat()
    {
        if (playerInstance == null)
        {
            yield break;
        }

        isOverheated = true;
        Debug.Log("Weapon overheated! Can't attack for " + overheatDuration + " seconds.");
        playerInstance.stat.currentShootCooldown = Mathf.Infinity;

        yield return new WaitForSeconds(overheatDuration);

        if (playerInstance != null)
        {
            isOverheated = false;
            playerInstance.stat.currentShootCooldown = playerInstance.stat.defaultShootCooldown;
            Debug.Log("Weapon cooled down. You can attack again.");
        }
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
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

    /// <summary>
    /// 능력의 현재 상태와 효과를 설명하는 문자열을 반환합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {
        Debug.Log($"GetDescription called. Current Level: {currentLevel + 1}, maxAttackSpeedMultipliers.Length: {maxAttackSpeedMultipliers.Length}, maxLevel: {maxLevel}");

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
