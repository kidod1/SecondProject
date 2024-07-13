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
            DogAnimationPlay(idleAnimation, true); // Start �ÿ� Idle �ִϸ��̼� ���
        }
    }

    protected override void InitializeStates()
    {
        idleState = new IdleState(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
        cooldownState = new DogCooldownState(this); // DogCooldownState ���
        currentState = idleState;
        currentState.EnterState();
    }

    public override void Attack()
    {
        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        Debug.Log("�÷��̾�� ����");
        DogAnimationPlay(attackAnimation, false);
        // �÷��̾�� ������ �������� ����
        if (Vector3.Distance(transform.position, player.transform.position) <= monsterBaseStat.attackRange)
        {
            player.TakeDamage(monsterBaseStat.attackDamage);
        }
        yield return new WaitForSeconds(0.1f); // ª�� �ð� ����Ͽ� ���� �ִϸ��̼��� ���������� ��
        TransitionToState(cooldownState); // ���� �� ��ٿ� ���·� ��ȯ
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(monsterBaseStat.attackDamage);
                TransitionToState(cooldownState); // ���� �� ��ٿ� ���·� ��ȯ
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

        // ��ٿ� ������ ���� �ִϸ��̼��� ����
        if (skeletonAnimation != null && currentState == cooldownState)
        {
            skeletonAnimation.AnimationName = null;
        }
    }

    private void OnEnable()
    {
        // ��ٿ� ���°� ���� �� �ٽ� Idle �ִϸ��̼� ���
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
        (monster as Dog)?.DogAnimationPlay((monster as Dog)?.idleAnimation, true); // Idle �ִϸ��̼� ���
    }
}
