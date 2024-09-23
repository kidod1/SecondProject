using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/RicochetStrike")]
public class RicochetStrike : Ability
{
    public int hitThreshold = 5;          // ���� �Ӱ谪
    public float range = 10f;             // �� Ž�� ����
    public GameObject projectilePrefab;   // ����� ����ü ������
    public int projectileCount = 3;       // �߻��� ����ü�� ��
    public float baseSpeedMultiplier = 1.0f;
    public float speedIncreaseMultiplier = 2.0f;

    private Player playerInstance;
    private int hitCount = 0;

    public override void Apply(Player player)
    {
        playerInstance = player;
    }

    public void OnProjectileHit(Collider2D enemy)
    {
        hitCount++;
        Debug.Log($"RicochetStrike HitCount: {hitCount}");
        if (hitCount >= hitThreshold)
        {
            hitCount = 0;
            SpawnHomingProjectiles();
        }
    }

    private void SpawnHomingProjectiles()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("����ü �������� �����ϴ�.");
            return;
        }

        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 spawnPosition = playerInstance.transform.position + GetRandomOffset();

            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            Projectile projScript = projectile.GetComponent<Projectile>();

            if (projScript != null)
            {
                Vector2 randomDirection = GetRandomDirection();
                projScript.Initialize(playerInstance.stat, playerInstance, true, baseSpeedMultiplier);
                projScript.SetDirection(randomDirection);

                // ���� ����
                playerInstance.StartCoroutine(HomingTowardsEnemy(projectile, projScript));
            }
        }
    }

    private IEnumerator HomingTowardsEnemy(GameObject projectile, Projectile projScript)
    {
        yield return new WaitForSecondsRealtime(0.25f);  // �ʿ信 ���� ������ ����

        Collider2D closestEnemy = FindClosestEnemy(projectile.transform.position);

        if (closestEnemy != null)
        {
            Vector2 directionToEnemy = (closestEnemy.transform.position - projectile.transform.position).normalized;
            projScript.SetDirection(directionToEnemy, speedIncreaseMultiplier);
        }
        else
        {
            Destroy(projectile);
        }
    }

    private Vector2 GetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
    }

    private Vector3 GetRandomOffset()
    {
        float offsetX = Random.Range(-1f, 1f);
        float offsetY = Random.Range(-1f, 1f);
        return new Vector3(offsetX, offsetY, 0);
    }

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

    public override void ResetLevel()
    {
        base.ResetLevel();
        hitCount = 0;
    }

    protected override int GetNextLevelIncrease()
    {
        return 1;
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            projectileCount++;
        }
    }
}
