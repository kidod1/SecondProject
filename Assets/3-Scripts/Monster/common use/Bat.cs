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
            Debug.LogError("SkeletonAnimation component is missing.");
        }
        else
        {
            PlayAnimation(idleAnimation, true); // Start 시에 Idle 애니메이션 재생
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

        // 자폭하여 플레이어에게 피해를 줌
        Debug.Log("자폭!");
        player.TakeDamage(stat.attackDamage);

        // 몬스터를 파괴
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
        // 자폭할 때는 다른 상태로 전환하지 않음
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Attack State");
    }
}
