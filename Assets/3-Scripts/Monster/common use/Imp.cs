using System.Collections;
using UnityEngine;

public class Imp : Monster
{
    public GameObject bulletPrefab;
    [SerializeField]
    private ImpMonsterData stat;

    protected override void InitializeStates()
    {
        idleState = new IdleState(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
        cooldownState = new CooldownState(this);
        currentState = idleState;
        currentState.EnterState();
    }

    public override void Attack()
    {
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        while (IsPlayerInRange(monsterBaseStat.attackRange))
        {
            FireBullet();
            TransitionToState(cooldownState);
            yield break;
        }
    }

    private void FireBullet()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = direction * stat.attackSpeed;

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetAttackDamage(monsterBaseStat.attackDamage);
        }
    }
}
