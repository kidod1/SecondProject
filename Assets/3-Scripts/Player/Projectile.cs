using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float damageMultiplier = 1.0f;
    private PlayerData stat;
    private bool isCloneProjectile = false;
    private Rigidbody2D rb; // Rigidbody2D를 추가

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing on the projectile.");
        }
    }

    private void OnEnable()
    {
        if (stat != null)
        {
            Invoke(nameof(Deactivate), stat.projectileRange);
            rb.velocity = direction * stat.projectileSpeed; // 초기 속도 설정
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
        rb.velocity = Vector2.zero; // 비활성화 시 속도 초기화
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        if (rb != null)
        {
            rb.velocity = direction * stat.projectileSpeed; // 방향 설정 시 속도도 설정
        }
    }

    public void Initialize(PlayerData playerStat, bool isClone = false, float multiplier = 1.0f)
    {
        stat = playerStat;
        isCloneProjectile = isClone;
        damageMultiplier = multiplier;
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
                int damage = stat.playerDamage;
                if (isCloneProjectile)
                {
                    damage = Mathf.RoundToInt(damage * damageMultiplier);
                }
                monster.TakeDamage(damage);
            }
            gameObject.SetActive(false);
        }
    }
}
