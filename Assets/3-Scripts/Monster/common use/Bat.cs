using System.Collections;
using UnityEngine;
using Spine.Unity;
using Spine;

public class Bat : Monster
{
    [SerializeField]
    private BatMonsterData stat;

    [SpineAnimation] public string dashAnimation;     // ���� �ִϸ��̼�
    [SpineAnimation] public string explodeAnimation;  // ���� �ִϸ��̼�

    private SkeletonAnimation skeletonAnimation;

    private enum BatState
    {
        Dashing
    }

    private BatState currentState = BatState.Dashing;
    private Vector3 dashDirection;
    private float dashSpeed;

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
            PlayAnimation(dashAnimation, true);
        }

        // ���� ���� ���: ��ȯ�� �� �÷��̾ ���� ����
        if (player != null)
        {
            dashDirection = (player.transform.position - transform.position).normalized;
        }
        else
        {
            Debug.LogError("Player ������ �����ϴ�.");
            dashDirection = Vector3.zero;
        }

        dashSpeed = stat.skillDashSpeed;
    }

    protected override void InitializeStates()
    {
        // �� Ŭ���������� ���� Ŭ������ ������� �����Ƿ� �� �޼���� �Ӵϴ�.
    }

    private void Update()
    {
        switch (currentState)
        {
            case BatState.Dashing:
                HandleDashState();
                break;
        }
    }

    private void HandleDashState()
    {
        if (player == null)
        {
            // �÷��̾ ������ �����
            Die();
            return;
        }

        // ���� �̵�
        float step = dashSpeed * Time.deltaTime;
        transform.position += dashDirection * step;

        // �߰������� �ִ� ���� �Ÿ��� �����ϰų�, Ư�� ���ǿ��� ���ߵ��� ������ �� �ֽ��ϴ�.
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("�����");
        GameObject collidedObject = collision.gameObject;

        // �浹�� ������Ʈ�� Player ������Ʈ�� �ִ��� Ȯ��
        Player playerScript = collidedObject.GetComponent<Player>();
        if (playerScript != null)
        {
            // �÷��̾�� �浹�� ���
            playerScript.TakeDamage(stat.attackDamage);
            Die();
        }
        else
        {
            Die();
        }
    }

    public override void Attack()
    {
        // ���� ������ OnCollisionEnter���� ó���ǹǷ� ���� ���� �ʿ� ����
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
        // Ž�� ������ ���� ������ ������ ǥ�� (�ʿ� ��)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stat.detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stat.attackRange);
    }
}
