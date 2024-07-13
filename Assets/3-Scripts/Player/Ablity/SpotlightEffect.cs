using System.Collections;
using UnityEngine;

public class SpotlightEffect : MonoBehaviour
{
    public float damageInterval = 1f; // 피해 간격
    public float damageRadius = 2f; // 범위
    public int damageAmount = 20; // 피해량
    public Player player;
    private Collider2D[] hits = new Collider2D[10];

    [SerializeField]
    private SpriteRenderer spriteRenderer; // 스프라이트 렌더러

    private void Start()
    {
        StartCoroutine(DamageCoroutine());
    }

    private IEnumerator DamageCoroutine()
    {
        while (true)
        {
            int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, damageRadius, hits);
            for (int i = 0; i < hitCount; i++)
            {
                Monster monster = hits[i].GetComponent<Monster>();
                if (monster != null)
                {
                    monster.TakeDamage(damageAmount);
                }
            }
            yield return new WaitForSeconds(damageInterval);
        }
    }

    private void Update()
    {
        if (player != null)
        {
            transform.position = player.transform.position;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
