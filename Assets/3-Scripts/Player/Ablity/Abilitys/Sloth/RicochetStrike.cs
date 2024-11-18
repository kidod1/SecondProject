using UnityEngine;
using System.Collections;
using AK.Wwise;

[CreateAssetMenu(menuName = "Abilities/RicochetStrike")]
public class RicochetStrike : Ability
{
    [Tooltip("������ ����ü ������ ������")]
    public int[] damageIncreases; // ������ ����ü ������ ������ �迭

    public int hitThreshold = 5;          // ���� �Ӱ谪
    public float range = 10f;             // �� Ž�� ����
    public GameObject projectilePrefab;   // ����� ����ü ������
    public int projectileCount = 3;       // �߻��� ����ü�� ��
    public float baseSpeedMultiplier = 1.0f;

    [Tooltip("����ü ��ȯ �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event spawnProjectileSound;

    private Player playerInstance;
    private int hitCount = 0;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        playerInstance = player;
    }

    /// <summary>
    /// ����ü�� ������ �¾��� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="enemy">���� ���� �ݶ��̴�</param>
    public void OnProjectileHit(Collider2D enemy)
    {
        hitCount++;
        if (hitCount >= hitThreshold)
        {
            hitCount = 0;
            SpawnHomingProjectiles();
        }
    }

    /// <summary>
    /// ���� ����ü�� �����մϴ�.
    /// </summary>
    private void SpawnHomingProjectiles()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("RicochetStrike: ����ü �������� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 spawnPosition = playerInstance.transform.position + GetRandomOffset();
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            Projectile projScript = projectile.GetComponent<Projectile>();
            Collider2D projectileCollider = projectile.GetComponent<Collider2D>();

            if (projScript != null)
            {
                // �ʱ⿡�� Collider ��Ȱ��ȭ
                if (projectileCollider != null)
                {
                    projectileCollider.enabled = false;
                }

                Vector2 randomDirection = GetRandomDirection();
                projScript.Initialize(playerInstance.stat, playerInstance, true, baseSpeedMultiplier, GetProjectileDamage());
                projScript.SetDirection(randomDirection, baseSpeedMultiplier);

                // 0.3�� �Ŀ� ���� ���� �� Collider Ȱ��ȭ
                playerInstance.StartCoroutine(HomingTowardsEnemy(projectile, projScript, 0.3f, projectileCollider));
            }
            else
            {
                Debug.LogError("RicochetStrike: Projectile ��ũ��Ʈ�� �����ϴ�.");
            }
        }

        // ����ü ��ȯ �� ���� ���
        if (spawnProjectileSound != null && playerInstance != null)
        {
            spawnProjectileSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// ����ü�� �������� ���� ������ ���� �����ɴϴ�.
    /// </summary>
    /// <returns>������ ������ ��</returns>
    private int GetProjectileDamage()
    {
        if (currentLevel < damageIncreases.Length)
        {
            return damageIncreases[currentLevel];
        }
        else
        {
            Debug.LogWarning($"RicochetStrike: currentLevel ({currentLevel}) exceeds damageIncreases �迭 ����. ������ ������ �������� ����մϴ�.");
            return damageIncreases[damageIncreases.Length - 1];
        }
    }

    /// <summary>
    /// ����ü�� �����ǵ��� �����ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <param name="projectile">����ü ���ӿ�����Ʈ</param>
    /// <param name="projScript">����ü ��ũ��Ʈ</param>
    /// <param name="delay">���� �ð�</param>
    /// <param name="projectileCollider">����ü �ݶ��̴�</param>
    /// <returns></returns>
    private IEnumerator HomingTowardsEnemy(GameObject projectile, Projectile projScript, float delay, Collider2D projectileCollider)
    {
        yield return new WaitForSeconds(delay);
        Destroy(projectile, 5f);  // ����ü�� ������ �� 5�� �ڿ� �ı�

        // Collider Ȱ��ȭ
        if (projectileCollider != null)
        {
            projectileCollider.enabled = true;
        }

        // ���� ����
        Collider2D closestEnemy = FindClosestEnemy(projectile.transform.position);

        if (closestEnemy != null)
        {
            Vector2 directionToEnemy = (closestEnemy.transform.position - projectile.transform.position).normalized;
            projScript.SetDirection(directionToEnemy);
            Debug.Log($"RicochetStrike: ���� ����ü�� {closestEnemy.name}�� ���� �����˴ϴ�.");
        }
        else
        {
            Debug.Log($"RicochetStrike: �ֺ��� ������ ���� �����ϴ�.");
        }
    }

    /// <summary>
    /// ������ ������ �����մϴ�.
    /// </summary>
    /// <returns>������ ���� ����</returns>
    private Vector2 GetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
    }

    /// <summary>
    /// ������ �������� �����մϴ�.
    /// </summary>
    /// <returns>������ ������ ����</returns>
    private Vector3 GetRandomOffset()
    {
        float offsetX = Random.Range(-1f, 1f);
        float offsetY = Random.Range(-1f, 1f);
        return new Vector3(offsetX, offsetY, 0);
    }

    /// <summary>
    /// ���� ����� ���� ã���ϴ�.
    /// </summary>
    /// <param name="position">Ž�� ��ġ</param>
    /// <returns>���� ����� ���� �ݶ��̴�</returns>
    private Collider2D FindClosestEnemy(Vector3 position)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(position, range);
        Collider2D closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            if (enemy.GetComponent<Monster>())
            {
                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        return closestEnemy;
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        hitCount = 0;
        currentLevel = 0;
    }

    /// <summary>
    /// ���� ������ ������ �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���������� ������ ������ (����)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < damageIncreases.Length)
        {
            return damageIncreases[currentLevel + 1];
        }
        return 0;
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ �������� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
    }

    /// <summary>
    /// �ɷ��� ���� ���¿� ȿ���� �����ϴ� ���ڿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < damageIncreases.Length && currentLevel >= 0)
        {
            int damageIncrease = damageIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: ���� {hitThreshold}ȸ ���� ������ ����ü ��ȯ. ������ +{damageIncrease}";
        }
        else if (currentLevel >= damageIncreases.Length)
        {
            int maxDamageIncrease = damageIncreases[damageIncreases.Length - 1];
            return $"{baseDescription}\n�ִ� ���� ����: ���� {hitThreshold}ȸ ���� ������ ����ü ��ȯ. ������ +{maxDamageIncrease}";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����.";
        }
    }
}
