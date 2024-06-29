using System.Collections.Generic;
using UnityEngine;

public class AbilityTree
{
    public string Name { get; private set; }
    public List<Ability> Abilities { get; private set; }
    public SpecialAbility SpecialAbility { get; private set; }

    public AbilityTree(string name, List<Ability> abilities, SpecialAbility specialAbility)
    {
        Name = name;
        Abilities = abilities;
        SpecialAbility = specialAbility;
    }

    public void ApplyAbility(Player player, int index)
    {
        if (index >= 0 && index < Abilities.Count)
        {
            player.AddAbility(Abilities[index]);
        }

        int acquiredAbilityCount = player.GetAcquiredAbilityCount();

        if (acquiredAbilityCount >= SpecialAbility.RequiredAbilityCount)
        {
            player.AddAbility(SpecialAbility);
        }
    }
}
