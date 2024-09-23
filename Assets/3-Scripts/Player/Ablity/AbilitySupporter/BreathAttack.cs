using UnityEngine;
using System.Collections;

public class BreathAttack : MonoBehaviour
{
    private float damage;
    private float range;
    private float angle;
    private float duration;
    private Player player;

    private Vector2 direction; // �극���� ���� ����

    public void Initialize(float damage, float range, float angle, float duration, Player player)
    {
        this.damage = damage;
        this.range = range;
        this.angle = angle;
        this.duration = duration;
        this.player = player;

        // �극���� ���� �ð��� ������ �ڵ� �ı�
        Destroy(gameObject, duration);

        // ���� �ڷ�ƾ ����
        StartCoroutine(DealDamageOverTime());
    }

    public void SetDirection(Vector2 direction)
    {
        this.direction = direction.normalized;

        // �극���� ȸ���� �����մϴ�.
        float angleInDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angleInDegrees);
    }

    private IEnumerator DealDamageOverTime()
    {
        float elapsedTime = 0f;
        float damageInterval = 0.5f; // 0.5�ʸ��� ���� ����

        while (elapsedTime < duration)
        {
            ApplyDamage();
            yield return new WaitForSecondsRealtime(damageInterval);
            elapsedTime += damageInterval;
        }
    }

    private void ApplyDamage()
    {
        Vector2 origin = (Vector2)transform.position;
        Vector2 direction = this.direction;

        // �극�� ���� ���� ��� �� Ž��
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range);
        foreach (Collider2D hit in hits)
        {
            Monster monster = hit.GetComponent<Monster>();
            if (monster != null && !monster.IsDead)
            {
                Vector2 monsterPosition = (Vector2)monster.transform.position;
                Vector2 toTarget = (monsterPosition - origin).normalized;
                float angleToTarget = Vector2.Angle(direction, toTarget);

                if (angleToTarget <= angle / 2f)
                {
                    monster.TakeDamage(Mathf.RoundToInt(damage));
                    // ����� �޽��� �߰�
                    Debug.Log($"[Breath Attack] {monster.gameObject.name}���� {damage} ���ظ� ����.");
                }
            }
        }
    }


    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (direction != Vector2.zero)
        {
            UnityEditor.Handles.color = new Color(1, 0, 0, 0.2f); // �������� ������

            Vector3 origin = transform.position;
            float halfAngle = angle / 2f;

            // �극���� ��ä�� ������ �׸��ϴ�.
            UnityEditor.Handles.DrawSolidArc(
                origin,
                Vector3.forward, // 2D������ Z���� ����
                Quaternion.Euler(0, 0, -halfAngle) * direction,
                angle,
                range
            );
        }
#endif
    }
}
