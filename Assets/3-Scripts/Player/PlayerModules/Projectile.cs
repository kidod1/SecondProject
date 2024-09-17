using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Player playerInstance;
    private Vector2 direction;
    private float damageMultiplier = 1.0f;
    [SerializeField]
    protected PlayerData stat;
    private bool isCloneProjectile = false;
    protected Rigidbody2D rb;
    private float lifetime;

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
            lifetime = stat.currentProjectileRange;
            Invoke(nameof(Deactivate), lifetime);
            rb.velocity = direction * stat.currentProjectileSpeed;
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
        rb.velocity = Vector2.zero;
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        if (rb != null)
        {
            rb.velocity = direction * stat.currentProjectileSpeed;
        }
    }

    public void Initialize(PlayerData playerStat, Player playerInstance, bool isClone = false, float multiplier = 1.0f)
    {
        stat = playerStat;
        this.playerInstance = playerInstance;
        isCloneProjectile = isClone;
        damageMultiplier = multiplier;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (gameObject.activeSelf)
        {
            OnEnable();
        }
    }


    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            gameObject.SetActive(false);
        }
        else if (collision.GetComponent<Monster>() != null)
        {
            Monster monster = collision.GetComponent<Monster>();
            if (monster != null)
            {
                // �÷��̾��� ���� ���ݷ��� ������ ������ ���
                int damage = stat.currentPlayerDamage;
                if (isCloneProjectile)
                {
                    damage = Mathf.RoundToInt(damage * damageMultiplier);
                }
                monster.TakeDamage(damage); // ���Ϳ��� ������ ����

                if (playerInstance != null)
                {
                    playerInstance.OnHitEnemy?.Invoke(collision);
                }

                // ���� ���� ����
                if (playerInstance != null && playerInstance.CanStun())
                {
                    float stunChance = 0.25f; // 25% Ȯ���� ����
                    if (Random.value < stunChance)
                    {
                        monster.Stun(); // ���� ������Ű��
                        Debug.Log($"{monster.name}��(��) �����߽��ϴ�."); // ���� �̸��� �Բ� ���� �޽��� ���
                    }
                    else
                    {
                        Debug.Log($"{monster.name}��(��) �������� �ʾҽ��ϴ�."); // �������� �ʾ��� �� �޽��� ���
                    }
                }
            }
            gameObject.SetActive(false); // ����ü ����
        }
        else if (collision.GetComponent<DestructibleObject>() != null)
        {
            DestructibleObject destructible = collision.GetComponent<DestructibleObject>();
            if (destructible != null)
            {
                // �÷��̾��� ���ݷ��� ������ ������ ���
                int damage = stat.currentPlayerDamage;
                if (isCloneProjectile)
                {
                    damage = Mathf.RoundToInt(damage * damageMultiplier);
                }
                destructible.TakeDamage(damage); // �ı� ������ ������Ʈ�� ������ ����
            }
            gameObject.SetActive(false); // ����ü ����
        }
    }
}
