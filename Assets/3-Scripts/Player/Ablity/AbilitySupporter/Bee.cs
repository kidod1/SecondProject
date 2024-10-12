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

        // �ڷ�ƾ ����
        StartCoroutine(HoverAndChase());
    }

    private IEnumerator HoverAndChase()
    {
        float elapsedTime = 0f;

        // �÷��̾� �ֺ��� �ɵ�
        while (elapsedTime < hoverDuration)
        {
            HoverAroundPlayer();
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ���� ���� ����
        isChasing = true;

        // ���� �ð� �� ����
        Destroy(gameObject, lifetime - hoverDuration);
    }

    private void HoverAroundPlayer()
    {
        if (playerInstance == null)
        {
            Destroy(gameObject);
            return;
        }

        // �÷��̾� �ֺ��� �������� �̵�
        float radius = 1.5f;
        float angularSpeed = 360f; // 1�ʿ� �� ���� ȸ��

        float angle = Time.time * angularSpeed % 360f;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        transform.position = playerInstance.transform.position + (Vector3)offset;
    }

    private void Update()
    {
        if (isChasing)
        {
            // ���� ����� ���� ã��
            Monster target = FindNearestMonster();

            if (target != null)
            {
                // ���͸� ���� �̵�
                Vector2 direction = (target.transform.position - transform.position).normalized;
                rb.velocity = direction * speed;

                // ���� ���� ���� ������ ����
                if (Vector2.Distance(transform.position, target.transform.position) <= attackRange)
                {
                    Attack(target);
                }
            }
            else
            {
                // ���Ͱ� ������ ���ڸ����� ���
                rb.velocity = Vector2.zero;
            }
        }
    }

    private Monster FindNearestMonster()
    {
        float detectionRange = 10f; // ���� ���� ���� ����
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
        Destroy(gameObject); // ���� �� �� ����
    }

    private void OnDrawGizmosSelected()
    {
        // ���� ���� �ð�ȭ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
