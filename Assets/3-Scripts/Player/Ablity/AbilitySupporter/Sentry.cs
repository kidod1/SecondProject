using System.Collections;
using UnityEngine;

public class Sentry : MonoBehaviour
{
    public float detectionRange = 5f;        // �� ���� ����
    public LayerMask enemyLayer;             // �� ���̾� ����ũ
    public GameObject projectilePrefab;      // ��Ʈ���� ����ü ������
    public Transform shootPoint;             // ����ü �߻� ��ġ

    private int damage;
    private float attackSpeed;
    private float duration;

    private float nextAttackTime;

    // �߰��� �ʵ�
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

        // ���� �ð� �� ��Ʈ�� ����
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        // ���� ��ٿ� üũ
        if (Time.time >= nextAttackTime)
        {
            // �ֺ��� ���� ����� �� ã��
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange, enemyLayer);
            if (hits.Length > 0)
            {
                Transform nearestEnemy = GetNearestEnemy(hits);

                if (nearestEnemy != null)
                {
                    // ���� ���� �߻�
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

        // ����ü ����
        GameObject projectileObject = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

        // ����ü�� ���� ����
        Vector2 direction = (target.position - shootPoint.position).normalized;

        // Projectile ��ũ��Ʈ �ʱ�ȭ
        Projectile projectileScript = projectileObject.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            // Projectile�� Initialize �޼��忡 �ʿ��� �Ű����� ����
            projectileScript.Initialize(playerData, playerInstance, false, 1.0f, damage);
            projectileScript.SetDirection(direction);

            // Rigidbody2D�� �ִٸ� �ӵ� ����
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
        // ���� ���� �ð�ȭ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
