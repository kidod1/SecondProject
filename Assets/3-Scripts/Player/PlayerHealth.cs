using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    private PlayerData stat;
    private int currentHP;
    private bool isInvincible = false; // ���� ���� ����
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        currentHP = stat.MaxHP; // ������ �� ���� HP�� �ִ� HP�� ����
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer ������Ʈ ��������
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isInvincible) return; // ���� ������ ���� �������� ���� ����

        currentHP -= damage; // HP ����

        // �������� ���� �� �ڷ� �з���
        StartCoroutine(KnockbackCoroutine(knockbackDirection));
        // ���� ���� ����
        StartCoroutine(InvincibilityCoroutine());

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        // �÷��̾� ���� ó�� ���� �߰� (��: ���� ���� ȭ�� ǥ��, ����� ��)
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > stat.MaxHP)
        {
            currentHP = stat.MaxHP;
        }
    }

    public int GetCurrentHP()
    {
        return currentHP;
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction)
    {
        float knockbackDuration = 0.1f; // �з����� �ð�
        float knockbackSpeed = 5.0f; // �з����� �ӵ�
        float timer = 0;

        while (timer < knockbackDuration)
        {
            timer += Time.deltaTime;
            transform.Translate(direction * knockbackSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float invincibilityDuration = 0.5f; // ���� ���� ���� �ð�
        float blinkInterval = 0.1f; // ������ ����

        for (float i = 0; i < invincibilityDuration; i += blinkInterval)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // �����̱�
            yield return new WaitForSeconds(blinkInterval);
        }

        spriteRenderer.enabled = true; // ������ ���� �� �ٽ� ���̱�
        isInvincible = false;
    }
}