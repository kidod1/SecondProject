using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Abilities/DivinePunishment")]
public class DivinePunishment : Ability
{
    [Header("DivinePunishment Settings")]
    [Tooltip("각 레벨에서 능력 발동 쿨다운 (초)")]
    public float[] cooldownPerLevel = { 5f, 4.5f, 4f, 3.5f, 3f }; // 레벨 1~5

    [Tooltip("각 레벨에서 번개의 피해량")]
    public int[] damagePerLevel = { 30, 40, 50, 60, 70 }; // 레벨 1~5

    [Tooltip("각 레벨에서 번개를 떨어뜨릴 최대 적의 수")]
    public int[] maxTargetsPerLevel = { 3, 4, 5, 6, 7 }; // 레벨 1~5

    [Tooltip("각 레벨에서 번개 범위 반경")]
    public float[] rangePerLevel = { 10f, 12f, 14f, 16f, 18f }; // 레벨 1~5

    [Tooltip("번개 이펙트 프리팹")]
    public GameObject lightningPrefab;

    private Player playerInstance;
    private Coroutine abilityCoroutine;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("DivinePunishment Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;

        if (abilityCoroutine == null)
        {
            abilityCoroutine = player.StartCoroutine(AbilityCoroutine());
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
            Debug.Log($"DivinePunishment 업그레이드: 현재 레벨 {currentLevel + 1}");
        }
        else
        {
            Debug.LogWarning("DivinePunishment: 이미 최대 레벨에 도달했습니다.");
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        if (abilityCoroutine != null)
        {
            playerInstance.StopCoroutine(abilityCoroutine);
            abilityCoroutine = null;
        }

        currentLevel = 0;
    }

    private IEnumerator AbilityCoroutine()
    {
        while (true)
        {
            float currentCooldown = GetCurrentCooldown();
            yield return new WaitForSeconds(currentCooldown);
            StrikeLightning();
        }
    }

    private void StrikeLightning()
    {
        if (lightningPrefab == null)
        {
            Debug.LogError("DivinePunishment: lightningPrefab이 할당되지 않았습니다.");
            return;
        }

        List<Monster> nearbyMonsters = FindNearbyMonsters();

        int maxTargets = GetCurrentMaxTargets();
        int targets = Mathf.Min(maxTargets, nearbyMonsters.Count);

        for (int i = 0; i < targets; i++)
        {
            Monster monster = nearbyMonsters[i];

            // 번개 이펙트 생성
            Vector3 strikePosition = monster.transform.position;
            GameObject lightningInstance = Instantiate(lightningPrefab, strikePosition, Quaternion.identity);

            // 2초 후에 번개 이펙트 삭제
            Destroy(lightningInstance, 2f);

            // 몬스터에게 피해 적용
            int currentDamage = GetCurrentDamage();
            monster.TakeDamage(currentDamage, playerInstance.transform.position);
        }
    }

    private List<Monster> FindNearbyMonsters()
    {
        List<Monster> nearbyMonsters = new List<Monster>();

        float currentRange = GetCurrentRange();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(playerInstance.transform.position, currentRange);

        foreach (Collider2D collider in colliders)
        {
            Monster monster = collider.GetComponent<Monster>();
            if (monster != null)
            {
                nearbyMonsters.Add(monster);
            }
        }

        // 거리 순으로 정렬
        nearbyMonsters.Sort((a, b) =>
        {
            float distanceA = Vector2.Distance(playerInstance.transform.position, a.transform.position);
            float distanceB = Vector2.Distance(playerInstance.transform.position, b.transform.position);
            return distanceA.CompareTo(distanceB);
        });

        return nearbyMonsters;
    }

    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";
        description += $"레벨 {currentLevel + 1}:\n";
        description += $"- 쿨타임: {GetCurrentCooldown()}초\n";
        description += $"- 피해량: {GetCurrentDamage()}\n";
        description += $"- 범위: {GetCurrentRange()}m\n";
        description += $"- 최대 타겟 수: {GetCurrentMaxTargets()}";

        return description;
    }

    // 현재 레벨에 맞는 쿨타임을 반환합니다.
    private float GetCurrentCooldown()
    {
        if (currentLevel < cooldownPerLevel.Length)
        {
            return cooldownPerLevel[currentLevel];
        }
        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})이 cooldownPerLevel 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return cooldownPerLevel[cooldownPerLevel.Length - 1];
    }

    // 현재 레벨에 맞는 피해량을 반환합니다.
    private int GetCurrentDamage()
    {
        if (currentLevel < damagePerLevel.Length)
        {
            return damagePerLevel[currentLevel];
        }
        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})이 damagePerLevel 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return damagePerLevel[damagePerLevel.Length - 1];
    }

    // 현재 레벨에 맞는 범위를 반환합니다.
    private float GetCurrentRange()
    {
        if (currentLevel < rangePerLevel.Length)
        {
            return rangePerLevel[currentLevel];
        }
        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})이 rangePerLevel 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return rangePerLevel[rangePerLevel.Length - 1];
    }

    // 현재 레벨에 맞는 최대 타겟 수를 반환합니다.
    private int GetCurrentMaxTargets()
    {
        if (currentLevel < maxTargetsPerLevel.Length)
        {
            return maxTargetsPerLevel[currentLevel];
        }
        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})이 maxTargetsPerLevel 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return maxTargetsPerLevel[maxTargetsPerLevel.Length - 1];
    }

    // GetNextLevelIncrease 메서드 구현
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel && damagePerLevel.Length > currentLevel + 1)
        {
            int nextDamageIncrease = damagePerLevel[currentLevel + 1] - damagePerLevel[currentLevel];
            return nextDamageIncrease;
        }

        return 0;
    }

    private void OnValidate()
    {
        // 배열의 길이가 maxLevel과 일치하도록 조정
        if (cooldownPerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"DivinePunishment: cooldownPerLevel 배열의 길이가 maxLevel ({maxLevel})과 일치하지 않습니다. 배열 길이를 맞춥니다.");
            System.Array.Resize(ref cooldownPerLevel, maxLevel);
        }

        if (damagePerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"DivinePunishment: damagePerLevel 배열의 길이가 maxLevel ({maxLevel})과 일치하지 않습니다. 배열 길이를 맞춥니다.");
            System.Array.Resize(ref damagePerLevel, maxLevel);
        }

        if (rangePerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"DivinePunishment: rangePerLevel 배열의 길이가 maxLevel ({maxLevel})과 일치하지 않습니다. 배열 길이를 맞춥니다.");
            System.Array.Resize(ref rangePerLevel, maxLevel);
        }

        if (maxTargetsPerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"DivinePunishment: maxTargetsPerLevel 배열의 길이가 maxLevel ({maxLevel})과 일치하지 않습니다. 배열 길이를 맞춥니다.");
            System.Array.Resize(ref maxTargetsPerLevel, maxLevel);
        }
    }
}
