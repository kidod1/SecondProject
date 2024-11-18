using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AK.Wwise; // WWISE ���ӽ����̽� �߰�

[CreateAssetMenu(menuName = "Abilities/DivinePunishment")]
public class DivinePunishment : Ability
{
    [Header("DivinePunishment Settings")]
    [Tooltip("������ �ɷ� �ߵ� ��ٿ� (��)")]
    public float[] cooldownPerLevel = { 5f, 4.5f, 4f, 3.5f, 3f }; // ���� 1~5

    [Tooltip("������ ������ ���ط�")]
    public int[] damagePerLevel = { 30, 40, 50, 60, 70 }; // ���� 1~5

    [Tooltip("������ ������ ����߸� �ִ� ���� ��")]
    public int[] maxTargetsPerLevel = { 3, 4, 5, 6, 7 }; // ���� 1~5

    [Tooltip("������ ���� ���� �ݰ�")]
    public float[] rangePerLevel = { 10f, 12f, 14f, 16f, 18f }; // ���� 1~5

    [Tooltip("���� ����Ʈ ������")]
    public GameObject lightningPrefab;

    [Header("WWISE Sound Events")]
    [Tooltip("DivinePunishment �ɷ� �ߵ� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event activateSound;

    [Tooltip("DivinePunishment ���׷��̵� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event upgradeSound;

    [Tooltip("DivinePunishment ���� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event deactivateSound;

    private Player playerInstance;
    private Coroutine abilityCoroutine;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
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

        // DivinePunishment �ɷ� �ߵ� �� WWISE ���� ���
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ������, ��Ÿ��, ��Ÿ�, �ִ� Ÿ�� ���� ������ŵ�ϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            // ���� ������ �´� �Ķ���͸� �����մϴ�.
            UpdateDivinePunishmentParameters();

            // ���׷��̵� �� WWISE ���� ���
            if (upgradeSound != null)
            {
                upgradeSound.Post(playerInstance.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("DivinePunishment: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        if (abilityCoroutine != null)
        {
            playerInstance.StopCoroutine(abilityCoroutine);
            abilityCoroutine = null;
        }

        currentLevel = 0;

        // DivinePunishment ���� �� WWISE ���� ���
        if (deactivateSound != null && playerInstance != null)
        {
            deactivateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// DivinePunishment�� �Ķ���͸� ���� ������ �°� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdateDivinePunishmentParameters()
    {
        // ���� ������ �´� ��Ÿ��, ������, ��Ÿ�, �ִ� Ÿ�� ���� �����մϴ�.
        // �ʿ信 ���� �ٸ� �Ķ���͵� ������Ʈ�� �� �ֽ��ϴ�.
        // ��: Ư�� �Ķ���Ͱ� ����� ��� �߰� ������ ������ �� �ֽ��ϴ�.
    }

    /// <summary>
    /// DivinePunishment �ߵ� �ڷ�ƾ�Դϴ�. ��Ÿ�Ӹ��� ������ ����߸��ϴ�.
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
    /// ������ ����߸��ϴ�.
    /// </summary>
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

        // DivinePunishment �ɷ� �ߵ� �� WWISE ���� ���
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// �ֺ��� ���͵��� ã���ϴ�.
    /// </summary>
    /// <returns>�ֺ� ���� ����Ʈ</returns>
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

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�. ������ ���� �������� 1���� �� ���� ������ ������ �����մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            // ���� ������ �ε����� currentLevel (currentLevel�� 0���� ����)
            int nextLevelIndex = currentLevel;
            float nextLevelCooldown = (nextLevelIndex < cooldownPerLevel.Length) ? cooldownPerLevel[nextLevelIndex] : cooldownPerLevel[cooldownPerLevel.Length - 1];
            int nextLevelDamage = (nextLevelIndex < damagePerLevel.Length) ? damagePerLevel[nextLevelIndex] : damagePerLevel[damagePerLevel.Length - 1];
            float nextLevelRange = (nextLevelIndex < rangePerLevel.Length) ? rangePerLevel[nextLevelIndex] : rangePerLevel[rangePerLevel.Length - 1];
            int nextLevelMaxTargets = (nextLevelIndex < maxTargetsPerLevel.Length) ? maxTargetsPerLevel[nextLevelIndex] : maxTargetsPerLevel[maxTargetsPerLevel.Length - 1];

            return $"{baseDescription}\n" +
                   $"Lv {currentLevel + 1}:\n" +
                   $"- ��Ÿ��: {nextLevelCooldown}��\n" +
                   $"- ���ط�: {nextLevelDamage}\n" +
                   $"- ��Ÿ�: {nextLevelRange}m\n" +
                   $"- �ִ� Ÿ�� ��: {nextLevelMaxTargets}\n";
        }
        else
        {
            // �ִ� ���� ����
            int maxLevelIndex = currentLevel - 1;
            float finalCooldown = (maxLevelIndex < cooldownPerLevel.Length) ? cooldownPerLevel[maxLevelIndex] : cooldownPerLevel[cooldownPerLevel.Length - 1];
            int finalDamage = (maxLevelIndex < damagePerLevel.Length) ? damagePerLevel[maxLevelIndex] : damagePerLevel[damagePerLevel.Length - 1];
            float finalRange = (maxLevelIndex < rangePerLevel.Length) ? rangePerLevel[maxLevelIndex] : rangePerLevel[rangePerLevel.Length - 1];
            int finalMaxTargets = (maxLevelIndex < maxTargetsPerLevel.Length) ? maxTargetsPerLevel[maxLevelIndex] : maxTargetsPerLevel[maxTargetsPerLevel.Length - 1];

            return $"{baseDescription}\n" +
                   $"Max Level: {currentLevel}\n" +
                   $"- ��Ÿ��: {finalCooldown}��\n" +
                   $"- ���ط�: {finalDamage}\n" +
                   $"- ��Ÿ�: {finalRange}m\n" +
                   $"- �ִ� Ÿ�� ��: {finalMaxTargets}\n";
        }
    }

    /// <summary>
    /// ���� ������ �´� ��Ÿ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ��Ÿ��</returns>
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

        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})�� cooldownPerLevel �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
        return cooldownPerLevel[cooldownPerLevel.Length - 1];
    }

    /// <summary>
    /// ���� ������ �´� ���ط��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ���ط�</returns>
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
        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})�� damagePerLevel �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
        return damagePerLevel[damagePerLevel.Length - 1];
    }

    /// <summary>
    /// ���� ������ �´� ��Ÿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ��Ÿ�</returns>
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
        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})�� rangePerLevel �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
        return rangePerLevel[rangePerLevel.Length - 1];
    }

    /// <summary>
    /// ���� ������ �´� �ִ� Ÿ�� ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ �ִ� Ÿ�� ��</returns>
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
        Debug.LogWarning($"DivinePunishment: currentLevel ({currentLevel})�� maxTargetsPerLevel �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
        return maxTargetsPerLevel[maxTargetsPerLevel.Length - 1];
    }

    /// <summary>
    /// ���� ���� ������ �ʿ��� ���� ��ȯ�մϴ�.
    /// (�� �޼���� �� �̻� ������ �����Ƿ� ������ �� �ֽ��ϴ�.)
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        // �� �̻� ������ �����Ƿ� 0�� ��ȯ�ϰų� �޼��带 ������ �� �ֽ��ϴ�.
        return 0;
    }

    /// <summary>
    /// OnValidate �޼��带 ���� �迭�� ���̸� maxLevel�� ��ġ��ŵ�ϴ�.
    /// </summary>
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

    /// <summary>
    /// Gizmos�� ����Ͽ� DivinePunishment �߻� ���� �ð�ȭ
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (playerInstance != null)
        {
            Vector2 facingDirection = playerInstance.GetFacingDirection();
            Vector2 backwardDirection = -facingDirection;

            Vector3 origin = playerInstance.transform.position;
            Vector3 direction = backwardDirection * 5f; // ����: 5 ���� ����

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + (Vector3)direction);
            Gizmos.DrawSphere(origin + (Vector3)direction, 0.2f);
        }
    }
}
