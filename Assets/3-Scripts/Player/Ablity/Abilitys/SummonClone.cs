using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SummonClone")]
public class SummonClone : Ability
{
    [Tooltip("������ Ŭ�� ������ ���� (��: 0.3f = 30%)")]
    [Range(0f, 2f)]
    public float[] damageMultipliers = { 0.3f, 0.5f, 0.7f, 1.0f, 1.2f };

    public GameObject clonePrefab;
    private GameObject cloneInstance;
    private RotatingObject rotatingObject;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        if (currentLevel < damageMultipliers.Length)
        {
            if (cloneInstance == null)
            {
                cloneInstance = Instantiate(clonePrefab, player.transform.position, Quaternion.identity, player.transform);
                rotatingObject = cloneInstance.GetComponent<RotatingObject>();
                if (rotatingObject != null)
                {
                    rotatingObject.player = player.transform;
                    rotatingObject.playerShooting = player;
                    rotatingObject.damageMultiplier = damageMultipliers[currentLevel];

                    player.OnShoot.AddListener(CloneShoot);

                    Debug.Log($"SummonClone applied at Level {currentLevel + 1} with Damage Multiplier: {damageMultipliers[currentLevel] * 100}%");
                }
                else
                {
                    Debug.LogError("SummonClone: RotatingObject ������Ʈ�� Ŭ�� �����տ� �����ϴ�.");
                }
            }
            else
            {
                if (rotatingObject != null)
                {
                    rotatingObject.damageMultiplier = damageMultipliers[currentLevel];
                }
                else
                {
                    Debug.LogError("SummonClone: RotatingObject�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
                }
            }
        }
    }

    /// <summary>
    /// Ŭ���� �÷��̾��� ������ �����ϰ� �ϴ� �޼���.
    /// </summary>
    /// <param name="direction">�߻� ����</param>
    /// <param name="prefabIndex">������ �ε���</param>
    /// <param name="projectile">���� ����ü</param>
    private void CloneShoot(Vector2 direction, int prefabIndex, GameObject originalProjectile)
    {
        if (rotatingObject == null || cloneInstance == null)
        {
            Debug.LogWarning("SummonClone: Ŭ�� �ν��Ͻ��� �������� �ʰų� RotatingObject�� �������� �ʾҽ��ϴ�.");
            return;
        }

        // Ŭ���� �߻��� ����ü ���� �� �ʱ�ȭ
        GameObject cloneProjectile = Instantiate(originalProjectile, rotatingObject.transform.position, Quaternion.identity);
        Projectile projScript = cloneProjectile.GetComponent<Projectile>();

        if (projScript != null)
        {
            // Ŭ���� ������ ���� ����
            float damageMultiplier = damageMultipliers[currentLevel];
            int adjustedDamage = Mathf.RoundToInt(projScript.projectileCurrentDamage * damageMultiplier);
            projScript.Initialize(rotatingObject.playerShooting.stat, rotatingObject.playerShooting, false, 1.0f, adjustedDamage);
            projScript.SetDirection(direction);
        }
        else
        {
            Debug.LogError("SummonClone: Ŭ���� Projectile ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
    }
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < damageMultipliers.Length)
        {
            return Mathf.RoundToInt(damageMultipliers[currentLevel + 1] * 100); // �ۼ�Ʈ�� ��ȯ
        }
        return 0;
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
            Apply(PlayManager.I.GetPlayer());
        }
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        if (cloneInstance != null)
        {
            Destroy(cloneInstance);
            cloneInstance = null;
            rotatingObject = null;

            // �÷��̾��� ���� �̺�Ʈ���� Ŭ���� �̺�Ʈ ����
            Player player = PlayManager.I.GetPlayer();
            if (player != null)
            {
                player.OnShoot.RemoveListener(CloneShoot);
            }
        }
    }

    /// <summary>
    /// �ɷ��� ���� ���¿� ȿ���� �����ϴ� ���ڿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < damageMultipliers.Length && currentLevel >= 0)
        {
            float damageMultiplierPercent = damageMultipliers[currentLevel] * 100f;
            return $"{baseDescription}\nLv {currentLevel + 1}: Ŭ���� ������ {damageMultiplierPercent}% ����";
        }
        else if (currentLevel >= damageMultipliers.Length)
        {
            float maxDamageMultiplierPercent = damageMultipliers[damageMultipliers.Length - 1] * 100f;
            return $"{baseDescription}\n�ִ� ���� ����: Ŭ���� ������ {maxDamageMultiplierPercent}% ����";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����.";
        }
    }
}