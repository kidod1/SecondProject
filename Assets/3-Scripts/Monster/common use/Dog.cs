using System.Collections;
using UnityEngine;
using Spine.Unity;

public class Dog : Monster
{
    [SerializeField]
    private DogMonsterData stat;

    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string attackAnimation;

    private SkeletonAnimation skeletonAnimation;

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
            DogAnimationPlay(idleAnimation, true); // Start 시에 Idle 애니메이션 재생
        }
    }

    protected override void InitializeStates()
    {
        idleState = new IdleState(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
        cooldownState = new DogCooldownState(this); // DogCooldownState 사용
        currentState = idleState;
        currentState.EnterState();
    }

    public override void Attack()
    {
        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        Debug.Log("플레이어에게 공격");
        DogAnimationPlay(attackAnimation, false);
        // 플레이어에게 닿으면 데미지를 입힘
        if (Vector3.Distance(transform.position, player.transform.position) <= monsterBaseStat.attackRange)
        {
            player.TakeDamage(monsterBaseStat.attackDamage);
        }
        yield return new WaitForSeconds(0.1f); // 짧은 시간 대기하여 공격 애니메이션이 보여지도록 함
        TransitionToState(cooldownState); // 공격 후 쿨다운 상태로 전환
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(monsterBaseStat.attackDamage);
                TransitionToState(cooldownState); // 공격 후 쿨다운 상태로 전환
            }
        }
    }

    public void DogAnimationPlay(string animationName, bool loop)
    {
        if (skeletonAnimation != null && !string.IsNullOrEmpty(animationName))
        {
            skeletonAnimation.state.SetAnimation(0, animationName, loop);
        }
    }

    private void Update()
    {
        currentState?.UpdateState();

        // 쿨다운 상태일 때만 애니메이션을 멈춤
        if (skeletonAnimation != null && currentState == cooldownState)
        {
            skeletonAnimation.AnimationName = null;
        }
    }

    private void OnEnable()
    {
        // 쿨다운 상태가 끝난 후 다시 Idle 애니메이션 재생
        currentState?.EnterState();
    }
}

public class DogCooldownState : MonsterState
{
    private float cooldownTimer;

    public DogCooldownState(Monster monster) : base(monster) { }

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
        (monster as Dog)?.DogAnimationPlay((monster as Dog)?.idleAnimation, true); // Idle 애니메이션 재생
    }
}
