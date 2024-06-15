using UnityEngine;

public class MonsterPatrol : Monster
{
    public Transform[] waypoints; // Waypoints 배열
    public float patrolSpeed = 2.0f; // 패트롤 이동 속도
    private int currentWaypointIndex = 0; // 현재 목표 Waypoint 인덱스

    private void Update()
    {
        Patrol();
    }

    private void Patrol()
    {
        if (waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = targetWaypoint.position - transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, patrolSpeed * Time.deltaTime);

        // 목표 Waypoint에 도달했는지 확인
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        // 몬스터의 방향을 목표 Waypoint로 회전
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(stat.contectDamage, knockbackDirection);
            }
        }
    }
}