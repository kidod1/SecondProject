using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageField : MonoBehaviour
{
    private int damageAmount;
    private float damageInterval;
    private HashSet<Monster> monstersInRange = new HashSet<Monster>();

    public void Initialize(int damage, float interval)
    {
        damageAmount = damage;
        damageInterval = interval;
    }

    private void OnEnable()
    {
        StartCoroutine(DealDamage());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            Monster monster = other.GetComponent<Monster>();
            if (monster != null)
            {
                monstersInRange.Add(monster);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            Monster monster = other.GetComponent<Monster>();
            if (monster != null)
            {
                monstersInRange.Remove(monster);
            }
        }
    }

    private IEnumerator DealDamage()
    {
        while (true)
        {
            // HashSet을 안전하게 순회하기 위해 복사본 사용
            var monstersSnapshot = new List<Monster>(monstersInRange);

            foreach (var monster in monstersSnapshot)
            {
                monster.TakeDamage(damageAmount);
            }
            yield return new WaitForSeconds(damageInterval);
        }
    }
}
