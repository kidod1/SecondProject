using UnityEngine;
using System.Collections;

public class Shark : MonoBehaviour
{
    private float speed;
    private float chaseDelay;
    private float maxSearchTime;
    private int damage;
    private bool isChasing = false;

    private SpriteRenderer spriteRenderer; // SpriteRenderer 참조 추가
    private bool hasSetRotation = false; // 회전 설정 여부를 추적하는 변수
    [Header("WWISE Sound Events")]
    [Tooltip("SharkStrike 능력 발동 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event activateSound; // 추가된 사운드 이벤트 필드
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

        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer 초기화

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
        Vector3 targetPosition = target.transform.position;

        // 이동 시작 시 방향 설정
        if (!hasSetRotation && target != null)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            SetInitialRotation(direction);
            hasSetRotation = true;
        }

        while (target != null && !target.IsDead && Vector3.Distance(transform.position, target.transform.position) > 0.5f)
        {
            // 이동 방향 고정
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
            yield return null;
        }

        // 적에게 도착했을 때 데미지 적용
        if (target != null && !target.IsDead)
        {
            activateSound?.Post(target.gameObject);
            target.TakeDamage(damage, transform.position);
        }

        // 데미지 적용 후 상어 파괴
        Destroy(gameObject);
    }

    /// <summary>
    /// 초기 이동 방향에 따라 스프라이트의 Z축 회전을 설정합니다.
    /// </summary>
    /// <param name="direction">이동 방향 벡터</param>
    private void SetInitialRotation(Vector3 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle - 90);
        transform.rotation = targetRotation;
    }

    private void OnDrawGizmosSelected()
    {
        // 탐지 범위 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 15f);
    }
}
