using UnityEngine;

public class CoinBomb : MonoBehaviour
{
    private int damage;
    private float bombDuration;
    private GameObject explosionEffectPrefab;

    /// <summary>
    /// 코인 폭탄을 초기화합니다.
    /// </summary>
    /// <param name="damage">폭탄의 피해량</param>
    /// <param name="bombDuration">폭탄의 지속 시간</param>
    /// <param name="explosionEffect">폭발 시 생성할 이펙트 프리팹</param>
    public void Initialize(int damage, float bombDuration, GameObject explosionEffect)
    {
        this.damage = damage;
        this.bombDuration = bombDuration;
        this.explosionEffectPrefab = explosionEffect;

        // 폭탄의 지속 시간 후 자동 폭발
        Invoke("Explode", bombDuration);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 몬스터와 충돌 시 폭발
        if (collision.CompareTag("Monster")) // 몬스터 태그 확인
        {
            Explode();
        }
    }

    /// <summary>
    /// 폭탄이 폭발하는 로직을 처리합니다.
    /// </summary>
    private void Explode()
    {
        // 폭발 이펙트 생성
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("CoinBomb: explosionEffectPrefab이 할당되지 않았습니다.");
        }

        // 주변 몬스터들에게 피해를 입히는 로직 (예시)
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2f); // 폭발 반경 5f

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Monster"))
            {
                // 몬스터의 TakeDamage 메서드 호출 (Assuming Monster 클래스가 존재)
                Monster monster = hitCollider.GetComponent<Monster>();
                if (monster != null)
                {
                    monster.TakeDamage(damage, this.transform.position);
                }
            }
        }

        // 폭발 후 폭탄 오브젝트 파괴
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // 폭발 반경 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 5f); // 폭발 반경 5f
    }
}
