using UnityEngine;

public class ExperienceItem : MonoBehaviour
{
    public int experienceAmount;
    private bool isAttracting = false;
    private Transform target; // 플레이어의 Transform
    private float attractSpeed;
    private float attractDuration;
    private float attractTimer = 0f;

    // **추가된 필드 시작**
    [Header("끌어당김 설정")]
    [Tooltip("플레이어에게 끌려오기 시작하는 거리")]
    [SerializeField]
    private float attractionRadius = 1.5f; // 플레이어와의 거리 임계값

    [Tooltip("플레이어에게 끌려오는 속도")]
    [SerializeField]
    private float attractionSpeed = 5f; // 끌어당김 속도

    // **추가된 필드 끝**

    private Player player; // 플레이어 스크립트 참조

    private void Start()
    {
        // 플레이어 오브젝트 찾기
        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("플레이어 오브젝트를 찾을 수 없습니다.");
        }
    }

    private void Update()
    {
        if (player == null) return;

        if (!isAttracting)
        {
            // 플레이어와의 거리 계산
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance <= attractionRadius)
            {
                // 끌어당김 시작
                StartAttractingToPlayer(player.transform, attractionSpeed, Mathf.Infinity); // 지속 시간을 무한대로 설정하여 플레이어에게 도달할 때까지 끌어당김
            }
        }

        if (isAttracting)
        {
            if (target != null)
            {
                // 플레이어를 향해 부드럽게 이동
                Vector3 direction = (target.position - transform.position).normalized;
                transform.position = Vector3.MoveTowards(transform.position, target.position, attractSpeed * Time.deltaTime);

                // 플레이어에게 도달했을 때 경험치 획득 및 아이템 파괴
                if (Vector2.Distance(transform.position, target.position) < 0.1f)
                {
                    CollectExperience();
                }
            }
        }
    }

    /// <summary>
    /// 플레이어에게 끌려오기 시작합니다.
    /// </summary>
    /// <param name="playerTransform">플레이어의 Transform</param>
    /// <param name="speed">끌어당김 속도</param>
    /// <param name="duration">끌어당김 지속 시간</param>
    public void StartAttractingToPlayer(Transform playerTransform, float speed, float duration)
    {
        isAttracting = true;
        target = playerTransform;
        attractSpeed = speed;
        attractDuration = duration;
        attractTimer = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.GainExperience(experienceAmount);
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// 경험치를 획득하고 아이템을 파괴합니다.
    /// </summary>
    private void CollectExperience()
    {
        if (player != null)
        {
            player.GainExperience(experienceAmount);
        }
        Destroy(gameObject);
    }
}
