using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Abilities/DivinePunishment")]
public class DivinePunishment : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("능력 발동 쿨타임 (초)")]
    public float cooldown = 5f;

    [Tooltip("번개의 피해량")]
    public int damage = 30;

    [Tooltip("번개를 떨어뜨릴 최대 적의 수")]
    public int maxTargets = 3;

    [Tooltip("번개 이펙트 프리팹")]
    public GameObject lightningPrefab;

    [Tooltip("번개 범위 반경")]
    public float range = 10f;

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
            damage += 10;      // 피해량 증가
            range += 2f;       // 범위 증가
            maxTargets += 1;   // 타겟 수 증가
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
        damage = 30;
        range = 10f;
        maxTargets = 3;
    }

    private IEnumerator AbilityCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(cooldown);
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
            monster.TakeDamage(damage, playerInstance.transform.position);
        }
    }

    private List<Monster> FindNearbyMonsters()
    {
        List<Monster> nearbyMonsters = new List<Monster>();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(playerInstance.transform.position, range);

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
        description += $"현재 레벨: {currentLevel + 1}\n";
        description += $"쿨타임: {cooldown}초\n";
        description += $"피해량: {damage}\n";
        description += $"범위: {range}m\n";
        description += $"최대 타겟 수: {maxTargets}";

        return description;
    }

    // GetNextLevelIncrease 메서드 구현
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel)
        {
            int nextDamageIncrease = 10;  // 다음 레벨에서 추가될 피해량
            return nextDamageIncrease;
        }

        return 0;
    }
}
