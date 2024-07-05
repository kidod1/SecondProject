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
        CooldownTime = 30f; // �⺻ ��Ÿ���� 10�ʷ� ����, �ʿ信 ���� ���� ����
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
        Name = "������ �߾�";
        Description = "ü���� 2 ���Ϸ� �������� ������ �ǵ带 2��ŭ �ο��մϴ�.";
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
        Name = "�������� ����";
        Description = "�ִ� HP�� 2 �����ϰ�, ���� �ð����� �ǵ带 2��ŭ �����մϴ�.";
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
        Name = "�����";
        Description = "�ִ� HP�� 2 �����ϰ�, ���ݷ��� 10 �����մϴ�.";
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
        Name = "������ ����";
        Description = "�ǵ尡 ������ �� ���ݷ��� 10 �����մϴ�.";
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
