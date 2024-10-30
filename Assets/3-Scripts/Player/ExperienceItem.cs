using UnityEngine;

public class ExperienceItem : MonoBehaviour
{
    public int experienceAmount;
    private bool isAttracting = false;
    private Transform target; // �÷��̾��� Transform
    private float attractSpeed;
    private float attractDuration;
    private float attractTimer = 0f;

    // **�߰��� �ʵ� ����**
    [Header("������ ����")]
    [Tooltip("�÷��̾�� �������� �����ϴ� �Ÿ�")]
    [SerializeField]
    private float attractionRadius = 1.5f; // �÷��̾���� �Ÿ� �Ӱ谪

    [Tooltip("�÷��̾�� �������� �ӵ�")]
    [SerializeField]
    private float attractionSpeed = 5f; // ������ �ӵ�

    // **�߰��� �ʵ� ��**

    private Player player; // �÷��̾� ��ũ��Ʈ ����

    private void Start()
    {
        // �÷��̾� ������Ʈ ã��
        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("�÷��̾� ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    private void Update()
    {
        if (player == null) return;

        if (!isAttracting)
        {
            // �÷��̾���� �Ÿ� ���
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance <= attractionRadius)
            {
                // ������ ����
                StartAttractingToPlayer(player.transform, attractionSpeed, Mathf.Infinity); // ���� �ð��� ���Ѵ�� �����Ͽ� �÷��̾�� ������ ������ ������
            }
        }

        if (isAttracting)
        {
            if (target != null)
            {
                // �÷��̾ ���� �ε巴�� �̵�
                Vector3 direction = (target.position - transform.position).normalized;
                transform.position = Vector3.MoveTowards(transform.position, target.position, attractSpeed * Time.deltaTime);

                // �÷��̾�� �������� �� ����ġ ȹ�� �� ������ �ı�
                if (Vector2.Distance(transform.position, target.position) < 0.1f)
                {
                    CollectExperience();
                }
            }
        }
    }

    /// <summary>
    /// �÷��̾�� �������� �����մϴ�.
    /// </summary>
    /// <param name="playerTransform">�÷��̾��� Transform</param>
    /// <param name="speed">������ �ӵ�</param>
    /// <param name="duration">������ ���� �ð�</param>
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
    /// ����ġ�� ȹ���ϰ� �������� �ı��մϴ�.
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
