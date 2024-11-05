using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;

public class BackDancer : Monster
{
    [SerializeField]
    private GameObject danceBulletPrefab; // ���ο� ���� ������
    [SerializeField]
    private BackDancerMonsterData stat; // ���ο� ���� ������

    [SpineAnimation] public string summonDanceAnimation;
    [SpineAnimation] public string idleDanceAnimation;
    [SpineAnimation] public string danceAttackAnimation;

    private SkeletonAnimation skeletonAnimation;

    [SerializeField] private Transform danceFirePoint; // ���ο� �߻� ����

    protected override void Start()
    {
        base.Start();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            Debug.LogError("���̷��� �ִϸ��̼��� null�Դϴ�.");
        }
        else
        {
            StartCoroutine(SummonDanceAnimationCoroutine());
        }

        if (danceFirePoint == null)
        {
            Debug.LogWarning("���� ��ġ�� �������� �ʾҽ��ϴ�.");
            danceFirePoint = transform;
        }
    }

    private IEnumerator SummonDanceAnimationCoroutine()
    {
        PlayAnimation(summonDanceAnimation, false);
        Debug.Log("��� ��ȯ �ִϸ��̼� ��� ��...");
        yield return new WaitForSpineAnimationComplete(skeletonAnimation);
        Debug.Log("��� ��ȯ �ִϸ��̼� �Ϸ�");
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
            Debug.LogWarning("�÷��̾ �������� �ʾҽ��ϴ�. FlipTowardsPlayer �޼��带 ȣ���� �� �����ϴ�.");
            return;
        }

        if (player.transform.position.x > transform.position.x)
        {
            // �÷��̾ ������ �����ʿ� ���� ��
            transform.localScale = new Vector3(-0.18f, 0.18f, 1f);
        }
        else
        {
            // �÷��̾ ������ ���ʿ� ���� ��
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
            Debug.LogError("DanceBullet �������� �������� �ʾҽ��ϴ�.");
            return;
        }

        Vector3 direction = (player.transform.position - danceFirePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject bullet = Instantiate(danceBulletPrefab, danceFirePoint.position, Quaternion.Euler(new Vector3(0, 0, -angle)));

        // �������� �ڽĿ� �ִ� ��ƼŬ�� ������ ����
        ParticleSystem particleSystem = bullet.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            var mainModule = particleSystem.main;
            mainModule.startRotation = Mathf.Deg2Rad * -angle + 110f; // ������ ���� ������ ��ȯ�Ͽ� ����
        }

        // �����տ� Rigidbody2D�� ����Ͽ� �߻�
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
                Debug.Log($"�ִϸ��̼� {animationName} ��� ���� (Loop: {loop})");
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
        Debug.Log("��� Idle ����: Idle �ִϸ��̼� ��� ��...");
    }

    public override void UpdateState()
    {
        (monster as BackDancer)?.FlipTowardsPlayer();

        if (monster.IsPlayerInRange(monster.monsterBaseStat.detectionRange))
        {
            monster.TransitionToState(monster.chaseState);
            Debug.Log("��� Idle ����: �÷��̾� ����, Chase ���·� ��ȯ");
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
        // ��ٿ� Ÿ�̸� �ʱ�ȭ
        cooldownTimer = (monster.monsterBaseStat as BackDancerMonsterData).danceCooldown;
        monster.isInCooldown = true;

        // Idle �ִϸ��̼� ���
        (monster as BackDancer)?.PlayAnimation((monster as BackDancer).idleDanceAnimation, true);
        Debug.Log("��� ��ٿ� ����: Idle �ִϸ��̼� ��� ��...");
    }

    public override void UpdateState()
    {
        // ������ ������ �÷��̾� ������ ���ϰ� ����
        (monster as BackDancer)?.FlipTowardsPlayer();

        // ��ٿ� Ÿ�̸� ����
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0)
        {
            monster.isInCooldown = false;
            monster.TransitionToState(monster.idleState);
            Debug.Log("��� ��ٿ� �Ϸ�: Idle ���·� ��ȯ");
        }
    }

    public override void ExitState() { }
}
