using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float damageMultiplier = 1.0f;
    private PlayerData stat;
    private bool isCloneProjectile = false;
    private Rigidbody2D rb;
    private float lifetime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D가 없습니다.");
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

    public void Initialize(PlayerData playerStat, bool isClone = false, float multiplier = 1.0f)
    {
        stat = playerStat;
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
                int damage = stat.currentPlayerDamage;
                if (isCloneProjectile)
                {
                    damage = Mathf.RoundToInt(damage * damageMultiplier);
                }
                monster.TakeDamage(damage);
            }
            gameObject.SetActive(false);
        }
        else if (collision.GetComponent<DestructibleObject>() != null)
        {
            DestructibleObject destructible = collision.GetComponent<DestructibleObject>();
            if (destructible != null)
            {
                int damage = stat.currentPlayerDamage;
                if (isCloneProjectile)
                {
                    damage = Mathf.RoundToInt(damage * damageMultiplier);
                }
                destructible.TakeDamage(damage);
            }
            gameObject.SetActive(false);
        }
    }
}
