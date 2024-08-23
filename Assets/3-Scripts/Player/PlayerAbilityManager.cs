using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{
    private Player player;

    private List<Ability> abilities = new List<Ability>();
    private List<Ability> availableAbilities = new List<Ability>();
    private Dictionary<string, bool> synergyAbilityAcquired = new Dictionary<string, bool>();
    private Dictionary<string, int> synergyLevels = new Dictionary<string, int>();

    public void Initialize(Player player)
    {
        this.player = player;

        LoadAvailableAbilities();
        InitializeSynergyDictionaries();
    }

    private void LoadAvailableAbilities()
    {
        Ability[] loadedAbilities = Resources.LoadAll<Ability>("Abilities");
        availableAbilities.AddRange(loadedAbilities);

        foreach (var ability in loadedAbilities)
        {
            Debug.Log($"Loaded ability: {ability.abilityName}");
        }
    }

    public List<Ability> GetAvailableAbilities()
    {
        return availableAbilities;
    }

    public void SelectAbility(Ability ability)
    {
        ability.Apply(player);

        if (ability.currentLevel == 0)
        {
            ability.currentLevel = 1;
            abilities.Add(ability);
        }
        else
        {
            ability.Upgrade();
        }

        if (ability.currentLevel >= 5)
        {
            availableAbilities.Remove(ability);
        }

        CheckForSynergy(ability.category);
    }

    private void InitializeSynergyDictionaries()
    {
        synergyAbilityAcquired = new Dictionary<string, bool>
        {
            { "Lust", false },
            { "Envy", false },
            { "Sloth", false },
            { "Gluttony", false },
            { "Greed", false },
            { "Wrath", false },
            { "Pride", false },
            { "Null", false }
        };

        synergyLevels = new Dictionary<string, int>
        {
            { "Lust", 0 },
            { "Envy", 0 },
            { "Sloth", 0 },
            { "Gluttony", 0 },
            { "Greed", 0 },
            { "Wrath", 0 },
            { "Pride", 0 },
            { "Null", 0 }
        };
    }

    private void CheckForSynergy(string category)
    {
        if (!synergyAbilityAcquired.ContainsKey(category) || !synergyLevels.ContainsKey(category))
        {
            Debug.LogError($"Category '{category}' not found in synergyAbilityAcquired or synergyLevels dictionary");
            return;
        }

        int totalLevel = 0;

        foreach (var ability in abilities)
        {
            if (ability.category == category)
            {
                totalLevel += ability.currentLevel;
            }
        }

        if (totalLevel >= 15 && synergyLevels[category] < 15)
        {
            AssignSynergyAbility(category, 15);
            synergyLevels[category] = 15;
        }
        else if (totalLevel >= 10 && synergyLevels[category] < 10)
        {
            AssignSynergyAbility(category, 10);
            synergyLevels[category] = 10;
        }
        else if (totalLevel >= 5 && synergyLevels[category] < 5)
        {
            AssignSynergyAbility(category, 5);
            synergyLevels[category] = 5;
        }
    }

    private void AssignSynergyAbility(string category, int level)
    {
        string synergyAbilityName = $"{category}Synergy{level}";
        SynergyAbility synergyAbility = Resources.Load<SynergyAbility>($"SynergyAbilities/{synergyAbilityName}");
        if (synergyAbility != null)
        {
            Debug.Log($"Synergy ability acquired: {synergyAbilityName}");
            ApplySynergyAbility(synergyAbility);
        }
        else
        {
            Debug.LogError($"Failed to load Synergy Ability: {synergyAbilityName}");
        }
    }

    public void ApplySynergyAbility(SynergyAbility synergyAbility)
    {
        synergyAbility.Apply(player);
    }
}
