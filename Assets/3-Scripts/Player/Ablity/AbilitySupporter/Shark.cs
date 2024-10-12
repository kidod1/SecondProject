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
    /// 상어를 초기화합니다.
    /// </summary>
    /// <param name="sharkSpeed">상어의 속도</param>
    /// <param name="delay">추격 시작 전 대기 시간</param>
    /// <param name="maxTime">적을 찾는 최대 시간</param>
    /// <param name="sharkDamage">상어의 데미지</param>
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
                    yield break; // 적을 찾았으므로 코루틴 종료
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 최대 시간 내에 적을 찾지 못했으므로 상어 파괴
        Destroy(gameObject);
    }

    private Collider2D FindClosestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 15f); // 탐지 범위 15으로 설정 (필요에 따라 조정)
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

        // 적에게 도착했을 때 데미지 적용
        if (target != null && !target.IsDead)
        {
            target.TakeDamage(damage, transform.position);
        }

        // 데미지 적용 후 상어 파괴
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // 탐지 범위 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 15f);
    }
}
