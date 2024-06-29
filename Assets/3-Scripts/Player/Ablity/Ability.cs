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
        Name = "���ݷ� ����";
        Description = "�÷��̾��� ���ݷ��� ������ŵ�ϴ�.";
        attackIncrease = 5; // �⺻ ������ ����
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
        Name = "��Ÿ� ����";
        Description = "�÷��̾��� ��Ÿ��� ������ŵ�ϴ�.";
        rangeIncrease = 1; // �⺻ ������ ����
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
        Name = "���� �ӵ� ����";
        Description = "�÷��̾��� ���� �ӵ��� ������ŵ�ϴ�.";
        speedIncrease = 0.1f; // �⺻ ������ ����
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
        Name = "���� ����";
        Description = "�÷��̾��� ������ 5��ŭ ������ŵ�ϴ�.";
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
        Name = "�г�";
        Description = "�÷��̾��� ���ݷ��� 5��ŭ ������ŵ�ϴ�.";
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
        Name = "�ʺг�";
        Description = "�÷��̾��� ���ݷ��� 10��ŭ ������ŵ�ϴ�.";
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
        Name = "���ʺг�";
        Description = "�÷��̾��� ���ݷ��� 15��ŭ ������ŵ�ϴ�.";
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
        Name = "��Ž ����";
        Description = "�÷��̾��� ��Ž�� 5��ŭ ������ŵ�ϴ�.";
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
        Name = "Ž�� ����";
        Description = "�÷��̾��� Ž���� 5��ŭ ������ŵ�ϴ�.";
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
        Name = "���� ����";
        Description = "�÷��̾��� ���¸� 5��ŭ ������ŵ�ϴ�.";
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
        Name = "���� ����";
        Description = "�÷��̾��� ������ 5��ŭ ������ŵ�ϴ�.";
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
        Name = "���� ����";
        Description = "�÷��̾��� ������ 5��ŭ ������ŵ�ϴ�.";
        lustIncrease = 5;
    }

    public override void Apply(Player player)
    {
        player.IncreaseLust(lustIncrease);
    }
}
