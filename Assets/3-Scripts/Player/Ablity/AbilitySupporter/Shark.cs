using UnityEngine;
using System.Collections;

public class Shark : MonoBehaviour
{
    private float speed;
    private float chaseDelay;
    private float maxSearchTime;
    private int damage;
    private bool isChasing = false;

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
        while (target != null && !target.IsDead && Vector3.Distance(transform.position, target.transform.position) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
            yield return null;
        }

        // ������ �������� �� ������ ����
        if (target != null && !target.IsDead)
        {
            target.TakeDamage(damage, transform.position);
        }

        // ������ ���� �� ��� �ı�
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Ž�� ���� �ð�ȭ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 15f);
    }
}
