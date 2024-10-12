using UnityEngine;
using System.Collections;

public class CoinBomb : MonoBehaviour
{
    private int damage;
    private float duration;

    private bool isExploded = false;

    /// <summary>
    /// ���� ��ź�� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="damage">��ź�� ���ط�</param>
    /// <param name="duration">��ź�� ���� �ð�</param>
    public void Initialize(int damage, float duration)
    {
        this.damage = damage;
        this.duration = duration;

        StartCoroutine(BombDuration());
    }

    /// <summary>
    /// ��ź�� ���� �ð��� ������ ��ź�� �����մϴ�.
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

                // ���� ȿ�� �߰� ����
                // ��: Instantiate(explosionEffect, transform.position, Quaternion.identity);

                isExploded = true;

                Destroy(gameObject);
            }
        }
    }
}
