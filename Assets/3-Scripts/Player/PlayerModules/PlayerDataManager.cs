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

        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.dat");
        SceneManager.sceneLoaded += OnSceneLoaded;
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
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���� ���� �̸��� ���� �̸����� �����ϼ���
        if (scene.name == "11_LustMap")
        {
            LoadPlayerData();
            StartCoroutine(ApplyDataWithDelay());
        }
    }

    private IEnumerator ApplyDataWithDelay()
    {
        yield return null; // �� ������ ���

        ApplyLoadedDataToPlayer();
    }

    private void ApplyLoadedDataToPlayer()
    {
        // ������ Player ��ü�� ã���ϴ�.
        Player player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("PlayerDataManager: Player ��ü�� ã�� �� �����ϴ�.");
            return;
        }

        // �÷��̾��� ������ �⺻������ �ʱ�ȭ
        player.stat.InitializeStats();

        // �÷��̾��� �ɷ� ����
        playerAbilityManager = player.GetComponent<PlayerAbilityManager>();
        if (playerAbilityManager != null)
        {
            // �ɷ� ������ �ʱ�ȭ
            playerAbilityManager.ResetAllAbilities();

            // ����� abilitiesData�� ����
            if (abilitiesData != null && abilitiesData.Count > 0)
            {
                playerAbilityManager.ApplySavedAbilities(abilitiesData);
            }

            // �ó��� �ɷ� ����
            if (synergyAbilityData != null && !string.IsNullOrEmpty(synergyAbilityData.abilityName))
            {
                // �ó��� �ɷ��� �ε��ϰ� ����
                SynergyAbility synergyAbilityTemplate = Resources.Load<SynergyAbility>($"SynergyAbilities/{synergyAbilityData.abilityName}");
                if (synergyAbilityTemplate != null)
                {
                    playerAbilityManager.ApplySynergyAbility(synergyAbilityTemplate);
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
            Debug.LogError("PlayerAbilityManager�� ã�� �� �����ϴ�.");
        }

        // �÷��̾��� ���� ��Ȳ ����
        player.stat.currentExperience = playerData.currentExperience;
        player.stat.currentCurrency = playerData.currentCurrency;
        player.stat.currentLevel = playerData.currentLevel;
        player.stat.currentHP = playerData.currentHP;
        player.stat.currentMaxHP = playerData.currentMaxHP;
        player.stat.currentShield = playerData.currentShield;

        // �÷��̾��� UI ������Ʈ
        player.UpdateUI();
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
        // �÷��̾� ���� ��Ȳ �ʱ�ȭ
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
