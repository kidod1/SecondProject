using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;

public class Bat : Monster
{
    [SerializeField]
    private BatMonsterData stat;

    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string prepareAnimation;  // ���� �غ� �ִϸ��̼�
    [SpineAnimation] public string dashAnimation;     // ���� �ִϸ��̼�
    [SpineAnimation] public string explodeAnimation;  // ���� �ִϸ��̼�

    private SkeletonAnimation skeletonAnimation;

    private enum BatState
    {
        Idle,
        Chasing,
        PreparingAttack,
        Dashing,
        Cooldown
    }

    private BatState currentState = BatState.Idle;
    private bool isAttacking = false;
    private Vector3 dashTargetPosition;
    private float cooldownTimer = 0f;

    protected override void Start()
    {
        base.Start();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            Debug.LogError("SkeletonAnimation�� null�Դϴ�.");
        }
        else
        {
            PlayAnimation(idleAnimation, true);
        }
    }

    protected override void InitializeStates()
    {
        // �� Ŭ���������� ���� Ŭ������ ������� �����Ƿ� �� �޼���� �Ӵϴ�.
        // �ʿ��� ��� ���� �ʱ�ȭ�� ���⼭ ������ �� �ֽ��ϴ�.
    }

    private void Update()
    {
        switch (currentState)
        {
            case BatState.Idle:
                HandleIdleState();
                break;
            case BatState.Chasing:
                HandleChaseState();
                break;
            case BatState.PreparingAttack:
                // ���� �غ� ���̹Ƿ� ���� ó���� �ʿ� ����
                break;
            case BatState.Dashing:
                HandleDashState();
                break;
            case BatState.Cooldown:
                HandleCooldownState();
                break;
        }
    }

    private void HandleIdleState()
    {
        if (IsPlayerInRange(stat.detectionRange))
        {
            currentState = BatState.Chasing;
            PlayAnimation(dashAnimation, true); // ���� �ִϸ��̼��� ������ �ʿ信 ���� ����
        }
    }

    private void HandleChaseState()
    {
        if (IsPlayerInRange(stat.attackRange))
        {
            if (!isAttacking)
            {
                StartCoroutine(PrepareAndAttack());
            }
        }
        else if (IsPlayerInRange(stat.detectionRange))
        {
            MoveTowards(player.transform.position);
        }
        else
        {
            currentState = BatState.Idle;
            PlayAnimation(idleAnimation, true);
        }
    }

    private IEnumerator PrepareAndAttack()
    {
        isAttacking = true;
        currentState = BatState.PreparingAttack;
        PlayAnimation(prepareAnimation, false);

        // 1�ʰ� ���� �غ�
        yield return new WaitForSeconds(1f);

        // �÷��̾��� ���� ��ġ�� �������� �� �� ��ġ ���
        if (player == null)
        {
            currentState = BatState.Idle;
            isAttacking = false;
            yield break;
        }

        Vector3 playerPosition = player.transform.position;
        Vector3 direction = (playerPosition - transform.position).normalized;
        dashTargetPosition = playerPosition + direction * 2f; // �÷��̾� ��ġ���� 2���� �� �� ��ġ�� ����

        currentState = BatState.Dashing;
        PlayAnimation(dashAnimation, true);
    }

    private void HandleDashState()
    {
        if (player == null)
        {
            currentState = BatState.Idle;
            isAttacking = false;
            PlayAnimation(idleAnimation, true);
            return;
        }

        // ���� �̵�
        float step = stat.skillDashSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, dashTargetPosition, step);

        // �÷��̾���� �Ÿ� Ȯ�� (�浹 �ݰ� 0.5���� �̳�)
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= 0.5f)
        {
            // ���� ���� ����
            StartCoroutine(Explode());
            return;
        }

        // ��ǥ ��ġ ���� Ȯ��
        if (Vector3.Distance(transform.position, dashTargetPosition) < 0.1f)
        {
            // ���� ����, ���� �缳��
            currentState = BatState.Chasing;
            isAttacking = false;
            PlayAnimation(dashAnimation, true); // ���� �ִϸ��̼� ���
        }
    }


    private IEnumerator Explode()
    {
        currentState = BatState.Cooldown;
        PlayAnimation(explodeAnimation, true);

        yield return new WaitForSeconds(0.1f);

        // �÷��̾�� ������ ����
        if (player != null)
        {
            player.TakeDamage(stat.attackDamage);
        }

        // ���� ��� ó��
        Die();
    }

    private void HandleCooldownState()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            currentState = BatState.Idle;
            isAttacking = false;
            PlayAnimation(idleAnimation, true);
        }
    }

    public override void Attack()
    {
        // ���� ������ PrepareAndAttack()�� Explode()���� ó���ǹǷ� ���� ���� �ʿ� ����
    }

    public void PlayAnimation(string animationName, bool loop)
    {
        if (skeletonAnimation != null && !string.IsNullOrEmpty(animationName))
        {
            skeletonAnimation.state.SetAnimation(0, animationName, loop);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Ž�� ������ ���� ������ ������ ǥ��
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stat.detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stat.attackRange);
    }
}
