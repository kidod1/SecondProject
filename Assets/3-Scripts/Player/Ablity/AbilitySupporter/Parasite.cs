using UnityEngine;

public class Parasite : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 10;
    public float lifetime = 5f;
    private Transform target;

    private void Start()
    {
        // 가장 가까운 적을 타겟으로 설정
        target = FindClosestEnemy();
        Destroy(gameObject, lifetime); // 일정 시간 후 자동 파괴
    }

    private void Update()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.unscaledDeltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private Transform FindClosestEnemy()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        Monster[] enemies = FindObjectsOfType<Monster>();
        foreach (Monster enemy in enemies)
        {
            if (enemy.IsDead) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Monster enemy = collision.GetComponent<Monster>();
        if (enemy != null && !enemy.IsDead)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
