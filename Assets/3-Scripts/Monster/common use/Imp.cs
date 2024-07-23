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

    [SerializeField] private Transform firePoint;

    protected override void Start()
    {
        base.Start();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            Debug.LogError("스켈레톤 애니메이션 is null");
        }
        else
        {
            PlayAnimation(idleAnimation, true);
        }

        if (firePoint == null)
        {
            Debug.LogWarning("공격 위치가 설정되지 않았습니다.");
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
        PlayAnimation(attackAnimation, false);
        FireBullet();
        yield return new WaitForSpineAnimationComplete(skeletonAnimation);
        PlayAnimation(idleAnimation, true);
        TransitionToState(cooldownState);
    }

    private void FireBullet()
    {
        Vector3 direction = (player.transform.position - firePoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
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
        (monster as Imp)?.PlayAnimation((monster as Imp).idleAnimation, true);
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
