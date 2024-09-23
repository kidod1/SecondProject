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
        // �������� 0.5�� ���� �̵�
        float elapsedTime = 0f;
        while (elapsedTime < chaseDelay)
        {
            transform.Translate(Vector3.right * sharkSpeed * Time.unscaledDeltaTime);  // ���� �̵�
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // 0.5�� �Ŀ� �߰� ����
        isChasing = true;
        FindClosestMonster();  // ���� ����� ���� ã��
    }

    private void Update()
    {
        if (isChasing && targetMonster != null)
        {
            // ���͸� �߰�
            Vector3 direction = (targetMonster.position - transform.position).normalized;
            transform.position += direction * sharkSpeed * Time.unscaledDeltaTime;

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

        if (targetMonster == null)
        {
            Debug.Log("No monsters found to chase.");
            Destroy(gameObject);  // ���Ͱ� ������ �� ����
        }
    }

    private void AttackMonster()
    {
        if (targetMonster != null)
        {
            Monster monster = targetMonster.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(50);  // �� ���Ϳ��� �������� ��
            }
        }

        Destroy(gameObject);  // ���� ���͸� ���� �� �����
    }
}
