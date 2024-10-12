using UnityEngine;

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

    private Player playerInstance;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
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

        // �̺�Ʈ ������ �߰� ���� �ߺ� ����
        playerInstance.OnShoot.RemoveListener(OnShootHandler);
        playerInstance.OnShoot.AddListener(OnShootHandler);
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ Homing �Ķ���Ͱ� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel�� 5�� ���, currentLevel�� 0~4
        {
            currentLevel++;

            // ���� �� �� �ʿ��� ���� �߰� (�ʿ� ��)
            // ���� �� �ɷ��� ������ �Ķ���� �迭�� ���� �ڵ����� �����ǹǷ� ������ ������ �ʿ� �����ϴ�.
        }
        else
        {
            Debug.LogWarning("HomingAttack: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

        // �̺�Ʈ ������ ����
        if (playerInstance != null)
        {
            playerInstance.OnShoot.RemoveListener(OnShootHandler);
            playerInstance = null;
        }

        // ������Ʈ Ÿ���� �⺻������ �ǵ��� (�ʿ� ��)
        // �� �κ��� �÷��̾� �ν��Ͻ��� null�̱� ������ ������� �ʽ��ϴ�.
        // �ʿ��ϴٸ�, �÷��̾� �ν��Ͻ��� ���� �����صΰ� ó���ؾ� �մϴ�.

        Debug.Log("HomingAttack ������ �ʱ�ȭ�Ǿ����ϴ�.");
    }

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ��� ���� ���ڿ�</returns>
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

    /// <summary>
    /// ���� ���� ������ �ʿ��� ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���� ���� �� �ʿ��� ��</returns>
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

    /// <summary>
    /// �ɷ��� ����� �� �÷��̾ �߻��� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="direction">�߻� ����</param>
    /// <param name="prefabIndex">������ �ε���</param>
    /// <param name="projectile">������ ������ƮƮ</param>
    private void OnShootHandler(Vector2 direction, int prefabIndex, GameObject projectile)
    {
        if (projectile == null)
        {
            Debug.LogError("HomingAttack: ���޵� ������ƮƮ�� null�Դϴ�.");
            return;
        }

        HomingProjectile projScript = projectile.GetComponent<HomingProjectile>();
        if (projScript != null)
        {
            // ���� ������ �´� �Ķ���� ����
            float currentDelay = GetCurrentHomingStartDelay();
            float currentSpeed = GetCurrentHomingSpeed();
            float currentRange = GetCurrentHomingRange();

            projScript.Initialize(playerInstance.stat, currentDelay, currentSpeed, currentRange);
            projScript.SetDirection(direction);
        }
        else
        {
            Debug.LogWarning("HomingAttack: ���޵� ������ƮƮ�� HomingProjectile ��ũ��Ʈ�� �����ϴ�.");
        }
    }


    /// <summary>
    /// ���� ������ Homing ���� ���� �ð��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ Homing ���� ���� �ð�</returns>
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

    /// <summary>
    /// ���� ������ Homing �ӵ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ Homing �ӵ�</returns>
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

    /// <summary>
    /// ���� ������ Homing ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ Homing ����</returns>
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
