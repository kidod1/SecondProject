using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class AbilityData
{
    public string abilityName;
    public int currentLevel;
    public string category;
}

[System.Serializable]
public class SynergyAbilityData
{
    public string abilityName;
    public int currentLevel;
    public string category;
}

[System.Serializable]
public class PlayerDataToJson
{
    // PlayerData ����
    public float currentPlayerSpeed;
    public int currentPlayerDamage;
    public float currentProjectileSpeed;
    public float currentProjectileRange;
    public int currentProjectileType;
    public int currentMaxHP;
    public int currentHP;
    public int currentShield;
    public float currentAttackSpeed;
    public int currentDefense;
    public int currentExperience;
    public int currentCurrency;
    public int currentLevel;
    public float experienceMultiplier;

    public int defaultPlayerDamage;
    public float defaultPlayerSpeed;
    public float defaultAttackSpeed;
    public int defaultMaxHP;
    public float defaultProjectileSpeed;
    public float defaultProjectileRange;

    // �ɷ� ������
    public List<AbilityData> abilitiesData;
    public SynergyAbilityData synergyAbilityData;
    public Dictionary<string, int> synergyLevels;
}

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    // �÷��̾��� ���� ������
    public PlayerData playerData;

    // �÷��̾��� �ɷ� ������
    public List<AbilityData> abilitiesData = new List<AbilityData>();
    public SynergyAbilityData synergyAbilityData;
    public Dictionary<string, int> synergyLevels = new Dictionary<string, int>();

    private PlayerAbilityManager playerAbilityManager;

    private string saveFilePath;

    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // �� ��ȯ �� �ı����� �ʵ��� ����
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");
    }

    private void Start()
    {
        // PlayerAbilityManager ã��
        playerAbilityManager = FindObjectOfType<PlayerAbilityManager>();
        if (playerAbilityManager != null)
        {
            // �ɷ� ���� �̺�Ʈ ����
            playerAbilityManager.OnAbilitiesChanged += UpdateAbilitiesData;
        }
        else
        {
            Debug.LogError("PlayerAbilityManager�� ã�� �� �����ϴ�.");
        }
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (playerAbilityManager != null)
        {
            playerAbilityManager.OnAbilitiesChanged -= UpdateAbilitiesData;
        }
    }

    /// <summary>
    /// �÷��̾� �����͸� �����մϴ�.
    /// </summary>
    public void SavePlayerData()
    {
        // PlayerData�� ���� ���� ����
        PlayerDataToJson data = new PlayerDataToJson
        {
            currentPlayerSpeed = playerData.currentPlayerSpeed,
            currentPlayerDamage = playerData.currentPlayerDamage,
            currentProjectileSpeed = playerData.currentProjectileSpeed,
            currentProjectileRange = playerData.currentProjectileRange,
            currentProjectileType = playerData.currentProjectileType,
            currentMaxHP = playerData.currentMaxHP,
            currentHP = playerData.currentHP,
            currentShield = playerData.currentShield,
            currentAttackSpeed = playerData.currentAttackSpeed,
            currentDefense = playerData.currentDefense,
            currentExperience = playerData.currentExperience,
            currentCurrency = playerData.currentCurrency,
            currentLevel = playerData.currentLevel,
            experienceMultiplier = playerData.experienceMultiplier,
            defaultPlayerDamage = playerData.defaultPlayerDamage,
            defaultPlayerSpeed = playerData.defaultPlayerSpeed,
            defaultAttackSpeed = playerData.defaultAttackSpeed,
            defaultMaxHP = playerData.defaultMaxHP,
            defaultProjectileSpeed = playerData.defaultProjectileSpeed,
            defaultProjectileRange = playerData.defaultProjectileRange,

            // �ɷ� ������ ����
            abilitiesData = abilitiesData,
            synergyAbilityData = synergyAbilityData,
            synergyLevels = synergyLevels
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log("�÷��̾� �����Ͱ� ����Ǿ����ϴ�.");
    }

    /// <summary>
    /// �÷��̾� �����͸� �ε��մϴ�.
    /// </summary>
    public void LoadPlayerData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerDataToJson data = JsonUtility.FromJson<PlayerDataToJson>(json);

            // PlayerData�� ���� ���� �ε�
            playerData.currentPlayerSpeed = data.currentPlayerSpeed;
            playerData.currentPlayerDamage = data.currentPlayerDamage;
            playerData.currentProjectileSpeed = data.currentProjectileSpeed;
            playerData.currentProjectileRange = data.currentProjectileRange;
            playerData.currentProjectileType = data.currentProjectileType;
            playerData.currentMaxHP = data.currentMaxHP;
            playerData.currentHP = data.currentHP;
            playerData.currentShield = data.currentShield;
            playerData.currentAttackSpeed = data.currentAttackSpeed;
            playerData.currentDefense = data.currentDefense;
            playerData.currentExperience = data.currentExperience;
            playerData.currentCurrency = data.currentCurrency;
            playerData.currentLevel = data.currentLevel;
            playerData.experienceMultiplier = data.experienceMultiplier;
            playerData.defaultPlayerDamage = data.defaultPlayerDamage;
            playerData.defaultPlayerSpeed = data.defaultPlayerSpeed;
            playerData.defaultAttackSpeed = data.defaultAttackSpeed;
            playerData.defaultMaxHP = data.defaultMaxHP;
            playerData.defaultProjectileSpeed = data.defaultProjectileSpeed;
            playerData.defaultProjectileRange = data.defaultProjectileRange;

            // �ɷ� ������ �ε�
            abilitiesData = data.abilitiesData;
            synergyAbilityData = data.synergyAbilityData;
            synergyLevels = data.synergyLevels;

            // **�ε�� abilitiesData�� PlayerAbilityManager�� ����**
            ApplyLoadedAbilities();

            Debug.Log("�÷��̾� �����Ͱ� �ε�Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("����� �÷��̾� �����Ͱ� �����ϴ�.");
        }
    }

    /// <summary>
    /// PlayerAbilityManager�� abilities ����Ʈ�� abilitiesData ����Ʈ�� ��ȯ�Ͽ� ����
    /// </summary>
    private void UpdateAbilitiesData()
    {
        if (playerAbilityManager != null)
        {
            abilitiesData = playerAbilityManager.GetAbilitiesData();
        }

        // �ʿ� �� ������ ����
        SavePlayerData();
    }

    /// <summary>
    /// �ε�� abilitiesData�� PlayerAbilityManager�� abilities ����Ʈ�� ����
    /// </summary>
    private void ApplyLoadedAbilities()
    {
        if (playerAbilityManager != null)
        {
            // ��� �ɷ��� �ʱ�ȭ
            playerAbilityManager.ResetAllAbilities();

            // ����� abilitiesData�� ����
            playerAbilityManager.ApplySavedAbilities(abilitiesData);

            // �ó��� �ɷ� ���� (�ʿ��ϴٸ�)
            if (synergyAbilityData != null && !string.IsNullOrEmpty(synergyAbilityData.abilityName))
            {
                // �ó��� �ɷ��� �ε��ϰ� ����
                Ability synergyAbilityTemplate = Resources.Load<Ability>($"Abilities/{synergyAbilityData.abilityName}");
                if (synergyAbilityTemplate != null)
                {
                    PlayerAbility synergyPlayerAbility = new PlayerAbility(synergyAbilityTemplate, synergyAbilityData.currentLevel);
                    playerAbilityManager.abilities.Add(synergyPlayerAbility);

                    synergyPlayerAbility.Apply(FindObjectOfType<Player>());
                }
                else
                {
                    Debug.LogWarning($"Synergy Ability '{synergyAbilityData.abilityName}'��(��) ã�� �� �����ϴ�.");
                }
            }

            // �ó��� ���� ����
            playerAbilityManager.synergyLevels = synergyLevels;
        }
        else
        {
            Debug.LogError("PlayerAbilityManager�� �������� �ʽ��ϴ�. �ɷ��� �ε��� �� �����ϴ�.");
        }
    }

    /// <summary>
    /// �÷��̾��� �ɷ� ������ ����Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    public List<AbilityData> GetAbilitiesData()
    {
        return abilitiesData;
    }
}
