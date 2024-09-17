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
                // 플레이어의 현재 공격력을 가져와 데미지 계산
                int damage = stat.currentPlayerDamage;
                if (isCloneProjectile)
                {
                    damage = Mathf.RoundToInt(damage * damageMultiplier);
                }
                monster.TakeDamage(damage); // 몬스터에게 데미지 적용

                if (playerInstance != null)
                {
                    playerInstance.OnHitEnemy?.Invoke(collision);
                }

                // 기절 여부 판정
                if (playerInstance != null && playerInstance.CanStun())
                {
                    float stunChance = 0.25f; // 25% 확률로 기절
                    if (Random.value < stunChance)
                    {
                        monster.Stun(); // 몬스터 기절시키기
                        Debug.Log($"{monster.name}이(가) 기절했습니다."); // 몬스터 이름과 함께 기절 메시지 출력
                    }
                    else
                    {
                        Debug.Log($"{monster.name}은(는) 기절하지 않았습니다."); // 기절하지 않았을 때 메시지 출력
                    }
                }
            }
            gameObject.SetActive(false); // 투사체 제거
        }
        else if (collision.GetComponent<DestructibleObject>() != null)
        {
            DestructibleObject destructible = collision.GetComponent<DestructibleObject>();
            if (destructible != null)
            {
                // 플레이어의 공격력을 가져와 데미지 계산
                int damage = stat.currentPlayerDamage;
                if (isCloneProjectile)
                {
                    damage = Mathf.RoundToInt(damage * damageMultiplier);
                }
                destructible.TakeDamage(damage); // 파괴 가능한 오브젝트에 데미지 적용
            }
            gameObject.SetActive(false); // 투사체 제거
        }
    }
}
