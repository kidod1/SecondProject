using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    private PlayerData stat;
    private int currentHP;
    private bool isInvincible = false; // 무적 상태 여부
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        currentHP = stat.MaxHP; // 시작할 때 현재 HP를 최대 HP로 설정
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer 컴포넌트 가져오기
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isInvincible) return; // 무적 상태일 때는 데미지를 받지 않음

        currentHP -= damage; // HP 감소

        // 데미지를 받을 때 뒤로 밀려남
        StartCoroutine(KnockbackCoroutine(knockbackDirection));
        // 무적 상태 시작
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
        // 플레이어 죽음 처리 로직 추가 (예: 게임 오버 화면 표시, 재시작 등)
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
        float knockbackDuration = 0.1f; // 밀려나는 시간
        float knockbackSpeed = 5.0f; // 밀려나는 속도
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
        float invincibilityDuration = 0.5f; // 무적 상태 지속 시간
        float blinkInterval = 0.1f; // 깜빡임 간격

        for (float i = 0; i < invincibilityDuration; i += blinkInterval)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // 깜빡이기
            yield return new WaitForSeconds(blinkInterval);
        }

        spriteRenderer.enabled = true; // 깜빡임 종료 후 다시 보이기
        isInvincible = false;
    }
}