using System.Collections;
using UnityEngine;

public class ElectricBeam : MonoBehaviour
{
    public int damage = 20;
    public float rotationSpeed = 30f; // 도는 속도
    public float range = 600f; // 사거리

    private Transform playerTransform;

    public void Initialize(Transform player)
    {
        playerTransform = player;
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            // 플레이어를 중심으로 회전
            transform.RotateAround(playerTransform.position, Vector3.forward, rotationSpeed * Time.deltaTime);

            // 범위 내 몬스터에게 데미지 주기
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

    // 기즈모를 사용하여 범위를 시각적으로 표시
    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.blue; // 기즈모 색상을 설정
            Gizmos.DrawWireSphere(playerTransform.position, range / 100f); // 범위를 원으로 그리기
        }
    }
}
