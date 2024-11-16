using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Barrier")]
public class Barrier : Ability
{
    [Header("Barrier Visuals")]
    [Tooltip("방패의 프리팹")]
    public GameObject shieldPrefab;

    [Tooltip("실드 브레이크 이펙트 프리팹")] // 추가된 변수
    public GameObject shieldBreakEffectPrefab;

    [Header("Cooldown Settings")]
    [Tooltip("각 레벨에서 방패의 쿨타임 시간 (초)")]
    public int[] cooldownTimes = { 30, 25, 20, 15, 10 }; // 레벨 1~5

    private GameObject activeShield;
    private Player playerInstance;
    private Coroutine cooldownCoroutine;
    private bool isShieldActive;


    private void start()
    {
        playerInstance = PlayManager.I.GetPlayer();
    }
    /// <summary>
    /// Ability 클래스의 GetNextLevelIncrease() 메서드를 오버라이드하여 현재 레벨에 해당하는 쿨타임 시간을 반환합니다.
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < cooldownTimes.Length)
        {
            return cooldownTimes[currentLevel];
        }
        Debug.LogWarning($"Barrier: currentLevel ({currentLevel})이 cooldownTimes 배열의 범위를 벗어났습니다. 기본값 0을 반환합니다.");
        return 0;
    }

    /// <summary>
    /// 방패 능력을 플레이어에게 적용합니다. 초기 레벨에서는 방패를 활성화합니다.
    /// </summary>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("Barrier Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;
        if (currentLevel == 1)
        {
            ActivateBarrierVisual();
        }
    }

    /// <summary>
    /// 방패 능력을 업그레이드합니다. 레벨이 증가할 때마다 쿨타임이 감소합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            Debug.Log($"Barrier 업그레이드: 현재 레벨 {currentLevel}");

            // 업그레이드 후 쿨타임 감소 값 적용
            if (isShieldActive)
            {
                if (cooldownCoroutine != null)
                {
                    playerInstance.StopCoroutine(cooldownCoroutine);
                    cooldownCoroutine = null;
                }
                StartCooldown();
            }
        }
        else
        {
            Debug.LogWarning("Barrier: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 방패의 시각적 효과를 활성화하고 쿨타임을 시작합니다.
    /// </summary>
    public void ActivateBarrierVisual()
    {
        if (playerInstance == null)
        {
            Debug.LogWarning("Barrier Activate: playerInstance가 설정되지 않았습니다. Apply 메서드를 먼저 호출하세요.");
            return;
        }

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
        else
        {
            Debug.LogError("Barrier: shieldPrefab이 할당되지 않았습니다.");
            return;
        }

        // 방패 활성화 후 쿨타임 시작
        StartCooldown();
    }

    /// <summary>
    /// 방패의 시각적 효과를 비활성화하고 쿨타임을 시작합니다.
    /// </summary>
    public void DeactivateBarrierVisual()
    {
        if (playerInstance == null)
        {
            Debug.LogWarning("Barrier Deactivate: playerInstance가 설정되지 않았습니다.");
            return;
        }

        isShieldActive = false;

        if (activeShield != null)
        {
            // 실드 브레이크 이펙트 생성
            if (shieldBreakEffectPrefab != null)
            {
                Instantiate(shieldBreakEffectPrefab, activeShield.transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Barrier: shieldBreakEffectPrefab이 할당되지 않았습니다.");
            }

            // 실드 오브젝트 파괴
            Destroy(activeShield);
            activeShield = null;
        }

        // 쿨다운 시작
        StartCooldown();
    }

    /// <summary>
    /// 방패가 현재 활성화되어 있는지 확인합니다.
    /// </summary>
    public bool IsShieldActive()
    {
        return isShieldActive;
    }
    /// <summary>
    /// 쿨타임을 시작합니다. 이미 쿨타임이 진행 중인 경우 중복으로 시작하지 않습니다.
    /// </summary>
    public void StartCooldown()
    {
        if (playerInstance == null)
        {
            Debug.LogWarning("Barrier StartCooldown: playerInstance가 설정되지 않았습니다. Apply 메서드를 먼저 호출하세요.");
            return;
        }

        if (cooldownCoroutine == null)
        {
            float cooldownTime = GetCurrentCooldownTime();
            Debug.Log($"Barrier: 쿨타임 시작 ({cooldownTime}초)");

            cooldownCoroutine = playerInstance.StartCoroutine(BarrierCooldown(cooldownTime));
        }
    }

    /// <summary>
    /// 쿨타임이 완료되면 방패를 다시 활성화합니다.
    /// </summary>
    private IEnumerator BarrierCooldown(float cooldownTime)
    {
        yield return new WaitForSeconds(cooldownTime);

        if (playerInstance != null)
        {
            ActivateBarrierVisual();
        }

        cooldownCoroutine = null;
    }

    /// <summary>
    /// 현재 레벨에 맞는 쿨타임 시간을 반환합니다.
    /// </summary>
    private float GetCurrentCooldownTime()
    {
        if (currentLevel < cooldownTimes.Length)
        {
            return cooldownTimes[currentLevel];
        }
        Debug.LogWarning($"Barrier: currentLevel ({currentLevel})이 cooldownTimes 배열의 범위를 벗어났습니다. 기본값 0을 반환합니다.");
        return 0f;
    }

    /// <summary>
    /// Ability 클래스의 GetDescription() 메서드를 오버라이드하여 방패의 현재 쿨타임 시간을 설명에 포함시킵니다.
    /// </summary>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            int currentCooldown = GetNextLevelIncrease();
            return $"{baseDescription}\nLv {currentLevel + 1}: {currentCooldown}초 쿨타임";
        }
        else
        {
            int finalCooldown = GetNextLevelIncrease();
            return $"{baseDescription}\n(Max Level: {finalCooldown}초 쿨타임)";
        }
    }

    /// <summary>
    /// 능력의 레벨을 초기화하고 쿨타임을 중지한 후 방패를 비활성화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

        if (cooldownCoroutine != null)
        {
            playerInstance.StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        DeactivateBarrierVisual();
        currentLevel = 0;
    }

    private void OnValidate()
    {
        if (cooldownTimes.Length != maxLevel)
        {
            Debug.LogWarning($"Barrier: cooldownTimes 배열의 길이가 maxLevel ({maxLevel})과 일치하지 않습니다. 배열 길이를 맞춥니다.");
            Array.Resize(ref cooldownTimes, maxLevel);
        }
    }
}
