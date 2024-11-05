using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;

public class BaseKid : Monster
{
    [SerializeField]
    private BaseMonsterData stat; // ���� ������ ��ũ���ͺ� ������Ʈ

    [SerializeField]
    private ParticleSystem attackEffect; // ���� ����Ʈ ��ƼŬ �ý���

    [SpineAnimation] public string summonAnimation;
    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string attackAnimation;

    private SkeletonAnimation skeletonAnimation;

    // ����Ʈ ��ġ�� ������ �� �ִ� ���� �߰�
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
            yield return new WaitForSeconds(0.3332f); // �ִϸ��̼� Ÿ�ֿ̹� �°� ����

            // ����Ʈ�� ������ ��ġ�� ����
            Vector3 effectPosition = transform.position + effectPositionOffset;
            ParticleSystem effectInstance = Instantiate(attackEffect, effectPosition, Quaternion.identity, transform);
            effectInstance.Play();

            // ��ƼŬ ����� ���� �� �ı�
            Destroy(effectInstance.gameObject, effectInstance.main.duration);

            // ������ ������
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
        // ���� ���� ���� ������ �������� ������ ����
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
        // ���� ������ ������ ǥ��
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stat.attackRange);

        // ����Ʈ ��ġ�� ������ ǥ��
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
        // ���� �ִϸ��̼ǰ� ������ ������ ��ٿ� ���·� ��ȯ
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
