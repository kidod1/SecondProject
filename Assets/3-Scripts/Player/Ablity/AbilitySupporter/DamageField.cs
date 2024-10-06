using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageField : MonoBehaviour
{
    private int damageAmount;
    private HashSet<Monster> monstersInRange = new HashSet<Monster>();
    private ParticleSystem particleSystem;

    public void Initialize(ElectricField electricField)
    {
        damageAmount = electricField.damageAmount;

        particleSystem = GetComponent<ParticleSystem>();

        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
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

    public void DealDamage(int amount)
    {
        // monstersInRange의 복사본을 만들어 순회
        List<Monster> monstersSnapshot = new List<Monster>(monstersInRange);
        foreach (var monster in monstersSnapshot)
        {
            if (monster != null) // 몬스터가 유효한지 확인
            {
                monster.TakeDamage(amount, transform.position);
            }
        }
    }

    // 파티클 재생 및 중지와 관련된 메서드
    public void Activate()
    {
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
    }

    public void Deactivate()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
    }
}
