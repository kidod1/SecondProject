using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IMonsterDeathAbility
{
    void OnMonsterDeath(Monster monster);
}
public class PlayerAbilityManager : MonoBehaviour
{
    private Player player;

    public List<Ability> abilities = new List<Ability>();
    private List<Ability> availableAbilities = new List<Ability>();

    private Dictionary<string, bool> synergyAbilityAcquired = new Dictionary<string, bool>();
    private Dictionary<string, int> synergyLevels = new Dictionary<string, int>();

    private Coroutine selectionAnimationCoroutine;
    // 능력 변경 시 호출할 이벤트
    public UnityEvent OnAbilitiesChanged;

    public void Initialize(Player player)
    {
        this.player = player;

        LoadAvailableAbilities();
        InitializeSynergyDictionaries();
        ResetAllAbilities();

        // 이미 리스너가 등록되어 있는지 확인
        player.OnHitEnemy.RemoveListener(ActivateAbilitiesOnHit);
        player.OnHitEnemy.AddListener(ActivateAbilitiesOnHit);
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.OnHitEnemy.RemoveListener(ActivateAbilitiesOnHit);
        }
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
    /// 몬스터와 충돌했을 때 능력을 활성화하는 메서드
    /// </summary>
    public void ActivateAbilitiesOnHit(Collider2D enemy)
    {
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
    /// 몬스터가 사망했을 때 능력을 활성화하는 메서드
    /// </summary>
    public void ActivateAbilitiesOnMonsterDeath(Monster monster)
    {

        foreach (var ability in abilities)
        {
            if (ability is HoneyDrop honeyDrop)
            {
                Debug.Log($"Activating OnMonsterDeath for ability: {ability.abilityName}");
                honeyDrop.OnMonsterDeath(monster);
            }
            
            if (ability is KillSpeedBoostAbility killSpeedBoostAbility)
            {
                 killSpeedBoostAbility.OnMonsterKilled();
            }
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
                abilityManager.TriggerShowSynergyAbility(synergyAbility); // 공개 메서드 호출
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
        if (synergyAbility != null)
        {
            synergyAbility.Apply(player);
        }
        else
        {
            Debug.LogError("PlayerAbilityManager: synergyAbility가 null입니다.");
        }
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
