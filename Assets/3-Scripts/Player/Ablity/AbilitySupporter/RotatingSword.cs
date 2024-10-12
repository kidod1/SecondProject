using UnityEngine;

public class RotatingSword : MonoBehaviour
{
    private Transform playerTransform;
    private float rotationSpeed;
    private int damageAmount;
    private float currentAngle;

    private float radius = 1.5f; // 플레이어로부터의 거리

    private void Update()
    {
        if (playerTransform == null)
        {
            Destroy(gameObject); // 플레이어가 없으면 오브젝트 제거
            return;
        }

        // 각도 업데이트
        currentAngle += rotationSpeed * Time.deltaTime;

        // 위치 업데이트
        float angleRad = currentAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
        transform.position = playerTransform.position + (Vector3)offset;
    }

    public void Initialize(Transform player, float speed, int damage)
    {
        playerTransform = player;
        rotationSpeed = speed;
        damageAmount = damage;

        // 현재 각도를 초기 위치에서 계산
        Vector2 direction = (transform.position - playerTransform.position).normalized;
        currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 적에게 데미지 적용
        Monster monster = collision.GetComponent<Monster>();
        if (monster != null)
        {
            // 몬스터에게 데미지 적용
            monster.TakeDamage(damageAmount, playerTransform.position);
        }
    }
}
