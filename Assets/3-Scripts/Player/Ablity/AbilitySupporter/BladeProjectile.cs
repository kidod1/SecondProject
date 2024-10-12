using UnityEngine;

public class BladeProjectile : MonoBehaviour
{
    private int damage;
    private float range;
    private float speed;
    private Vector3 startPosition;
    private Vector2 direction;  // ���� ������ ���� (Vector2�� ����)
    private Player owner;

    public void Initialize(int damage, float range, float speed, Player owner, Vector2 direction)
    {
        this.damage = damage;
        this.range = range;
        this.speed = speed;
        this.owner = owner;
        this.direction = direction.normalized;  // ���� ����
        startPosition = transform.position;
    }

    private void Update()
    {
        // ������ �������� �̵� (Z���� 0���� ����)
        Vector3 movement = new Vector3(direction.x, direction.y, 0f) * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // ��Ÿ� üũ
        if (Vector3.Distance(startPosition, transform.position) >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �� �±׸� ���� ������Ʈ�� �浹�ߴ��� Ȯ��
        if (other.CompareTag("Monster"))
        {
            Monster enemy = other.GetComponent<Monster>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, owner.transform.position);
            }

            // Į���� ���͸� �����ϹǷ� �ı����� ����
            // Destroy(gameObject); // �������� �������� �� ���� Ȱ��ȭ
        }
    }
}
