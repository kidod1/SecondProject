using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;
using AK.Wwise; // Wwise 네임스페이스 추가

public class Drone : Monster
{
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private DroneMonsterData stat;

    [SpineAnimation] public string summonAnimation;
    [SpineAnimation] public string idleMoveAnimation;
    [SpineAnimation] public string attackAnimation;

    private SkeletonAnimation skeletonAnimation;

    [SerializeField] private Transform firePoint;

    // 추가된 Wwise 이벤트 필드
    [SerializeField]
    private AK.Wwise.Event attackSoundEvent; // 공격 사운드 이벤트 참조

    private uint attackSoundPlayingID = 0; // 현재 재생 중인 공격 사운드의 ID

    protected override void Start()
    {
        base.Start();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        skeletonAnimation.Initialize(true);
        if (skeletonAnimation == null)
        {
            Debug.LogError("스켈레톤 애니메이션 is null");
        }
        else
        {
            StartCoroutine(SummonAnimationCoroutine());
        }

        if (firePoint == null)
        {
            Debug.LogWarning("공격 위치가 설정되지 않았습니다.");
            firePoint = transform;
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
        idleState = new DroneIdleState(this);
        chaseState = new DroneChaseState(this);
        attackState = new DroneAttackState(this);
        cooldownState = new DroneCooldownState(this);
        currentState = idleState;
    }

    public override void Attack()
    {
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        PlayAnimation(attackAnimation, false);

        // 공격 사운드 재생
        PlayAttackSound();

        FireBulletWithParticle();
        TransitionToState(cooldownState);
        yield return new WaitForSpineAnimationComplete(skeletonAnimation);
        skeletonAnimation.Initialize(true);
        PlayAnimation(idleMoveAnimation, false);
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

    private void FireBulletWithParticle()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet 프리팹이 설정되지 않았습니다.");
            return;
        }

        Vector3 direction = (player.transform.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(new Vector3(0, 0, -angle)));

        // 프리팹의 자식에 있는 파티클의 각도를 조정
        ParticleSystem particleSystem = bullet.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            var mainModule = particleSystem.main;
            mainModule.startRotation = Mathf.Deg2Rad * -angle; // 각도를 라디안 값으로 변환하여 설정
        }

        // 프리팹에 Rigidbody2D를 사용하여 발사
        Rigidbody2D bulletRigidbody = bullet.GetComponent<Rigidbody2D>();
        if (bulletRigidbody != null)
        {
            bulletRigidbody.velocity = direction * stat.attackSpeed;
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
            }
        }
    }
}

public class DroneIdleState : MonsterState
{
    public DroneIdleState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        (monster as Drone)?.PlayAnimation((monster as Drone).idleMoveAnimation, true);
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

public class DroneChaseState : MonsterState
{
    public DroneChaseState(Monster monster) : base(monster) { }

    public override void EnterState() { }

    public override void UpdateState()
    {
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

public class DroneAttackState : MonsterState
{
    public DroneAttackState(Monster monster) : base(monster) { }

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

public class DroneCooldownState : MonsterState
{
    private float cooldownTimer;

    public DroneCooldownState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        cooldownTimer = (monster.monsterBaseStat as DroneMonsterData).attackCooldown;
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
