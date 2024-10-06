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
        // �θ��� �÷��̾��� ���� ��ǥ�� �������� 0,0���� ����
        transform.localPosition = Vector3.zero;
    }

    public void Initialize(float damage, float radius, float interval, Player player)
    {
        damagePerTick = damage;
        barrierRadius = radius;
        damageInterval = interval;
        playerInstance = player;

        // Collider�� �ð��� ũ�� ������ ���⼭ �ʿ信 ���� �߰�
        transform.localScale = new Vector3(barrierRadius * 2, barrierRadius * 2, 1);

        // ���� �ڷ�ƾ ����
        StartCoroutine(DamageRoutine());
    }

    public void UpdateParameters(float damage, float radius, float interval)
    {
        damagePerTick = damage;
        barrierRadius = radius;
        damageInterval = interval;

        // Collider �ݰ� ������Ʈ
        transform.localScale = new Vector3(barrierRadius * 2, barrierRadius * 2, 1);
    }

    private IEnumerator DamageRoutine()
    {
        while (true)
        {
            // �ӽ� ����Ʈ�� monstersInRange ����
            List<Monster> tempMonstersInRange = new List<Monster>(monstersInRange);

            // for ���� ����Ͽ� ������ ���� �� ����
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
        // �÷��̾ �������� �帷 ���� �׸���
        if (playerInstance != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerInstance.transform.position, barrierRadius);
        }
    }
}
