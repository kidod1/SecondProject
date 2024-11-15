using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;
using AK.Wwise; // Wwise 네임스페이스 추가

public class Turret : Monster
{
    public GameObject bulletPrefab;
    [SerializeField]
    private TurretMonsterData stat;

    [SpineAnimation] public string summonAnimation;
    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string frontAttackAnimation;
    [SpineAnimation] public string backAttackAnimation;
    [SpineAnimation] public string sideAttackAnimation;
    [SpineAnimation] public string reloadAnimation;
    [SpineAnimation] public string reloadIdleAnimation;

    protected SkeletonAnimation skeletonAnimation;
    public SkeletonAnimation GetSkeletonAnimation()
    {
        return skeletonAnimation;
    }

    [SerializeField] private Transform[] firePoints;
    [SerializeField] private int bulietQuantity = 4;
    private int currentFirePoint = 0;
    private int attackCount = 0;
    public bool isAttacking = false;

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

        if (firePoints == null || firePoints.Length < 2)
        {
            Debug.LogError("Fire Points가 설정되지 않았거나 부족합니다.");
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
        idleState = new TurretIdleState(this);
        chaseState = new TurretChaseState(this);
        attackState = new TurretAttackState(this);
        cooldownState = new TurretCooldownState(this);
        currentState = idleState;
    }

    public override void Attack()
    {
        if (!isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        UpdateSkinAndAnimation();


        for (int i = 0; i < bulietQuantity; i++)
        {
            FireBullet();
            PlayAttackSound();
            yield return new WaitForSeconds(0.29175f);
            attackCount++;
        }

        yield return new WaitForSpineAnimationComplete(skeletonAnimation);

        if (attackCount >= bulietQuantity)
        {
            attackCount = 0;
            PlayAnimation(reloadAnimation, false);
            yield return new WaitForSpineAnimationComplete(skeletonAnimation);
            PlayAnimation(reloadIdleAnimation, true);
            isAttacking = false;
            TransitionToState(cooldownState);
        }
        else
        {
            isAttacking = false;
            TransitionToState(idleState);
        }
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

    public void ClearCurrentSkin()
    {
        if (skeletonAnimation != null)
        {
            skeletonAnimation.state.ClearTracks();
            skeletonAnimation.Skeleton.SetSlotsToSetupPose();
            skeletonAnimation.Initialize(true);
        }
    }

    private void UpdateSkinAndAnimation()
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        string skinName;
        string attackAnimationName;

        // 스킨 변경 전에 현재 슬롯 상태를 초기화
        ClearCurrentSkin();

        if (Mathf.Abs(directionToPlayer.y) > Mathf.Abs(directionToPlayer.x))
        {
            if (directionToPlayer.y > 0)
            {
                skinName = "back";
                attackAnimationName = backAttackAnimation;
            }
            else
            {
                skinName = "front";
                attackAnimationName = frontAttackAnimation;

                if (isElite)
                {
                    transform.localScale = new Vector3(0.6f, 0.6f, 1);
                    damageArea.transform.localScale = new Vector3(2.5f, 2.5f, 1);
                }
                else
                {
                    transform.localScale = new Vector3(0.3f, 0.3f, 1);
                    damageArea.transform.localScale = new Vector3(1.2f, 1.2f, 1);
                }
            }
        }
        else
        {
            skinName = "side";
            attackAnimationName = sideAttackAnimation;

            if (isElite)
            {
                if (directionToPlayer.x > 0)
                {
                    transform.localScale = new Vector3(-0.6f, 0.6f, 1);
                    damageArea.transform.localScale = new Vector3(-2.5f, 2.5f, 1);
                }
                else
                {
                    transform.localScale = new Vector3(0.6f, 0.6f, 1);
                    damageArea.transform.localScale = new Vector3(2.5f, 2.5f, 1);
                }
            }
            else
            {
                if (directionToPlayer.x > 0)
                {
                    transform.localScale = new Vector3(-0.3f, 0.3f, 1);
                    damageArea.transform.localScale = new Vector3(-1.2f, 1.2f, 1);
                }
                else
                {
                    transform.localScale = new Vector3(0.3f, 0.3f, 1);
                    damageArea.transform.localScale = new Vector3(1.2f, 1.2f, 1);
                }
            }
        }
        skeletonAnimation.Skeleton.SetSkin(skinName);
        PlayAnimation(attackAnimationName, false);
    }


    private void FireBullet()
    {
        Vector3 direction = (player.transform.position - firePoints[currentFirePoint].position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoints[currentFirePoint].position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        bulletRb.velocity = direction * stat.attackSpeed;

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetAttackDamage(monsterBaseStat.attackDamage);
            bulletScript.SetSourceMonster(this);
        }

        currentFirePoint = (currentFirePoint + 1) % firePoints.Length;
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
        Gizmos.DrawWireSphere(transform.position, stat.attackRange);
    }
}

public class TurretIdleState : MonsterState
{
    public TurretIdleState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        (monster as Turret)?.PlayAnimation((monster as Turret).idleAnimation, true);
    }

    public override void UpdateState()
    {
        if (!(monster as Turret).isAttacking && monster.IsPlayerInRange(monster.monsterBaseStat.detectionRange))
        {
            monster.TransitionToState(monster.chaseState);
        }
    }

    public override void ExitState()
    {
    }
}

public class TurretChaseState : MonsterState
{
    public TurretChaseState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
    }

    public override void UpdateState()
    {
        if (!(monster as Turret).isAttacking && monster.IsPlayerInRange(monster.monsterBaseStat.attackRange))
        {
            monster.TransitionToState(monster.attackState);
        }
        else
        {
            monster.MoveTowards(monster.player.transform.position);
        }
    }

    public override void ExitState()
    {
    }
}

public class TurretAttackState : MonsterState
{
    public TurretAttackState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        monster.Attack();
    }

    public override void UpdateState()
    {
        if (!(monster as Turret).isAttacking && !monster.IsPlayerInRange(monster.monsterBaseStat.attackRange))
        {
            monster.TransitionToState(monster.idleState);
        }
    }

    public override void ExitState()
    {
    }
}

public class TurretCooldownState : MonsterState
{
    private float cooldownTimer;

    public TurretCooldownState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        cooldownTimer = (monster.monsterBaseStat as TurretMonsterData).attackCooldown;
        monster.isInCooldown = true;
    }

    public override void UpdateState()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0)
        {
            monster.isInCooldown = false;
            monster.TransitionToState(monster.idleState); // 쿨다운이 끝나면 대기 상태로 전환
        }
    }

    public override void ExitState()
    {
    }
}
