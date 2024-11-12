using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int bulletAttackDamage;
    private Monster sourceMonster;
    private Rigidbody2D rb;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private CircleCollider2D bulletCollider;

    [SerializeField]
    private float maxLifetime = 5f;
    private float currentLifetime = 0f;
    private Vector3 initialSpriteScale;
    private bool scaleStart = false;

    [SerializeField]
    private float scaleMultiplier = 2f; // ������ ���� ����

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // SpriteRenderer�� �Ҵ���� �ʾҴٸ� ������Ʈ���� �ڵ����� �����ɴϴ�.
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning("SpriteRenderer�� �Ҵ���� �ʾ�����, �ش� GameObject�� SpriteRenderer ������Ʈ�� �����ϴ�.");
            }
        }

        if (spriteRenderer != null)
        {
            initialSpriteScale = spriteRenderer.transform.localScale;
            AdjustSpriteDirection();
        }
        else
        {
            // SpriteRenderer�� ���� �� �ʱ� �������� ������ �⺻ ��
            initialSpriteScale = Vector3.one;
        }
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
            float t = Mathf.Clamp01((currentLifetime - 0.5f) / (maxLifetime - 0.5f)); // 0.5�� �ĺ��� maxLifetime���� ����
            float newScaleX = Mathf.Lerp(initialSpriteScale.x, initialSpriteScale.x * scaleMultiplier, t); // X �������� �ʱⰪ���� ������ ������ ����

            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localScale = new Vector3(newScaleX, initialSpriteScale.y, initialSpriteScale.z);
            }
            else
            {
                // SpriteRenderer�� ���� �� �ٸ� ������ �����ϰų� �������� �������� ���� �� �ֽ��ϴ�.
                transform.localScale = new Vector3(newScaleX, initialSpriteScale.y, initialSpriteScale.z);
            }
        }

        // ������Ÿ�� ���� �� źȯ �ı�
        if (currentLifetime >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// źȯ�� ���� �������� �����մϴ�.
    /// </summary>
    /// <param name="damage">������ ������ ��</param>
    public void SetAttackDamage(int damage)
    {
        bulletAttackDamage = damage;
    }

    /// <summary>
    /// źȯ�� ��ó ���͸� �����մϴ�.
    /// </summary>
    /// <param name="monster">��ó�� �Ǵ� ����</param>
    public void SetSourceMonster(Monster monster)
    {
        sourceMonster = monster;
    }

    /// <summary>
    /// źȯ�� ���⿡ �°� ��������Ʈ�� �����մϴ�.
    /// </summary>
    private void AdjustSpriteDirection()
    {
        if (rb == null)
        {
            Debug.LogWarning("Rigidbody2D�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        Vector2 direction = rb.velocity.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (spriteRenderer != null)
        {
            if (direction.x < 0)
            {
                spriteRenderer.flipY = true;
            }
            else
            {
                spriteRenderer.flipY = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(bulletAttackDamage);
            }
            RecordHit("Player");
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            RecordHit("Wall");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// źȯ�� ��Ʈ�� Ÿ�԰� ��ó ���� ������ ����մϴ�.
    /// </summary>
    /// <param name="hitType">��Ʈ�� Ÿ�� ("Player" �Ǵ� "Wall")</param>
    private void RecordHit(string hitType)
    {
        if (sourceMonster != null)
        {
            // �߰����� ��Ʈ ��� ������ ���⿡ ������ �� �ֽ��ϴ�.
            // ��: sourceMonster.RegisterHit(hitType);
        }
    }
}
