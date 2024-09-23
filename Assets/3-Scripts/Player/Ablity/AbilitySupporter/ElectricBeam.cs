using System.Collections;
using UnityEngine;

public class ElectricBeam : MonoBehaviour
{
    public int damage = 20;
    public float rotationSpeed = 30f; // ���� �ӵ�
    public float range = 600f; // ��Ÿ�

    private Transform playerTransform;

    public void Initialize(Transform player)
    {
        playerTransform = player;
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            // �÷��̾ �߽����� ȸ��
            transform.RotateAround(playerTransform.position, Vector3.forward, rotationSpeed * Time.deltaTime);

            // ���� �� ���Ϳ��� ������ �ֱ�
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(playerTransform.position, range / 100f);
            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Monster"))
                {
                    Monster monster = hitCollider.GetComponent<Monster>();
                    if (monster != null)
                    {
                        monster.TakeDamage(damage);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            Monster monster = collision.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(damage);
            }
        }
    }

    // ����� ����Ͽ� ������ �ð������� ǥ��
    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.blue; // ����� ������ ����
            Gizmos.DrawWireSphere(playerTransform.position, range / 100f); // ������ ������ �׸���
        }
    }
}
