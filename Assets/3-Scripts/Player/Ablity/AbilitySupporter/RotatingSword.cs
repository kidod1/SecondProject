using UnityEngine;

public class RotatingSword : MonoBehaviour
{
    private Transform playerTransform;
    private float rotationSpeed;
    private int damageAmount;
    private float currentAngle;

    private float radius = 1.5f; // �÷��̾�κ����� �Ÿ�

    private void Update()
    {
        if (playerTransform == null)
        {
            Destroy(gameObject); // �÷��̾ ������ ������Ʈ ����
            return;
        }

        // ���� ������Ʈ
        currentAngle += rotationSpeed * Time.deltaTime;

        // ��ġ ������Ʈ
        float angleRad = currentAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
        transform.position = playerTransform.position + (Vector3)offset;
    }

    public void Initialize(Transform player, float speed, int damage)
    {
        playerTransform = player;
        rotationSpeed = speed;
        damageAmount = damage;

        // ���� ������ �ʱ� ��ġ���� ���
        Vector2 direction = (transform.position - playerTransform.position).normalized;
        currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ������ ������ ����
        Monster monster = collision.GetComponent<Monster>();
        if (monster != null)
        {
            // ���Ϳ��� ������ ����
            monster.TakeDamage(damageAmount, playerTransform.position);
        }
    }
}
