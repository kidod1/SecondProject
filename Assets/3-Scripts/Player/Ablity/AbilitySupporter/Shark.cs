using UnityEngine;
using System.Collections;

public class Shark : MonoBehaviour
{
    private float sharkSpeed;
    private float chaseDelay;
    private bool isChasing = false;
    private Transform targetMonster;

    public void Initialize(float speed, float delay)
    {
        sharkSpeed = speed;
        chaseDelay = delay;
        StartCoroutine(SharkAction());
    }

    private IEnumerator SharkAction()
    {
        // 직선으로 0.5초 동안 이동
        float elapsedTime = 0f;
        while (elapsedTime < chaseDelay)
        {
            transform.Translate(Vector3.right * sharkSpeed * Time.unscaledDeltaTime);  // 직선 이동
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // 0.5초 후에 추격 시작
        isChasing = true;
        FindClosestMonster();  // 가장 가까운 몬스터 찾기
    }

    private void Update()
    {
        if (isChasing && targetMonster != null)
        {
            // 몬스터를 추격
            Vector3 direction = (targetMonster.position - transform.position).normalized;
            transform.position += direction * sharkSpeed * Time.unscaledDeltaTime;

            // 몬스터에 닿으면 공격
            if (Vector3.Distance(transform.position, targetMonster.position) < 0.5f)
            {
                AttackMonster();
            }
        }
    }

    private void FindClosestMonster()
    {
        Monster[] monsters = FindObjectsOfType<Monster>();
        float closestDistance = Mathf.Infinity;

        foreach (Monster monster in monsters)
        {
            float distance = Vector3.Distance(transform.position, monster.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                targetMonster = monster.transform;
            }
        }

        if (targetMonster == null)
        {
            Debug.Log("No monsters found to chase.");
            Destroy(gameObject);  // 몬스터가 없으면 상어를 제거
        }
    }

    private void AttackMonster()
    {
        if (targetMonster != null)
        {
            Monster monster = targetMonster.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(50);  // 상어가 몬스터에게 데미지를 줌
            }
        }

        Destroy(gameObject);  // 상어는 몬스터를 공격 후 사라짐
    }
}
