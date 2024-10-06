using UnityEngine;
using System.Collections;

public class Shark : MonoBehaviour
{
    private float sharkSpeed;
    private float chaseDelay;
    private bool isChasing = false;
    private Transform targetMonster;

    private float maxSearchTime = 3f; // 최대 탐색 시간
    private float currentSearchTime = 0f;

    public void Initialize(float speed, float delay, float searchTime)
    {
        sharkSpeed = speed;
        chaseDelay = delay;
        maxSearchTime = searchTime;
        StartCoroutine(SharkAction());
    }

    private IEnumerator SharkAction()
    {
        // 직선으로 chaseDelay 동안 이동
        float elapsedTime = 0f;
        while (elapsedTime < chaseDelay)
        {
            transform.Translate(Vector3.right * sharkSpeed * Time.deltaTime);  // 직선 이동
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 추격 시작
        isChasing = true;
        currentSearchTime = 0f;
        StartCoroutine(SearchForMonster());
    }

    private IEnumerator SearchForMonster()
    {
        while (currentSearchTime < maxSearchTime && targetMonster == null)
        {
            FindClosestMonster();

            if (targetMonster != null)
            {
                // 몬스터를 찾았으므로 코루틴 종료
                yield break;
            }

            currentSearchTime += 0.5f; // 검색 주기와 일치
            yield return new WaitForSeconds(0.5f); // 0.5초마다 검색
        }

        // 최대 탐색 시간 초과, 상어 제거
        Destroy(gameObject);
    }

    private void Update()
    {
        if (isChasing && targetMonster != null)
        {
            // 몬스터를 추격
            Vector3 direction = (targetMonster.position - transform.position).normalized;
            transform.position += direction * sharkSpeed * Time.deltaTime;

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
    }

    private void AttackMonster()
    {
        if (targetMonster != null)
        {
            Monster monster = targetMonster.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(50, PlayManager.I.GetPlayerPosition());  // 상어가 몬스터에게 데미지를 줌
            }
        }

        Destroy(gameObject);  // 상어는 몬스터를 공격 후 사라짐
    }
}
