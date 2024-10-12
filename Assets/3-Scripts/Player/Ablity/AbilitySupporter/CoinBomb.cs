using UnityEngine;
using System.Collections;

public class CoinBomb : MonoBehaviour
{
    private int damage;
    private float duration;

    private bool isExploded = false;

    /// <summary>
    /// 코인 폭탄을 초기화합니다.
    /// </summary>
    /// <param name="damage">폭탄의 피해량</param>
    /// <param name="duration">폭탄의 지속 시간</param>
    public void Initialize(int damage, float duration)
    {
        this.damage = damage;
        this.duration = duration;

        StartCoroutine(BombDuration());
    }

    /// <summary>
    /// 폭탄의 지속 시간이 지나면 폭탄을 제거합니다.
    /// </summary>
    private IEnumerator BombDuration()
    {
        yield return new WaitForSeconds(duration);

        if (!isExploded)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isExploded)
            return;

        if (collision.CompareTag("Monster"))
        {
            Monster monster = collision.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(damage, transform.position);

                // 폭발 효과 추가 가능
                // 예: Instantiate(explosionEffect, transform.position, Quaternion.identity);

                isExploded = true;

                Destroy(gameObject);
            }
        }
    }
}
