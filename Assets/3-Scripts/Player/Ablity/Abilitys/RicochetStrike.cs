using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/RicochetStrike")]
public class RicochetStrike : Ability
{
    public int hitThreshold = 5;          // 적충 임계값
    public float range = 10f;             // 적 탑상 범위
    public GameObject projectilePrefab;   // 사용할 투사체 프리텀
    public int projectileCount = 3;       // 발생할 투사체의 수
    public float baseSpeedMultiplier = 1.0f;

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
            Debug.LogError("투사체 프리텀이 없습니다.");
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
                // 초기에는 Collider 비활성화
                if (projectileCollider != null)
                {
                    projectileCollider.enabled = false;
                }

                Vector2 randomDirection = GetRandomDirection();
                projScript.Initialize(playerInstance.stat, playerInstance, true, baseSpeedMultiplier);
                projScript.SetDirection(randomDirection, baseSpeedMultiplier);

                // 0.5초 후에 유도 시작 및 Collider 활성화
                playerInstance.StartCoroutine(HomingTowardsEnemy(projectile, projScript, 0.3f, projectileCollider));
            }
        }
    }

    private IEnumerator HomingTowardsEnemy(GameObject projectile, Projectile projScript, float delay, Collider2D projectileCollider)
    {
        yield return new WaitForSeconds(delay);
        Destroy(projectile, 5f);  // 투사체가 생성된 후 5초 뒤에 파괴

        // Collider 활성화
        if (projectileCollider != null)
        {
            projectileCollider.enabled = true;
        }

        // 유도 시작전 1초 동안 속도를 0으로 설정


        // 유도 시작
        Collider2D closestEnemy = FindClosestEnemy(projectile.transform.position);

        if (closestEnemy != null)
        {
            Vector2 directionToEnemy = (closestEnemy.transform.position - projectile.transform.position).normalized;
            projScript.SetDirection(directionToEnemy);
        }
        else
        {
            // 적이 없을 경우 투사체 파괴는 하지 않음
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
        currentLevel = 0;
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