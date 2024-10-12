using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAbilityManager : MonoBehaviour
{
    private Player player;

    public List<Ability> abilities = new List<Ability>();
    private List<Ability> availableAbilities = new List<Ability>();

    private Dictionary<string, bool> synergyAbilityAcquired = new Dictionary<string, bool>();
    private Dictionary<string, int> synergyLevels = new Dictionary<string, int>();

    // 능력 변경 시 호출할 이벤트
    public UnityEvent OnAbilitiesChanged;

    public void Initialize(Player player)
    {
        this.player = player;

        LoadAvailableAbilities();
        InitializeSynergyDictionaries();
        ResetAllAbilities();

        // Player의 OnHitEnemy 이벤트에 대한 리스너를 추가
        player.OnHitEnemy.AddListener(ActivateAbilitiesOnHit);
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

        // 능력이 변경되었음을 알림
        OnAbilitiesChanged?.Invoke();
    }

    /// <summary>
    /// 플레이어가 몬스터와 충돌했을 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="enemy">충돌한 몬스터의 Collider2D</param>
    public void ActivateAbilitiesOnHit(Collider2D enemy)
    {
        // 능력 리스트에서 각 능력에 대한 트리거 처리
        foreach (var ability in abilities)
        {
            if (ability is FlashBlade flashBladeAbility)
            {
                flashBladeAbility.OnProjectileHit(enemy);
            }
            if (ability is JokerDraw jokerDrawAbility)
            {
                jokerDrawAbility.OnHitMonster(enemy);
            }
            else if (ability is CardStrike cardStrikeAbility)
            {
                cardStrikeAbility.OnProjectileHit(enemy);
            }
            else if (ability is RicochetStrike ricochetAbility)
            {
                ricochetAbility.OnProjectileHit(enemy);
            }
            else if (ability is SharkStrike sharkStrikeAbility)
            {
                sharkStrikeAbility.OnProjectileHit(enemy);
            }
            else if (ability is ParasiticNest parasiticNestAbility)
            {
                parasiticNestAbility.OnProjectileHit(enemy); // ParasiticNest의 OnProjectileHit 호출
            }
        }
    }

    /// <summary>
    /// 몬스터가 사망했을 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="monster">사망한 몬스터</param>
    public void ActivateAbilitiesOnMonsterDeath(Monster monster)
    {
        foreach (var ability in abilities)
        {
            if (ability is KillSpeedBoostAbility killSpeedBoostAbility)
            {
                killSpeedBoostAbility.OnMonsterKilled();
            }
            // ParasiticNest는 OnMonsterKilled 메서드를 필요로 하지 않으므로 호출을 제거합니다.
            // 다른 능력들의 OnMonsterKilled 메서드가 있다면 여기에 추가
        }
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

    public void ResetAllAbilities()
    {
        foreach (var ability in availableAbilities)
        {
            ability.ResetLevel();
        }
        abilities.Clear();

        // 능력이 변경되었음을 알림
        OnAbilitiesChanged?.Invoke();
    }

    public void CheckForSynergy(string category)
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

        Debug.Log($"Category: {category}, Total Level: {totalLevel}");

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

            AbilityManager abilityManager = FindObjectOfType<AbilityManager>();
            if (abilityManager != null)
            {
                abilityManager.ShowSynergyAbility(synergyAbility);
            }
            else
            {
                Debug.LogError("AbilityManager not found. Cannot show synergy ability panel.");
            }
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

    public T GetAbilityOfType<T>() where T : Ability
    {
        foreach (var ability in abilities)
        {
            if (ability is T)
            {
                return ability as T;
            }
        }
        return null;
    }
}
