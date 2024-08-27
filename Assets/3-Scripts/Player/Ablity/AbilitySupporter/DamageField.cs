using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageField : MonoBehaviour
{
    private int damageAmount;
    private float damageInterval;
    private float damageFieldDuration;
    private float cooldownDurations;
    private ElectricField electricField; // ElectricField 참조
    private HashSet<Monster> monstersInRange = new HashSet<Monster>();
    private ParticleSystem particleSystem;
    private Animator animator;
    private bool isInitialized = false; // 초기화 여부를 확인하는 변수 추가
    private bool isCooldown = false; // 쿨다운 상태

    public void Initialize(ElectricField electricField, Player playerInstance)
    {
        this.electricField = electricField;
        damageAmount = electricField.damageAmount;
        damageInterval = electricField.damageInterval;
        damageFieldDuration = electricField.damageFieldDuration;
        cooldownDurations = electricField.cooldownDurations;

        particleSystem = GetComponent<ParticleSystem>();
        animator = GetComponent<Animator>();

        isInitialized = true; // 초기화 완료로 설정
        particleSystem?.Stop();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster") && !isCooldown)
        {
            Monster monster = other.GetComponent<Monster>();
            if (monster != null)
            {
                monstersInRange.Add(monster);
                StartCoroutine(AttackAndCooldownRoutine());
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

    private IEnumerator AttackAndCooldownRoutine()
    {
        if (isCooldown)
            yield break;

        isCooldown = true;

        if (particleSystem != null)
        {
            particleSystem.Play();
        }

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        var monstersSnapshot = new List<Monster>(monstersInRange);
        foreach (var monster in monstersSnapshot)
        {
            monster.TakeDamage(damageAmount);
        }

        yield return new WaitForSeconds(damageFieldDuration);

        if (particleSystem != null)
        {
            particleSystem.Stop();
        }

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
        }

        yield return new WaitForSeconds(cooldownDurations);

        isCooldown = false;
    }
}
