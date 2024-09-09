using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int attackDamage;
    private Monster sourceMonster;
    private Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public CircleCollider2D bulletCollider;

    public float maxLifetime = 3f;
    private float currentLifetime = 0f;
    private Vector3 initialSpriteScale;
    private bool scaleStart = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialSpriteScale = spriteRenderer.transform.localScale;
        AdjustSpriteDirection();
    }

    private void Update()
    {
        currentLifetime += Time.deltaTime;

        // 0.5�� ���Ŀ� ������ ���� ����
        if (currentLifetime > 0.5f && !scaleStart)
        {
            scaleStart = true;
        }

        if (scaleStart)
        {
            float t = Mathf.Clamp01((currentLifetime - 0.5f) / (maxLifetime - 0.5f)); // 0.5�� �ĺ��� 2���� ����
            float newScaleX = Mathf.Lerp(1.5f, 2f, t); // X �������� 1.5���� 2�� ���������� ����
            spriteRenderer.transform.localScale = new Vector3(newScaleX, initialSpriteScale.y, initialSpriteScale.z);
        }

        // ������Ÿ�� ���� �� źȯ �ı�
        if (currentLifetime >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetAttackDamage(int damage)
    {
        attackDamage = damage;
    }

    public void SetSourceMonster(Monster monster)
    {
        sourceMonster = monster;
    }

    private void AdjustSpriteDirection()
    {
        Vector2 direction = rb.velocity.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (direction.x < 0)
        {
            spriteRenderer.flipY = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().TakeDamage(attackDamage);
            RecordHit();
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }

    private void RecordHit()
    {
        if (sourceMonster != null)
        {
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
