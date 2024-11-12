using System.Collections;
using UnityEngine;
using Spine.Unity;

public class BoomImp : Monster
{
    [SerializeField]
    private BoomImpData stat; // BoomImp 전용 몬스터 데이터

    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string boomAnimation;
    [SpineAnimation] public string boomAfterAnimation;

    private SkeletonAnimation skeletonAnimation;
    private bool isExploding = false;
    private bool hasExploded = false;

    [SerializeField]
    private GameObject explosionEffectPrefab; // 폭발 이펙트 프리팹 추가

    protected override void Start()
    {
        base.Start();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            Debug.LogError("SkeletonAnimation 컴포넌트가 없습니다.");
        }

        InitializeStates();
        TransitionToState(idleState);
    }

    protected override void InitializeStates()
    {
        idleState = new BoomImpIdleState(this);
        chaseState = new BoomImpChaseState(this);
        attackState = new BoomImpAttackState(this);
        // BoomImp는 CooldownState가 필요하지 않습니다.
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
        // Boom 애니메이션 재생
        PlayAnimation(boomAnimation, false);

        // Boom 애니메이션이 끝날 때까지 대기
        yield return new WaitForSpineAnimationComplete(skeletonAnimation);

        if (!isDead)
        {
            // 폭발 로직 실행
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        // 폭발 이펙트 생성
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 폭발 반경 내의 플레이어에게 데미지 적용
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

        // BoomAfter 애니메이션 재생
        PlayAnimation(boomAfterAnimation, false);

        // BoomAfter 애니메이션이 끝나면 Die 메서드 호출
        StartCoroutine(WaitAndDieCoroutine());
    }

    private IEnumerator WaitAndDieCoroutine()
    {
        yield return new WaitForSpineAnimationComplete(skeletonAnimation);

        // Die 메서드 호출
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

        // BoomImp가 폭발하기 전에 죽으면 폭발을 취소
        if (currentHP <= 0 && isExploding && !hasExploded)
        {
            // 폭발 코루틴 중지
            StopAllCoroutines();
            isExploding = false;
            // 필요한 경우 죽는 애니메이션 재생 가능
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
