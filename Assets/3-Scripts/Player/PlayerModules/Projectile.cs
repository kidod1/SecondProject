using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float damageMultiplier = 1.0f;
    private PlayerData stat;
    private bool isCloneProjectile = false;
    private Rigidbody2D rb;
    private float lifetime; // ����ü�� ���� �ð�

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D�� �����ϴ�.");
        }
    }

    private void OnEnable()
    {
        if (stat != null)
        {
            // ����ü�� ���� �ð��� stat���� ������ ����
            lifetime = stat.currentProjectileRange;
            // lifetime �Ŀ� Deactivate �޼��带 ȣ��
            Invoke(nameof(Deactivate), lifetime);
            // ����ü�� ���⿡ ���� �ӵ� ����
            rb.velocity = direction * stat.currentProjectileSpeed;
        }
    }

    private void OnDisable()
    {
        // Ȱ��ȭ ���� �� ��� Invoke ���
        CancelInvoke();
        // ����ü�� �ӵ��� 0���� ����
        rb.velocity = Vector2.zero;
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        if (rb != null)
        {
            // ���ο� ����� �ӵ� ����
            rb.velocity = direction * stat.currentProjectileSpeed;
        }
    }

    public void Initialize(PlayerData playerStat, bool isClone = false, float multiplier = 1.0f)
    {
        stat = playerStat;
        isCloneProjectile = isClone;
        damageMultiplier = multiplier;

        if (rb != null)
        {
            // �ʱ�ȭ �� ����ü�� �ӵ��� 0���� ����
            rb.velocity = Vector2.zero;
        }

        // ���࿡ Initialize ȣ�� �� �ٷ� OnEnable ȿ���� �ְ� ������ ���⿡ �缳��
        if (gameObject.activeSelf)
        {
            // ���� ������Ʈ�� Ȱ��ȭ�� ���¶�� �ٽ� OnEnable�� ������ ����
            OnEnable();
        }
    }

    private void Deactivate()
    {
        // ����ü ��Ȱ��ȭ
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            // ���� ������ ��Ȱ��ȭ
            gameObject.SetActive(false);
        }
        else if (collision.GetComponent<Monster>() != null)
        {
            Monster monster = collision.GetComponent<Monster>();
            if (monster != null)
            {
                // ���Ϳ��� ���� ����
                int damage = stat.currentPlayerDamage;
                if (isCloneProjectile)
                {
                    damage = Mathf.RoundToInt(damage * damageMultiplier);
                }
                monster.TakeDamage(damage);
            }
            // ���Ϳ� ������ ��Ȱ��ȭ
            gameObject.SetActive(false);
        }
    }
}
