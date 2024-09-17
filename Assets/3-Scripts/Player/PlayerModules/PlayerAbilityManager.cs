using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{
    private Player player;

    // 플레이어가 가진 능력과 사용 가능한 능력을 관리하는 리스트
    public List<Ability> abilities = new List<Ability>();
    private List<Ability> availableAbilities = new List<Ability>();

    // 시너지 능력 관리용 Dictionary
    private Dictionary<string, bool> synergyAbilityAcquired = new Dictionary<string, bool>();
    private Dictionary<string, int> synergyLevels = new Dictionary<string, int>();

    // 초기화 메서드: 플레이어와 능력 데이터 로드
    public void Initialize(Player player)
    {
        this.player = player;

        // 사용 가능한 능력 로드
        LoadAvailableAbilities();

        // 시너지 능력 관련 Dictionary 초기화
        InitializeSynergyDictionaries();

        // 게임 시작 시 모든 능력 초기화
        ResetAllAbilities();
    }

    // 사용 가능한 능력들을 리소스에서 로드하는 메서드
    private void LoadAvailableAbilities()
    {
        Ability[] loadedAbilities = Resources.LoadAll<Ability>("Abilities");
        availableAbilities.AddRange(loadedAbilities);

        // 로드된 능력을 디버그 출력
        foreach (var ability in loadedAbilities)
        {
            Debug.Log($"Loaded ability: {ability.abilityName}");
        }
    }

    // 사용 가능한 능력들을 반환하는 메서드
    public List<Ability> GetAvailableAbilities()
    {
        return availableAbilities;
    }

    // 능력 선택 시 호출되는 메서드
    public void SelectAbility(Ability ability)
    {
        ability.Apply(player);

        if (ability.currentLevel == 0)
        {
            // 능력을 처음 선택할 경우 레벨 1로 설정 후 리스트에 추가
            ability.currentLevel = 1;
            abilities.Add(ability);
        }
        else
        {
            // 이미 보유한 능력은 업그레이드
            ability.Upgrade();
        }

        // 능력 레벨이 5 이상이면 선택 불가능하게 리스트에서 제거
        if (ability.currentLevel >= 5)
        {
            availableAbilities.Remove(ability);
        }

        // 시너지 능력 체크
        CheckForSynergy(ability.category);
    }

    // 시너지 능력 관련 Dictionary 초기화
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

    // 시너지 능력 획득 조건 확인
    private void CheckForSynergy(string category)
    {
        if (!synergyAbilityAcquired.ContainsKey(category) || !synergyLevels.ContainsKey(category))
        {
            Debug.LogError($"Category '{category}' not found in synergyAbilityAcquired or synergyLevels dictionary");
            return;
        }

        // 해당 카테고리 능력들의 총 레벨 계산
        int totalLevel = 0;
        foreach (var ability in abilities)
        {
            if (ability.category == category)
            {
                totalLevel += ability.currentLevel;
            }
        }

        // 디버그 메시지 출력
        Debug.Log($"Category: {category}, Total Level: {totalLevel}");

        // 특정 레벨 기준으로 시너지 능력 할당
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

    // 시너지 능력 할당 및 UI 패널 표시
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
                // 시너지 능력 획득 시 패널 표시
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

    public void ResetAllAbilities()
    {
        foreach (var ability in availableAbilities)
        {
            ability.ResetLevel();
        }
        abilities.Clear();
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
