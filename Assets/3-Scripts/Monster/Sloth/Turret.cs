using Spine.Unity;
using UnityEngine;
using System.Collections;
using System.ComponentModel;
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

    protected override void Start()
    {
        base.Start();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
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
            yield return new WaitForSecondsRealtime(0.29175f);
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
