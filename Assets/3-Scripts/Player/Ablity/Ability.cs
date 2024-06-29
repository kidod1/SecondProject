using UnityEngine;

public abstract class Ability
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }

    public abstract void Apply(Player player);
}

public abstract class SpecialAbility : Ability
{
    public int RequiredAbilityCount { get; protected set; }
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

public class IncreaseWrath : Ability
{
    private int wrathIncrease;

    public IncreaseWrath()
    {
        Name = "분노";
        Description = "플레이어의 공격력을 5만큼 증가시킵니다.";
        wrathIncrease = 5;
    }

    public override void Apply(Player player)
    {
        player.IncreaseAttack(wrathIncrease);
    }
}
public class IncreaseSuperWrath : Ability
{
    private int superWrathIncrease;

    public IncreaseSuperWrath()
    {
        Name = "초분노";
        Description = "플레이어의 공격력을 10만큼 증가시킵니다.";
        superWrathIncrease = 10;
    }

    public override void Apply(Player player)
    {
        player.IncreaseAttack(superWrathIncrease);
    }
}
public class IncreaseUltraWrath : Ability
{
    private int ultraWrathIncrease;

    public IncreaseUltraWrath()
    {
        Name = "초초분노";
        Description = "플레이어의 공격력을 15만큼 증가시킵니다.";
        ultraWrathIncrease = 15;
    }

    public override void Apply(Player player)
    {
        player.IncreaseAttack(ultraWrathIncrease);
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
