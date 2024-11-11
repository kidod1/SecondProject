using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Abilities/DivinePunishment")]
public class DivinePunishment : Ability
{
    [Header("DivinePunishment Settings")]
    [Tooltip("�� �������� �ɷ� �ߵ� ��ٿ� (��)")]
    public float[] cooldownPerLevel = { 5f, 4.5f, 4f, 3.5f, 3f }; // ���� 1~5

    [Tooltip("�� �������� ������ ���ط�")]
    public int[] damagePerLevel = { 30, 40, 50, 60, 70 }; // ���� 1~5

    [Tooltip("�� �������� ������ ����߸� �ִ� ���� ��")]
    public int[] maxTargetsPerLevel = { 3, 4, 5, 6, 7 }; // ���� 1~5

    [Tooltip("�� �������� ���� ���� �ݰ�")]
    public float[] rangePerLevel = { 10f, 12f, 14f, 16f, 18f }; // ���� 1~5

    [Tooltip("���� ����Ʈ ������")]
    public GameObject lightningPrefab;

    private Player playerInstance;
    private Coroutine abilityCoroutine;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("DivinePunishment Apply: player �ν��Ͻ��� null�Դϴ�.");
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
            Debug.Log($"DivinePunishment ���׷��̵�: ���� ���� {currentLevel + 1}");
        }
        else
        {
            Debug.LogWarning("DivinePunishment: �̹� �ִ� ������ �����߽��ϴ�.");
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
            Debug.LogError("DivinePunishment: lightningPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        List<Monster> nearbyMonsters = FindNearbyMonsters();

        int maxTargets = GetCurrentMaxTargets();
        int targets = Mathf.Min(maxTargets, nearbyMonsters.Count);

        for (int i = 0; i < targets; i++)
        {
            Monster monster = nearbyMonsters[i];

            // ���� ����Ʈ ����
            Vector3 strikePosition = monster.transform.position;
            GameObject lightningInstance = Instantiate(lightningPrefab, strikePosition, Quaternion.identity);

            // 2�� �Ŀ� ���� ����Ʈ ����
            Destroy(lightningInstance, 2f);

            // ���Ϳ��� ���� ����
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

        // �Ÿ� ������ ����
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
        description += $"���� {currentLevel + 1}:\n";
        description += $"- ��Ÿ��: {GetCurrentCooldown()}��\n";
        description += $"- ���ط�: {GetCurrentDamage()}\n";
        description += $"- ����: {GetCurrentRange()}m\n";
        description += $"- �ִ� Ÿ�� ��: {GetCurrentMaxTargets()}";

        return description;
    }

    // ���� ������ �´� ��Ÿ���� ��ȯ�մϴ�.
    private float GetCurrentCooldown()
    {
        if (currentLevel < cooldownPerLevel.Length)
        {
            return cooldownPerLevel[currentLevel];
        }
        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})�� cooldownPerLevel �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
        return cooldownPerLevel[cooldownPerLevel.Length - 1];
    }

    // ���� ������ �´� ���ط��� ��ȯ�մϴ�.
    private int GetCurrentDamage()
    {
        if (currentLevel < damagePerLevel.Length)
        {
            return damagePerLevel[currentLevel];
        }
        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})�� damagePerLevel �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
        return damagePerLevel[damagePerLevel.Length - 1];
    }

    // ���� ������ �´� ������ ��ȯ�մϴ�.
    private float GetCurrentRange()
    {
        if (currentLevel < rangePerLevel.Length)
        {
            return rangePerLevel[currentLevel];
        }
        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})�� rangePerLevel �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
        return rangePerLevel[rangePerLevel.Length - 1];
    }

    // ���� ������ �´� �ִ� Ÿ�� ���� ��ȯ�մϴ�.
    private int GetCurrentMaxTargets()
    {
        if (currentLevel < maxTargetsPerLevel.Length)
        {
            return maxTargetsPerLevel[currentLevel];
        }
        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})�� maxTargetsPerLevel �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
        return maxTargetsPerLevel[maxTargetsPerLevel.Length - 1];
    }

    // GetNextLevelIncrease �޼��� ����
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
        // �迭�� ���̰� maxLevel�� ��ġ�ϵ��� ����
        if (cooldownPerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"DivinePunishment: cooldownPerLevel �迭�� ���̰� maxLevel ({maxLevel})�� ��ġ���� �ʽ��ϴ�. �迭 ���̸� ����ϴ�.");
            System.Array.Resize(ref cooldownPerLevel, maxLevel);
        }

        if (damagePerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"DivinePunishment: damagePerLevel �迭�� ���̰� maxLevel ({maxLevel})�� ��ġ���� �ʽ��ϴ�. �迭 ���̸� ����ϴ�.");
            System.Array.Resize(ref damagePerLevel, maxLevel);
        }

        if (rangePerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"DivinePunishment: rangePerLevel �迭�� ���̰� maxLevel ({maxLevel})�� ��ġ���� �ʽ��ϴ�. �迭 ���̸� ����ϴ�.");
            System.Array.Resize(ref rangePerLevel, maxLevel);
        }

        if (maxTargetsPerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"DivinePunishment: maxTargetsPerLevel �迭�� ���̰� maxLevel ({maxLevel})�� ��ġ���� �ʽ��ϴ�. �迭 ���̸� ����ϴ�.");
            System.Array.Resize(ref maxTargetsPerLevel, maxLevel);
        }
    }
}
