using UnityEngine;
using System.Collections;

public class DestructibleObject : MonoBehaviour
{
    [SerializeField]
    private ObjectData objectData;

    private int currentHealth;

    private SpriteRenderer spriteRenderer;

    private bool isInvincible = false;

    [SerializeField]
    private GameObject[] spawnPrefabs;

    [SerializeField]
    private float invincibilityDuration = 0.5f;

    [SerializeField]
    private float blinkInterval = 0.1f;

    // 오브젝트가 파괴될 때 호출되는 이벤트
    public event System.Action<GameObject> OnDestroyed;

    private bool isFading = false; // 페이드 중인지 추적

    private void Start()
    {
        if (objectData != null)
        {
            currentHealth = objectData.health;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer가 없습니다. 이 스크립트는 SpriteRenderer가 필요합니다.");
        }
        else
        {
            // 초기 알파를 0으로 설정 (완전히 투명)
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;

            // Fade-In 시작
            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// 오브젝트가 데미지를 받을 때 호출됩니다.
    /// </summary>
    /// <param name="damage">받는 데미지 양</param>
    public void TakeDamage(int damage)
    {
        if (!isInvincible && !isFading)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                DestroyObject();
            }
            else
            {
                StartCoroutine(InvincibilityCoroutine());
            }
        }
    }

    /// <summary>
    /// 무적 상태를 관리하는 코루틴입니다.
    /// </summary>
    /// <returns></returns>
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        for (float i = 0; i < invincibilityDuration; i += blinkInterval)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(blinkInterval);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvincible = false;
    }

    /// <summary>
    /// 오브젝트를 파괴하기 위한 메서드입니다.
    /// </summary>
    private void DestroyObject()
    {
        if (!isFading)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    /// <summary>
    /// 오브젝트를 서서히 나타내는 코루틴입니다.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeIn()
    {
        float duration = 1f; // 1초 동안 페이드 인
        float elapsed = 0f;
        Color color = spriteRenderer.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            color.a = alpha;
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = 1f;
        spriteRenderer.color = color;
    }

    /// <summary>
    /// 오브젝트를 서서히 사라지게 하고 파괴하는 코루틴입니다.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOutAndDestroy()
    {
        isFading = true;

        float duration = 0.5f; // 0.5초 동안 페이드 아웃
        float elapsed = 0f;
        Color color = spriteRenderer.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            color.a = alpha;
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = 0f;
        spriteRenderer.color = color;

        // 스폰 프리팹 생성
        if (spawnPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPrefabs.Length);
            Instantiate(spawnPrefabs[randomIndex], transform.position, Quaternion.identity);
        }

        // 파괴 이벤트 호출
        OnDestroyed?.Invoke(gameObject);

        // 완전히 사라지기 전에 잠깐 대기 (선택 사항)
        yield return new WaitForSeconds(0.1f); // 필요 시 조정

        // 오브젝트 파괴
        Destroy(gameObject);
    }

    /// <summary>
    /// 다른 오브젝트와 충돌 시 호출되는 메서드입니다.
    /// </summary>
    /// <param name="collision">충돌 정보</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                TakeDamage(player.stat.currentPlayerDamage);
            }
        }
    }
}
