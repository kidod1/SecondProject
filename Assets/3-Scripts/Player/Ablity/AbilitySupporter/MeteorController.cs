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

    [Tooltip("���׿� �浹 �� ����Ʈ ������")]
    public GameObject explosionEffectPrefab; // ���׿� �浹 �� ����Ʈ ������ (�ɼ�)

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
            // ���׿��� Ÿ�� ��ġ�� �̵�
            Vector2 currentPosition = transform.position;
            Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, fallSpeed * Time.deltaTime);
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);

            // Ÿ�� ��ġ�� �����ϸ� �浹 ó��
            if (Vector2.Distance(newPosition, targetPosition) < 1.5f)
            {
                hasLanded = true;
                StartCoroutine(Explode());
            }
        }
    }

    private IEnumerator Explode()
    {
        // ���׿��� Ÿ�� ��ġ�� ������ �� 0�� ��� (��� ����)
        yield return new WaitForSeconds(0);

        // ���� ����Ʈ ����
        GameObject explosionEffectInstance = null;
        if (explosionEffectPrefab != null)
        {
            explosionEffectInstance = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        Sound.Post(PlayManager.I.GetPlayer().gameObject);
        // �浹 ���� ���� ���鿡�� ���� ����
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hits)
        {
            Monster monster = hit.GetComponent<Monster>();
            if (monster != null && !monster.IsDead)
            {
                monster.TakeDamage(Mathf.RoundToInt(damage), PlayManager.I.GetPlayerPosition());
                // �߰� ȿ���� ����Ʈ ���� ����
            }
        }

        // ���� ����Ʈ�� ������ 3�� �� �ı�
        if (explosionEffectInstance != null)
        {
            Destroy(explosionEffectInstance, 3f);
        }

        // ���׿� ������Ʈ ����
        Destroy(gameObject);

        yield return null;
    }

    private void OnDrawGizmos()
    {
        // ���׿� �浹 ���� ǥ��
        if (hasLanded)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
