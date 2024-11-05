using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;

public class Bat : Monster
{
    [SerializeField]
    private BatMonsterData stat;

    [SpineAnimation] public string dashAnimation;     // 돌진 애니메이션
    [SpineAnimation] public string explodeAnimation;  // 자폭 애니메이션

    private SkeletonAnimation skeletonAnimation;

    private enum BatState
    {
        Dashing
    }

    private BatState currentState = BatState.Dashing;
    private Vector3 dashDirection;
    private float dashSpeed;

    protected override void Start()
    {
        base.Start();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            Debug.LogError("SkeletonAnimation이 null입니다.");
        }
        else
        {
            PlayAnimation(dashAnimation, true);
        }

        // 돌진 방향 계산: 소환될 때 플레이어를 향해 설정
        if (player != null)
        {
            dashDirection = (player.transform.position - transform.position).normalized;
        }
        else
        {
            Debug.LogError("Player 참조가 없습니다.");
            dashDirection = Vector3.zero;
        }

        dashSpeed = stat.skillDashSpeed;
    }

    protected override void InitializeStates()
    {
        // 이 클래스에서는 상태 클래스를 사용하지 않으므로 빈 메서드로 둡니다.
    }

    private void Update()
    {
        switch (currentState)
        {
            case BatState.Dashing:
                HandleDashState();
                break;
        }
    }

    private void HandleDashState()
    {
        if (player == null)
        {
            // 플레이어가 없으면 사라짐
            Die();
            return;
        }

        // 돌진 이동
        float step = dashSpeed * Time.deltaTime;
        transform.position += dashDirection * step;

        // 추가적으로 최대 돌진 거리를 설정하거나, 특정 조건에서 멈추도록 구현할 수 있습니다.
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("닿았음");
        GameObject collidedObject = collision.gameObject;

        // 충돌한 오브젝트에 Player 컴포넌트가 있는지 확인
        Player playerScript = collidedObject.GetComponent<Player>();
        if (playerScript != null)
        {
            // 플레이어와 충돌한 경우
            playerScript.TakeDamage(stat.attackDamage);
            Die();
        }
        else
        {
            Die();
        }
    }

    public override void Attack()
    {
        // 공격 로직은 OnCollisionEnter에서 처리되므로 별도 구현 필요 없음
    }

    public void PlayAnimation(string animationName, bool loop)
    {
        if (skeletonAnimation != null && !string.IsNullOrEmpty(animationName))
        {
            skeletonAnimation.state.SetAnimation(0, animationName, loop);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 탐지 범위와 공격 범위를 기즈모로 표시 (필요 시)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stat.detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stat.attackRange);
    }
}
