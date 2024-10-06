using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlameBarrierEffect : MonoBehaviour
{
    private float damagePerTick;
    private float barrierRadius;
    private float damageInterval;
    private Player playerInstance;

    private List<Monster> monstersInRange = new List<Monster>();

    private void Update()
    {
        // 부모인 플레이어의 로컬 좌표를 기준으로 0,0으로 고정
        transform.localPosition = Vector3.zero;
    }

    public void Initialize(float damage, float radius, float interval, Player player)
    {
        damagePerTick = damage;
        barrierRadius = radius;
        damageInterval = interval;
        playerInstance = player;

        // Collider나 시각적 크기 조절은 여기서 필요에 따라 추가
        transform.localScale = new Vector3(barrierRadius * 2, barrierRadius * 2, 1);

        // 피해 코루틴 시작
        StartCoroutine(DamageRoutine());
    }

    public void UpdateParameters(float damage, float radius, float interval)
    {
        damagePerTick = damage;
        barrierRadius = radius;
        damageInterval = interval;

        // Collider 반경 업데이트
        transform.localScale = new Vector3(barrierRadius * 2, barrierRadius * 2, 1);
    }

    private IEnumerator DamageRoutine()
    {
        while (true)
        {
            // 임시 리스트에 monstersInRange 복사
            List<Monster> tempMonstersInRange = new List<Monster>(monstersInRange);

            // for 루프 사용하여 데미지 적용 및 제거
            for (int i = tempMonstersInRange.Count - 1; i >= 0; i--)
            {
                Monster monster = tempMonstersInRange[i];
                if (monster != null && !monster.IsDead && Vector3.Distance(monster.transform.position, playerInstance.transform.position) <= barrierRadius)
                {
                    monster.TakeDamage(Mathf.RoundToInt(damagePerTick), PlayManager.I.GetPlayerPosition());
                }
                else
                {
                    monstersInRange.Remove(monster);
                }
            }

            yield return new WaitForSecondsRealtime(damageInterval);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Monster monster = collision.GetComponent<Monster>();
        if (monster != null && !monstersInRange.Contains(monster))
        {
            monstersInRange.Add(monster);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Monster monster = collision.GetComponent<Monster>();
        if (monster != null && monstersInRange.Contains(monster))
        {
            monstersInRange.Remove(monster);
        }
    }

    private void OnDrawGizmos()
    {
        // 플레이어를 기준으로 장막 범위 그리기
        if (playerInstance != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerInstance.transform.position, barrierRadius);
        }
    }
}
