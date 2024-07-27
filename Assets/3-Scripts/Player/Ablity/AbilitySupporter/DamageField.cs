using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageField : MonoBehaviour
{
    private int damageAmount;
    private float damageInterval;
    private List<Collider2D> monstersInRange = new List<Collider2D>();

    public void Initialize(int damage, float interval)
    {
        damageAmount = damage;
        damageInterval = interval;
        StartCoroutine(DamageOverTime());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            monstersInRange.Add(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            monstersInRange.Remove(other);
        }
    }

    private IEnumerator DamageOverTime()
    {
        while (true)
        {
            foreach (var monster in monstersInRange)
            {
                // 몬스터에게 데미지 주기
                // monster.GetComponent<Monster>().TakeDamage(damageAmount);
            }
            yield return new WaitForSeconds(damageInterval);
        }
    }
}
