using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;

public class Bat : Monster
{
    [SerializeField]
    private BatMonsterData stat;

    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string prepareAnimation;  // 공격 준비 애니메이션
    [SpineAnimation] public string dashAnimation;     // 돌진 애니메이션
    [SpineAnimation] public string explodeAnimation;  // 자폭 애니메이션

    private SkeletonAnimation skeletonAnimation;

    private enum BatState
    {
        Idle,
        Chasing,
        PreparingAttack,
        Dashing,
        Cooldown
    }

    private BatState currentState = BatState.Idle;
    private bool isAttacking = false;
    private Vector3 dashTargetPosition;
    private float cooldownTimer = 0f;

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
            PlayAnimation(idleAnimation, true);
        }
    }

    protected override void InitializeStates()
    {
        // 이 클래스에서는 상태 클래스를 사용하지 않으므로 빈 메서드로 둡니다.
        // 필요한 경우 상태 초기화를 여기서 수행할 수 있습니다.
    }

    private void Update()
    {
        switch (currentState)
        {
            case BatState.Idle:
                HandleIdleState();
                break;
            case BatState.Chasing:
                HandleChaseState();
                break;
            case BatState.PreparingAttack:
                // 공격 준비 중이므로 별도 처리가 필요 없음
                break;
            case BatState.Dashing:
                HandleDashState();
                break;
            case BatState.Cooldown:
                HandleCooldownState();
                break;
        }
    }

    private void HandleIdleState()
    {
        if (IsPlayerInRange(stat.detectionRange))
        {
            currentState = BatState.Chasing;
            PlayAnimation(dashAnimation, true); // 추적 애니메이션이 없으면 필요에 따라 변경
        }
    }

    private void HandleChaseState()
    {
        if (IsPlayerInRange(stat.attackRange))
        {
            if (!isAttacking)
            {
                StartCoroutine(PrepareAndAttack());
            }
        }
        else if (IsPlayerInRange(stat.detectionRange))
        {
            MoveTowards(player.transform.position);
        }
        else
        {
            currentState = BatState.Idle;
            PlayAnimation(idleAnimation, true);
        }
    }

    private IEnumerator PrepareAndAttack()
    {
        isAttacking = true;
        currentState = BatState.PreparingAttack;
        PlayAnimation(prepareAnimation, false);

        // 1초간 공격 준비
        yield return new WaitForSeconds(1f);

        // 플레이어의 현재 위치를 기준으로 더 먼 위치 계산
        if (player == null)
        {
            currentState = BatState.Idle;
            isAttacking = false;
            yield break;
        }

        Vector3 playerPosition = player.transform.position;
        Vector3 direction = (playerPosition - transform.position).normalized;
        dashTargetPosition = playerPosition + direction * 2f; // 플레이어 위치보다 2유닛 더 먼 위치로 설정

        currentState = BatState.Dashing;
        PlayAnimation(dashAnimation, true);
    }

    private void HandleDashState()
    {
        if (player == null)
        {
            currentState = BatState.Idle;
            isAttacking = false;
            PlayAnimation(idleAnimation, true);
            return;
        }

        // 돌진 이동
        float step = stat.skillDashSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, dashTargetPosition, step);

        // 플레이어와의 거리 확인 (충돌 반경 0.5유닛 이내)
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= 0.5f)
        {
            // 자폭 공격 실행
            StartCoroutine(Explode());
            return;
        }

        // 목표 위치 도달 확인
        if (Vector3.Distance(transform.position, dashTargetPosition) < 0.1f)
        {
            // 공격 실패, 상태 재설정
            currentState = BatState.Chasing;
            isAttacking = false;
            PlayAnimation(dashAnimation, true); // 추적 애니메이션 재생
        }
    }


    private IEnumerator Explode()
    {
        currentState = BatState.Cooldown;
        PlayAnimation(explodeAnimation, true);

        yield return new WaitForSeconds(0.1f);

        // 플레이어에게 데미지 적용
        if (player != null)
        {
            player.TakeDamage(stat.attackDamage);
        }

        // 몬스터 사망 처리
        Die();
    }

    private void HandleCooldownState()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            currentState = BatState.Idle;
            isAttacking = false;
            PlayAnimation(idleAnimation, true);
        }
    }

    public override void Attack()
    {
        // 공격 로직은 PrepareAndAttack()과 Explode()에서 처리되므로 별도 구현 필요 없음
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
        // 탐지 범위와 공격 범위를 기즈모로 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stat.detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stat.attackRange);
    }
}
