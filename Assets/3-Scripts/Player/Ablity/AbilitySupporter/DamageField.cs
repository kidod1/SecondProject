using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageField : MonoBehaviour
{
    private int damageAmount;
    private float damageInterval;
    private float damageFieldDuration;
    private float fieldCooldownDurations;
    private ElectricField electricField;
    private HashSet<Monster> monstersInRange = new HashSet<Monster>();
    private ParticleSystem particleSystem;
    private Animator animator;
    private bool isInitialized = false;
    private bool isCooldown = false;

    public void Initialize(ElectricField electricField, Player playerInstance)
    {
        this.electricField = electricField;
        damageAmount = electricField.damageAmount;
        damageInterval = electricField.damageInterval;
        damageFieldDuration = electricField.damageFieldDuration;
        fieldCooldownDurations = electricField.ElectricAblityCooldownDurations;

        particleSystem = GetComponent<ParticleSystem>();
        animator = GetComponent<Animator>();

        isInitialized = true;
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

    public void DealDamage(int amount)
    {
        foreach (var monster in monstersInRange)
        {
            monster.TakeDamage(amount);
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

        yield return new WaitForSecondsRealtime(damageFieldDuration);

        if (particleSystem != null)
        {
            particleSystem.Stop();
        }

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
        }

        yield return new WaitForSecondsRealtime(fieldCooldownDurations);

        isCooldown = false;
    }
}
