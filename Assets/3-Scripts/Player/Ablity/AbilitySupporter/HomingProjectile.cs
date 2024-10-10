using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    private PlayerData stat;
    private float startDelay;
    private float speed;
    private float range;
    private Vector2 direction;

    private Vector3 initialPosition;

    /// <summary>
    /// 유도 탄환을 초기화합니다.
    /// </summary>
    /// <param name="playerStat">플레이어의 데이터</param>
    /// <param name="delay">유도 시작 지연 시간 (초)</param>
    /// <param name="homingSpeed">유도 속도</param>
    /// <param name="homingRange">유도 범위</param>
    public void Initialize(PlayerData playerStat, float delay, float homingSpeed, float homingRange)
    {
        stat = playerStat;
        startDelay = delay;
        speed = homingSpeed;
        range = homingRange;
        initialPosition = transform.position;

        // 유도 시작 지연 후 유도 로직 시작
        Invoke(nameof(StartHoming), startDelay);
    }

    /// <summary>
    /// 유도 로직을 시작합니다.
    /// </summary>
    private void StartHoming()
    {
        // 목표를 설정 (예: 가장 가까운 적)
        GameObject target = FindNearestEnemy();
        if (target != null)
        {
            direction = (target.transform.position - transform.position).normalized;
        }
    }

    private void Update()
    {
        // 이동 로직
        if (direction != Vector2.zero)
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }

        // 범위 초과 시 파괴
        if (Vector3.Distance(initialPosition, transform.position) > range)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 가장 가까운 적을 찾습니다.
    /// </summary>
    /// <returns>가장 가까운 적의 게임 오브젝트</returns>
    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(enemy.transform.position, currentPos);
            if (dist < minDist)
            {
                nearest = enemy;
                minDist = dist;
            }
        }

        return nearest;
    }

    /// <summary>
    /// 발사 방향을 설정합니다.
    /// </summary>
    /// <param name="dir">발사 방향</param>
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }
}
