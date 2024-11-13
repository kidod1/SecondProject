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
    // PlayerData 스탯
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

    // 능력 데이터
    public List<AbilityData> abilitiesData;
    public SynergyAbilityData synergyAbilityData;
    public Dictionary<string, int> synergyLevels;
}

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    // 플레이어의 스탯 데이터
    public PlayerData playerData;

    // 플레이어의 능력 데이터
    public List<AbilityData> abilitiesData = new List<AbilityData>();
    public SynergyAbilityData synergyAbilityData;
    public Dictionary<string, int> synergyLevels = new Dictionary<string, int>();

    private PlayerAbilityManager playerAbilityManager;

    private string saveFilePath;

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 씬 전환 시 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");
    }

    private void Start()
    {
        // PlayerAbilityManager 찾기
        playerAbilityManager = FindObjectOfType<PlayerAbilityManager>();
        if (playerAbilityManager != null)
        {
            // 능력 변경 이벤트 구독
            playerAbilityManager.OnAbilitiesChanged += UpdateAbilitiesData;
        }
        else
        {
            Debug.LogError("PlayerAbilityManager를 찾을 수 없습니다.");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (playerAbilityManager != null)
        {
            playerAbilityManager.OnAbilitiesChanged -= UpdateAbilitiesData;
        }
    }

    /// <summary>
    /// 플레이어 데이터를 저장합니다.
    /// </summary>
    public void SavePlayerData()
    {
        // PlayerData의 현재 스탯 저장
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

            // 능력 데이터 저장
            abilitiesData = abilitiesData,
            synergyAbilityData = synergyAbilityData,
            synergyLevels = synergyLevels
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log("플레이어 데이터가 저장되었습니다.");
    }

    /// <summary>
    /// 플레이어 데이터를 로드합니다.
    /// </summary>
    public void LoadPlayerData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerDataToJson data = JsonUtility.FromJson<PlayerDataToJson>(json);

            // PlayerData의 현재 스탯 로드
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

            // 능력 데이터 로드
            abilitiesData = data.abilitiesData;
            synergyAbilityData = data.synergyAbilityData;
            synergyLevels = data.synergyLevels;

            // **로드된 abilitiesData를 PlayerAbilityManager에 적용**
            ApplyLoadedAbilities();

            Debug.Log("플레이어 데이터가 로드되었습니다.");
        }
        else
        {
            Debug.LogWarning("저장된 플레이어 데이터가 없습니다.");
        }
    }

    /// <summary>
    /// PlayerAbilityManager의 abilities 리스트를 abilitiesData 리스트로 변환하여 저장
    /// </summary>
    private void UpdateAbilitiesData()
    {
        if (playerAbilityManager != null)
        {
            abilitiesData = playerAbilityManager.GetAbilitiesData();
        }

        // 필요 시 데이터 저장
        SavePlayerData();
    }

    /// <summary>
    /// 로드된 abilitiesData를 PlayerAbilityManager의 abilities 리스트에 적용
    /// </summary>
    private void ApplyLoadedAbilities()
    {
        if (playerAbilityManager != null)
        {
            // 모든 능력을 초기화
            playerAbilityManager.ResetAllAbilities();

            // 저장된 abilitiesData를 적용
            playerAbilityManager.ApplySavedAbilities(abilitiesData);

            // 시너지 능력 적용 (필요하다면)
            if (synergyAbilityData != null && !string.IsNullOrEmpty(synergyAbilityData.abilityName))
            {
                // 시너지 능력을 로드하고 적용
                Ability synergyAbilityTemplate = Resources.Load<Ability>($"Abilities/{synergyAbilityData.abilityName}");
                if (synergyAbilityTemplate != null)
                {
                    PlayerAbility synergyPlayerAbility = new PlayerAbility(synergyAbilityTemplate, synergyAbilityData.currentLevel);
                    playerAbilityManager.abilities.Add(synergyPlayerAbility);

                    synergyPlayerAbility.Apply(FindObjectOfType<Player>());
                }
                else
                {
                    Debug.LogWarning($"Synergy Ability '{synergyAbilityData.abilityName}'을(를) 찾을 수 없습니다.");
                }
            }

            // 시너지 레벨 적용
            playerAbilityManager.synergyLevels = synergyLevels;
        }
        else
        {
            Debug.LogError("PlayerAbilityManager가 존재하지 않습니다. 능력을 로드할 수 없습니다.");
        }
    }

    /// <summary>
    /// 플레이어의 능력 데이터 리스트를 반환합니다.
    /// </summary>
    public List<AbilityData> GetAbilitiesData()
    {
        return abilitiesData;
    }
}
