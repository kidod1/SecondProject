using System.Collections;
using UnityEngine;

public class Bat : Monster
{
    [SerializeField]
    private BatMonsterData stat;

    private float attackCooldownTimer;

    protected override void Start()
    {
        base.Start();
        attackCooldownTimer = monsterBaseStat.attackDelay;
    }

    protected override void InitializeStates()
    {
        idleState = new IdleState(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
        cooldownState = new CooldownState(this);
        currentState = idleState;
        currentState.EnterState();
    }

    private void Update()
    {
        attackCooldownTimer -= Time.deltaTime;
        currentState?.UpdateState();
    }

    public override void Attack()
    {
        if (attackCooldownTimer <= 0)
        {
            StartCoroutine(SkillDash());
            attackCooldownTimer = monsterBaseStat.attackDelay;
        }
        else
        {
            TransitionToState(cooldownState);
        }
    }

    private IEnumerator SkillDash()
    {
        Debug.Log("플레이어를 향해 돌진중");
        Vector3 direction = (player.transform.position - transform.position).normalized;
        float dashDuration = 1f;
        float elapsed = 0f;
        bool hasDamagedPlayer = false;

        while (elapsed < dashDuration)
        {
            Debug.Log("플레이어를 향해 돌진중");
            transform.position += direction * stat.skillDashSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;

            if (!hasDamagedPlayer && Vector3.Distance(transform.position, player.transform.position) < 0.5f)
            {
                player.TakeDamage(stat.attackDamage);
                hasDamagedPlayer = true;
            }

            yield return null;
        }

        TransitionToState(cooldownState);
    }
}
