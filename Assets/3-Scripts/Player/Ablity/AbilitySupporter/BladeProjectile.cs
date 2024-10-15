using UnityEngine;
using System.Collections;

public class BladeProjectile : MonoBehaviour
{
    private int damage;
    private float range;
    private float speed;
    private Vector3 startPosition;
    private Vector2 direction;  // 방향 벡터
    private Player owner;

    private SpriteRenderer spriteRenderer;

    // 초기 속도와 최종 속도 설정
    [SerializeField]
    private float initialSpeed = 6f; // 초기 느린 속도
    private float finalSpeed;

    // 알파값 변화 및 속도 변경 시간 설정
    [SerializeField]
    private float fadeInDuration = 0.5f; // 알파값 0에서 1로 변화하는 시간 (초)
    [SerializeField]
    private float fadeOutDuration = 1f; // 알파값 1에서 0으로 변화하는 시간 (초)

    private float totalLifetime;

    /// <summary>
    /// 칼날을 초기화하는 메서드
    /// </summary>
    /// <param name="damage">칼날의 피해량</param>
    /// <param name="range">칼날의 사거리</param>
    /// <param name="speed">칼날의 최종 속도</param>
    /// <param name="owner">칼날을 발사한 플레이어</param>
    /// <param name="direction">칼날의 이동 방향</param>
    public void Initialize(int damage, float range, float speed, Player owner, Vector2 direction)
    {
        this.damage = damage;
        this.range = range;
        this.finalSpeed = speed;
        this.owner = owner;
        this.direction = direction.normalized;
        this.speed = initialSpeed; // 초기 속도 설정
        startPosition = transform.position;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("BladeProjectile: SpriteRenderer 컴포넌트를 찾을 수 없습니다.");
        }
        else
        {
            // 초기 알파값을 0으로 설정
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }

        // 코루틴 시작
        StartCoroutine(BladeCoroutine());
    }

    /// <summary>
    /// 알파값 변화와 속도 변경을 관리하는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator BladeCoroutine()
    {
        // 1. 알파값을 0에서 1로 변화시키고 속도를 초기에서 최종으로 증가시킵니다.
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);

            // 알파값 증가
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(0f, 1f, t);
                spriteRenderer.color = color;
            }

            // 속도 증가
            speed = Mathf.Lerp(initialSpeed, finalSpeed, t);

            yield return null;
        }

        // 알파값과 속도 최종값 설정
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
        speed = finalSpeed;

        // 2. 전체 라이프타임 계산
        float distanceTraveledDuringFadeIn = initialSpeed * fadeInDuration;
        float remainingDistance = range - distanceTraveledDuringFadeIn;
        if (remainingDistance < 0)
        {
            remainingDistance = 0;
        }

        float remainingTime = remainingDistance / finalSpeed;
        totalLifetime = fadeInDuration + remainingTime;

        // 3. 페이드 아웃 시작 시점 계산
        float timeToStartFadeOut = totalLifetime - fadeOutDuration;
        if (timeToStartFadeOut < fadeInDuration)
        {
            timeToStartFadeOut = fadeInDuration;
        }

        // 4. 페이드 아웃 시작까지 대기
        float timeElapsed = 0f;
        while (timeElapsed < timeToStartFadeOut)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // 5. 알파값을 1에서 0으로 서서히 감소시킵니다.
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

        // 알파값을 완전히 0으로 설정하고 오브젝트 파괴
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
        // 설정된 방향으로 이동
        Vector3 movement = new Vector3(direction.x, direction.y, 0f) * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // 설정된 사거리 도달 시 오브젝트 파괴
        if (Vector3.Distance(startPosition, transform.position) >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 적 태그를 가진 오브젝트에 충돌 시 데미지 적용
        if (other.CompareTag("Monster"))
        {
            Monster enemy = other.GetComponent<Monster>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, owner.transform.position);
            }

            // 칼날이 관통하지 않도록 할 경우 아래 주석을 해제하세요.
            // Destroy(gameObject);
        }
    }
}
