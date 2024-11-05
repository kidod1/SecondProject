using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;

public class BackDancer : Monster
{
    [SerializeField]
    private GameObject danceBulletPrefab; // 새로운 공격 프리팹
    [SerializeField]
    private BackDancerMonsterData stat; // 새로운 몬스터 데이터

    [SpineAnimation] public string summonDanceAnimation;
    [SpineAnimation] public string idleDanceAnimation;
    [SpineAnimation] public string danceAttackAnimation;

    private SkeletonAnimation skeletonAnimation;

    [SerializeField] private Transform danceFirePoint; // 새로운 발사 지점

    protected override void Start()
    {
        base.Start();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            Debug.LogError("스켈레톤 애니메이션이 null입니다.");
        }
        else
        {
            StartCoroutine(SummonDanceAnimationCoroutine());
        }

        if (danceFirePoint == null)
        {
            Debug.LogWarning("공격 위치가 설정되지 않았습니다.");
            danceFirePoint = transform;
        }
    }

    private IEnumerator SummonDanceAnimationCoroutine()
    {
        PlayAnimation(summonDanceAnimation, false);
        Debug.Log("백댄서 소환 애니메이션 재생 중...");
        yield return new WaitForSpineAnimationComplete(skeletonAnimation);
        Debug.Log("백댄서 소환 애니메이션 완료");
        TransitionToState(idleState);
    }

    protected override void InitializeStates()
    {
        idleState = new BackDancerIdleState(this);
        chaseState = new BackDancerChaseState(this);
        attackState = new BackDancerAttackState(this);
        cooldownState = new BackDancerCooldownState(this);
        currentState = idleState;
    }

    public void FlipTowardsPlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("플레이어가 설정되지 않았습니다. FlipTowardsPlayer 메서드를 호출할 수 없습니다.");
            return;
        }

        if (player.transform.position.x > transform.position.x)
        {
            // 플레이어가 몬스터의 오른쪽에 있을 때
            transform.localScale = new Vector3(-0.18f, 0.18f, 1f);
        }
        else
        {
            // 플레이어가 몬스터의 왼쪽에 있을 때
            transform.localScale = new Vector3(0.18f, 0.18f, 1f);
        }
    }

    public override void Attack()
    {
        StartCoroutine(DanceAttackCoroutine());
    }

    private IEnumerator DanceAttackCoroutine()
    {
        PlayAnimation(danceAttackAnimation, false);
        yield return new WaitForSeconds(0.33f);
        FireDanceBulletWithParticle();

        yield return new WaitForSpineAnimationComplete(skeletonAnimation);
        TransitionToState(cooldownState);
    }

    private void FireDanceBulletWithParticle()
    {
        if (danceBulletPrefab == null)
        {
            Debug.LogError("DanceBullet 프리팹이 설정되지 않았습니다.");
            return;
        }

        Vector3 direction = (player.transform.position - danceFirePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject bullet = Instantiate(danceBulletPrefab, danceFirePoint.position, Quaternion.Euler(new Vector3(0, 0, -angle)));

        // 프리팹의 자식에 있는 파티클의 각도를 조정
        ParticleSystem particleSystem = bullet.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            var mainModule = particleSystem.main;
            mainModule.startRotation = Mathf.Deg2Rad * -angle + 110f; // 각도를 라디안 값으로 변환하여 설정
        }

        // 프리팹에 Rigidbody2D를 사용하여 발사
        Rigidbody2D bulletRigidbody = bullet.GetComponent<Rigidbody2D>();
        if (bulletRigidbody != null)
        {
            bulletRigidbody.velocity = direction * stat.danceSpeed;
        }

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetAttackDamage(monsterBaseStat.attackDamage);
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
                Debug.Log($"애니메이션 {animationName} 재생 시작 (Loop: {loop})");
            }
        }
    }
}

public class BackDancerIdleState : MonsterState
{
    public BackDancerIdleState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        (monster as BackDancer)?.PlayAnimation((monster as BackDancer).idleDanceAnimation, true);
        Debug.Log("백댄서 Idle 상태: Idle 애니메이션 재생 중...");
    }

    public override void UpdateState()
    {
        (monster as BackDancer)?.FlipTowardsPlayer();

        if (monster.IsPlayerInRange(monster.monsterBaseStat.detectionRange))
        {
            monster.TransitionToState(monster.chaseState);
            Debug.Log("백댄서 Idle 상태: 플레이어 감지, Chase 상태로 전환");
        }
    }

    public override void ExitState() { }
}

public class BackDancerChaseState : MonsterState
{
    public BackDancerChaseState(Monster monster) : base(monster) { }

    public override void EnterState() { }

    public override void UpdateState()
    {
        (monster as BackDancer)?.FlipTowardsPlayer();

        if (monster.IsPlayerInRange(monster.monsterBaseStat.attackRange) && !monster.isInCooldown)
        {
            monster.TransitionToState(monster.attackState);
        }
        else
        {
            monster.MoveTowards(PlayManager.I.GetPlayerPosition());
        }
    }

    public override void ExitState() { }
}

public class BackDancerAttackState : MonsterState
{
    public BackDancerAttackState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        monster.Attack();
    }

    public override void UpdateState()
    {
        (monster as BackDancer)?.FlipTowardsPlayer();

        if (!monster.IsPlayerInRange(monster.monsterBaseStat.attackRange))
        {
            monster.TransitionToState(monster.chaseState);
        }
    }

    public override void ExitState() { }
}

public class BackDancerCooldownState : MonsterState
{
    private float cooldownTimer;

    public BackDancerCooldownState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        // 쿨다운 타이머 초기화
        cooldownTimer = (monster.monsterBaseStat as BackDancerMonsterData).danceCooldown;
        monster.isInCooldown = true;

        // Idle 애니메이션 재생
        (monster as BackDancer)?.PlayAnimation((monster as BackDancer).idleDanceAnimation, true);
        Debug.Log("백댄서 쿨다운 상태: Idle 애니메이션 재생 중...");
    }

    public override void UpdateState()
    {
        // 몬스터의 방향을 플레이어 쪽으로 향하게 설정
        (monster as BackDancer)?.FlipTowardsPlayer();

        // 쿨다운 타이머 감소
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0)
        {
            monster.isInCooldown = false;
            monster.TransitionToState(monster.idleState);
            Debug.Log("백댄서 쿨다운 완료: Idle 상태로 전환");
        }
    }

    public override void ExitState() { }
}
