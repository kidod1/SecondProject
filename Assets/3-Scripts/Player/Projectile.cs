using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    [SerializeField]
    private PlayerData stat;
    public int damage = 10; // �߻�ü�� �� ������

    private void OnEnable()
    {
        Invoke(nameof(Deactivate), stat.projectileRange);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Update()
    {
        transform.Translate(direction * stat.projectileSpeed * Time.deltaTime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �浹�� ��ü�� �±װ� Wall�� ���
        if (collision.CompareTag("Wall"))
        {
            gameObject.SetActive(false); // �߻�ü ��Ȱ��ȭ
        }
        // �浹�� ��ü�� Monster�� ���
        else if (collision.GetComponent<Monster>() != null)
        {
            // Monster�� TakeDamage �޼��� ȣ��
            Monster monster = collision.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(damage); // ������ �ֱ�
            }
            gameObject.SetActive(false); // �߻�ü ��Ȱ��ȭ
        }
    }
}