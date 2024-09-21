using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PoisonCloud : MonoBehaviour
{
    private float poisonDamage;
    private float poisonRange;
    private float poisonDuration;
    private float damageInterval = 1f; // 1초마다 피해

    private List<Monster> affectedMonsters = new List<Monster>();

    public void Initialize(float damage, float range, float duration)
    {
        poisonDamage = damage;
        poisonRange = range;
        poisonDuration = duration;

        transform.localScale = new Vector3(poisonRange * 2, poisonRange * 2, 1);

        Destroy(gameObject, poisonDuration);

        StartCoroutine(DamageOverTime());
    }

    private IEnumerator DamageOverTime()
    {
        while (true)
        {
            ApplyDamage();
            yield return new WaitForSeconds(damageInterval);
        }
    }

    private void ApplyDamage()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, poisonRange);
        foreach (Collider2D collider in colliders)
        {
            Monster monster = collider.GetComponent<Monster>();
            if (monster != null && !monster.IsDead)
            {
                if (!affectedMonsters.Contains(monster))
                {
                    affectedMonsters.Add(monster);
                }
                monster.TakeDamage(Mathf.RoundToInt(poisonDamage));
            }
        }

        affectedMonsters.RemoveAll(monster => Vector2.Distance(transform.position, monster.transform.position) > poisonRange || monster.IsDead);
    }
}
