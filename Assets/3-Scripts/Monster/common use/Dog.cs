using Spine.Unity;
using UnityEngine;
using System.Collections;
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
            Debug.LogError("���̷��� �ִϸ��̼� is null");
        }
        else
        {
            DogAnimationPlay(idleAnimation, true);
        }
    }

    protected override void InitializeStates()
    {
        idleState = new IdleState(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
        cooldownState = new DogCooldownState(this);
        currentState = idleState;
        currentState.EnterState();
    }

    public override void Attack()
    {
        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        DogAnimationPlay(attackAnimation, false);
        if (Vector3.Distance(transform.position, player.transform.position) <= monsterBaseStat.attackRange)
        {
            player.TakeDamage(monsterBaseStat.attackDamage);
        }
        yield return new WaitForSeconds(0.5f);
        TransitionToState(cooldownState);
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(monsterBaseStat.attackDamage);
                TransitionToState(cooldownState);
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
        FlipDirection();

        if (skeletonAnimation != null && currentState == cooldownState)
        {
            skeletonAnimation.AnimationName = null;
        }
    }

    private void FlipDirection()
    {
        var players = PlayManager.I.GetPlayer();
        if (PlayManager.I.GetPlayer() != null)
        {
            Vector3 directionToPlayer = players.transform.position - transform.position;
            if (directionToPlayer.x > 0 && transform.localScale.x > 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (directionToPlayer.x < 0 && transform.localScale.x < 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    private void OnEnable()
    {
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
        (monster as Dog)?.DogAnimationPlay((monster as Dog)?.idleAnimation, true);
    }
}
