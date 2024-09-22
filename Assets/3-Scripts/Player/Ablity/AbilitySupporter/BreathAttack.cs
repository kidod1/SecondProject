using UnityEngine;
using System.Collections;

public class BreathAttack : MonoBehaviour
{
    private float damage;
    private float range;
    private float angle;
    private float duration;
    private Player player;

    private Vector2 direction; // 브레스의 방향 벡터

    public void Initialize(float damage, float range, float angle, float duration, Player player)
    {
        this.damage = damage;
        this.range = range;
        this.angle = angle;
        this.duration = duration;
        this.player = player;

        // 브레스의 지속 시간이 지나면 자동 파괴
        Destroy(gameObject, duration);

        // 피해 코루틴 시작
        StartCoroutine(DealDamageOverTime());
    }

    public void SetDirection(Vector2 direction)
    {
        this.direction = direction.normalized;

        // 브레스의 회전을 설정합니다.
        float angleInDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angleInDegrees);
    }

    private IEnumerator DealDamageOverTime()
    {
        float elapsedTime = 0f;
        float damageInterval = 0.5f; // 0.5초마다 피해 적용

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

        // 브레스 범위 내의 모든 적 탐색
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
                    // 디버그 메시지 추가
                    Debug.Log($"[Breath Attack] {monster.gameObject.name}에게 {damage} 피해를 입힘.");
                }
            }
        }
    }


    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (direction != Vector2.zero)
        {
            UnityEditor.Handles.color = new Color(1, 0, 0, 0.2f); // 반투명한 빨간색

            Vector3 origin = transform.position;
            float halfAngle = angle / 2f;

            // 브레스의 부채꼴 영역을 그립니다.
            UnityEditor.Handles.DrawSolidArc(
                origin,
                Vector3.forward, // 2D에서는 Z축이 전면
                Quaternion.Euler(0, 0, -halfAngle) * direction,
                angle,
                range
            );
        }
#endif
    }
}
