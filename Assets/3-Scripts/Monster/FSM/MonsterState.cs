using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class MonsterState : IMonsterState
{
    protected Monster monster;

    protected MonsterState(Monster monster)
    {
        this.monster = monster;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
}

public class IdleState : MonsterState
{
    public IdleState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        Debug.Log("Entering Idle State");
    }

    public override void UpdateState()
    {
        if (monster.IsPlayerInRange(monster.monsterBaseStat.detectionRange))
        {
            monster.TransitionToState(monster.chaseState);
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Idle State");
    }
}

public class ChaseState : MonsterState
{
    public ChaseState(Monster monster) : base(monster) { }

    public override void EnterState()
    {
        Debug.Log("Entering Chase State");
    }

    public override void UpdateState()
    {
        if (monster.IsPlayerInRange(monster.monsterBaseStat.attackRange))
        {
            monster.TransitionToState(monster.attackState);
        }
        else
        {
            monster.MoveTowards(PlayManager.I.GetPlayerPosition());
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Chase State");
    }
}

public class AttackState : MonsterState
{
    public AttackState(Monster monster) : base(monster) { }

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

    public override void ExitState()
    {
        Debug.Log("Exiting Attack State");
    }
}
public class CooldownState : MonsterState
{
    private float cooldownTimer;

    public CooldownState(Monster monster) : base(monster)
    {
    }

    public override void EnterState()
    {
        cooldownTimer = monster.monsterBaseStat.attackDelay;
        monster.isInCooldown = true;
        Debug.Log("Entering Cooldown State");
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

    public override void ExitState()
    {
        Debug.Log("Exiting Cooldown State");
    }
}
