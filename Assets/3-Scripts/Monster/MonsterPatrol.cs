using UnityEngine;

public class MonsterPatrol : MonoBehaviour
{
    public Transform[] waypoints; // Waypoints �迭
    public float speed = 2.0f; // �̵� �ӵ�
    private int currentWaypointIndex = 0; // ���� ��ǥ Waypoint �ε���

    void Update()
    {
        if (waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = targetWaypoint.position - transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

        // ��ǥ Waypoint�� �����ߴ��� Ȯ��
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        // ������ ������ ��ǥ Waypoint�� ȸ��
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        }
    }
}
