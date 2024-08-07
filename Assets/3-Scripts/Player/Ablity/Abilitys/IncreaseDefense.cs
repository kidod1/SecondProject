using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseDefense")]
public class IncreaseDefense : Ability
{
    public int[] defenseIncreases;

    public override void Apply(Player player)
    {
        if (currentLevel > 0)
        {
            player.stat.defense += defenseIncreases[currentLevel - 1];
        }
    }

    // 어빌리티 업그레이드
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
        }
    }

    // 다음 레벨 증가값 반환
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel)
        {
            return defenseIncreases[currentLevel];
        }
        return 0;
    }
}
