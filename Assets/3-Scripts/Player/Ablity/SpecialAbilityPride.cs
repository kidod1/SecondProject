using UnityEngine;

public class SpecialAbilityPride : SpecialAbility
{
    public SpecialAbilityPride()
    {
        Name = "궁극 오만";
        Description = "오만 능력을 3개 모아 궁극 오만을 획득합니다.";
        RequiredAbilityCount = 3;
    }

    public override void Apply(Player player)
    {
        player.IncreasePride(20); // 예시로 20만큼 오만 증가
    }
}

public class SpecialAbilitySuperPride : SpecialAbility
{
    public SpecialAbilitySuperPride()
    {
        Name = "초오만";
        Description = "오만 능력을 5개 모아 초오만을 획득합니다.";
        RequiredAbilityCount = 5;
    }

    public override void Apply(Player player)
    {
        player.IncreasePride(10); // 예시로 10만큼 오만 증가
    }
}

public class SpecialAbilityUltraPride : SpecialAbility
{
    public SpecialAbilityUltraPride()
    {
        Name = "초초오만";
        Description = "오만 능력을 7개 모아 초초오만을 획득합니다.";
        RequiredAbilityCount = 7;
    }

    public override void Apply(Player player)
    {
        player.IncreasePride(15); // 예시로 15만큼 오만 증가
    }
}

public class SpecialAbilityWrath : SpecialAbility
{
    public SpecialAbilityWrath()
    {
        Name = "궁극 분노";
        Description = "분노 능력을 3개 모아 궁극 분노를 획득합니다.";
        RequiredAbilityCount = 3;
    }

    public override void Apply(Player player)
    {
        player.IncreaseAttack(20); // 예시로 20만큼 공격력 증가
        Debug.Log("분노 능력을 3개 모아 시너지 - 궁극 분노를 획득합니다.");
    }
}

public class SpecialAbilitySuperWrath : SpecialAbility
{
    public SpecialAbilitySuperWrath()
    {
        Name = "초 궁극 분노";
        Description = "분노 능력을 5개 모아 초 궁극 분노를 획득합니다.";
        RequiredAbilityCount = 5;
    }

    public override void Apply(Player player)
    {
        player.IncreaseAttack(30); // 예시로 30만큼 공격력 증가
        Debug.Log("분노 능력을 5개 모아 시너지 - 초 궁극 분노를 획득합니다.");
    }
}

public class SpecialAbilityUltraWrath : SpecialAbility
{
    public SpecialAbilityUltraWrath()
    {
        Name = "초초 궁극 분노";
        Description = "분노 능력을 7개 모아 초초 궁극 분노를 획득합니다.";
        RequiredAbilityCount = 7;
    }

    public override void Apply(Player player)
    {
        player.IncreaseAttack(40); // 예시로 40만큼 공격력 증가
        Debug.Log("분노 능력을 7개 모아 시너지 - 초초 궁극 분노를 획득합니다.");
    }
}


public class SpecialAbilityGluttony : SpecialAbility
{
    public SpecialAbilityGluttony()
    {
        Name = "궁극 식탐";
        Description = "식탐 능력을 3개 모아 궁극 식탐을 획득합니다.";
        RequiredAbilityCount = 3;
    }

    public override void Apply(Player player)
    {
        player.IncreaseGluttony(20); // 예시로 20만큼 식탐 증가
    }
}

public class SpecialAbilityGreed : SpecialAbility
{
    public SpecialAbilityGreed()
    {
        Name = "궁극 탐욕";
        Description = "탐욕 능력을 3개 모아 궁극 탐욕을 획득합니다.";
        RequiredAbilityCount = 3;
    }

    public override void Apply(Player player)
    {
        player.IncreaseGreed(20); // 예시로 20만큼 탐욕 증가
    }
}

public class SpecialAbilitySloth : SpecialAbility
{
    public SpecialAbilitySloth()
    {
        Name = "궁극 나태";
        Description = "나태 능력을 3개 모아 궁극 나태를 획득합니다.";
        RequiredAbilityCount = 3;
    }

    public override void Apply(Player player)
    {
        player.IncreaseSloth(20); // 예시로 20만큼 나태 증가
    }
}

public class SpecialAbilityEnvy : SpecialAbility
{
    public SpecialAbilityEnvy()
    {
        Name = "궁극 질투";
        Description = "질투 능력을 3개 모아 궁극 질투를 획득합니다.";
        RequiredAbilityCount = 3;
    }

    public override void Apply(Player player)
    {
        player.IncreaseEnvy(20); // 예시로 20만큼 질투 증가
    }
}

public class SpecialAbilityLust : SpecialAbility
{
    public SpecialAbilityLust()
    {
        Name = "궁극 색욕";
        Description = "색욕 능력을 3개 모아 궁극 색욕을 획득합니다.";
        RequiredAbilityCount = 3;
    }

    public override void Apply(Player player)
    {
        player.IncreaseLust(20); // 예시로 20만큼 색욕 증가
    }
}
