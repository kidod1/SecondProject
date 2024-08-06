using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;

public class ButtonMonster : Monster
{
    [SerializeField]
    private ButtonMonsterData stat;
    [SerializeField]
    private GameObject attackEffect;
    [SpineAnimation] public string summonAnimation;
    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string attackAnimation;

    private SkeletonAnimation skeletonAnimation;

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
        idleState = new ButtonIdleState(this);
        chaseState = new ButtonChaseState(this);
        attackState = new ButtonAttackState(this);
        cooldownState = new ButtonCooldownState(this);
        currentState = idleState;
    }

    public override void Attack()
    {
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        PlayAnimation(attackAnimation, false);
        yield return new WaitForSpineAnimationComplete(skeletonAnimation);
        ExecuteAttack();
        TransitionToState(cooldownState);
    }

    private void ExecuteAttack()
    {
        if (attackEffect != null)
        {
            GameObject effect = Instantiate(attackEffect, transform.position, Quaternion.identity, transform);
            effect.SetActive(true);
            // attackEffect의 애니메이션이 끝난 후 비활성화
            StartCoroutine(DeactivateAfterAnimation(effect));
        }

        // 범위 내의 적에게 데미지를 입히는 로직
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, stat.buttonMosnterAttackRange);
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

    private IEnumerator DeactivateAfterAnimation(GameObject effect)
    {
        yield return new WaitForSeconds(1f);
        effect.SetActive(false);
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
        // 공격 범위를 기즈모로 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stat.buttonMosnterAttackRange);
    }
}

public class ButtonIdleState : MonsterState
{
    public ButtonIdleState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        (monster as ButtonMonster)?.PlayAnimation((monster as ButtonMonster).idleAnimation, true);
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

public class ButtonChaseState : MonsterState
{
    public ButtonChaseState(Monster monster) : base(monster) { }

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

public class ButtonAttackState : MonsterState
{
    public ButtonAttackState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        monster.Attack();
    }

    public override void UpdateState()
    {
        if (!monster.IsPlayerInRange(monster.monsterBaseStat.attackRange))
        {
            monster.TransitionToState(monster.chaseState);
        }
    }

    public override void ExitState() { }
}

public class ButtonCooldownState : MonsterState
{
    private float cooldownTimer;

    public ButtonCooldownState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        cooldownTimer = (monster.monsterBaseStat as ButtonMonsterData).attackDelay;
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
