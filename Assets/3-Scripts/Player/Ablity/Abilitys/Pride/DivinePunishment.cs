using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AK.Wwise; // WWISE 네임스페이스 추가

[CreateAssetMenu(menuName = "Abilities/DivinePunishment")]
public class DivinePunishment : Ability
{
    [Header("DivinePunishment Settings")]
    [Tooltip("레벨별 능력 발동 쿨다운 (초)")]
    public float[] cooldownPerLevel = { 5f, 4.5f, 4f, 3.5f, 3f }; // 레벨 1~5

    [Tooltip("레벨별 번개의 피해량")]
    public int[] damagePerLevel = { 30, 40, 50, 60, 70 }; // 레벨 1~5

    [Tooltip("레벨별 번개를 떨어뜨릴 최대 적의 수")]
    public int[] maxTargetsPerLevel = { 3, 4, 5, 6, 7 }; // 레벨 1~5

    [Tooltip("레벨별 번개 범위 반경")]
    public float[] rangePerLevel = { 10f, 12f, 14f, 16f, 18f }; // 레벨 1~5

    [Tooltip("번개 이펙트 프리팹")]
    public GameObject lightningPrefab;

    [Header("WWISE Sound Events")]
    [Tooltip("DivinePunishment 능력 발동 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event activateSound;

    [Tooltip("DivinePunishment 업그레이드 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event upgradeSound;

    [Tooltip("DivinePunishment 제거 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event deactivateSound;

    private Player playerInstance;
    private Coroutine abilityCoroutine;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
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

        // DivinePunishment 능력 발동 시 WWISE 사운드 재생
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 데미지, 쿨타임, 사거리, 최대 타겟 수를 증가시킵니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            // 현재 레벨에 맞는 파라미터를 적용합니다.
            UpdateDivinePunishmentParameters();

            // 업그레이드 시 WWISE 사운드 재생
            if (upgradeSound != null)
            {
                upgradeSound.Post(playerInstance.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("DivinePunishment: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        if (abilityCoroutine != null)
        {
            playerInstance.StopCoroutine(abilityCoroutine);
            abilityCoroutine = null;
        }

        currentLevel = 0;

        // DivinePunishment 제거 시 WWISE 사운드 재생
        if (deactivateSound != null && playerInstance != null)
        {
            deactivateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// DivinePunishment의 파라미터를 현재 레벨에 맞게 업데이트합니다.
    /// </summary>
    private void UpdateDivinePunishmentParameters()
    {
        // 현재 레벨에 맞는 쿨타임, 데미지, 사거리, 최대 타겟 수를 적용합니다.
        // 필요에 따라 다른 파라미터도 업데이트할 수 있습니다.
        // 예: 특정 파라미터가 변경될 경우 추가 로직을 구현할 수 있습니다.
    }

    /// <summary>
    /// DivinePunishment 발동 코루틴입니다. 쿨타임마다 번개를 떨어뜨립니다.
    /// </summary>
    private IEnumerator AbilityCoroutine()
    {
        while (true)
        {
            float currentCooldown = GetCurrentCooldown();
            yield return new WaitForSeconds(currentCooldown);
            StrikeLightning();
        }
    }

    /// <summary>
    /// 번개를 떨어뜨립니다.
    /// </summary>
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

        // DivinePunishment 능력 발동 시 WWISE 사운드 재생
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// 주변의 몬스터들을 찾습니다.
    /// </summary>
    /// <returns>주변 몬스터 리스트</returns>
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

    /// <summary>
    /// 능력의 설명을 반환합니다. 설명은 현재 레벨보다 1레벨 더 높은 레벨의 정보를 포함합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            // 다음 레벨의 인덱스는 currentLevel (currentLevel은 0부터 시작)
            int nextLevelIndex = currentLevel;
            float nextLevelCooldown = (nextLevelIndex < cooldownPerLevel.Length) ? cooldownPerLevel[nextLevelIndex] : cooldownPerLevel[cooldownPerLevel.Length - 1];
            int nextLevelDamage = (nextLevelIndex < damagePerLevel.Length) ? damagePerLevel[nextLevelIndex] : damagePerLevel[damagePerLevel.Length - 1];
            float nextLevelRange = (nextLevelIndex < rangePerLevel.Length) ? rangePerLevel[nextLevelIndex] : rangePerLevel[rangePerLevel.Length - 1];
            int nextLevelMaxTargets = (nextLevelIndex < maxTargetsPerLevel.Length) ? maxTargetsPerLevel[nextLevelIndex] : maxTargetsPerLevel[maxTargetsPerLevel.Length - 1];

            return $"{baseDescription}\n" +
                   $"Lv {currentLevel + 1}:\n" +
                   $"- 쿨타임: {nextLevelCooldown}초\n" +
                   $"- 피해량: {nextLevelDamage}\n" +
                   $"- 사거리: {nextLevelRange}m\n" +
                   $"- 최대 타겟 수: {nextLevelMaxTargets}\n";
        }
        else
        {
            // 최대 레벨 설명
            int maxLevelIndex = currentLevel - 1;
            float finalCooldown = (maxLevelIndex < cooldownPerLevel.Length) ? cooldownPerLevel[maxLevelIndex] : cooldownPerLevel[cooldownPerLevel.Length - 1];
            int finalDamage = (maxLevelIndex < damagePerLevel.Length) ? damagePerLevel[maxLevelIndex] : damagePerLevel[damagePerLevel.Length - 1];
            float finalRange = (maxLevelIndex < rangePerLevel.Length) ? rangePerLevel[maxLevelIndex] : rangePerLevel[rangePerLevel.Length - 1];
            int finalMaxTargets = (maxLevelIndex < maxTargetsPerLevel.Length) ? maxTargetsPerLevel[maxLevelIndex] : maxTargetsPerLevel[maxTargetsPerLevel.Length - 1];

            return $"{baseDescription}\n" +
                   $"Max Level: {currentLevel}\n" +
                   $"- 쿨타임: {finalCooldown}초\n" +
                   $"- 피해량: {finalDamage}\n" +
                   $"- 사거리: {finalRange}m\n" +
                   $"- 최대 타겟 수: {finalMaxTargets}\n";
        }
    }

    /// <summary>
    /// 현재 레벨에 맞는 쿨타임을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 쿨타임</returns>
    private float GetCurrentCooldown()
    {
        if (currentLevel == 0)
        {
            return cooldownPerLevel[0];
        }
        else if (currentLevel - 1 < cooldownPerLevel.Length)
        {
            return cooldownPerLevel[currentLevel - 1];
        }

        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})이 cooldownPerLevel 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return cooldownPerLevel[cooldownPerLevel.Length - 1];
    }

    /// <summary>
    /// 현재 레벨에 맞는 피해량을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 피해량</returns>
    private int GetCurrentDamage()
    {
        if (currentLevel == 0)
        {
            return damagePerLevel[0];
        }
        else if (currentLevel - 1 < damagePerLevel.Length)
        {
            return damagePerLevel[currentLevel - 1];
        }
        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})이 damagePerLevel 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return damagePerLevel[damagePerLevel.Length - 1];
    }

    /// <summary>
    /// 현재 레벨에 맞는 사거리를 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 사거리</returns>
    private float GetCurrentRange()
    {
        if (currentLevel == 0)
        {
            return rangePerLevel[0];
        }
        else if (currentLevel - 1 < rangePerLevel.Length)
        {
            return rangePerLevel[currentLevel - 1];
        }
        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})이 rangePerLevel 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return rangePerLevel[rangePerLevel.Length - 1];
    }

    /// <summary>
    /// 현재 레벨에 맞는 최대 타겟 수를 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 최대 타겟 수</returns>
    private int GetCurrentMaxTargets()
    {
        if (currentLevel == 0)
        {
            return maxTargetsPerLevel[0];
        }
        else if (currentLevel - 1 < maxTargetsPerLevel.Length)
        {
            return maxTargetsPerLevel[currentLevel - 1];
        }
        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})이 maxTargetsPerLevel 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return maxTargetsPerLevel[maxTargetsPerLevel.Length - 1];
    }

    /// <summary>
    /// 다음 레벨 증가에 필요한 값을 반환합니다.
    /// (이 메서드는 더 이상 사용되지 않으므로 제거할 수 있습니다.)
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        // 더 이상 사용되지 않으므로 0을 반환하거나 메서드를 제거할 수 있습니다.
        return 0;
    }

    /// <summary>
    /// OnValidate 메서드를 통해 배열의 길이를 maxLevel과 일치시킵니다.
    /// </summary>
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

    /// <summary>
    /// Gizmos를 사용하여 DivinePunishment 발사 방향 시각화
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (playerInstance != null)
        {
            Vector2 facingDirection = playerInstance.GetFacingDirection();
            Vector2 backwardDirection = -facingDirection;

            Vector3 origin = playerInstance.transform.position;
            Vector3 direction = backwardDirection * 5f; // 예시: 5 단위 길이

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + (Vector3)direction);
            Gizmos.DrawSphere(origin + (Vector3)direction, 0.2f);
        }
    }
}
