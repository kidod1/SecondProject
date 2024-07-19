using System.Collections;
using UnityEngine;

public class SpotlightEffect : MonoBehaviour
{
    public float damageInterval = 1f; // 피해 간격
    public float damageRadius = 0f; // 범위
    public int damageAmount = 0; // 피해량
    public Player player;
    public int currentLevel = 0; // 현재 레벨
    private Collider2D[] hits;

    [SerializeField]
    private SpriteRenderer spriteRenderer; // 스프라이트 렌더러
    private Animator animator; // 애니메이터

    private void Start()
    {
        animator = GetComponent<Animator>();
        hits = new Collider2D[10]; // 초기 크기 설정
        StartCoroutine(DamageCoroutine());
    }

    private IEnumerator DamageCoroutine()
    {
        while (true)
        {
            // 스프라이트 깜빡이기 효과
            spriteRenderer.enabled = true; // 스프라이트 표시
            yield return new WaitForSeconds(0.1f); // 잠시 대기
            spriteRenderer.enabled = false; // 스프라이트 숨기기
            yield return new WaitForSeconds(0.1f); // 잠시 대기
            spriteRenderer.enabled = true; // 스프라이트 다시 표시

            // 데미지 주기
            int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, damageRadius, hits);
            for (int i = 0; i < hitCount; i++)
            {
                Monster monster = hits[i].GetComponent<Monster>();
                if (monster != null)
                {
                    monster.TakeDamage(damageAmount);
                }
            }

            yield return new WaitForSeconds(damageInterval - 0.2f); // 데미지 간격 대기 (깜빡이는 시간 제외)
        }
    }

    private void Update()
    {
        if (player != null)
        {
            transform.position = player.transform.position;
            UpdateScale(); // Scale 업데이트
        }

        // 애니메이션 트리거
        animator.SetTrigger("Activate");
    }

    private void UpdateScale()
    {
        float scaleValue = 0.3f + (currentLevel - 1) * 0.1f;
        transform.localScale = new Vector3(scaleValue, scaleValue, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
