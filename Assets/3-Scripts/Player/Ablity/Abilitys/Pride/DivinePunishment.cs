using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Abilities/DivinePunishment")]
public class DivinePunishment : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("�ɷ� �ߵ� ��Ÿ�� (��)")]
    public float cooldown = 5f;

    [Tooltip("������ ���ط�")]
    public int damage = 30;

    [Tooltip("������ ����߸� �ִ� ���� ��")]
    public int maxTargets = 3;

    [Tooltip("���� ����Ʈ ������")]
    public GameObject lightningPrefab;

    [Tooltip("���� ���� �ݰ�")]
    public float range = 10f;

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
            damage += 10;      // ���ط� ����
            range += 2f;       // ���� ����
            maxTargets += 1;   // Ÿ�� �� ����
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
            Debug.LogError("DivinePunishment: lightningPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        List<Monster> nearbyMonsters = FindNearbyMonsters();

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
        description += $"���� ����: {currentLevel + 1}\n";
        description += $"��Ÿ��: {cooldown}��\n";
        description += $"���ط�: {damage}\n";
        description += $"����: {range}m\n";
        description += $"�ִ� Ÿ�� ��: {maxTargets}";

        return description;
    }

    // GetNextLevelIncrease �޼��� ����
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel)
        {
            int nextDamageIncrease = 10;  // ���� �������� �߰��� ���ط�
            return nextDamageIncrease;
        }

        return 0;
    }
}
