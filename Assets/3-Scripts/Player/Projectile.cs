using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    [SerializeField]
    private PlayerData stat;
    public int damage = 10; // 발사체가 줄 데미지

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
        // 충돌한 객체의 태그가 Wall인 경우
        if (collision.CompareTag("Wall"))
        {
            gameObject.SetActive(false); // 발사체 비활성화
        }
        // 충돌한 객체가 Monster인 경우
        else if (collision.GetComponent<Monster>() != null)
        {
            // Monster의 TakeDamage 메서드 호출
            Monster monster = collision.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(damage); // 데미지 주기
            }
            gameObject.SetActive(false); // 발사체 비활성화
        }
    }
}