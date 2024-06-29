using UnityEngine;

public class SpecialAbilityPride : SpecialAbility
{
    public SpecialAbilityPride()
    {
        Name = "�ñ� ����";
        Description = "���� �ɷ��� 3�� ��� �ñ� ������ ȹ���մϴ�.";
        RequiredAbilityCount = 3;
    }

    public override void Apply(Player player)
    {
        player.IncreasePride(20); // ���÷� 20��ŭ ���� ����
    }
}

public class SpecialAbilitySuperPride : SpecialAbility
{
    public SpecialAbilitySuperPride()
    {
        Name = "�ʿ���";
        Description = "���� �ɷ��� 5�� ��� �ʿ����� ȹ���մϴ�.";
        RequiredAbilityCount = 5;
    }

    public override void Apply(Player player)
    {
        player.IncreasePride(10); // ���÷� 10��ŭ ���� ����
    }
}

public class SpecialAbilityUltraPride : SpecialAbility
{
    public SpecialAbilityUltraPride()
    {
        Name = "���ʿ���";
        Description = "���� �ɷ��� 7�� ��� ���ʿ����� ȹ���մϴ�.";
        RequiredAbilityCount = 7;
    }

    public override void Apply(Player player)
    {
        player.IncreasePride(15); // ���÷� 15��ŭ ���� ����
    }
}

public class SpecialAbilityWrath : SpecialAbility
{
    public SpecialAbilityWrath()
    {
        Name = "�ñ� �г�";
        Description = "�г� �ɷ��� 3�� ��� �ñ� �г븦 ȹ���մϴ�.";
        RequiredAbilityCount = 3;
    }

    public override void Apply(Player player)
    {
        player.IncreaseAttack(20); // ���÷� 20��ŭ ���ݷ� ����
        Debug.Log("�г� �ɷ��� 3�� ��� �ó��� - �ñ� �г븦 ȹ���մϴ�.");
    }
}

public class SpecialAbilitySuperWrath : SpecialAbility
{
    public SpecialAbilitySuperWrath()
    {
        Name = "�� �ñ� �г�";
        Description = "�г� �ɷ��� 5�� ��� �� �ñ� �г븦 ȹ���մϴ�.";
        RequiredAbilityCount = 5;
    }

    public override void Apply(Player player)
    {
        player.IncreaseAttack(30); // ���÷� 30��ŭ ���ݷ� ����
        Debug.Log("�г� �ɷ��� 5�� ��� �ó��� - �� �ñ� �г븦 ȹ���մϴ�.");
    }
}

public class SpecialAbilityUltraWrath : SpecialAbility
{
    public SpecialAbilityUltraWrath()
    {
        Name = "���� �ñ� �г�";
        Description = "�г� �ɷ��� 7�� ��� ���� �ñ� �г븦 ȹ���մϴ�.";
        RequiredAbilityCount = 7;
    }

    public override void Apply(Player player)
    {
        player.IncreaseAttack(40); // ���÷� 40��ŭ ���ݷ� ����
        Debug.Log("�г� �ɷ��� 7�� ��� �ó��� - ���� �ñ� �г븦 ȹ���մϴ�.");
    }
}


public class SpecialAbilityGluttony : SpecialAbility
{
    public SpecialAbilityGluttony()
    {
        Name = "�ñ� ��Ž";
        Description = "��Ž �ɷ��� 3�� ��� �ñ� ��Ž�� ȹ���մϴ�.";
        RequiredAbilityCount = 3;
    }

    public override void Apply(Player player)
    {
        player.IncreaseGluttony(20); // ���÷� 20��ŭ ��Ž ����
    }
}

public class SpecialAbilityGreed : SpecialAbility
{
    public SpecialAbilityGreed()
    {
        Name = "�ñ� Ž��";
        Description = "Ž�� �ɷ��� 3�� ��� �ñ� Ž���� ȹ���մϴ�.";
        RequiredAbilityCount = 3;
    }

    public override void Apply(Player player)
    {
        player.IncreaseGreed(20); // ���÷� 20��ŭ Ž�� ����
    }
}

public class SpecialAbilitySloth : SpecialAbility
{
    public SpecialAbilitySloth()
    {
        Name = "�ñ� ����";
        Description = "���� �ɷ��� 3�� ��� �ñ� ���¸� ȹ���մϴ�.";
        RequiredAbilityCount = 3;
    }

    public override void Apply(Player player)
    {
        player.IncreaseSloth(20); // ���÷� 20��ŭ ���� ����
    }
}

public class SpecialAbilityEnvy : SpecialAbility
{
    public SpecialAbilityEnvy()
    {
        Name = "�ñ� ����";
        Description = "���� �ɷ��� 3�� ��� �ñ� ������ ȹ���մϴ�.";
        RequiredAbilityCount = 3;
    }

    public override void Apply(Player player)
    {
        player.IncreaseEnvy(20); // ���÷� 20��ŭ ���� ����
    }
}

public class SpecialAbilityLust : SpecialAbility
{
    public SpecialAbilityLust()
    {
        Name = "�ñ� ����";
        Description = "���� �ɷ��� 3�� ��� �ñ� ������ ȹ���մϴ�.";
        RequiredAbilityCount = 3;
    }

    public override void Apply(Player player)
    {
        player.IncreaseLust(20); // ���÷� 20��ŭ ���� ����
    }
}
