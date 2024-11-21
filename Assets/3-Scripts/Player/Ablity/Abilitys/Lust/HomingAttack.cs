using UnityEngine;
using AK.Wwise;

[CreateAssetMenu(menuName = "Abilities/HomingAttack")]
public class HomingAttack : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("������ Homing ���� ���� �ð� (��)")]
    public float[] homingStartDelayLevels = { 0.3f, 0.25f, 0.2f, 0.15f, 0.1f }; // ���� 1~5

    [Tooltip("������ Homing �ӵ�")]
    public float[] homingSpeedLevels = { 5f, 6f, 7f, 8f, 9f }; // ���� 1~5

    [Tooltip("������ Homing ����")]
    public float[] homingRangeLevels = { 10f, 12f, 14f, 16f, 18f }; // ���� 1~5

    [Tooltip("���� źȯ ������")]
    public GameObject homingProjectilePrefab;

    [Tooltip("ȣ�� źȯ ���� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event homingProjectileSound;

    private Player playerInstance;
    private int attackCounter = 0; // ���� ī���� �߰�

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("HomingAttack Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }
            // ���� �÷��̾� �ν��Ͻ��� �����ʰ� ��ϵǾ� ������ ����
            if (playerInstance != null)
            {
                playerInstance.OnShoot.RemoveListener(OnShootHandler);
            }

            playerInstance = player;

            // ������ �ߺ� ��� ����
            playerInstance.OnShoot.RemoveListener(OnShootHandler);
            playerInstance.OnShoot.AddListener(OnShootHandler);
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel�� 5�� ���, currentLevel�� 0~4
        {
        }
        else
        {
            Debug.LogWarning("HomingAttack: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    public override void ResetLevel()
    {
        // �̺�Ʈ ������ ����
        if (playerInstance != null)
        {
            playerInstance.OnShoot.RemoveListener(OnShootHandler);
            playerInstance = null;
        }

        // ���� ī���� �ʱ�ȭ
        attackCounter = 0;
    }

    public override string GetDescription()
    {
        if (currentLevel < homingSpeedLevels.Length)
        {
            float currentDelay = homingStartDelayLevels[currentLevel];
            float currentSpeed = homingSpeedLevels[currentLevel];
            float currentRange = homingRangeLevels[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: ���� {currentDelay}s, �ӵ� x{currentSpeed}, ���� {currentRange}m";
        }
        else
        {
            Debug.LogWarning($"HomingAttack: currentLevel ({currentLevel})�� homingSpeedLevels �迭�� ������ ������ϴ�. �ִ� ���� ������ ��ȯ�մϴ�.");
            float finalDelay = homingStartDelayLevels[homingStartDelayLevels.Length - 1];
            float finalSpeed = homingSpeedLevels[homingSpeedLevels.Length - 1];
            float finalRange = homingRangeLevels[homingRangeLevels.Length - 1];
            return $"{baseDescription}\nMax Level: ���� {finalDelay}s, �ӵ� x{finalSpeed}, ���� {finalRange}m";
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < homingSpeedLevels.Length)
        {
            // ����: ���� ������ Homing �ӵ��� ��ȯ
            return Mathf.RoundToInt(homingSpeedLevels[currentLevel]);
        }
        Debug.LogWarning($"HomingAttack: currentLevel ({currentLevel})�� homingSpeedLevels �迭�� ������ ������ϴ�. �⺻�� 1�� ��ȯ�մϴ�.");
        return 1;
    }

    private void OnShootHandler(Vector2 direction, int prefabIndex, GameObject projectile)
    {
        attackCounter++; // ���� ī���� ����

        if (attackCounter >= 9) // 3��° ���� ��
        {
            CreateHomingProjectile(direction);
            attackCounter = 0; // ī���� �ʱ�ȭ
        }
    }

    private void CreateHomingProjectile(Vector2 direction)
    {
        if (homingProjectilePrefab == null)
        {
            Debug.LogError("HomingAttack: homingProjectilePrefab�� �����Ǿ� ���� �ʽ��ϴ�.");
            return;
        }

        // �÷��̾��� ��ġ���� HomingProjectile ����
        GameObject homingProjectile = Instantiate(homingProjectilePrefab, playerInstance.transform.position, Quaternion.identity);

        HomingProjectile projScript = homingProjectile.GetComponent<HomingProjectile>();
        if (projScript != null)
        {
            // ���� ������ �´� �Ķ���� ����
            float currentDelay = GetCurrentHomingStartDelay();
            float currentSpeed = GetCurrentHomingSpeed();
            float currentRange = GetCurrentHomingRange();

            projScript.Initialize(playerInstance.stat, currentDelay, currentSpeed, currentRange);
            projScript.SetDirection(direction);

            // ȣ�� źȯ ���� �� ���� ���
            if (homingProjectileSound != null)
            {
                homingProjectileSound.Post(homingProjectile.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("HomingAttack: homingProjectilePrefab�� HomingProjectile ��ũ��Ʈ�� �����ϴ�.");
        }
    }

    private float GetCurrentHomingStartDelay()
    {
        if (currentLevel < homingStartDelayLevels.Length)
        {
            return homingStartDelayLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"HomingAttack: currentLevel ({currentLevel})�� homingStartDelayLevels �迭�� ������ ������ϴ�. �⺻�� {homingStartDelayLevels[homingStartDelayLevels.Length - 1]}�� ��ȯ�մϴ�.");
            return homingStartDelayLevels[homingStartDelayLevels.Length - 1];
        }
    }

    private float GetCurrentHomingSpeed()
    {
        if (currentLevel < homingSpeedLevels.Length)
        {
            return homingSpeedLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"HomingAttack: currentLevel ({currentLevel})�� homingSpeedLevels �迭�� ������ ������ϴ�. �⺻�� {homingSpeedLevels[homingSpeedLevels.Length - 1]}�� ��ȯ�մϴ�.");
            return homingSpeedLevels[homingSpeedLevels.Length - 1];
        }
    }

    private float GetCurrentHomingRange()
    {
        if (currentLevel < homingRangeLevels.Length)
        {
            return homingRangeLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"HomingAttack: currentLevel ({currentLevel})�� homingRangeLevels �迭�� ������ ������ϴ�. �⺻�� {homingRangeLevels[homingRangeLevels.Length - 1]}�� ��ȯ�մϴ�.");
            return homingRangeLevels[homingRangeLevels.Length - 1];
        }
    }
}
