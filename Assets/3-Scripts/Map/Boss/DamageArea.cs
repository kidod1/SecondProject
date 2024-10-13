using UnityEngine;
using System.Collections;

public class DamageArea : MonoBehaviour
{
    public int damage = 10;
    public float duration = 2f;
    public bool isContinuous = false; // true인 경우 지속적으로 데미지를 입힘

    private void Start()
    {
        Destroy(gameObject, duration);
        if (isContinuous)
        {
            StartCoroutine(ContinuousDamage());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isContinuous && collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }

    private IEnumerator ContinuousDamage()
    {
        while (true)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, GetComponent<BoxCollider2D>().size, 0f);
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    Player player = hit.GetComponent<Player>();
                    if (player != null)
                    {
                        player.TakeDamage(damage);
                    }
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
