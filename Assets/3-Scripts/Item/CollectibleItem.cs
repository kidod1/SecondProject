using UnityEngine;
using System.Collections;

public abstract class CollectibleItem : MonoBehaviour
{
    public int experienceAmount; // ����ġ �Ǵ� �ٸ� ȿ����
    private bool isAttracting = false;
    private Transform target; // �÷��̾��� Transform
    private float attractSpeed;
    private float attractDuration;
    private float attractTimer = 0f;

    [Header("������ ����")]
    [Tooltip("�÷��̾�� �������� �����ϴ� �Ÿ�")]
    [SerializeField]
    private float attractionRadius = 1.5f; // �÷��̾���� �Ÿ� �Ӱ谪

    [Tooltip("�÷��̾�� �������� �ӵ�")]
    [SerializeField]
    private float attractionSpeed = 5f; // ������ �ӵ�

    protected Player player; // �÷��̾� ��ũ��Ʈ ����

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
                    Collect();
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
            Collect();
        }
    }

    /// <summary>
    /// �������� ȹ���ϴ� �߻� �޼���
    /// </summary>
    protected abstract void Collect();
}
