using System.Collections;
using UnityEngine;
using Spine.Unity;

public class Bat : Monster
{
    [SerializeField]
    private BatMonsterData stat;

    [SpineAnimation] public string idleAnimation;

    private SkeletonAnimation skeletonAnimation;

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
    }

    protected override void InitializeStates()
    {
        idleState = new IdleState(this);
        chaseState = new ChaseState(this);
        attackState = new ExplodeAttackState(this);
        cooldownState = new CooldownState(this);
        currentState = idleState;
        currentState.EnterState();
    }

    private void Update()
    {
        currentState?.UpdateState();
    }

    public override void Attack()
    {
        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        Debug.Log("플레이어에게 접근 중");

        while (Vector3.Distance(transform.position, player.transform.position) > 0.5f)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            transform.position += direction * stat.skillDashSpeed * Time.deltaTime;
            yield return null;
        }

        Debug.Log("자폭!");
        player.TakeDamage(stat.attackDamage);

        Die();
    }

    public void PlayAnimation(string animationName, bool loop)
    {
        if (skeletonAnimation != null && !string.IsNullOrEmpty(animationName))
        {
            skeletonAnimation.state.SetAnimation(0, animationName, loop);
        }
    }
}

public class ExplodeAttackState : MonsterState
{
    public ExplodeAttackState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        monster.Attack();
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Attack State");
    }
}
