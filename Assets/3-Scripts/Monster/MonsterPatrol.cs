using UnityEngine;

public class MonsterPatrol : MonoBehaviour
{
    public Transform[] waypoints; // Waypoints 배열
    public float speed = 2.0f; // 이동 속도
    private int currentWaypointIndex = 0; // 현재 목표 Waypoint 인덱스

    void Update()
    {
        if (waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = targetWaypoint.position - transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

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
}
