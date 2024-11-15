using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;
using AK.Wwise; // Wwise ���ӽ����̽� �߰�

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

    // �߰��� Wwise �̺�Ʈ �ʵ�
    [SerializeField]
    private AK.Wwise.Event attackSoundEvent; // ���� ���� �̺�Ʈ ����

    private uint attackSoundPlayingID = 0; // ���� ��� ���� ���� ������ ID

    protected override void Start()
    {
        base.Start();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        skeletonAnimation.Initialize(true);
        if (skeletonAnimation == null)
        {
            Debug.LogError("���̷��� �ִϸ��̼� is null");
        }
        else
        {
            StartCoroutine(SummonAnimationCoroutine());
        }

        if (firePoints == null || firePoints.Length < 2)
        {
            Debug.LogError("Fire Points�� �������� �ʾҰų� �����մϴ�.");
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

        // ��Ų ���� ���� ���� ���� ���¸� �ʱ�ȭ
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
        // ���� ������ ������ ǥ��
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
            monster.TransitionToState(monster.idleState); // ��ٿ��� ������ ��� ���·� ��ȯ
        }
    }

    public override void ExitState()
    {
    }
}
