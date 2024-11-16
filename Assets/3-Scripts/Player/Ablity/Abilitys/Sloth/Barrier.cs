using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise; // WWISE 네임스페이스 추가

[CreateAssetMenu(menuName = "Abilities/Barrier")]
public class Barrier : Ability
{
    [Header("Barrier Prefab")]
    [Tooltip("방패의 프리팹")]
    public GameObject barrierPrefab;

    [Header("Shield Settings")]
    [Tooltip("방패가 활성화될 위치의 오프셋")]
    public Vector3 spawnOffset = Vector3.zero;

    [Header("Cooldown Settings")]
    [Tooltip("각 레벨에서 방패의 쿨타임 시간 (초)")]
    public int[] cooldownTimes = { 30, 25, 20, 15, 10 }; // 레벨 1~5

    [Header("WWISE Sound Events")]
    [Tooltip("Barrier 방패 활성화 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event activateSound;

    [Tooltip("Barrier 방패 업그레이드 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event upgradeSound;

    [Tooltip("Barrier 방패 비활성화 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event deactivateSound;

    // Barrier 활성화 인스턴스 관리
    private GameObject activeBarrierInstance;

    /// <summary>
    /// Barrier 능력을 플레이어에게 적용합니다. 방패를 활성화합니다.
    /// </summary>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("Barrier Apply: player 인스턴스가 null입니다.");
            return;
        }

        if (activeBarrierInstance != null)
        {
            Debug.LogWarning("Barrier Apply: 이미 활성화된 방패가 존재합니다.");
            return;
        }

        // 방패 프리팹을 인스턴스화
        activeBarrierInstance = Instantiate(barrierPrefab, player.transform.position + spawnOffset, Quaternion.identity, player.transform);

        // 방패 오브젝트에 "Barrier" 태그 할당
        activeBarrierInstance.tag = "Barrier";

        // 방패 오브젝트에 Shield 컴포넌트가 있는지 확인
        Shield shieldComponent = activeBarrierInstance.GetComponent<Shield>();
        if (shieldComponent == null)
        {
            Debug.LogError("Barrier Apply: barrierPrefab에 Shield 컴포넌트가 없습니다.");
            return;
        }

        // WWISE 사운드 재생
        if (activateSound != null)
        {
            activateSound.Post(activeBarrierInstance);
        }

        Debug.Log("Barrier가 활성화되었습니다.");
    }

    /// <summary>
    /// Barrier 능력을 업그레이드합니다. 레벨이 증가할 때마다 쿨타임이 감소합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            // 레벨 증가
            currentLevel++;
            Debug.Log($"Barrier 업그레이드: 현재 레벨 {currentLevel}");

            // WWISE 업그레이드 사운드 재생
            if (upgradeSound != null && activeBarrierInstance != null)
            {
                upgradeSound.Post(activeBarrierInstance);
            }

            // 추가적인 업그레이드 로직 (예: 쿨타임 감소) 필요 시 구현
        }
        else
        {
            Debug.LogWarning("Barrier: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// Barrier 능력의 레벨을 초기화하고 방패를 비활성화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

        if (activeBarrierInstance != null)
        {
            // Barrier 비활성화
            Shield shield = activeBarrierInstance.GetComponent<Shield>();
            if (shield != null)
            {
                shield.BreakShield();
            }
            else
            {
                Debug.LogWarning("Barrier 오브젝트에 Shield 컴포넌트가 없습니다.");
                Destroy(activeBarrierInstance);
            }

            activeBarrierInstance = null;
        }

        currentLevel = 0;
    }

    /// <summary>
    /// 능력의 설명을 반환합니다.
    /// </summary>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            int nextLevelIndex = currentLevel;
            int nextLevelCooldown = (nextLevelIndex < cooldownTimes.Length) ? cooldownTimes[nextLevelIndex] : cooldownTimes[cooldownTimes.Length - 1];

            return $"{baseDescription}\n" +
                   $"Lv {currentLevel + 1}:\n" +
                   $"- 쿨타임: {nextLevelCooldown}초\n";
        }
        else
        {
            int maxLevelIndex = currentLevel - 1;
            int finalCooldown = (maxLevelIndex < cooldownTimes.Length) ? cooldownTimes[maxLevelIndex] : cooldownTimes[cooldownTimes.Length - 1];

            return $"{baseDescription}\n" +
                   $"Max Level: {currentLevel}\n" +
                   $"- 쿨타임: {finalCooldown}초\n";
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < cooldownTimes.Length)
        {
            return cooldownTimes[currentLevel];
        }
        Debug.LogWarning($"Barrier: currentLevel ({currentLevel})이 cooldownTimes 배열의 범위를 벗어났습니다. 기본값 0을 반환합니다.");
        return 0;
    }
}
