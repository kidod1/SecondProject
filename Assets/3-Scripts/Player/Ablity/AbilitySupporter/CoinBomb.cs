using UnityEngine;
using AK.Wwise; // WWISE ���ӽ����̽� �߰�

public class CoinBomb : MonoBehaviour
{
    private int damage;
    private float bombDuration;
    private GameObject explosionEffectPrefab;

    [Header("WWISE Events")]
    [Tooltip("���� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event explosionSound;

    /// <summary>
    /// ���� ��ź�� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="damage">��ź�� ���ط�</param>
    /// <param name="bombDuration">��ź�� ���� �ð�</param>
    /// <param name="explosionEffect">���� �� ������ ����Ʈ ������</param>
    public void Initialize(int damage, float bombDuration, GameObject explosionEffect)
    {
        this.damage = damage;
        this.bombDuration = bombDuration;
        this.explosionEffectPrefab = explosionEffect;

        // ��ź�� ���� �ð� �� �ڵ� ����
        Invoke("Explode", bombDuration);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ���Ϳ� �浹 �� ����
        if (collision.CompareTag("Monster")) // ���� �±� Ȯ��
        {
            Explode();
        }
    }

    /// <summary>
    /// ��ź�� �����ϴ� ������ ó���մϴ�.
    /// </summary>
    private void Explode()
    {
        // ���� ����Ʈ ����
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("CoinBomb: explosionEffectPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // WWISE ���� ���
        if (explosionSound != null)
        {
            explosionSound.Post(gameObject);
        }
        else
        {
            Debug.LogWarning("CoinBomb: explosionSound�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // �ֺ� ���͵鿡�� ���ظ� ������ ���� (����)
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2f); // ���� �ݰ� 2f

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Monster"))
            {
                // ������ TakeDamage �޼��� ȣ�� (Assuming Monster Ŭ������ ����)
                Monster monster = hitCollider.GetComponent<Monster>();
                if (monster != null)
                {
                    monster.TakeDamage(damage, this.transform.position);
                }
            }
        }

        // ���� �� ��ź ������Ʈ �ı�
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // ���� �ݰ� �ð�ȭ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2f); // ���� �ݰ� 2f
    }
}
