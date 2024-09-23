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
    private float[] cooldownTimes = { 5f, 5f, 5f, 5f, 5f }; // 각 레벨별 쿨타임
    [SerializeField]
    private int[] damageValues = { 10, 10, 10, 10, 20 }; // 각 레벨별 피해량
    [SerializeField]
    private float[] pushRanges = { 200f, 200f, 250f, 250f, 300f }; // 각 레벨별 범위

    private bool isReady = false;

    private void Awake()
    {
        animator = GetComponent<Animator>(); // Animator 컴포넌트를 가져옴
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
            // 준비 상태로 전환하고 적을 기다림
            SetReady(true);

            while (!IsMonsterInRange())
            {
                yield return null;
            }

            // 적이 감지되면 넉백을 수행하고 준비 상태 해제
            SetReady(false);
            PerformPushAway();

            // 넉백이 끝나면 쿨타임 대기
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
        Debug.Log("데미지 다 줌");

        // 'Play' 애니메이션이 끝나면 false로 설정하여 Ready 상태로 복귀
        StartCoroutine(EndPlayAnimation());
    }

    private IEnumerator EndPlayAnimation()
    {
        yield return new WaitForSecondsRealtime(1.13f); // 애니메이션이 재생되는 시간 (대략적인 시간 설정)

        if (animator != null)
        {
            animator.SetBool("Play", false);
            Debug.Log("Play 꺼짐");
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
