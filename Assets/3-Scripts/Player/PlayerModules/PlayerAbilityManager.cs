using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{
    private Player player;

    // �÷��̾ ���� �ɷ°� ��� ������ �ɷ��� �����ϴ� ����Ʈ
    public List<Ability> abilities = new List<Ability>();
    private List<Ability> availableAbilities = new List<Ability>();

    // �ó��� �ɷ� ������ Dictionary
    private Dictionary<string, bool> synergyAbilityAcquired = new Dictionary<string, bool>();
    private Dictionary<string, int> synergyLevels = new Dictionary<string, int>();

    // �ʱ�ȭ �޼���: �÷��̾�� �ɷ� ������ �ε�
    public void Initialize(Player player)
    {
        this.player = player;

        // ��� ������ �ɷ� �ε�
        LoadAvailableAbilities();

        // �ó��� �ɷ� ���� Dictionary �ʱ�ȭ
        InitializeSynergyDictionaries();

        // ���� ���� �� ��� �ɷ� �ʱ�ȭ
        ResetAllAbilities();
    }

    // ��� ������ �ɷµ��� ���ҽ����� �ε��ϴ� �޼���
    private void LoadAvailableAbilities()
    {
        Ability[] loadedAbilities = Resources.LoadAll<Ability>("Abilities");
        availableAbilities.AddRange(loadedAbilities);

        // �ε�� �ɷ��� ����� ���
        foreach (var ability in loadedAbilities)
        {
            Debug.Log($"Loaded ability: {ability.abilityName}");
        }
    }

    // ��� ������ �ɷµ��� ��ȯ�ϴ� �޼���
    public List<Ability> GetAvailableAbilities()
    {
        return availableAbilities;
    }

    // �ɷ� ���� �� ȣ��Ǵ� �޼���
    public void SelectAbility(Ability ability)
    {
        ability.Apply(player);

        if (ability.currentLevel == 0)
        {
            // �ɷ��� ó�� ������ ��� ���� 1�� ���� �� ����Ʈ�� �߰�
            ability.currentLevel = 1;
            abilities.Add(ability);
        }
        else
        {
            // �̹� ������ �ɷ��� ���׷��̵�
            ability.Upgrade();
        }

        // �ɷ� ������ 5 �̻��̸� ���� �Ұ����ϰ� ����Ʈ���� ����
        if (ability.currentLevel >= 5)
        {
            availableAbilities.Remove(ability);
        }

        // �ó��� �ɷ� üũ
        CheckForSynergy(ability.category);
    }

    // �ó��� �ɷ� ���� Dictionary �ʱ�ȭ
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

    // �ó��� �ɷ� ȹ�� ���� Ȯ��
    private void CheckForSynergy(string category)
    {
        if (!synergyAbilityAcquired.ContainsKey(category) || !synergyLevels.ContainsKey(category))
        {
            Debug.LogError($"Category '{category}' not found in synergyAbilityAcquired or synergyLevels dictionary");
            return;
        }

        // �ش� ī�װ� �ɷµ��� �� ���� ���
        int totalLevel = 0;
        foreach (var ability in abilities)
        {
            if (ability.category == category)
            {
                totalLevel += ability.currentLevel;
            }
        }

        // ����� �޽��� ���
        Debug.Log($"Category: {category}, Total Level: {totalLevel}");

        // Ư�� ���� �������� �ó��� �ɷ� �Ҵ�
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

    // �ó��� �ɷ� �Ҵ� �� UI �г� ǥ��
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
                // �ó��� �ɷ� ȹ�� �� �г� ǥ��
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
