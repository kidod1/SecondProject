using UnityEngine;
using System.Collections;
using AK.Wwise; // WWISE ���ӽ����̽� �߰�

[CreateAssetMenu(menuName = "Abilities/BladeRush")]
public class BladeRush : Ability
{
    [Header("BladeRush Settings")]
    [Tooltip("������ �ɷ� �ߵ� ��ٿ� (��)")]
    public float[] cooldownPerLevel = { 5f, 4.5f, 4f, 3.5f, 3f }; // ���� 1~5

    [Tooltip("������ Į���� ���ط�")]
    public int[] damagePerLevel = { 20, 25, 30, 35, 40 }; // ���� 1~5

    [Tooltip("������ Į���� ��Ÿ�")]
    public float[] rangePerLevel = { 10f, 12f, 14f, 16f, 18f }; // ���� 1~5

    [Tooltip("Į���� �̵� �ӵ�")]
    public float bladeSpeed = 15f;

    [Tooltip("Į���� ������")]
    public GameObject bladePrefab;

    // WWISE �̺�Ʈ ���� �߰�
    [Header("WWISE Sound Events")]
    [Tooltip("BladeRush �ɷ� �ߵ� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event activateSound;

    [Tooltip("BladeRush ���׷��̵� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event upgradeSound;

    [Tooltip("BladeRush ���� �� ����� WWISE �̺�Ʈ")]
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
            Debug.LogError("BladeRush Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;

        if (abilityCoroutine == null)
        {
            abilityCoroutine = player.StartCoroutine(AbilityCoroutine());
        }

        // BladeRush �ɷ� �ߵ� �� WWISE ���� ���
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ �������� ��Ÿ��� ������ŵ�ϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            Debug.Log($"BladeRush ���׷��̵�: ���� ���� {currentLevel}");

            // ���� ������ �´� �������� ��Ÿ��� �����մϴ�.
            UpdateBladeRushParameters();

            // ���׷��̵� �� WWISE ���� ���
            if (upgradeSound != null)
            {
                upgradeSound.Post(playerInstance.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("BladeRush: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

        if (abilityCoroutine != null)
        {
            playerInstance.StopCoroutine(abilityCoroutine);
            abilityCoroutine = null;
        }

        currentLevel = 0;

        // BladeRush ���� �� WWISE ���� ���
        if (deactivateSound != null && playerInstance != null)
        {
            deactivateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// BladeRush�� �Ķ���͸� ���� ������ �°� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdateBladeRushParameters()
    {
        // ���� ������ �´� ������, ��Ÿ��, ��Ÿ��� �����մϴ�.
        // ��: �ʿ� �� �ٸ� �Ķ���͵� ������Ʈ�� �� �ֽ��ϴ�.
    }

    /// <summary>
    /// BladeRush �ߵ� �ڷ�ƾ�Դϴ�. ��Ÿ�Ӹ��� Į���� �߻��մϴ�.
    /// </summary>
    private IEnumerator AbilityCoroutine()
    {
        while (true)
        {
            float currentCooldown = GetCurrentCooldown();
            yield return new WaitForSeconds(currentCooldown);
            FireBlade();
        }
    }

    /// <summary>
    /// Į���� �߻��մϴ�.
    /// </summary>
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

        if (facingDirection == Vector2.zero)
        {
            Debug.LogWarning("BladeRush: �÷��̾��� ������ �������� �ʾҽ��ϴ�.");
            return;
        }

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

            return $"{baseDescription}\n" +
                   $"Lv {currentLevel + 1}:\n" +
                   $"- ��Ÿ��: {nextLevelCooldown}��\n" +
                   $"- ���ط�: {nextLevelDamage}\n" +
                   $"- ��Ÿ�: {nextLevelRange}m\n";
        }
        else
        {
            // �ִ� ���� ����
            int maxLevelIndex = currentLevel - 1;
            float finalCooldown = (maxLevelIndex < cooldownPerLevel.Length) ? cooldownPerLevel[maxLevelIndex] : cooldownPerLevel[cooldownPerLevel.Length - 1];
            int finalDamage = (maxLevelIndex < damagePerLevel.Length) ? damagePerLevel[maxLevelIndex] : damagePerLevel[damagePerLevel.Length - 1];
            float finalRange = (maxLevelIndex < rangePerLevel.Length) ? rangePerLevel[maxLevelIndex] : rangePerLevel[rangePerLevel.Length - 1];

            return $"{baseDescription}\n" +
                   $"Max Level: {currentLevel}\n" +
                   $"- ��Ÿ��: {finalCooldown}��\n" +
                   $"- ���ط�: {finalDamage}\n" +
                   $"- ��Ÿ�: {finalRange}m\n";
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

        Debug.LogWarning($"BladeRush: currentLevel ({currentLevel})�� cooldownPerLevel �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
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
            return damagePerLevel[currentLevel];
        }
        if (currentLevel < damagePerLevel.Length)
        {
            return damagePerLevel[currentLevel - 1];
        }
        Debug.LogWarning($"BladeRush: currentLevel ({currentLevel})�� damagePerLevel �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
        return damagePerLevel[damagePerLevel.Length - 1];
    }

    /// <summary>
    /// ���� ������ �´� ��Ÿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ��Ÿ�</returns>
    private float GetCurrentRange()
    {
        if (currentLevel < rangePerLevel.Length)
        {
            return rangePerLevel[currentLevel];
        }
        Debug.LogWarning($"BladeRush: currentLevel ({currentLevel})�� rangePerLevel �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
        return rangePerLevel[rangePerLevel.Length - 1];
    }

    /// <summary>
    /// ���� ���� ������ �ʿ��� ���� ��ȯ�մϴ�.
    /// (�� �޼���� �� �̻� ������ �����Ƿ� ������ �� �ֽ��ϴ�.)
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        // �ʿ信 ���� �����ϰų� ����
        return 0;
    }

    /// <summary>
    /// Gizmos�� ����Ͽ� BladeRush �߻� ���� �ð�ȭ
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

    /// <summary>
    /// OnValidate �޼��带 ���� �迭�� ���̸� maxLevel�� ��ġ��ŵ�ϴ�.
    /// </summary>
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
