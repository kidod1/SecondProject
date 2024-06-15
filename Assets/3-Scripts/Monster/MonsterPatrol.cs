using UnityEngine;

public class MonsterPatrol : Monster
{
    public Transform[] waypoints; // Waypoints �迭
    public float patrolSpeed = 2.0f; // ��Ʈ�� �̵� �ӵ�
    private int currentWaypointIndex = 0; // ���� ��ǥ Waypoint �ε���

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