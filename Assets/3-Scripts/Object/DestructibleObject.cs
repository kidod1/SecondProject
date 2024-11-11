using UnityEngine;
using UnityEngine.Events;
using System.Collections;

// UnityEvent에 GameObject를 전달할 수 있도록 커스텀 클래스 정의
[System.Serializable]
public class GameObjectEvent : UnityEvent<GameObject> { }

public class DestructibleObject : MonoBehaviour
{
    [SerializeField]
    private ObjectData objectData;

    [SerializeField]
    private GameObject[] spawnPrefabs;

    [SerializeField]
    private float invincibilityDuration = 0.5f;

    [SerializeField]
    private float blinkInterval = 0.1f;

    private int currentHealth;

    private SpriteRenderer spriteRenderer;

    private bool isInvincible = false;

    private bool isFading = false; // 페이드 중인지 추적

    public GameObjectEvent onDestroyedEvent;

    // 추가된 부분: Wwise 이벤트 참조
    [SerializeField]
    private AK.Wwise.Event destroySoundEvent; // Wwise 이벤트 할당을 위해 추가

    private void Awake()
    {
        if (onDestroyedEvent == null)
            onDestroyedEvent = new GameObjectEvent();
    }

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
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;

            StartCoroutine(FadeIn());
        }
    }

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

    private void DestroyObject()
    {
        if (!isFading)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }

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

        // Wwise 이벤트 호출: 오브젝트 파괴 시 사운드 재생
        if (destroySoundEvent != null)
        {
            // 게임 오브젝트의 위치에서 사운드를 재생
            destroySoundEvent.Post(gameObject);
        }
        else
        {
            Debug.LogWarning("Destroy Sound Event가 할당되지 않았습니다.");
        }

        // UnityEvent 호출 with GameObject parameter
        onDestroyedEvent?.Invoke(gameObject);

        // 잠깐 대기 (선택 사항)
        yield return new WaitForSeconds(0.1f); // 필요 시 조정

        // 오브젝트 파괴
        Destroy(gameObject);
    }

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
