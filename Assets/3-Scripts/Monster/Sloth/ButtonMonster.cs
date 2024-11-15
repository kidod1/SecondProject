using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;
using AK.Wwise; // Wwise ���ӽ����̽� �߰�

public class ButtonMonster : Monster
{
    [SerializeField]
    private ButtonMonsterData stat;

    [SerializeField]
    private ParticleSystem attackEffect; // GameObject���� ParticleSystem���� ����

    [SpineAnimation] public string summonAnimation;
    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string attackAnimation;

    private SkeletonAnimation skeletonAnimation;

    // �߰��� Wwise �̺�Ʈ �ʵ�
    [SerializeField]
    private AK.Wwise.Event attackSoundEvent; // ���� ���� �̺�Ʈ ����

    private uint attackSoundPlayingID = 0; // ���� ��� ���� ���� ������ ID

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

        if (attackEffect != null)
        {
            yield return new WaitForSeconds(0.72f);

            // ���� ���� ���
            PlayAttackSound();
            // ��ƼŬ �ý��� �ν��Ͻ�ȭ �� ���
            ParticleSystem effectInstance = Instantiate(attackEffect, transform.position, Quaternion.identity, transform);
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

    /// <summary>
    /// ���� ���带 ����ϴ� �޼���
    /// </summary>
    private void PlayAttackSound()
    {
        if (attackSoundEvent != null)
        {
            // ���� �̺�Ʈ�� ����ϰ� ��� ID�� ����
            attackSoundPlayingID = attackSoundEvent.Post(gameObject, (uint)AkCallbackType.AK_EndOfEvent, OnAttackSoundEnd);
        }
        else
        {
            Debug.LogWarning("Attack sound event is not assigned.");
        }
    }

    /// <summary>
    /// ���� ���尡 ������ �� ȣ��Ǵ� �ݹ� �޼���
    /// </summary>
    /// <param name="in_cookie">����� ������</param>
    /// <param name="in_type">�ݹ� Ÿ��</param>
    /// <param name="in_info">�߰� ����</param>
    private void OnAttackSoundEnd(object in_cookie, AkCallbackType in_type, object in_info)
    {
        if (in_type == AkCallbackType.AK_EndOfEvent)
        {
            Debug.Log("Attack sound has ended.");
            attackSoundPlayingID = 0; // ��� ID �ʱ�ȭ
            // �߰����� ������ ���Ѵٸ� ���⼭ ����
        }
    }

    /// <summary>
    /// ���� ���带 ������Ű�� �޼���
    /// </summary>
    public void StopAttackSound()
    {
        if (attackSoundEvent != null && attackSoundPlayingID != 0)
        {
            attackSoundEvent.Stop(gameObject);
            attackSoundPlayingID = 0; // ��� ID �ʱ�ȭ
            Debug.Log("Attack sound has been stopped.");
        }
        else
        {
            Debug.LogWarning("Attack sound event is not assigned or not playing.");
        }
    }

    private void ExecuteAttack()
    {
        // ���� ���� ������ �������� ������ ����
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
