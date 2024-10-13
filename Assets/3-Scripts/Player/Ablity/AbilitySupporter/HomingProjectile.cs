using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    private PlayerData stat;
    private float startDelay;
    private float speed;
    private float range;
    private Vector2 initialDirection;

    private Vector3 initialPosition;

    private GameObject target;
    private Rigidbody2D rb;

    private bool isHomingActive = false; // 유도 시작 여부

    /// <summary>
    /// 유도 탄환을 초기화합니다.
    /// </summary>
    /// <param name="playerStat">플레이어의 데이터</param>
    /// <param name="delay">유도 시작 지연 시간 (초)</param>
    /// <param name="homingSpeed">유도 속도</param>
    /// <param name="homingRange">유도 범위</param>
    public void Initialize(PlayerData playerStat, float delay, float homingSpeed, float homingRange)
    {
        stat = playerStat;
        startDelay = delay;
        speed = homingSpeed;
        range = homingRange;
        initialPosition = transform.position;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("HomingProjectile: Rigidbody2D가 없습니다.");
        }

        // 유도 시작 지연 후 유도 로직 시작
        Invoke(nameof(StartHoming), startDelay);
    }

    /// <summary>
    /// 유도 로직을 시작합니다.
    /// </summary>
    private void StartHoming()
    {
        isHomingActive = true;
    }

    private void FixedUpdate()
    {
        // 범위 초과 시 비활성화
        if (Vector3.Distance(initialPosition, transform.position) > range)
        {
            gameObject.SetActive(false);
            return;
        }

        if (rb != null)
        {
            if (isHomingActive)
            {
                // 타겟을 지속적으로 추적
                if (target == null || !target.activeInHierarchy)
                {
                    // 타겟이 없거나 비활성화되었으면 새로 찾기
                    target = FindNearestEnemy();
                    if (target == null)
                    {
                        // 타겟이 없으면 직선으로 이동
                        rb.velocity = initialDirection * speed;
                        return;
                    }
                }

                Vector2 direction = ((Vector2)target.transform.position - rb.position).normalized;
                rb.velocity = direction * speed;

                // 탄환의 회전 설정 (탄환이 진행 방향을 바라보도록)
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rb.rotation = angle;
            }
            else
            {
                // 유도 시작 전에는 초기 방향으로 이동
                rb.velocity = initialDirection * speed;
            }
        }
    }

    /// <summary>
    /// 가장 가까운 적을 찾습니다.
    /// </summary>
    /// <returns>가장 가까운 적의 게임 오브젝트</returns>
    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Monster");
        GameObject nearest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            // 적이 활성화되어 있는지 확인
            if (!enemy.activeInHierarchy)
                continue;

            float dist = Vector3.Distance(enemy.transform.position, currentPos);
            if (dist < minDist)
            {
                nearest = enemy;
                minDist = dist;
            }
        }

        return nearest;
    }

    /// <summary>
    /// 발사 방향을 설정합니다.
    /// </summary>
    /// <param name="dir">발사 방향</param>
    public void SetDirection(Vector2 dir)
    {
        initialDirection = dir.normalized;
        // Rigidbody2D를 이용하여 초기 속도 설정
        if (rb != null)
        {
            rb.velocity = initialDirection * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 몬스터와 충돌 처리
        if (collision.CompareTag("Monster"))
        {
            Monster monster = collision.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(stat.currentPlayerDamage, transform.position);
                // 탄환 비활성화
                gameObject.SetActive(false);
            }
        }
        // 기타 충돌 처리 (벽 등)
        else if (collision.CompareTag("Wall"))
        {
            // 탄환 비활성화
            gameObject.SetActive(false);
        }
    }
}
