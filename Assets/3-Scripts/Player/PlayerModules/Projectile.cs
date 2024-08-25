using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float damageMultiplier = 1.0f;
    private PlayerData stat;
    private bool isCloneProjectile = false;
    private Rigidbody2D rb;
    private float lifetime; // 투사체의 생명 시간

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
            // 투사체의 생명 시간을 stat에서 가져와 설정
            lifetime = stat.currentProjectileRange;
            // lifetime 후에 Deactivate 메서드를 호출
            Invoke(nameof(Deactivate), lifetime);
            // 투사체의 방향에 따른 속도 설정
            rb.velocity = direction * stat.currentProjectileSpeed;
        }
    }

    private void OnDisable()
    {
        // 활성화 해제 시 모든 Invoke 취소
        CancelInvoke();
        // 투사체의 속도를 0으로 설정
        rb.velocity = Vector2.zero;
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        if (rb != null)
        {
            // 새로운 방향과 속도 설정
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
            // 초기화 시 투사체의 속도를 0으로 설정
            rb.velocity = Vector2.zero;
        }

        // 만약에 Initialize 호출 후 바로 OnEnable 효과를 주고 싶으면 여기에 재설정
        if (gameObject.activeSelf)
        {
            // 현재 오브젝트가 활성화된 상태라면 다시 OnEnable의 로직을 실행
            OnEnable();
        }
    }

    private void Deactivate()
    {
        // 투사체 비활성화
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            // 벽에 닿으면 비활성화
            gameObject.SetActive(false);
        }
        else if (collision.GetComponent<Monster>() != null)
        {
            Monster monster = collision.GetComponent<Monster>();
            if (monster != null)
            {
                // 몬스터에게 피해 적용
                int damage = stat.currentPlayerDamage;
                if (isCloneProjectile)
                {
                    damage = Mathf.RoundToInt(damage * damageMultiplier);
                }
                monster.TakeDamage(damage);
            }
            // 몬스터에 닿으면 비활성화
            gameObject.SetActive(false);
        }
    }
}
