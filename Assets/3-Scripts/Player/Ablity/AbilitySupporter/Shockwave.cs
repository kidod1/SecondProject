using UnityEngine;
using System.Collections;

public class Shockwave : MonoBehaviour
{
    private Player player;
    private int currentLevel;
    private float pushRange;
    private int damageValue;
    private float cooldownTime;
    private Coroutine pushCoroutine;
    private Animator animator;

    [SerializeField]
    private float[] cooldownTimes = { 5f, 5f, 5f, 5f, 5f }; // �� ������ ��Ÿ��
    [SerializeField]
    private int[] damageValues = { 10, 10, 10, 10, 20 }; // �� ������ ���ط�
    [SerializeField]
    private float[] pushRanges = { 200f, 200f, 250f, 250f, 300f }; // �� ������ ����

    private bool isReady = false;

    private void Awake()
    {
        animator = GetComponent<Animator>(); // Animator ������Ʈ�� ������
    }

    public void Initialize(Player playerInstance, int level)
    {
        player = playerInstance;
        currentLevel = level;
        if (currentLevel == 0)
        {
            pushRange = pushRanges[1];
            damageValue = damageValues[1];
            cooldownTime = cooldownTimes[1];
        }
        else
        {
            pushRange = pushRanges[currentLevel - 1];
            damageValue = damageValues[currentLevel - 1];
            cooldownTime = cooldownTimes[currentLevel - 1];
        }

        StartShockwave();
    }

    private void StartShockwave()
    {
        if (pushCoroutine == null)
        {
            pushCoroutine = StartCoroutine(PushAwayCoroutine());
        }
    }

    private IEnumerator PushAwayCoroutine()
    {
        while (true)
        {
            // �غ� ���·� ��ȯ�ϰ� ���� ��ٸ�
            SetReady(true);

            while (!IsMonsterInRange())
            {
                yield return null;
            }

            // ���� �����Ǹ� �˹��� �����ϰ� �غ� ���� ����
            SetReady(false);
            PerformPushAway();

            // �˹��� ������ ��Ÿ�� ���
            yield return new WaitForSecondsRealtime(cooldownTime);
        }
    }

    private bool IsMonsterInRange()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, pushRange);

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Monster"))
            {
                return true;
            }
        }
        return false;
    }

    private void PerformPushAway()
    {
        if (animator != null)
        {
            animator.SetBool("Play", true);
            Debug.Log("Play");
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, pushRange);

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Monster"))
            {
                Monster monster = hitCollider.GetComponent<Monster>();
                if (monster != null)
                {
                    monster.TakeDamage(damageValue);

                    Rigidbody2D monsterRb = monster.GetComponent<Rigidbody2D>();
                    if (monsterRb != null)
                    {
                        Vector2 pushDirection = (monster.transform.position - transform.position).normalized;
                        monsterRb.AddForce(pushDirection * 1000f);
                    }
                }
            }
        }
        Debug.Log("������ �� ��");

        // 'Play' �ִϸ��̼��� ������ false�� �����Ͽ� Ready ���·� ����
        StartCoroutine(EndPlayAnimation());
    }

    private IEnumerator EndPlayAnimation()
    {
        yield return new WaitForSecondsRealtime(1.13f); // �ִϸ��̼��� ����Ǵ� �ð� (�뷫���� �ð� ����)

        if (animator != null)
        {
            animator.SetBool("Play", false);
            Debug.Log("Play ����");
        }
    }

    private void SetReady(bool ready)
    {
        isReady = ready;

        if (animator != null)
        {
            animator.SetBool("Ready", ready);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pushRange);
    }
}
