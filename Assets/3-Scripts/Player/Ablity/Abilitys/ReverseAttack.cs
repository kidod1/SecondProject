using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ReverseAttack")]
public class ReverseAttack : Ability
{
    [Tooltip("������ ���� ���� ������ �ۼ�Ʈ (0.0f ~ 1.0f)")]
    [Range(0f, 1f)]
    public float[] damagePercentages; // ������ ���� ���� ������ �ۼ�Ʈ �迭

    private Player playerInstance;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        // �ߺ� ��� ����
        if (playerInstance != null)
        {
            playerInstance.OnShoot.RemoveListener(OnShootHandler);
        }

        playerInstance = player;
        playerInstance.OnShoot.AddListener(OnShootHandler); // �ñ״�ó ��ġ

        // ���� ������ ���� ������ �ۼ�Ʈ ����
        ApplyDamagePercentage();

        Debug.Log($"ReverseAttack applied. Current Level: {currentLevel}");
    }

    /// <summary>
    /// ���� ������ ���� ������ �ۼ�Ʈ�� �÷��̾�� �����մϴ�.
    /// </summary>
    private void ApplyDamagePercentage()
    {
        Debug.Log($"Applying damage percentage. Current Level: {currentLevel}, damagePercentages.Length: {damagePercentages.Length}");

        if (currentLevel <= 0)
        {
            Debug.LogWarning("ReverseAttack: currentLevel is less than or equal to 0.");
            return;
        }

        if (currentLevel - 1 < damagePercentages.Length)
        {
            // �÷��̾��� ���� ���� ������ �ۼ�Ʈ�� ����
            playerInstance.stat.reverseAttackDamagePercentage = damagePercentages[currentLevel - 1];
            Debug.Log($"ReverseAttack: Level {currentLevel} damage percentage set to {damagePercentages[currentLevel - 1] * 100}%");
        }
        else
        {
            Debug.LogWarning($"ReverseAttack: currentLevel ({currentLevel}) exceeds damagePercentages array bounds ({damagePercentages.Length}). Setting to max defined level.");
            // �ִ� ���ǵ� ������ ����
            playerInstance.stat.reverseAttackDamagePercentage = damagePercentages[damagePercentages.Length - 1];
            currentLevel = damagePercentages.Length; // currentLevel�� �ִ� ������ ����
        }
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ������ �ۼ�Ʈ�� ����˴ϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            ApplyDamagePercentage();
            Debug.Log($"ReverseAttack upgraded. Current Level: {currentLevel}");
        }
        else
        {
            Debug.LogWarning("ReverseAttack: Already at max level.");
        }
    }

    /// <summary>
    /// ���� ������ ������ �ۼ�Ʈ �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���������� ������ �ۼ�Ʈ (�ۼ�Ʈ)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < damagePercentages.Length)
        {
            return Mathf.RoundToInt(damagePercentages[currentLevel] * 100); // �ۼ�Ʈ�� ��ȯ
        }
        return 0;
    }

    /// <summary>
    /// �ɷ��� ����� �� �÷��̾ �߻��� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="direction">�߻� ����</param>
    /// <param name="prefabIndex">������ �ε���</param>
    /// <param name="projectile">������ ������ƮƮ</param>
    private void OnShootHandler(Vector2 direction, int prefabIndex, GameObject projectile)
    {
        if (playerInstance != null && projectile != null)
        {
            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                // ���� �������� ������ �ۼ�Ʈ��ŭ ����
                int originalDamage = projScript.projectileCurrentDamage; // �ùٸ� �ʵ�� ���
                float reverseDamagePercentage = playerInstance.stat.reverseAttackDamagePercentage;
                int adjustedDamage = Mathf.RoundToInt(originalDamage * reverseDamagePercentage);
                projScript.projectileCurrentDamage = adjustedDamage;

                // ������ ���� ���� �״�� ����
                projScript.SetDirection(direction);
                Debug.Log($"ReverseAttack: Original damage adjusted to {adjustedDamage} based on current level percentage.");
            }
            else
            {
                Debug.LogError("ReverseAttack: Projectile script not found.");
            }
        }
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        if (playerInstance != null)
        {
            playerInstance.OnShoot.RemoveListener(OnShootHandler);
            playerInstance.stat.reverseAttackDamagePercentage = 1.0f; // �ʱ�ȭ (100%)
        }
        playerInstance = null;
        Debug.Log("ReverseAttack level has been reset.");
    }

    /// <summary>
    /// �ɷ��� ���� ���¿� ȿ���� �����ϴ� ���ڿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        Debug.Log($"GetDescription called. Current Level: {currentLevel}, damagePercentages.Length: {damagePercentages.Length}, maxLevel: {maxLevel}");

        if (currentLevel < maxLevel && currentLevel <= damagePercentages.Length)
        {
            float percentChance = damagePercentages[currentLevel] * 100f;
            return $"{baseDescription}\nLv {currentLevel+1}: �߻� �� ������ {percentChance}% �������� ���� ���� �ߵ�";
        }
        else if (currentLevel == maxLevel && currentLevel <= damagePercentages.Length)
        {
            float percentChance = damagePercentages[currentLevel] * 100f;
            return $"{baseDescription}\n�ִ� ���� ����: �߻� �� ������ {percentChance}% �������� ���� ���� �ߵ�";
        }
        else
        {
            return $"{baseDescription}\nMaximum level reached.";
        }
    }
}
