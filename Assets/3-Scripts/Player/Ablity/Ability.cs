using UnityEngine;
using System.Collections;
public abstract class Ability
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }

    public virtual void Apply(Player player)
    {

    }
}

public abstract class SpecialAbility : Ability
{
    public int RequiredAbilityCount { get; protected set; }
    public float CooldownTime { get; protected set; }
    private bool isOnCooldown;

    public SpecialAbility()
    {
        CooldownTime = 30f; // 기본 쿨타임을 10초로 설정, 필요에 따라 변경 가능
        isOnCooldown = false;
    }

    public virtual void Activate(Player player)
    {
        if (isOnCooldown)
        {
            Debug.Log("Ability is on cooldown.");
            return;
        }

        ApplyEffect(player);
        StartCooldown(player);
    }

    protected virtual void ApplyEffect(Player player)
    {

    }

    private void StartCooldown(Player player)
    {
        isOnCooldown = true;
        player.StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(CooldownTime);
        isOnCooldown = false;
    }
}


public class ShieldOnLowHP : Ability
{
    public ShieldOnLowHP()
    {
        Name = "마지막 발악";
        Description = "체력이 2 이하로 떨어졌을 때마다 실드를 2만큼 부여합니다.";
    }

    public override void Apply(Player player)
    {
        player.EnableShieldOnLowHP();
    }
}
public class ReduceMaxHPAndRefillShield : Ability
{
    public ReduceMaxHPAndRefillShield()
    {
        Name = "광전사의 의지";
        Description = "최대 HP가 2 감소하고, 일정 시간마다 실드를 2만큼 리필합니다.";
    }

    public override void Apply(Player player)
    {
        player.ReduceMaxHPAndStartShieldRefill();
    }
}
public class ReduceMaxHPIncreaseAttack : Ability
{
    public ReduceMaxHPIncreaseAttack()
    {
        Name = "배수진";
        Description = "최대 HP가 2 감소하고, 공격력이 10 증가합니다.";
    }

    public override void Apply(Player player)
    {
        player.ReduceMaxHPAndIncreaseAttack();
    }
}

public class IncreaseAttackWithShield : Ability
{
    public IncreaseAttackWithShield()
    {
        Name = "안정된 마음";
        Description = "실드가 존재할 때 공격력이 10 증가합니다.";
    }

    public override void Apply(Player player)
    {
        player.EnableAttackBoostWithShield();
    }
}

public class IncreaseAttack : Ability
{
    private int attackIncrease;

    public IncreaseAttack()
    {
        Name = "공격력 증가";
        Description = "플레이어의 공격력을 증가시킵니다.";
        attackIncrease = 5; // 기본 증가값 설정
    }

    public override void Apply(Player player)
    {
        player.IncreaseAttack(attackIncrease);
    }
}

public class IncreaseRange : Ability
{
    private int rangeIncrease;

    public IncreaseRange()
    {
        Name = "사거리 증가";
        Description = "플레이어의 사거리를 증가시킵니다.";
        rangeIncrease = 1; // 기본 증가값 설정
    }

    public override void Apply(Player player)
    {
        player.IncreaseRange(rangeIncrease);
    }
}

public class IncreaseAttackSpeed : Ability
{
    private float speedIncrease;

    public IncreaseAttackSpeed()
    {
        Name = "공격 속도 증가";
        Description = "플레이어의 공격 속도를 증가시킵니다.";
        speedIncrease = 0.1f; // 기본 증가값 설정
    }

    public override void Apply(Player player)
    {
        player.IncreaseAttackSpeed(speedIncrease);
    }
}



public class IncreasePride : Ability
{
    private int prideIncrease;

    public IncreasePride()
    {
        Name = "오만 증가";
        Description = "플레이어의 오만을 5만큼 증가시킵니다.";
        prideIncrease = 5;
    }

    public override void Apply(Player player)
    {
        player.IncreasePride(prideIncrease);
    }
}


public class IncreaseGluttony : Ability
{
    private int gluttonyIncrease;

    public IncreaseGluttony()
    {
        Name = "식탐 증가";
        Description = "플레이어의 식탐을 5만큼 증가시킵니다.";
        gluttonyIncrease = 5;
    }

    public override void Apply(Player player)
    {
        player.IncreaseGluttony(gluttonyIncrease);
    }
}

public class IncreaseGreed : Ability
{
    private int greedIncrease;

    public IncreaseGreed()
    {
        Name = "탐욕 증가";
        Description = "플레이어의 탐욕을 5만큼 증가시킵니다.";
        greedIncrease = 5;
    }

    public override void Apply(Player player)
    {
        player.IncreaseGreed(greedIncrease);
    }
}

public class IncreaseSloth : Ability
{
    private int slothIncrease;

    public IncreaseSloth()
    {
        Name = "나태 증가";
        Description = "플레이어의 나태를 5만큼 증가시킵니다.";
        slothIncrease = 5;
    }

    public override void Apply(Player player)
    {
        player.IncreaseSloth(slothIncrease);
    }
}

public class IncreaseEnvy : Ability
{
    private int envyIncrease;

    public IncreaseEnvy()
    {
        Name = "질투 증가";
        Description = "플레이어의 질투를 5만큼 증가시킵니다.";
        envyIncrease = 5;
    }

    public override void Apply(Player player)
    {
        player.IncreaseEnvy(envyIncrease);
    }
}

public class IncreaseLust : Ability
{
    private int lustIncrease;

    public IncreaseLust()
    {
        Name = "색욕 증가";
        Description = "플레이어의 색욕을 5만큼 증가시킵니다.";
        lustIncrease = 5;
    }

    public override void Apply(Player player)
    {
        player.IncreaseLust(lustIncrease);
    }
}
