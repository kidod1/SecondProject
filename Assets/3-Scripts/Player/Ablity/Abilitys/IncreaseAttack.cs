using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseAttack")]
public class IncreaseAttack : Ability
{
    public int[] attackIncreases;

    public override void Apply(Player player)
    {
        if (currentLevel > 0)
        {
            player.stat.currentPlayerDamage += attackIncreases[currentLevel];
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel)
        {
            return attackIncreases[currentLevel];
        }
        return 0;
    }
}