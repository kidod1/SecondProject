using UnityEngine;
using System.Collections;
using AK.Wwise;

[CreateAssetMenu(menuName = "Abilities/RicochetStrike")]
public class RicochetStrike : Ability
{
    [Tooltip("레벨별 투사체 데미지 증가량")]
    public int[] damageIncreases; // 레벨별 투사체 데미지 증가량 배열

    public int hitThreshold = 5;          // 적충 임계값
    public float range = 10f;             // 적 탐지 범위
    public GameObject projectilePrefab;   // 사용할 투사체 프리팹
    public int projectileCount = 3;       // 발생할 투사체의 수
    public float baseSpeedMultiplier = 1.0f;

    [Tooltip("투사체 소환 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event spawnProjectileSound;

    private Player playerInstance;
    private int hitCount = 0;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        playerInstance = player;
    }

    /// <summary>
    /// 투사체가 적에게 맞았을 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="enemy">맞은 적의 콜라이더</param>
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
    /// 유도 투사체를 생성합니다.
    /// </summary>
    private void SpawnHomingProjectiles()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("RicochetStrike: 투사체 프리팹이 할당되지 않았습니다.");
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
                projScript.Initialize(playerInstance.stat, playerInstance, true, baseSpeedMultiplier, GetProjectileDamage());
                projScript.SetDirection(randomDirection, baseSpeedMultiplier);

                // 0.3초 후에 유도 시작 및 Collider 활성화
                playerInstance.StartCoroutine(HomingTowardsEnemy(projectile, projScript, 0.3f, projectileCollider));
            }
            else
            {
                Debug.LogError("RicochetStrike: Projectile 스크립트가 없습니다.");
            }
        }

        // 투사체 소환 시 사운드 재생
        if (spawnProjectileSound != null && playerInstance != null)
        {
            spawnProjectileSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// 투사체의 데미지를 현재 레벨에 따라 가져옵니다.
    /// </summary>
    /// <returns>조정된 데미지 값</returns>
    private int GetProjectileDamage()
    {
        if (currentLevel < damageIncreases.Length)
        {
            return damageIncreases[currentLevel];
        }
        else
        {
            Debug.LogWarning($"RicochetStrike: currentLevel ({currentLevel}) exceeds damageIncreases 배열 범위. 마지막 레벨의 데미지를 사용합니다.");
            return damageIncreases[damageIncreases.Length - 1];
        }
    }

    /// <summary>
    /// 투사체가 유도되도록 설정하는 코루틴입니다.
    /// </summary>
    /// <param name="projectile">투사체 게임오브젝트</param>
    /// <param name="projScript">투사체 스크립트</param>
    /// <param name="delay">지연 시간</param>
    /// <param name="projectileCollider">투사체 콜라이더</param>
    /// <returns></returns>
    private IEnumerator HomingTowardsEnemy(GameObject projectile, Projectile projScript, float delay, Collider2D projectileCollider)
    {
        yield return new WaitForSeconds(delay);
        Destroy(projectile, 5f);  // 투사체가 생성된 후 5초 뒤에 파괴

        // Collider 활성화
        if (projectileCollider != null)
        {
            projectileCollider.enabled = true;
        }

        // 유도 시작
        Collider2D closestEnemy = FindClosestEnemy(projectile.transform.position);

        if (closestEnemy != null)
        {
            Vector2 directionToEnemy = (closestEnemy.transform.position - projectile.transform.position).normalized;
            projScript.SetDirection(directionToEnemy);
            Debug.Log($"RicochetStrike: 유도 투사체가 {closestEnemy.name}를 향해 유도됩니다.");
        }
        else
        {
            Debug.Log($"RicochetStrike: 주변에 유도할 적이 없습니다.");
        }
    }

    /// <summary>
    /// 무작위 방향을 생성합니다.
    /// </summary>
    /// <returns>무작위 방향 벡터</returns>
    private Vector2 GetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
    }

    /// <summary>
    /// 무작위 오프셋을 생성합니다.
    /// </summary>
    /// <returns>무작위 오프셋 벡터</returns>
    private Vector3 GetRandomOffset()
    {
        float offsetX = Random.Range(-1f, 1f);
        float offsetY = Random.Range(-1f, 1f);
        return new Vector3(offsetX, offsetY, 0);
    }

    /// <summary>
    /// 가장 가까운 적을 찾습니다.
    /// </summary>
    /// <param name="position">탐색 위치</param>
    /// <returns>가장 가까운 적의 콜라이더</returns>
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
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        hitCount = 0;
        currentLevel = 0;
    }

    /// <summary>
    /// 다음 레벨의 데미지 증가값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨에서의 데미지 증가량 (정수)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < damageIncreases.Length)
        {
            return damageIncreases[currentLevel + 1];
        }
        return 0;
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 데미지가 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
    }

    /// <summary>
    /// 능력의 현재 상태와 효과를 설명하는 문자열을 반환합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {
        if (currentLevel < damageIncreases.Length && currentLevel >= 0)
        {
            int damageIncrease = damageIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 적을 {hitThreshold}회 맞출 때마다 투사체 소환. 데미지 +{damageIncrease}";
        }
        else if (currentLevel >= damageIncreases.Length)
        {
            int maxDamageIncrease = damageIncreases[damageIncreases.Length - 1];
            return $"{baseDescription}\n최대 레벨 도달: 적을 {hitThreshold}회 맞출 때마다 투사체 소환. 데미지 +{maxDamageIncrease}";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달.";
        }
    }
}
