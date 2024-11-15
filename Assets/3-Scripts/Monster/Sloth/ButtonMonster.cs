using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;
using AK.Wwise; // Wwise 네임스페이스 추가

public class ButtonMonster : Monster
{
    [SerializeField]
    private ButtonMonsterData stat;

    [SerializeField]
    private ParticleSystem attackEffect; // GameObject에서 ParticleSystem으로 변경

    [SpineAnimation] public string summonAnimation;
    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string attackAnimation;

    private SkeletonAnimation skeletonAnimation;

    // 추가된 Wwise 이벤트 필드
    [SerializeField]
    private AK.Wwise.Event attackSoundEvent; // 공격 사운드 이벤트 참조

    private uint attackSoundPlayingID = 0; // 현재 재생 중인 공격 사운드의 ID

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

            // 공격 사운드 재생
            PlayAttackSound();
            // 파티클 시스템 인스턴스화 및 재생
            ParticleSystem effectInstance = Instantiate(attackEffect, transform.position, Quaternion.identity, transform);
            effectInstance.Play();

            // 파티클 재생이 끝난 후 파괴
            Destroy(effectInstance.gameObject, effectInstance.main.duration);

            // 데미지 입히기
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
    /// 공격 사운드를 재생하는 메서드
    /// </summary>
    private void PlayAttackSound()
    {
        if (attackSoundEvent != null)
        {
            // 사운드 이벤트를 재생하고 재생 ID를 저장
            attackSoundPlayingID = attackSoundEvent.Post(gameObject, (uint)AkCallbackType.AK_EndOfEvent, OnAttackSoundEnd);
        }
        else
        {
            Debug.LogWarning("Attack sound event is not assigned.");
        }
    }

    /// <summary>
    /// 공격 사운드가 끝났을 때 호출되는 콜백 메서드
    /// </summary>
    /// <param name="in_cookie">사용자 데이터</param>
    /// <param name="in_type">콜백 타입</param>
    /// <param name="in_info">추가 정보</param>
    private void OnAttackSoundEnd(object in_cookie, AkCallbackType in_type, object in_info)
    {
        if (in_type == AkCallbackType.AK_EndOfEvent)
        {
            Debug.Log("Attack sound has ended.");
            attackSoundPlayingID = 0; // 재생 ID 초기화
            // 추가적인 동작을 원한다면 여기서 수행
        }
    }

    /// <summary>
    /// 공격 사운드를 정지시키는 메서드
    /// </summary>
    public void StopAttackSound()
    {
        if (attackSoundEvent != null && attackSoundPlayingID != 0)
        {
            attackSoundEvent.Stop(gameObject);
            attackSoundPlayingID = 0; // 재생 ID 초기화
            Debug.Log("Attack sound has been stopped.");
        }
        else
        {
            Debug.LogWarning("Attack sound event is not assigned or not playing.");
        }
    }

    private void ExecuteAttack()
    {
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
