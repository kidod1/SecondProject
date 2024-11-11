using UnityEngine;
using UnityEngine.Events;
using System.Collections;

// UnityEvent�� GameObject�� ������ �� �ֵ��� Ŀ���� Ŭ���� ����
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

    private bool isFading = false; // ���̵� ������ ����

    public GameObjectEvent onDestroyedEvent;

    // �߰��� �κ�: Wwise �̺�Ʈ ����
    [SerializeField]
    private AK.Wwise.Event destroySoundEvent; // Wwise �̺�Ʈ �Ҵ��� ���� �߰�

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
            Debug.LogError("SpriteRenderer�� �����ϴ�. �� ��ũ��Ʈ�� SpriteRenderer�� �ʿ��մϴ�.");
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
        float duration = 1f; // 1�� ���� ���̵� ��
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

        float duration = 0.5f; // 0.5�� ���� ���̵� �ƿ�
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

        // ���� ������ ����
        if (spawnPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPrefabs.Length);
            Instantiate(spawnPrefabs[randomIndex], transform.position, Quaternion.identity);
        }

        // Wwise �̺�Ʈ ȣ��: ������Ʈ �ı� �� ���� ���
        if (destroySoundEvent != null)
        {
            // ���� ������Ʈ�� ��ġ���� ���带 ���
            destroySoundEvent.Post(gameObject);
        }
        else
        {
            Debug.LogWarning("Destroy Sound Event�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // UnityEvent ȣ�� with GameObject parameter
        onDestroyedEvent?.Invoke(gameObject);

        // ��� ��� (���� ����)
        yield return new WaitForSeconds(0.1f); // �ʿ� �� ����

        // ������Ʈ �ı�
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
