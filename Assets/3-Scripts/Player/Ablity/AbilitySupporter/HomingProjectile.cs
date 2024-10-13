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

    private bool isHomingActive = false; // ���� ���� ����

    /// <summary>
    /// ���� źȯ�� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="playerStat">�÷��̾��� ������</param>
    /// <param name="delay">���� ���� ���� �ð� (��)</param>
    /// <param name="homingSpeed">���� �ӵ�</param>
    /// <param name="homingRange">���� ����</param>
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
            Debug.LogError("HomingProjectile: Rigidbody2D�� �����ϴ�.");
        }

        // ���� ���� ���� �� ���� ���� ����
        Invoke(nameof(StartHoming), startDelay);
    }

    /// <summary>
    /// ���� ������ �����մϴ�.
    /// </summary>
    private void StartHoming()
    {
        isHomingActive = true;
    }

    private void FixedUpdate()
    {
        // ���� �ʰ� �� ��Ȱ��ȭ
        if (Vector3.Distance(initialPosition, transform.position) > range)
        {
            gameObject.SetActive(false);
            return;
        }

        if (rb != null)
        {
            if (isHomingActive)
            {
                // Ÿ���� ���������� ����
                if (target == null || !target.activeInHierarchy)
                {
                    // Ÿ���� ���ų� ��Ȱ��ȭ�Ǿ����� ���� ã��
                    target = FindNearestEnemy();
                    if (target == null)
                    {
                        // Ÿ���� ������ �������� �̵�
                        rb.velocity = initialDirection * speed;
                        return;
                    }
                }

                Vector2 direction = ((Vector2)target.transform.position - rb.position).normalized;
                rb.velocity = direction * speed;

                // źȯ�� ȸ�� ���� (źȯ�� ���� ������ �ٶ󺸵���)
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rb.rotation = angle;
            }
            else
            {
                // ���� ���� ������ �ʱ� �������� �̵�
                rb.velocity = initialDirection * speed;
            }
        }
    }

    /// <summary>
    /// ���� ����� ���� ã���ϴ�.
    /// </summary>
    /// <returns>���� ����� ���� ���� ������Ʈ</returns>
    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Monster");
        GameObject nearest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            // ���� Ȱ��ȭ�Ǿ� �ִ��� Ȯ��
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
    /// �߻� ������ �����մϴ�.
    /// </summary>
    /// <param name="dir">�߻� ����</param>
    public void SetDirection(Vector2 dir)
    {
        initialDirection = dir.normalized;
        // Rigidbody2D�� �̿��Ͽ� �ʱ� �ӵ� ����
        if (rb != null)
        {
            rb.velocity = initialDirection * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ���Ϳ� �浹 ó��
        if (collision.CompareTag("Monster"))
        {
            Monster monster = collision.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(stat.currentPlayerDamage, transform.position);
                // źȯ ��Ȱ��ȭ
                gameObject.SetActive(false);
            }
        }
        // ��Ÿ �浹 ó�� (�� ��)
        else if (collision.CompareTag("Wall"))
        {
            // źȯ ��Ȱ��ȭ
            gameObject.SetActive(false);
        }
    }
}
