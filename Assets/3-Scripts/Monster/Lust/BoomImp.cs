using System.Collections;
using UnityEngine;
using Spine.Unity;

public class BoomImp : Monster
{
    [SerializeField]
    private BoomImpData stat; // BoomImp ���� ���� ������

    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string boomAnimation;
    [SpineAnimation] public string boomAfterAnimation;

    private SkeletonAnimation skeletonAnimation;
    private bool isExploding = false;
    private bool hasExploded = false;

    [SerializeField]
    private GameObject explosionEffectPrefab; // ���� ����Ʈ ������ �߰�

    protected override void Start()
    {
        base.Start();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            Debug.LogError("SkeletonAnimation ������Ʈ�� �����ϴ�.");
        }

        InitializeStates();
        TransitionToState(idleState);
    }

    protected override void InitializeStates()
    {
        idleState = new BoomImpIdleState(this);
        chaseState = new BoomImpChaseState(this);
        attackState = new BoomImpAttackState(this);
        // BoomImp�� CooldownState�� �ʿ����� �ʽ��ϴ�.
        currentState = idleState;
    }

    public override void Attack()
    {
        if (isDead || isExploding) return;

        isExploding = true;
        StartCoroutine(ExplosionCoroutine());
    }

    private IEnumerator ExplosionCoroutine()
    {
        // Boom �ִϸ��̼� ���
        PlayAnimation(boomAnimation, false);

        // Boom �ִϸ��̼��� ���� ������ ���
        yield return new WaitForSpineAnimationComplete(skeletonAnimation);

        if (!isDead)
        {
            // ���� ���� ����
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        // ���� ����Ʈ ����
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // ���� �ݰ� ���� �÷��̾�� ������ ����
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, stat.explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                Player player = hitCollider.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage(stat.explosionDamage);
                }
            }
        }

        // BoomAfter �ִϸ��̼� ���
        PlayAnimation(boomAfterAnimation, false);

        // BoomAfter �ִϸ��̼��� ������ Die �޼��� ȣ��
        StartCoroutine(WaitAndDieCoroutine());
    }

    private IEnumerator WaitAndDieCoroutine()
    {
        yield return new WaitForSpineAnimationComplete(skeletonAnimation);

        // Die �޼��� ȣ��
        Die();
    }

    public void PlayAnimation(string animationName, bool loop)
    {
        if (skeletonAnimation != null && !string.IsNullOrEmpty(animationName))
        {
            skeletonAnimation.state.SetAnimation(0, animationName, loop);
        }
    }

    public override void TakeDamage(int damage, Vector3 damageSourcePosition, bool deathAbilityKill = false)
    {
        if (isDead) return;

        base.TakeDamage(damage, damageSourcePosition, deathAbilityKill);

        // BoomImp�� �����ϱ� ���� ������ ������ ���
        if (currentHP <= 0 && isExploding && !hasExploded)
        {
            // ���� �ڷ�ƾ ����
            StopAllCoroutines();
            isExploding = false;
            // �ʿ��� ��� �״� �ִϸ��̼� ��� ����
            Die();
        }
    }
}

public class BoomImpIdleState : MonsterState
{
    public BoomImpIdleState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        (monster as BoomImp)?.PlayAnimation((monster as BoomImp).idleAnimation, true);
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
public class BoomImpChaseState : MonsterState
{
    public BoomImpChaseState(Monster monster) : base(monster) { }

    public override void EnterState() { }

    public override void UpdateState()
    {
        // Move towards the player
        monster.MoveTowards(monster.player.transform.position);

        // Check if within attack range
        if (monster.IsPlayerInRange(monster.monsterBaseStat.attackRange))
        {
            monster.TransitionToState(monster.attackState);
        }
    }

    public override void ExitState() { }
}
public class BoomImpAttackState : MonsterState
{
    public BoomImpAttackState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        monster.Attack();
    }

    public override void UpdateState()
    {
        // Do nothing during attack state
    }

    public override void ExitState() { }
}
