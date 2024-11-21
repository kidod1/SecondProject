using UnityEngine;
using System.Collections;

public class Shark : MonoBehaviour
{
    private float speed;
    private float chaseDelay;
    private float maxSearchTime;
    private int damage;
    private bool isChasing = false;

    private SpriteRenderer spriteRenderer; // SpriteRenderer ���� �߰�
    private bool hasSetRotation = false; // ȸ�� ���� ���θ� �����ϴ� ����
    [Header("WWISE Sound Events")]
    [Tooltip("SharkStrike �ɷ� �ߵ� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event activateSound; // �߰��� ���� �̺�Ʈ �ʵ�
    /// <summary>
    /// �� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="sharkSpeed">����� �ӵ�</param>
    /// <param name="delay">�߰� ���� �� ��� �ð�</param>
    /// <param name="maxTime">���� ã�� �ִ� �ð�</param>
    /// <param name="sharkDamage">����� ������</param>
    public void Initialize(float sharkSpeed, float delay, float maxTime, int sharkDamage)
    {
        speed = sharkSpeed;
        chaseDelay = delay;
        maxSearchTime = maxTime;
        damage = sharkDamage;

        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer �ʱ�ȭ

        StartCoroutine(StartChasing());
    }

    private IEnumerator StartChasing()
    {
        yield return new WaitForSeconds(chaseDelay);
        isChasing = true;
        StartCoroutine(FindAndChaseEnemy());
    }

    private IEnumerator FindAndChaseEnemy()
    {
        float elapsedTime = 0f;

        while (elapsedTime < maxSearchTime)
        {
            Collider2D closestEnemy = FindClosestEnemy();
            if (closestEnemy != null && closestEnemy.gameObject != null)
            {
                Monster targetMonster = closestEnemy.GetComponent<Monster>();
                if (targetMonster != null && !targetMonster.IsDead)
                {
                    ChaseEnemy(targetMonster);
                    yield break; // ���� ã�����Ƿ� �ڷ�ƾ ����
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �ִ� �ð� ���� ���� ã�� �������Ƿ� ��� �ı�
        Destroy(gameObject);
    }

    private Collider2D FindClosestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 15f); // Ž�� ���� 15���� ���� (�ʿ信 ���� ����)
        Collider2D closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            if (enemy.GetComponent<Monster>())
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        return closestEnemy;
    }

    private void ChaseEnemy(Monster target)
    {
        StartCoroutine(MoveTowards(target));
    }

    private IEnumerator MoveTowards(Monster target)
    {
        Vector3 targetPosition = target.transform.position;

        // �̵� ���� �� ���� ����
        if (!hasSetRotation && target != null)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            SetInitialRotation(direction);
            hasSetRotation = true;
        }

        while (target != null && !target.IsDead && Vector3.Distance(transform.position, target.transform.position) > 0.5f)
        {
            // �̵� ���� ����
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
            yield return null;
        }

        // ������ �������� �� ������ ����
        if (target != null && !target.IsDead)
        {
            activateSound?.Post(target.gameObject);
            target.TakeDamage(damage, transform.position);
        }

        // ������ ���� �� ��� �ı�
        Destroy(gameObject);
    }

    /// <summary>
    /// �ʱ� �̵� ���⿡ ���� ��������Ʈ�� Z�� ȸ���� �����մϴ�.
    /// </summary>
    /// <param name="direction">�̵� ���� ����</param>
    private void SetInitialRotation(Vector3 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle - 90);
        transform.rotation = targetRotation;
    }

    private void OnDrawGizmosSelected()
    {
        // Ž�� ���� �ð�ȭ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 15f);
    }
}
