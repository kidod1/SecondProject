using UnityEngine;

public class BladeProjectile : MonoBehaviour
{
    private int damage;
    private float range;
    private float speed;
    private Vector3 startPosition;
    private Vector2 direction;  // 방향 정보를 저장 (Vector2로 변경)
    private Player owner;

    public void Initialize(int damage, float range, float speed, Player owner, Vector2 direction)
    {
        this.damage = damage;
        this.range = range;
        this.speed = speed;
        this.owner = owner;
        this.direction = direction.normalized;  // 방향 설정
        startPosition = transform.position;
    }

    private void Update()
    {
        // 설정된 방향으로 이동 (Z축은 0으로 고정)
        Vector3 movement = new Vector3(direction.x, direction.y, 0f) * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // 사거리 체크
        if (Vector3.Distance(startPosition, transform.position) >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 적 태그를 가진 오브젝트에 충돌했는지 확인
        if (other.CompareTag("Monster"))
        {
            Monster enemy = other.GetComponent<Monster>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, owner.transform.position);
            }

            // 칼날은 몬스터를 관통하므로 파괴하지 않음
            // Destroy(gameObject); // 관통하지 않으려면 이 줄을 활성화
        }
    }
}
