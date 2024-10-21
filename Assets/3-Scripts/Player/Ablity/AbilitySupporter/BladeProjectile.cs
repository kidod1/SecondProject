using UnityEngine;
using System.Collections;

public class BladeProjectile : MonoBehaviour
{
    private int damage;
    private float range;
    private float speed;
    private Vector3 startPosition;
    private Vector2 direction;  // ���� ����
    private Player owner;

    private SpriteRenderer spriteRenderer;

    // �ʱ� �ӵ��� ���� �ӵ� ����
    [SerializeField]
    private float initialSpeed = 6f; // �ʱ� ���� �ӵ�
    private float finalSpeed;

    // ���İ� ��ȭ �� �ӵ� ���� �ð� ����
    [SerializeField]
    private float fadeInDuration = 0.5f; // ���İ� 0���� 1�� ��ȭ�ϴ� �ð� (��)
    [SerializeField]
    private float fadeOutDuration = 1f; // ���İ� 1���� 0���� ��ȭ�ϴ� �ð� (��)

    private float totalLifetime;

    /// <summary>
    /// Į���� �ʱ�ȭ�ϴ� �޼���
    /// </summary>
    /// <param name="damage">Į���� ���ط�</param>
    /// <param name="range">Į���� ��Ÿ�</param>
    /// <param name="speed">Į���� ���� �ӵ�</param>
    /// <param name="owner">Į���� �߻��� �÷��̾�</param>
    /// <param name="direction">Į���� �̵� ����</param>
    public void Initialize(int damage, float range, float speed, Player owner, Vector2 direction)
    {
        this.damage = damage;
        this.range = range;
        this.finalSpeed = speed;
        this.owner = owner;
        this.direction = direction.normalized;
        this.speed = initialSpeed; // �ʱ� �ӵ� ����
        startPosition = transform.position;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("BladeProjectile: SpriteRenderer ������Ʈ�� ã�� �� �����ϴ�.");
        }
        else
        {
            // �ʱ� ���İ��� 0���� ����
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }

        // �ڷ�ƾ ����
        StartCoroutine(BladeCoroutine());
    }

    /// <summary>
    /// ���İ� ��ȭ�� �ӵ� ������ �����ϴ� �ڷ�ƾ
    /// </summary>
    /// <returns></returns>
    private IEnumerator BladeCoroutine()
    {
        // 1. ���İ��� 0���� 1�� ��ȭ��Ű�� �ӵ��� �ʱ⿡�� �������� ������ŵ�ϴ�.
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);

            // ���İ� ����
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(0f, 1f, t);
                spriteRenderer.color = color;
            }

            // �ӵ� ����
            speed = Mathf.Lerp(initialSpeed, finalSpeed, t);

            yield return null;
        }

        // ���İ��� �ӵ� ������ ����
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
        speed = finalSpeed;

        // 2. ��ü ������Ÿ�� ���
        float distanceTraveledDuringFadeIn = initialSpeed * fadeInDuration;
        float remainingDistance = range - distanceTraveledDuringFadeIn;
        if (remainingDistance < 0)
        {
            remainingDistance = 0;
        }

        float remainingTime = remainingDistance / finalSpeed;
        totalLifetime = fadeInDuration + remainingTime;

        // 3. ���̵� �ƿ� ���� ���� ���
        float timeToStartFadeOut = totalLifetime - fadeOutDuration;
        if (timeToStartFadeOut < fadeInDuration)
        {
            timeToStartFadeOut = fadeInDuration;
        }

        // 4. ���̵� �ƿ� ���۱��� ���
        float timeElapsed = 0f;
        while (timeElapsed < timeToStartFadeOut)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // 5. ���İ��� 1���� 0���� ������ ���ҽ�ŵ�ϴ�.
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(1f, 0f, t);
                spriteRenderer.color = color;
            }

            yield return null;
        }

        // ���İ��� ������ 0���� �����ϰ� ������Ʈ �ı�
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }

        Destroy(gameObject);
    }

    private void Update()
    {
        // ������ �������� �̵�
        Vector3 movement = new Vector3(direction.x, direction.y, 0f) * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // ������ ��Ÿ� ���� �� ������Ʈ �ı�
        if (Vector3.Distance(startPosition, transform.position) >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �� �±׸� ���� ������Ʈ�� �浹 �� ������ ����
        if (other.CompareTag("Monster"))
        {
            Monster enemy = other.GetComponent<Monster>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, owner.transform.position);
            }

            // Į���� �������� �ʵ��� �� ��� �Ʒ� �ּ��� �����ϼ���.
            // Destroy(gameObject);
        }
    }
}
