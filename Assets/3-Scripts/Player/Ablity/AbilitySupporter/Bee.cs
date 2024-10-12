using System.Collections;
using UnityEngine;

public class Bee : MonoBehaviour
{
    private Player playerInstance;
    private float speed;
    private int damage;
    private float hoverDuration;
    private float lifetime;
    private float attackRange;

    private Rigidbody2D rb;
    private bool isChasing = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Player player, float speed, int damage, float hoverDuration, float lifetime, float attackRange)
    {
        this.playerInstance = player;
        this.speed = speed;
        this.damage = damage;
        this.hoverDuration = hoverDuration;
        this.lifetime = lifetime;
        this.attackRange = attackRange;

        // 코루틴 시작
        StartCoroutine(HoverAndChase());
    }

    private IEnumerator HoverAndChase()
    {
        float elapsedTime = 0f;

        // 플레이어 주변을 맴돔
        while (elapsedTime < hoverDuration)
        {
            HoverAroundPlayer();
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 몬스터 추적 시작
        isChasing = true;

        // 일정 시간 후 제거
        Destroy(gameObject, lifetime - hoverDuration);
    }

    private void HoverAroundPlayer()
    {
        if (playerInstance == null)
        {
            Destroy(gameObject);
            return;
        }

        // 플레이어 주변을 원형으로 이동
        float radius = 1.5f;
        float angularSpeed = 360f; // 1초에 한 바퀴 회전

        float angle = Time.time * angularSpeed % 360f;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        transform.position = playerInstance.transform.position + (Vector3)offset;
    }

    private void Update()
    {
        if (isChasing)
        {
            // 가장 가까운 몬스터 찾기
            Monster target = FindNearestMonster();

            if (target != null)
            {
                // 몬스터를 향해 이동
                Vector2 direction = (target.transform.position - transform.position).normalized;
                rb.velocity = direction * speed;

                // 공격 범위 내에 들어오면 공격
                if (Vector2.Distance(transform.position, target.transform.position) <= attackRange)
                {
                    Attack(target);
                }
            }
            else
            {
                // 몬스터가 없으면 제자리에서 대기
                rb.velocity = Vector2.zero;
            }
        }
    }

    private Monster FindNearestMonster()
    {
        float detectionRange = 10f; // 몬스터 감지 범위 설정
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        Monster nearestMonster = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Monster monster = hit.GetComponent<Monster>();
            if (monster != null)
            {
                float distance = Vector2.Distance(transform.position, monster.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestMonster = monster;
                }
            }
        }

        return nearestMonster;
    }

    private void Attack(Monster target)
    {
        target.TakeDamage(damage, transform.position);
        Destroy(gameObject); // 공격 후 벌 제거
    }

    private void OnDrawGizmosSelected()
    {
        // 공격 범위 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
