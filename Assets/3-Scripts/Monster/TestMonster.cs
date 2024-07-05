using System.Collections;
using UnityEngine;

public class TestMonster : Monster
{
    [SerializeField]
    public Transform[] patrolPoints; // 배회 포인트들
    [SerializeField]
    public float speed = 2f;
    [SerializeField]
    public float detectionRange = 5f;
    [SerializeField]
    public float explosionDelay = 1f;
    [SerializeField]
    public float explosionRadius = 3f;
    [SerializeField]
    public int damage = 50;

    private Collider2D collision;
    private int currentPatrolIndex = 0;
    private Transform player;
    private bool isChasingPlayer = false;

    protected override void Start()
    {
        currentHP = stat.maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        collision = GetComponent<Collider2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("플레이어 오브젝트를 찾을 수 없습니다.");
        }
    }

    private void Update()
    {
        if (isChasingPlayer)
        {
            MoveTowards(player.position);

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= 0.1f)
            {
                StartCoroutine(BlinkAndExplode());
            }
        }
        else
        {
            Patrol();

            if (Vector2.Distance(transform.position, player.position) <= detectionRange)
            {
                isChasingPlayer = true;
            }
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0)
            return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        MoveTowards(targetPoint.position);

        if (Vector2.Distance(transform.position, targetPoint.position) <= 0.1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private IEnumerator BlinkAndExplode()
    {
        if (speed == 0)
            yield break;
        Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;

        speed = 0;

        // 깜빡임 효과
        for (int i = 0; i < 5; i++)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            yield return new WaitForSeconds(0.1f);
            GetComponent<SpriteRenderer>().enabled = true;
            yield return new WaitForSeconds(0.1f);
        }

        // 폭발 효과
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D nearbyObject in colliders)
        {
            Player playerHealth = nearbyObject.GetComponent<Player>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, knockbackDirection);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}