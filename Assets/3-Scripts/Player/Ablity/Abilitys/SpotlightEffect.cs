using System.Collections;
using UnityEngine;

public class SpotlightEffect : MonoBehaviour
{
    public float damageInterval = 1f; // ���� ����
    public float damageRadius = 0f; // ����
    public int damageAmount = 0; // ���ط�
    public Player player;
    public int currentLevel = 0; // ���� ����
    private Collider2D[] hits;

    [SerializeField]
    private SpriteRenderer spriteRenderer; // ��������Ʈ ������
    private Animator animator; // �ִϸ�����

    private void Start()
    {
        animator = GetComponent<Animator>();
        hits = new Collider2D[10]; // �ʱ� ũ�� ����
        StartCoroutine(DamageCoroutine());
    }

    private IEnumerator DamageCoroutine()
    {
        while (true)
        {
            // ��������Ʈ �����̱� ȿ��
            spriteRenderer.enabled = true; // ��������Ʈ ǥ��
            yield return new WaitForSeconds(0.1f); // ��� ���
            spriteRenderer.enabled = false; // ��������Ʈ �����
            yield return new WaitForSeconds(0.1f); // ��� ���
            spriteRenderer.enabled = true; // ��������Ʈ �ٽ� ǥ��

            // ������ �ֱ�
            int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, damageRadius, hits);
            for (int i = 0; i < hitCount; i++)
            {
                Monster monster = hits[i].GetComponent<Monster>();
                if (monster != null)
                {
                    monster.TakeDamage(damageAmount);
                }
            }

            yield return new WaitForSeconds(damageInterval - 0.2f); // ������ ���� ��� (�����̴� �ð� ����)
        }
    }

    private void Update()
    {
        if (player != null)
        {
            transform.position = player.transform.position;
            UpdateScale(); // Scale ������Ʈ
        }

        // �ִϸ��̼� Ʈ����
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
