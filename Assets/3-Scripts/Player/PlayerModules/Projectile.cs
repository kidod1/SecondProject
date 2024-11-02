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

    private static Transform projectileParent;

    [SerializeField]
    private int projectileRealDamage;
    public int projectileCurrentDamage = 0; // 현재 데미지

    [SerializeField]
    private float customSpeed = 10f; // 커스텀 속도
    [SerializeField]
    private float customLifetime = 2f; // 커스텀 생명 시간

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D가 없습니다.");
        }

        if (projectileParent == null)
        {
            GameObject parentObj = new GameObject("ProjectileParent");
            projectileParent = parentObj.transform;
        }

        transform.SetParent(projectileParent);
    }

    private void OnEnable()
    {
        if (stat != null)
        {
            // 생명 시간을 커스텀 값으로 설정
            lifetime = customLifetime;
            Invoke(nameof(Deactivate), lifetime);

            // 속도를 커스텀 값으로 설정
            rb.velocity = direction * customSpeed;
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
        rb.velocity = Vector2.zero;
        isCloneProjectile = false; // 비활성화 시 초기화
    }

    /// <summary>
    /// 프로젝트일의 방향과 속도를 설정합니다.
    /// </summary>
    /// <param name="newDirection">설정할 방향</param>
    /// <param name="speedMultiplier">속도 증가 배율</param>
    public void SetDirection(Vector2 newDirection, float speedMultiplier = 1.0f)
    {
        direction = newDirection.normalized;
        if (rb != null)
        {
            rb.velocity = direction * customSpeed * speedMultiplier;
        }
    }

    /// <summary>
    /// 프로젝트일을 초기화합니다.
    /// </summary>
    /// <param name="playerStat">플레이어의 데이터</param>
    /// <param name="playerInstance">플레이어 인스턴스</param>
    /// <param name="isClone">클론 프로젝트일 여부</param>
    /// <param name="multiplier">데미지 배율</param>
    /// <param name="damage">프로젝트일의 데미지</param>
    /// <param name="speed">프로젝트일의 속도</param>
    /// <param name="lifetime">프로젝트일의 생명 시간</param>
    public void Initialize(PlayerData playerStat, Player playerInstance, bool isClone = false, float multiplier = 1.0f, int damage = 10, float speed = 10f, float lifetime = 2f)
    {
        stat = playerStat;
        this.playerInstance = playerInstance;
        isCloneProjectile = isClone;
        damageMultiplier = multiplier;
        projectileCurrentDamage = damage; // 데미지 설정

        // 커스텀 속도와 생명 시간 설정
        customSpeed = speed;
        customLifetime = lifetime;

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
        if (isCloneProjectile)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            Deactivate();
        }
        else
        {
            // Monster와 MidBoss를 함께 처리
            bool isDamageApplied = false;

            // Monster 또는 MidBoss 컴포넌트 가져오기
            MonoBehaviour damageable = collision.GetComponent<Monster>() as MonoBehaviour;
            if (damageable == null)
            {
                damageable = collision.GetComponent<MidBoss>() as MonoBehaviour;
            }
            if (damageable == null)
            {
                damageable = collision.GetComponentInParent<Monster>() as MonoBehaviour;
            }
            if (damageable == null)
            {
                damageable = collision.GetComponentInParent<MidBoss>() as MonoBehaviour;
            }

            if (damageable != null)
            {
                int damage = Mathf.RoundToInt(projectileCurrentDamage);
                if (isCloneProjectile)
                {
                    damage = Mathf.RoundToInt(damage * damageMultiplier);
                }

                // 데미지 적용 및 추가 로직 처리
                if (damageable is Monster monster)
                {
                    monster.TakeDamage(damage, transform.position);

                    if (playerInstance != null && !isCloneProjectile)
                    {
                        playerInstance.abilityManager.ActivateAbilitiesOnHit(collision);
                    }

                    if (playerInstance != null && playerInstance.CanStun())
                    {
                        float stunChance = 0.25f;
                        if (UnityEngine.Random.value < stunChance)
                        {
                            monster.Stun(2f);
                            Debug.Log($"{monster.name}이(가) 기절했습니다.");
                        }
                    }
                    isDamageApplied = true;
                }
                else if (damageable is MidBoss midBoss)
                {
                    midBoss.TakeDamage(damage, transform.position);

                    if (playerInstance != null && !isCloneProjectile)
                    {
                        playerInstance.abilityManager.ActivateAbilitiesOnHit(collision);
                    }

                    // MidBoss는 스턴되지 않도록 처리하거나 필요 시 스턴 로직 추가
                    isDamageApplied = true;
                }
            }

            if (isDamageApplied)
            {
                Deactivate();
            }
            else if (collision.GetComponent<DestructibleObject>() != null)
            {
                // 파괴 가능한 오브젝트 처리
                DestructibleObject destructible = collision.GetComponent<DestructibleObject>();
                if (destructible != null)
                {
                    int damage = Mathf.RoundToInt(projectileCurrentDamage);
                    if (isCloneProjectile)
                    {
                        damage = Mathf.RoundToInt(damage * damageMultiplier);
                    }
                    destructible.TakeDamage(damage);
                }
                Deactivate();
            }
        }
    }
}
