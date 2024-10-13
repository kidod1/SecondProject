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
    private float maxLifetime = 3f;
    private float currentLifetime = 0f;
    private Vector3 initialSpriteScale;
    private bool scaleStart = false;

    [SerializeField]
    private float scaleMultiplier = 2f; // 스케일 증가 배율

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialSpriteScale = spriteRenderer.transform.localScale;
        AdjustSpriteDirection();
    }

    private void Update()
    {
        currentLifetime += Time.deltaTime;

        // 0.5초 이후에 스케일 변경 시작
        if (currentLifetime > 0.5f && !scaleStart)
        {
            scaleStart = true;
        }

        if (scaleStart)
        {
            float t = Mathf.Clamp01((currentLifetime - 0.5f) / (maxLifetime - 0.5f)); // 0.5초 후부터 maxLifetime까지 보간
            float newScaleX = Mathf.Lerp(initialSpriteScale.x, initialSpriteScale.x * scaleMultiplier, t); // X 스케일을 초기값에서 증가된 값으로 변경
            spriteRenderer.transform.localScale = new Vector3(newScaleX, initialSpriteScale.y, initialSpriteScale.z);
        }

        // 라이프타임 종료 시 탄환 파괴
        if (currentLifetime >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 탄환의 공격 데미지를 설정합니다.
    /// </summary>
    /// <param name="damage">설정할 데미지 값</param>
    public void SetAttackDamage(int damage)
    {
        bulletAttackDamage = damage;
    }

    /// <summary>
    /// 탄환의 출처 몬스터를 설정합니다.
    /// </summary>
    /// <param name="monster">출처가 되는 몬스터</param>
    public void SetSourceMonster(Monster monster)
    {
        sourceMonster = monster;
    }

    /// <summary>
    /// 탄환의 방향에 맞게 스프라이트를 조정합니다.
    /// </summary>
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
    /// 탄환이 히트된 타입과 출처 몬스터 정보를 기록합니다.
    /// </summary>
    /// <param name="hitType">히트된 타입 ("Player" 또는 "Wall")</param>
    private void RecordHit(string hitType)
    {
        if (sourceMonster != null)
        {
            // 추가적인 히트 기록 로직을 여기에 구현할 수 있습니다.
        }
    }
}
