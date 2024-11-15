using UnityEngine;
using System.Collections;
using AK.Wwise; // WWISE ���ӽ����̽� �߰�

[CreateAssetMenu(menuName = "Abilities/BladeRush")]
public class BladeRush : Ability
{
    [Header("BladeRush Settings")]
    [Tooltip("�� �������� �ɷ� �ߵ� ��ٿ� (��)")]
    public float[] cooldownPerLevel = { 5f, 4.5f, 4f, 3.5f, 3f }; // ���� 1~5

    [Tooltip("�� �������� Į���� ���ط�")]
    public int[] damagePerLevel = { 20, 25, 30, 35, 40 }; // ���� 1~5

    [Tooltip("�� �������� Į���� ��Ÿ�")]
    public float[] rangePerLevel = { 10f, 12f, 14f, 16f, 18f }; // ���� 1~5

    [Tooltip("Į���� �̵� �ӵ�")]
    public float bladeSpeed = 15f;

    [Tooltip("Į���� ������")]
    public GameObject bladePrefab;

    // WWISE �̺�Ʈ ���� �߰�
    [Header("WWISE Sound Events")]
    [Tooltip("BladeRush �ɷ� �ߵ� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event activateSound;

    // �ʿ��� ��� �߰����� WWISE �̺�Ʈ�� ������ �� �ֽ��ϴ�.
    // ��: public AK.Wwise.Event upgradeSound;

    private Player playerInstance;
    private Coroutine abilityCoroutine;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("BladeRush Apply: player �ν��Ͻ��� null�Դϴ�.");
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
            Debug.Log($"BladeRush ���׷��̵�: ���� ���� {currentLevel + 1}");

            // ���׷��̵� �� WWISE ���� ��� (���� ����)
            // activateSound.Post(playerInstance.gameObject);
        }
        else
        {
            Debug.LogWarning("BladeRush: �̹� �ִ� ������ �����߽��ϴ�.");
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
            FireBlade();
        }
    }

    private void FireBlade()
    {
        if (bladePrefab == null)
        {
            Debug.LogError("BladeRush: bladePrefab�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        Vector3 spawnPosition = playerInstance.transform.position;

        // �÷��̾��� �ٶ󺸴� ������ �����ɴϴ�.
        Vector2 facingDirection = playerInstance.GetFacingDirection();

        // Į���� ȸ���� ���⿡ �°� �����մϴ�.
        float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, angle - 90);

        GameObject bladeInstance = Instantiate(bladePrefab, spawnPosition, spawnRotation);
        BladeProjectile bladeProjectile = bladeInstance.GetComponent<BladeProjectile>();

        if (bladeProjectile != null)
        {
            int currentDamage = GetCurrentDamage();
            float currentRange = GetCurrentRange();

            bladeProjectile.Initialize(currentDamage, currentRange, bladeSpeed, playerInstance, facingDirection);
        }
        else
        {
            Debug.LogError("BladeRush: bladePrefab�� BladeProjectile ������Ʈ�� �����ϴ�.");
        }

        // BladeRush �ɷ� �ߵ� �� WWISE ���� ���
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";
        description += $"Lv {currentLevel + 1}:\n";
        description += $"- ��Ÿ��: {GetCurrentCooldown()}��\n";
        description += $"- ���ط�: {GetCurrentDamage()}\n";
        description += $"- ��Ÿ�: {GetCurrentRange()}m";

        return description;
    }

    // ���� ������ �´� ��Ÿ���� ��ȯ�մϴ�.
    private float GetCurrentCooldown()
    {
        if (currentLevel < cooldownPerLevel.Length)
        {
            return cooldownPerLevel[currentLevel];
        }
        Debug.LogWarning($"BladeRush: currentLevel ({currentLevel})�� cooldownPerLevel �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
        return cooldownPerLevel[cooldownPerLevel.Length - 1];
    }

    // ���� ������ �´� ���ط��� ��ȯ�մϴ�.
    private int GetCurrentDamage()
    {
        if (currentLevel < damagePerLevel.Length)
        {
            return damagePerLevel[currentLevel];
        }
        Debug.LogWarning($"BladeRush: currentLevel ({currentLevel})�� damagePerLevel �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
        return damagePerLevel[damagePerLevel.Length - 1];
    }

    // ���� ������ �´� ��Ÿ��� ��ȯ�մϴ�.
    private float GetCurrentRange()
    {
        if (currentLevel < rangePerLevel.Length)
        {
            return rangePerLevel[currentLevel];
        }
        Debug.LogWarning($"BladeRush: currentLevel ({currentLevel})�� rangePerLevel �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
        return rangePerLevel[rangePerLevel.Length - 1];
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
            Debug.LogWarning($"BladeRush: cooldownPerLevel �迭�� ���̰� maxLevel ({maxLevel})�� ��ġ���� �ʽ��ϴ�. �迭 ���̸� ����ϴ�.");
            System.Array.Resize(ref cooldownPerLevel, maxLevel);
        }

        if (damagePerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"BladeRush: damagePerLevel �迭�� ���̰� maxLevel ({maxLevel})�� ��ġ���� �ʽ��ϴ�. �迭 ���̸� ����ϴ�.");
            System.Array.Resize(ref damagePerLevel, maxLevel);
        }

        if (rangePerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"BladeRush: rangePerLevel �迭�� ���̰� maxLevel ({maxLevel})�� ��ġ���� �ʽ��ϴ�. �迭 ���̸� ����ϴ�.");
            System.Array.Resize(ref rangePerLevel, maxLevel);
        }
    }
}
