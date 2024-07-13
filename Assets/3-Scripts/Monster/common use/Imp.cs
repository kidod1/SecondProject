using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;

public class Imp : Monster
{
    public GameObject bulletPrefab;
    [SerializeField]
    private ImpMonsterData stat;

    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string attackAnimation;

    private SkeletonAnimation skeletonAnimation;

    [SerializeField] private Transform firePoint; // 총알 발사 피벗 위치

    protected override void Start()
    {
        base.Start();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            Debug.LogError("SkeletonAnimation component is missing.");
        }
        else
        {
            PlayAnimation(idleAnimation, true); // Start 시에 Idle 애니메이션 재생
        }

        // firePoint가 설정되지 않은 경우 기본 위치로 초기화
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint is not set. Using default position.");
            firePoint = transform;
        }
    }

    protected override void InitializeStates()
    {
        idleState = new ImpIdleState(this);
        attackState = new ImpAttackState(this);
        cooldownState = new ImpCooldownState(this);
        currentState = idleState;
        currentState.EnterState();
    }

    public override void Attack()
    {
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        PlayAnimation(attackAnimation, false); // 공격 애니메이션 재생
        FireBullet();
        yield return new WaitForSpineAnimationComplete(skeletonAnimation); // 공격 애니메이션이 끝날 때까지 대기
        PlayAnimation(idleAnimation, true); // Idle 애니메이션 재생
        TransitionToState(cooldownState); // 쿨다운 상태로 전환
    }

    private void FireBullet()
    {
        Vector3 direction = (player.transform.position - firePoint.position).normalized; // firePoint 기준으로 방향 계산
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity); // firePoint 위치에서 총알 발사
        bullet.GetComponent<Rigidbody2D>().velocity = direction * stat.attackSpeed;

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetAttackDamage(monsterBaseStat.attackDamage);
        }
    }

    public void PlayAnimation(string animationName, bool loop)
    {
        if (skeletonAnimation != null && !string.IsNullOrEmpty(animationName))
        {
            skeletonAnimation.state.SetAnimation(0, animationName, loop);
        }
    }
}

public class ImpIdleState : MonsterState
{
    public ImpIdleState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        Debug.Log("Entering Idle State");
        (monster as Imp)?.PlayAnimation((monster as Imp).idleAnimation, true);
    }

    public override void UpdateState()
    {
        if (monster.IsPlayerInRange(monster.monsterBaseStat.attackRange) && !monster.isInCooldown)
        {
            monster.TransitionToState(monster.attackState);
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Idle State");
    }
}

public class ImpAttackState : MonsterState
{
    public ImpAttackState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        monster.Attack();
    }

    public override void UpdateState()
    {
        if (!monster.IsPlayerInRange(monster.monsterBaseStat.attackRange))
        {
            monster.TransitionToState(monster.idleState);
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Attack State");
        (monster as Imp)?.PlayAnimation((monster as Imp).idleAnimation, true); // 공격 상태를 빠져나올 때 Idle 애니메이션 재생
    }
}

public class ImpCooldownState : MonsterState
{
    private float cooldownTimer;

    public ImpCooldownState(Monster monster) : base(monster)
    {
    }

    public override void EnterState()
    {
        cooldownTimer = monster.monsterBaseStat.attackDelay;
        monster.isInCooldown = true;
        Debug.Log("Entering Cooldown State");
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

    public override void ExitState()
    {
        Debug.Log("Exiting Cooldown State");
    }
}

public class WaitForSpineAnimationComplete : CustomYieldInstruction
{
    private readonly SkeletonAnimation skeletonAnimation;

    public WaitForSpineAnimationComplete(SkeletonAnimation skeletonAnimation)
    {
        this.skeletonAnimation = skeletonAnimation;
    }

    public override bool keepWaiting
    {
        get
        {
            if (skeletonAnimation == null || skeletonAnimation.state == null) return false;
            TrackEntry currentTrack = skeletonAnimation.state.GetCurrent(0);
            return currentTrack != null && !currentTrack.IsComplete;
        }
    }
}
