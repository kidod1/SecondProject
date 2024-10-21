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

    // ������Ʈ�� �ı��� �� ȣ��Ǵ� �̺�Ʈ
    public event System.Action<GameObject> OnDestroyed;

    private bool isFading = false; // ���̵� ������ ����

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
            // �ʱ� ���ĸ� 0���� ���� (������ ����)
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;

            // Fade-In ����
            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// ������Ʈ�� �������� ���� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="damage">�޴� ������ ��</param>
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
    /// ���� ���¸� �����ϴ� �ڷ�ƾ�Դϴ�.
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
    /// ������Ʈ�� �ı��ϱ� ���� �޼����Դϴ�.
    /// </summary>
    private void DestroyObject()
    {
        if (!isFading)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    /// <summary>
    /// ������Ʈ�� ������ ��Ÿ���� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// ������Ʈ�� ������ ������� �ϰ� �ı��ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <returns></returns>
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

        // �ı� �̺�Ʈ ȣ��
        OnDestroyed?.Invoke(gameObject);

        // ������ ������� ���� ��� ��� (���� ����)
        yield return new WaitForSeconds(0.1f); // �ʿ� �� ����

        // ������Ʈ �ı�
        Destroy(gameObject);
    }

    /// <summary>
    /// �ٸ� ������Ʈ�� �浹 �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="collision">�浹 ����</param>
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
