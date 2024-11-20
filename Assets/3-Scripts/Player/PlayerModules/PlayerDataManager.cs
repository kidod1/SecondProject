using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

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

    // �÷��̾��� ���� ������
    public PlayerData playerData;

    // �÷��̾��� �ɷ� ������
    public List<AbilityData> abilitiesData = new List<AbilityData>();
    public SynergyAbilityData synergyAbilityData;
    public Dictionary<string, int> synergyLevels = new Dictionary<string, int>();

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

        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.dat");

        // �� �ε� �̺�Ʈ ����
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // �� �ε� �̺�Ʈ ���� ����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "11_LustMap")
        {
            LoadPlayerData();
        }
    }

    public void SavePlayerData()
    {
        // ���� ���Ϸ� ����
        using (FileStream fs = new FileStream(saveFilePath, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // �÷��̾� ���� ��Ȳ ����
                writer.Write(playerData.currentExperience);
                writer.Write(playerData.currentCurrency);
                writer.Write(playerData.currentLevel);
                writer.Write(playerData.currentHP);
                writer.Write(playerData.currentMaxHP);
                writer.Write(playerData.currentShield);

                // abilitiesData ����
                writer.Write(abilitiesData.Count);
                foreach (var ability in abilitiesData)
                {
                    writer.Write(ability.abilityName);
                    writer.Write(ability.currentLevel);
                    writer.Write(ability.category);
                }

                // synergyAbilityData ����
                bool hasSynergyAbility = synergyAbilityData != null && !string.IsNullOrEmpty(synergyAbilityData.abilityName);
                writer.Write(hasSynergyAbility);
                if (hasSynergyAbility)
                {
                    writer.Write(synergyAbilityData.abilityName);
                    writer.Write(synergyAbilityData.currentLevel);
                    writer.Write(synergyAbilityData.category);
                }

                // synergyLevels ����
                writer.Write(synergyLevels.Count);
                foreach (var kvp in synergyLevels)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }
            }
        }

        Debug.Log("�÷��̾� �����Ͱ� ����Ǿ����ϴ�.");
    }

    public void LoadPlayerData()
    {
        playerData.InitializeStats();
        if (File.Exists(saveFilePath))
        {
            using (FileStream fs = new FileStream(saveFilePath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    // �÷��̾� ���� ��Ȳ �ε�
                    playerData.currentExperience = reader.ReadInt32();
                    playerData.currentCurrency = reader.ReadInt32();
                    playerData.currentLevel = reader.ReadInt32();
                    playerData.currentHP = reader.ReadInt32();
                    playerData.currentMaxHP = reader.ReadInt32();
                    playerData.currentShield = reader.ReadInt32();

                    // abilitiesData �ε�
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

                    // synergyAbilityData �ε�
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

                    // synergyLevels �ε�
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

            Debug.Log("�÷��̾� �����Ͱ� �ε�Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("����� �÷��̾� �����Ͱ� �����ϴ�.");
        }
    }

    public List<AbilityData> GetAbilitiesData()
    {
        return abilitiesData;
    }

    public void SetAbilitiesData(List<AbilityData> data)
    {
        abilitiesData = data;
    }

    public Dictionary<string, int> GetSynergyLevels()
    {
        return synergyLevels;
    }

    public void SetSynergyLevels(Dictionary<string, int> levels)
    {
        synergyLevels = levels;
    }

    public void ResetPlayerData()
    {
        // �÷��̾� ���� ��Ȳ �ʱ�ȭ
        playerData.InitializeStats();
        playerData.currentExperience = 0;
        playerData.currentCurrency = 0;
        playerData.currentLevel = 1;
        playerData.currentHP = playerData.defaultMaxHP;
        playerData.currentMaxHP = playerData.defaultMaxHP;
        playerData.currentShield = 0;

        // �ɷ� ������ �ʱ�ȭ
        abilitiesData.Clear();
        synergyAbilityData = new SynergyAbilityData();
        synergyLevels.Clear();

        // ������ ����
        SavePlayerData();

        Debug.Log("�÷��̾� �����Ͱ� �ʱ�ȭ�Ǿ����ϴ�.");
    }

    private void OnApplicationQuit()
    {
        Debug.Log("������ ����˴ϴ�. �����͸� �����մϴ�.");
        SavePlayerData();
    }
}
