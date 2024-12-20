using UnityEngine;
using System.Collections;

public class MeteorController : MonoBehaviour
{
    private float damage;
    private float radius;
    private float fallSpeed;
    private Vector2 targetPosition;

    private bool hasLanded = false;

    [SerializeField]
    private AK.Wwise.Event Sound;

    [Tooltip("메테오 충돌 시 이펙트 프리팹")]
    public GameObject explosionEffectPrefab; // 메테오 충돌 시 이펙트 프리팹 (옵션)

    public void Initialize(float damage, float radius, float fallSpeed, Vector2 targetPosition)
    {
        this.damage = damage;
        this.radius = radius;
        this.fallSpeed = fallSpeed;
        this.targetPosition = targetPosition;
    }

    private void Update()
    {
        if (!hasLanded)
        {
            // 메테오를 타겟 위치로 이동
            Vector2 currentPosition = transform.position;
            Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, fallSpeed * Time.deltaTime);
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);

            // 타겟 위치에 도달하면 충돌 처리
            if (Vector2.Distance(newPosition, targetPosition) < 1.5f)
            {
                hasLanded = true;
                StartCoroutine(Explode());
            }
        }
    }

    private IEnumerator Explode()
    {
        // 메테오가 타겟 위치에 도착한 후 0초 대기 (즉시 실행)
        yield return new WaitForSeconds(0);

        // 번개 이펙트 생성
        GameObject explosionEffectInstance = null;
        if (explosionEffectPrefab != null)
        {
            explosionEffectInstance = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        Sound.Post(PlayManager.I.GetPlayer().gameObject);
        // 충돌 범위 내의 적들에게 피해 적용
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hits)
        {
            Monster monster = hit.GetComponent<Monster>();
            if (monster != null && !monster.IsDead)
            {
                monster.TakeDamage(Mathf.RoundToInt(damage), PlayManager.I.GetPlayerPosition());
                // 추가 효과나 이펙트 적용 가능
            }
        }

        // 폭발 이펙트가 있으면 3초 후 파괴
        if (explosionEffectInstance != null)
        {
            Destroy(explosionEffectInstance, 3f);
        }

        // 메테오 오브젝트 제거
        Destroy(gameObject);

        yield return null;
    }

    private void OnDrawGizmos()
    {
        // 메테오 충돌 범위 표시
        if (hasLanded)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
