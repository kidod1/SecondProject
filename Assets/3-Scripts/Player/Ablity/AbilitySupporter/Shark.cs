using UnityEngine;
using System.Collections;

public class Shark : MonoBehaviour
{
    private float sharkSpeed;
    private float chaseDelay;
    private bool isChasing = false;
    private Transform targetMonster;

    private float maxSearchTime = 3f; // �ִ� Ž�� �ð�
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
        // �������� chaseDelay ���� �̵�
        float elapsedTime = 0f;
        while (elapsedTime < chaseDelay)
        {
            transform.Translate(Vector3.right * sharkSpeed * Time.deltaTime);  // ���� �̵�
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �߰� ����
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
                // ���͸� ã�����Ƿ� �ڷ�ƾ ����
                yield break;
            }

            currentSearchTime += 0.5f; // �˻� �ֱ�� ��ġ
            yield return new WaitForSeconds(0.5f); // 0.5�ʸ��� �˻�
        }

        // �ִ� Ž�� �ð� �ʰ�, ��� ����
        Destroy(gameObject);
    }

    private void Update()
    {
        if (isChasing && targetMonster != null)
        {
            // ���͸� �߰�
            Vector3 direction = (targetMonster.position - transform.position).normalized;
            transform.position += direction * sharkSpeed * Time.deltaTime;

            // ���Ϳ� ������ ����
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
                monster.TakeDamage(50, PlayManager.I.GetPlayerPosition());  // �� ���Ϳ��� �������� ��
            }
        }

        Destroy(gameObject);  // ���� ���͸� ���� �� �����
    }
}
