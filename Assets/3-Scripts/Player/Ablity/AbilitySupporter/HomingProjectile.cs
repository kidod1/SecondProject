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
    /// ���� źȯ�� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="playerStat">�÷��̾��� ������</param>
    /// <param name="delay">���� ���� ���� �ð� (��)</param>
    /// <param name="homingSpeed">���� �ӵ�</param>
    /// <param name="homingRange">���� ����</param>
    public void Initialize(PlayerData playerStat, float delay, float homingSpeed, float homingRange)
    {
        stat = playerStat;
        startDelay = delay;
        speed = homingSpeed;
        range = homingRange;
        initialPosition = transform.position;

        // ���� ���� ���� �� ���� ���� ����
        Invoke(nameof(StartHoming), startDelay);
    }

    /// <summary>
    /// ���� ������ �����մϴ�.
    /// </summary>
    private void StartHoming()
    {
        // ��ǥ�� ���� (��: ���� ����� ��)
        GameObject target = FindNearestEnemy();
        if (target != null)
        {
            direction = (target.transform.position - transform.position).normalized;
        }
    }

    private void Update()
    {
        // �̵� ����
        if (direction != Vector2.zero)
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }

        // ���� �ʰ� �� �ı�
        if (Vector3.Distance(initialPosition, transform.position) > range)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ���� ����� ���� ã���ϴ�.
    /// </summary>
    /// <returns>���� ����� ���� ���� ������Ʈ</returns>
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
    /// �߻� ������ �����մϴ�.
    /// </summary>
    /// <param name="dir">�߻� ����</param>
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }
}
