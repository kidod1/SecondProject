using System.Collections;
using UnityEngine;

public class Sentry : MonoBehaviour
{
    public float detectionRange = 5f;        // 적 감지 범위
    public LayerMask enemyLayer;             // 적 레이어 마스크
    public GameObject projectilePrefab;      // 센트리의 투사체 프리팹
    public Transform shootPoint;             // 투사체 발사 위치

    private int damage;
    private float attackSpeed;
    private float duration;

    private float nextAttackTime;

    // 추가된 필드
    private PlayerData playerData;
    private Player playerInstance;

    public AK.Wwise.Event attackSound;

    public void Initialize(int damage, float attackSpeed, float duration, PlayerData playerData, Player playerInstance)
    {
        this.damage = damage;
        this.attackSpeed = attackSpeed;
        this.duration = duration;
        this.playerData = playerData;
        this.playerInstance = playerInstance;

        // 일정 시간 후 센트리 제거
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        // 공격 쿨다운 체크
        if (Time.time >= nextAttackTime)
        {
            // 주변의 가장 가까운 적 찾기
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange, enemyLayer);
            if (hits.Length > 0)
            {
                Transform nearestEnemy = GetNearestEnemy(hits);

                if (nearestEnemy != null)
                {
                    // 적을 향해 발사
                    attackSound.Post(PlayManager.I.GetPlayer().gameObject);
                    Shoot(nearestEnemy);
                    nextAttackTime = Time.time + attackSpeed;
                }
            }
        }
    }

    private Transform GetNearestEnemy(Collider2D[] enemies)
    {
        Transform nearestEnemy = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D enemyCollider in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemyCollider.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestEnemy = enemyCollider.transform;
            }
        }

        return nearestEnemy;
    }

    private void Shoot(Transform target)
    {
        if (projectilePrefab == null || shootPoint == null)
        {
            Debug.LogError("Projectile prefab or shoot point is not assigned.");
            return;
        }

        // 투사체 생성
        GameObject projectileObject = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

        // 투사체의 방향 설정
        Vector2 direction = (target.position - shootPoint.position).normalized;

        // Projectile 스크립트 초기화
        Projectile projectileScript = projectileObject.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            // Projectile의 Initialize 메서드에 필요한 매개변수 전달
            projectileScript.Initialize(playerData, playerInstance, false, 1.0f, damage);
            projectileScript.SetDirection(direction);

            // Rigidbody2D가 있다면 속도 설정
            Rigidbody2D rb = projectileObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * playerData.currentProjectileSpeed;
            }
        }
        else
        {
            Debug.LogError("Projectile prefab is missing Projectile component.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 감지 범위 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
