using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;

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

        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.dat");
        SceneManager.sceneLoaded += OnSceneLoaded;
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
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 전투 씬의 이름을 실제 이름으로 변경하세요
        if (scene.name == "11_LustMap")
        {
            LoadPlayerData();
            StartCoroutine(ApplyDataWithDelay());
        }
    }

    private IEnumerator ApplyDataWithDelay()
    {
        yield return null; // 한 프레임 대기

        ApplyLoadedDataToPlayer();
    }

    private void ApplyLoadedDataToPlayer()
    {
        // 씬에서 Player 객체를 찾습니다.
        Player player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("PlayerDataManager: Player 객체를 찾을 수 없습니다.");
            return;
        }

        // 플레이어의 스탯을 기본값으로 초기화
        player.stat.InitializeStats();

        // 플레이어의 능력 적용
        playerAbilityManager = player.GetComponent<PlayerAbilityManager>();
        if (playerAbilityManager != null)
        {
            // 능력 데이터 초기화
            playerAbilityManager.ResetAllAbilities();

            // 저장된 abilitiesData를 적용
            if (abilitiesData != null && abilitiesData.Count > 0)
            {
                playerAbilityManager.ApplySavedAbilities(abilitiesData);
            }

            // 시너지 능력 적용
            if (synergyAbilityData != null && !string.IsNullOrEmpty(synergyAbilityData.abilityName))
            {
                // 시너지 능력을 로드하고 적용
                SynergyAbility synergyAbilityTemplate = Resources.Load<SynergyAbility>($"SynergyAbilities/{synergyAbilityData.abilityName}");
                if (synergyAbilityTemplate != null)
                {
                    playerAbilityManager.ApplySynergyAbility(synergyAbilityTemplate);
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
            Debug.LogError("PlayerAbilityManager를 찾을 수 없습니다.");
        }

        // 플레이어의 진행 상황 적용
        player.stat.currentExperience = playerData.currentExperience;
        player.stat.currentCurrency = playerData.currentCurrency;
        player.stat.currentLevel = playerData.currentLevel;
        player.stat.currentHP = playerData.currentHP;
        player.stat.currentMaxHP = playerData.currentMaxHP;
        player.stat.currentShield = playerData.currentShield;

        // 플레이어의 UI 업데이트
        player.UpdateUI();
    }

    public void SavePlayerData()
    {
        // 이진 파일로 저장
        using (FileStream fs = new FileStream(saveFilePath, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // 플레이어 진행 상황 저장
                writer.Write(playerData.currentExperience);
                writer.Write(playerData.currentCurrency);
                writer.Write(playerData.currentLevel);
                writer.Write(playerData.currentHP);
                writer.Write(playerData.currentMaxHP);
                writer.Write(playerData.currentShield);

                // abilitiesData 저장
                writer.Write(abilitiesData.Count);
                foreach (var ability in abilitiesData)
                {
                    writer.Write(ability.abilityName);
                    writer.Write(ability.currentLevel);
                    writer.Write(ability.category);
                }

                // synergyAbilityData 저장
                bool hasSynergyAbility = synergyAbilityData != null && !string.IsNullOrEmpty(synergyAbilityData.abilityName);
                writer.Write(hasSynergyAbility);
                if (hasSynergyAbility)
                {
                    writer.Write(synergyAbilityData.abilityName);
                    writer.Write(synergyAbilityData.currentLevel);
                    writer.Write(synergyAbilityData.category);
                }

                // synergyLevels 저장
                writer.Write(synergyLevels.Count);
                foreach (var kvp in synergyLevels)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }
            }
        }

        Debug.Log("플레이어 데이터가 저장되었습니다.");
    }

    public void LoadPlayerData()
    {
        if (File.Exists(saveFilePath))
        {
            using (FileStream fs = new FileStream(saveFilePath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    // 플레이어 진행 상황 로드
                    playerData.currentExperience = reader.ReadInt32();
                    playerData.currentCurrency = reader.ReadInt32();
                    playerData.currentLevel = reader.ReadInt32();
                    playerData.currentHP = reader.ReadInt32();
                    playerData.currentMaxHP = reader.ReadInt32();
                    playerData.currentShield = reader.ReadInt32();

                    // abilitiesData 로드
                    int abilitiesCount = reader.ReadInt32();
                    abilitiesData = new List<AbilityData>();
                    for (int i = 0; i < abilitiesCount; i++)
                    {
                        AbilityData ability = new AbilityData();
                        ability.abilityName = reader.ReadString();
                        ability.currentLevel = reader.ReadInt32();
                        ability.category = reader.ReadString();
                        abilitiesData.Add(ability);
                    }

                    // synergyAbilityData 로드
                    bool hasSynergyAbility = reader.ReadBoolean();
                    if (hasSynergyAbility)
                    {
                        synergyAbilityData = new SynergyAbilityData();
                        synergyAbilityData.abilityName = reader.ReadString();
                        synergyAbilityData.currentLevel = reader.ReadInt32();
                        synergyAbilityData.category = reader.ReadString();
                    }
                    else
                    {
                        synergyAbilityData = null;
                    }

                    // synergyLevels 로드
                    int synergyLevelsCount = reader.ReadInt32();
                    synergyLevels = new Dictionary<string, int>();
                    for (int i = 0; i < synergyLevelsCount; i++)
                    {
                        string key = reader.ReadString();
                        int value = reader.ReadInt32();
                        synergyLevels.Add(key, value);
                    }
                }
            }

            Debug.Log("플레이어 데이터가 로드되었습니다.");
        }
        else
        {
            Debug.LogWarning("저장된 플레이어 데이터가 없습니다.");
        }
    }

    private void UpdateAbilitiesData()
    {
        if (playerAbilityManager != null)
        {
            abilitiesData = playerAbilityManager.GetAbilitiesData();
            synergyLevels = playerAbilityManager.synergyLevels;
        }
    }

    public List<AbilityData> GetAbilitiesData()
    {
        return abilitiesData;
    }

    public void ResetPlayerData()
    {
        // 플레이어 진행 상황 초기화
        playerData.currentExperience = 0;
        playerData.currentCurrency = 0;
        playerData.currentLevel = 1;
        playerData.currentHP = playerData.defaultMaxHP;
        playerData.currentMaxHP = playerData.defaultMaxHP;
        playerData.currentShield = 0;

        // 능력 데이터 초기화
        abilitiesData.Clear();
        synergyAbilityData = new SynergyAbilityData();
        synergyLevels.Clear();

        // 데이터 저장
        SavePlayerData();

        Debug.Log("플레이어 데이터가 초기화되었습니다.");
    }

    private void OnApplicationQuit()
    {
        Debug.Log("게임이 종료됩니다. 데이터를 저장합니다.");
        SavePlayerData();
    }
}
