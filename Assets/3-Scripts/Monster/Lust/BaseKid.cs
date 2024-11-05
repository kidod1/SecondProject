using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;

public class BaseKid : Monster
{
    [SerializeField]
    private BaseMonsterData stat; // 몬스터 데이터 스크립터블 오브젝트

    [SerializeField]
    private ParticleSystem attackEffect; // 공격 이펙트 파티클 시스템

    [SpineAnimation] public string summonAnimation;
    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string attackAnimation;

    private SkeletonAnimation skeletonAnimation;

    // 이펙트 위치를 조정할 수 있는 변수 추가
    [Header("Effect Settings")]
    public Vector3 effectPositionOffset = Vector3.zero;

    protected override void Start()
    {
        base.Start();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            Debug.LogError("Skeleton Animation is null");
        }
        else
        {
            StartCoroutine(SummonAnimationCoroutine());
        }
    }

    private IEnumerator SummonAnimationCoroutine()
    {
        PlayAnimation(summonAnimation, false);
        yield return new WaitForSpineAnimationComplete(skeletonAnimation);
        TransitionToState(idleState);
    }

    protected override void InitializeStates()
    {
        idleState = new BaseKidIdleState(this);
        chaseState = new BaseKidChaseState(this);
        attackState = new BaseKidAttackState(this);
        cooldownState = new BaseKidCooldownState(this);
        currentState = idleState;
    }

    public override void Attack()
    {
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        PlayAnimation(attackAnimation, false);

        if (attackEffect != null)
        {
            yield return new WaitForSeconds(0.3332f); // 애니메이션 타이밍에 맞게 조정

            // 이펙트를 조정된 위치에 생성
            Vector3 effectPosition = transform.position + effectPositionOffset;
            ParticleSystem effectInstance = Instantiate(attackEffect, effectPosition, Quaternion.identity, transform);
            effectInstance.Play();

            // 파티클 재생이 끝난 후 파괴
            Destroy(effectInstance.gameObject, effectInstance.main.duration);

            // 데미지 입히기
            yield return new WaitForSeconds(0.2f);
            ExecuteAttack();
        }
        else
        {
            Debug.LogWarning("Attack effect is not assigned.");
        }

        yield return new WaitForSpineAnimationComplete(skeletonAnimation);
        skeletonAnimation.Initialize(true);
        PlayAnimation(idleAnimation, false);
        TransitionToState(cooldownState);
    }

    private void ExecuteAttack()
    {
        // 공격 범위 내의 적에게 데미지를 입히는 로직
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, stat.attackRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                Player player = hitCollider.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage(monsterBaseStat.attackDamage);
                }
            }
        }
    }

    public void PlayAnimation(string animationName, bool loop)
    {
        if (skeletonAnimation != null && !string.IsNullOrEmpty(animationName))
        {
            var currentTrackEntry = skeletonAnimation.state.GetCurrent(0);
            if (currentTrackEntry == null || currentTrackEntry.Animation.Name != animationName)
            {
                skeletonAnimation.state.SetAnimation(0, animationName, loop);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 공격 범위를 기즈모로 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stat.attackRange);

        // 이펙트 위치를 기즈모로 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + effectPositionOffset, 0.2f);
    }
}

public class BaseKidIdleState : MonsterState
{
    public BaseKidIdleState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        (monster as BaseKid)?.PlayAnimation((monster as BaseKid).idleAnimation, true);
    }

    public override void UpdateState()
    {
        if (monster.IsPlayerInRange(monster.monsterBaseStat.detectionRange))
        {
            monster.TransitionToState(monster.chaseState);
        }
    }

    public override void ExitState() { }
}

public class BaseKidChaseState : MonsterState
{
    public BaseKidChaseState(Monster monster) : base(monster) { }

    public override void EnterState() { }

    public override void UpdateState()
    {
        if (monster.IsPlayerInRange(monster.monsterBaseStat.attackRange) && !monster.isInCooldown)
        {
            monster.TransitionToState(monster.attackState);
        }
        else
        {
            monster.MoveTowards(monster.player.transform.position);
        }
    }

    public override void ExitState() { }
}

public class BaseKidAttackState : MonsterState
{
    public BaseKidAttackState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        monster.Attack();
    }

    public override void UpdateState()
    {
        // 공격 애니메이션과 공격이 끝나면 쿨다운 상태로 전환
    }

    public override void ExitState() { }
}

public class BaseKidCooldownState : MonsterState
{
    private float cooldownTimer;

    public BaseKidCooldownState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        cooldownTimer = (monster.monsterBaseStat as BaseMonsterData).attackDelay;
        monster.isInCooldown = true;
    }

    public override void UpdateState()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0)
        {
            monster.isInCooldown = false;
            monster.TransitionToState(monster.idleState);
        }
    }

    public override void ExitState() { }
}
