using System.Collections;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField]
    protected MonsterData stat;
    protected int currentHP;
    protected SpriteRenderer spriteRenderer;
    protected bool isInvincible = false;

    [SerializeField]
    private float invincibilityDuration = 0.5f; // ���� ���� ���� �ð�
    [SerializeField]
    private float blinkInterval = 0.1f; // ������ ����

    protected virtual void Start()
    {
        currentHP = stat.maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public virtual void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHP -= damage;

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    protected virtual void Die()
    {
        // ���� ���� ó�� ����
        gameObject.SetActive(false);
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        for (float i = 0; i < invincibilityDuration; i += blinkInterval)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }

        spriteRenderer.enabled = true;
        isInvincible = false;
    }
}
