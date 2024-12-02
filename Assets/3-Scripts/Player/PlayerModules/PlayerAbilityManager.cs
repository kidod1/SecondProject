using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerAbilityManager : MonoBehaviour
{
    private Player player;

    public List<PlayerAbility> abilities = new List<PlayerAbility>();
    private List<Ability> availableAbilities = new List<Ability>();

    // Ability �̸����� PlayerAbility�� ������ ã�� ���� ��ųʸ�
    private Dictionary<string, PlayerAbility> abilityNameToPlayerAbility = new Dictionary<string, PlayerAbility>();

    // �ó��� ���� ������
    private Dictionary<string, bool> synergyAbilityAcquired = new Dictionary<string, bool>();
    public Dictionary<string, int> synergyLevels = new Dictionary<string, int>();

    // �ó��� �ɷ� ����
    public SynergyAbility currentSynergyAbility; // ���� ȹ���� �ó��� �ɷ�

    public SynergyAbilityData loadedSynergyAbilityData; // �ε�� Synergy Ability �����͸� ������ ����

    private bool hasAcquiredSynergy = false;

    // UI ���� ������
    private Coroutine selectionAnimationCoroutine;
    public event Action OnAbilitiesChanged;

    private int abilitiesAcquiredCount = 0;

    [SerializeField]
    private GameObject[] resultImages; // �ν����Ϳ��� �Ҵ�� �̹��� �迭

    [SerializeField]
    private TextMeshProUGUI[] levelTexts; // �ν����Ϳ��� �Ҵ�� ���� �ؽ�Ʈ �迭

    [Header("Synergy Ability UI")]
    [Tooltip("�ó��� �ɷ� ������ �̹���")]
    public GameObject synergyAbilityIcon; // �ó��� �ɷ� ������ �̹���

    [Tooltip("�ó��� �ɷ� ��Ÿ�� �������� �г�")]
    public GameObject synergyCooldownOverlayPanel; // �ó��� �ɷ� ��Ÿ�� �������� �г�

    [Tooltip("�ó��� �ɷ� ��Ÿ�� �ؽ�Ʈ")]
    public TextMeshProUGUI synergyCooldownText; // �ó��� �ɷ� ��Ÿ�� �ؽ�Ʈ

    [Header("Synergy Button Resources")]
    [Tooltip("�� �ó��� ī�װ��� �����ϴ� ��ư ��������Ʈ �迭.")]
    [SerializeField]
    private string[] synergyCategories;

    [Tooltip("�� �ó��� ī�װ��� �����ϴ� ��ư ��������Ʈ �迭.")]
    [SerializeField]
    private Sprite[] synergyButtonSprites; // ī�װ��� �´� ��������Ʈ �迭

    private Dictionary<string, Sprite> synergyCategoryToSprite = new Dictionary<string, Sprite>();

    // Ability �̸��� �̹��� �ε����� �����ϱ� ���� ��ųʸ�
    private Dictionary<string, int> abilityToImageIndex = new Dictionary<string, int>();

    public List<PlayerAbility> GetPlayerAbilities()
    {
        return abilities;
    }

    private void Update()
    {
        // �ó��� �ɷ� ��Ÿ�� UI ������Ʈ
        UpdateSynergyCooldownUI();
    }

    public void Initialize(Player player)
    {
        this.player = player;

        LoadAvailableAbilities();
        InitializeSynergyDictionaries();
        ResetAllAbilities();

        // �÷��̾� �̺�Ʈ ������ ����
        player.OnHitEnemy.RemoveListener(ActivateAbilitiesOnHit);
        player.OnHitEnemy.AddListener(ActivateAbilitiesOnHit);

        InitializeSynergyCategoryToSprite();

        // PlayerDataManager�κ��� ����� �ɷ� ������ ��������
        PlayerDataManager dataManager = PlayerDataManager.Instance;
        if (dataManager != null)
        {
            List<AbilityData> savedAbilitiesData = dataManager.GetAbilitiesData();
            if (savedAbilitiesData != null && savedAbilitiesData.Count > 0)
            {
                ApplySavedAbilities(savedAbilitiesData);
            }

            // �ó��� ���� �ε�
            var savedSynergyLevels = dataManager.GetSynergyLevels();
            if (savedSynergyLevels != null && savedSynergyLevels.Count > 0)
            {
                // ���� synergyLevels�� ����� ������ ������Ʈ
                foreach (var kvp in savedSynergyLevels)
                {
                    if (synergyLevels.ContainsKey(kvp.Key))
                    {
                        synergyLevels[kvp.Key] = kvp.Value;
                    }
                    else
                    {
                        synergyLevels.Add(kvp.Key, kvp.Value);
                    }
                }
            }

            // �ε�� Synergy Ability �����͸� ����
            loadedSynergyAbilityData = dataManager.synergyAbilityData;

            // Synergy Ability ����
            ApplyLoadedSynergyAbility();
        }
    }
    private void UpdateSynergyAbilityData()
    {
        if (currentSynergyAbility != null)
        {
            SynergyAbilityData data = new SynergyAbilityData
            {
                assetName = currentSynergyAbility.assetName,
                currentLevel = currentSynergyAbility.currentLevel,
                category = currentSynergyAbility.category
            };
            PlayerDataManager.Instance.synergyAbilityData = data;
        }
        else
        {
            PlayerDataManager.Instance.synergyAbilityData = null;
        }
    }



    private void LoadAvailableAbilities()
    {
        // Resources �������� ��� Ability ScriptableObject�� �ε�
        Ability[] loadedAbilities = Resources.LoadAll<Ability>("Abilities");
        availableAbilities.AddRange(loadedAbilities);
    }

    public List<Ability> GetAvailableAbilities()
    {
        return availableAbilities;
    }

    /// <summary>
    /// ����� AbilityData ����Ʈ�� ������� �ɷ��� �����մϴ�.
    /// </summary>
    public void ApplySavedAbilities(List<AbilityData> savedAbilitiesData)
    {
        foreach (var abilityData in savedAbilitiesData)
        {
            Ability abilityTemplate = availableAbilities.Find(a => a.abilityName == abilityData.abilityName);
            if (abilityTemplate != null)
            {
                // Ability�� �ν��Ͻ��� �����Ͽ� PlayerAbility�� ����
                PlayerAbility playerAbility = new PlayerAbility(abilityTemplate, abilityData.currentLevel);
                abilities.Add(playerAbility);
                abilityNameToPlayerAbility.Add(playerAbility.ability.abilityName, playerAbility);

                // �ɷ� ����
                playerAbility.Apply(player);

                // UI ������Ʈ�� ���� �̹��� �ε��� ���� �� �̹��� Ȱ��ȭ
                if (abilitiesAcquiredCount < resultImages.Length)
                {
                    abilityToImageIndex[playerAbility.ability.abilityName] = abilitiesAcquiredCount;
                    abilitiesAcquiredCount++;

                    // UI ������Ʈ �޼��� ȣ��
                    ActivateNextResultImage(playerAbility);
                }
                else
                {
                    Debug.LogWarning("��� ��� �̹����� �̹� Ȱ��ȭ�Ǿ����ϴ�. �߰� �ɷ¿� ���� UI�� �����ϴ�.");
                }
            }
            else
            {
                Debug.LogWarning($"Ability '{abilityData.abilityName}'��(��) ã�� �� �����ϴ�.");
            }
        }

        OnAbilitiesChanged?.Invoke();
    }

    public void ApplyLoadedSynergyAbility()
    {
        if (loadedSynergyAbilityData != null && !string.IsNullOrEmpty(loadedSynergyAbilityData.assetName))
        {
            // assetName�� ���� ����� �α׷� ����մϴ�.
            string assetName = loadedSynergyAbilityData.assetName;
            Debug.Log($"Attempting to load Synergy Ability Asset: '{assetName}'");

            // Resources �������� SynergyAbility�� �ε��մϴ�.
            SynergyAbility loadedSynergyAbility = Resources.Load<SynergyAbility>($"SynergyAbilities/{assetName}");

            if (loadedSynergyAbility != null)
            {
                currentSynergyAbility = Instantiate(loadedSynergyAbility);
                currentSynergyAbility.currentLevel = loadedSynergyAbilityData.currentLevel;
                currentSynergyAbility.category = loadedSynergyAbilityData.category;

                // �÷��̾�� ����
                currentSynergyAbility.Apply(player);

                // �ó��� �ɷ� UI ������Ʈ
                UpdateSynergyAbilityUI();
            }
            else
            {
                Debug.LogError($"Synergy Ability Asset '{assetName}'��(��) �ε��� �� �����ϴ�. ��ο� ���� �̸��� Ȯ���ϼ���.");
            }
        }
        else
        {
            Debug.Log("No Synergy Ability data to load.");
        }
    }

    /// <summary>
    /// ���ο� �ɷ��� �����ϰų� ���� �ɷ��� ���׷��̵��մϴ�.
    /// </summary>
    public void SelectAbility(string abilityName)
    {
        Ability abilityTemplate = availableAbilities.Find(a => a.abilityName == abilityName);
        if (abilityTemplate == null)
        {
            Debug.LogError($"Ability '{abilityName}'��(��) �������� �ʽ��ϴ�.");
            return;
        }

        if (abilityNameToPlayerAbility.TryGetValue(abilityName, out PlayerAbility existingAbility))
        {
            existingAbility.Upgrade(player);

            // ���� �ɷ��� ���׷��̵�� ���, �ش� �ɷ��� ���� �ؽ�Ʈ ������Ʈ
            UpdateAbilityLevelText(existingAbility);
        }
        else
        {
            PlayerAbility newAbility = new PlayerAbility(abilityTemplate, 1);
            abilities.Add(newAbility);
            abilityNameToPlayerAbility.Add(newAbility.ability.abilityName, newAbility);
            newAbility.Apply(player);

            // �ɷ°� �̹��� �ε����� ����
            if (abilitiesAcquiredCount < resultImages.Length)
            {
                abilityToImageIndex[newAbility.ability.abilityName] = abilitiesAcquiredCount;
                abilitiesAcquiredCount++;
            }
            else
            {
                Debug.LogWarning("��� ��� �̹����� �̹� Ȱ��ȭ�Ǿ����ϴ�. �߰� �ɷ¿� ���� UI�� �����ϴ�.");
            }

            // ���ο� �ɷ� ȹ�� �� �̹��� Ȱ��ȭ
            ActivateNextResultImage(newAbility);
        }

        CheckForSynergy(abilityTemplate.category);

        // �ɷ��� ����Ǿ����Ƿ� PlayerDataManager�� ������Ʈ
        UpdateAbilitiesData();

        OnAbilitiesChanged?.Invoke();
    }

    /// <summary>
    /// ���� ��� �̹����� Ȱ��ȭ�ϰ� ���� �ؽ�Ʈ�� �����մϴ�.
    /// </summary>
    private void ActivateNextResultImage(PlayerAbility playerAbility)
    {
        if (resultImages == null || resultImages.Length == 0)
        {
            Debug.LogWarning("resultImages �迭�� ����ְų� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        string abilityName = playerAbility.ability.abilityName;

        if (abilityToImageIndex.TryGetValue(abilityName, out int imageIndex))
        {
            if (imageIndex >= 0 && imageIndex < resultImages.Length)
            {
                if (resultImages[imageIndex] != null)
                {
                    resultImages[imageIndex].SetActive(true);

                    // ���� �ؽ�Ʈ ����
                    if (levelTexts != null && imageIndex < levelTexts.Length)
                    {
                        if (levelTexts[imageIndex] != null)
                        {
                            levelTexts[imageIndex].text = $"Lv {playerAbility.currentLevel}";
                        }
                        else
                        {
                            Debug.LogWarning($"levelTexts[{imageIndex}]�� �Ҵ���� �ʾҽ��ϴ�.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("levelTexts �迭�� ����ְų� �ε����� �ʰ��Ǿ����ϴ�.");
                    }
                }
                else
                {
                    Debug.LogWarning($"resultImages[{imageIndex}]�� �Ҵ���� �ʾҽ��ϴ�.");
                }
            }
            else
            {
                Debug.LogWarning("�ش� �ɷ¿� ���� �̹��� �ε����� ��ȿ���� �ʽ��ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("�ش� �ɷ¿� ���� �̹��� �ε����� ���ε��� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// �ó��� �ɷ� UI�� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdateSynergyAbilityUI()
    {
        if (synergyAbilityIcon != null && currentSynergyAbility != null)
        {
            // ������ �̹��� ����
            Image iconImage = synergyAbilityIcon.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = currentSynergyAbility.abilityIcon;
                iconImage.gameObject.SetActive(true);
            }

            // ��Ÿ�� �������� �г� �ʱ�ȭ
            if (synergyCooldownOverlayPanel != null)
            {
                synergyCooldownOverlayPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("synergyAbilityIcon �Ǵ� currentSynergyAbility�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// �ó��� �ɷ��� ��Ÿ�� UI�� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdateSynergyCooldownUI()
    {
        if (currentSynergyAbility != null)
        {
            if (currentSynergyAbility.IsReady)
            {
                // ��Ÿ�� �Ϸ� - �������� �г� ��Ȱ��ȭ
                if (synergyCooldownOverlayPanel != null)
                {
                    synergyCooldownText.gameObject.SetActive(false);
                    synergyCooldownOverlayPanel.SetActive(false);
                }
            }
            else
            {
                // ��Ÿ�� ���� �� - �������� �г� Ȱ��ȭ �� ���� �ð� ǥ��
                if (synergyCooldownOverlayPanel != null)
                {
                    synergyCooldownText.gameObject.SetActive(true);
                    synergyCooldownOverlayPanel.SetActive(true);

                    if (synergyCooldownText != null)
                    {
                        float remainingCooldown = currentSynergyAbility.cooldownDuration - (Time.time - currentSynergyAbility.lastUsedTime);
                        remainingCooldown = Mathf.Max(0, remainingCooldown); // ���� �ð��� 0���� �۾����� �ʵ��� ����
                        synergyCooldownText.text = Mathf.Ceil(remainingCooldown).ToString();
                    }
                }
            }
        }
    }

    /// <summary>
    /// ���� �ɷ��� ���׷��̵�� �� �ش� �ɷ��� ���� �ؽ�Ʈ�� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdateAbilityLevelText(PlayerAbility playerAbility)
    {
        string abilityName = playerAbility.ability.abilityName;

        if (abilityToImageIndex.TryGetValue(abilityName, out int imageIndex))
        {
            if (levelTexts != null && imageIndex < levelTexts.Length)
            {
                if (levelTexts[imageIndex] != null)
                {
                    levelTexts[imageIndex].text = $"Lv {playerAbility.currentLevel}";
                    Debug.Log($"Result Image {imageIndex + 1}�� ���� �ؽ�Ʈ�� 'Lv {playerAbility.currentLevel}'���� ������Ʈ��.");
                }
                else
                {
                    Debug.LogWarning($"levelTexts[{imageIndex}]�� �Ҵ���� �ʾҽ��ϴ�.");
                }
            }
            else
            {
                Debug.LogWarning("levelTexts �迭�� ����ְų� �ε����� �ʰ��Ǿ����ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("�ش� �ɷ¿� ���� �̹��� �ε����� ���ε��� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// ��� �ɷ��� �ʱ�ȭ�ϰ� ��� �̹����� �����մϴ�.
    /// </summary>
    public void ResetAllAbilities()
    {
        abilities.Clear();
        abilityNameToPlayerAbility.Clear();
        abilitiesAcquiredCount = 0;

        ResetResultImages();

        // �߰��� �κ�: �ó��� ȹ�� ���θ� �ʱ�ȭ
        hasAcquiredSynergy = false;

        // �ó��� �ɷ� �ʱ�ȭ
        currentSynergyAbility = null;

        OnAbilitiesChanged?.Invoke();
    }

    /// <summary>
    /// ��� ��� �̹����� ��Ȱ��ȭ�ϰ� �ε����� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void ResetResultImages()
    {
        if (resultImages == null || resultImages.Length == 0)
        {
            Debug.LogWarning("resultImages �迭�� ����ְų� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        for (int i = 0; i < resultImages.Length; i++)
        {
            if (resultImages[i] != null)
            {
                resultImages[i].SetActive(false);
            }
        }

        if (levelTexts != null)
        {
            for (int i = 0; i < levelTexts.Length; i++)
            {
                if (levelTexts[i] != null)
                {
                    levelTexts[i].text = "";
                }
            }
        }

        abilityToImageIndex.Clear();
        Debug.Log("��� ��� �̹����� ��Ȱ��ȭ�ǰ� ���� �ؽ�Ʈ�� �ʱ�ȭ�Ǿ����ϴ�.");
    }

    /// <summary>
    /// �÷��̾��� �ɷ� �����͸� AbilityData ����Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    public List<AbilityData> GetAbilitiesData()
    {
        List<AbilityData> dataList = new List<AbilityData>();
        foreach (var playerAbility in abilities)
        {
            AbilityData data = new AbilityData
            {
                abilityName = playerAbility.ability.abilityName,
                currentLevel = playerAbility.currentLevel,
                category = playerAbility.ability.category
            };
            dataList.Add(data);
        }
        return dataList;
    }

    /// <summary>
    /// �ɷ��� ����Ǿ��� �� PlayerDataManager�� �����͸� ������Ʈ�մϴ�.
    /// </summary>
    public void UpdateAbilitiesData()
    {
        // abilitiesData ������Ʈ
        List<AbilityData> updatedAbilitiesData = GetAbilitiesData();

        // PlayerDataManager�� ����
        PlayerDataManager dataManager = PlayerDataManager.Instance;
        if (dataManager != null)
        {
            dataManager.SetAbilitiesData(updatedAbilitiesData);
            dataManager.SetSynergyLevels(synergyLevels);
            UpdateSynergyAbilityData();
        }
    }


    public void ActivateAbilitiesOnHit(Collider2D enemy)
    {
        foreach (var playerAbility in abilities)
        {
            // �� �ɷ¿� ���� OnProjectileHit �Ǵ� OnHitMonster �޼��� ȣ��
            if (playerAbility.ability is FlashBlade flashBladeAbility)
            {
                flashBladeAbility.OnProjectileHit(enemy);
            }
            else if (playerAbility.ability is JokerDraw jokerDrawAbility)
            {
                jokerDrawAbility.OnHitMonster(enemy);
            }
            else if (playerAbility.ability is CardStrike cardStrikeAbility)
            {
                cardStrikeAbility.OnProjectileHit(enemy);
            }
            else if (playerAbility.ability is RicochetStrike ricochetAbility)
            {
                ricochetAbility.OnProjectileHit(enemy);
            }
            else if (playerAbility.ability is SharkStrike sharkStrikeAbility)
            {
                sharkStrikeAbility.OnProjectileHit(enemy);
            }
            else if (playerAbility.ability is ParasiticNest parasiticNestAbility)
            {
                parasiticNestAbility.OnProjectileHit(enemy);
            }
        }
    }

    /// <summary>
    /// ���Ͱ� ������� �� �ɷ��� Ȱ��ȭ�մϴ�.
    /// </summary>
    public void ActivateAbilitiesOnMonsterDeath(Monster monster)
    {
        foreach (var playerAbility in abilities)
        {
            if (playerAbility.ability is HoneyDrop honeyDrop)
            {
                Debug.Log($"Activating OnMonsterDeath for ability: {playerAbility.ability.abilityName}");
                honeyDrop.OnMonsterDeath(monster);
            }

            if (playerAbility.ability is KillSpeedBoostAbility killSpeedBoostAbility)
            {
                killSpeedBoostAbility.OnMonsterKilled();
            }
        }
    }

    private void InitializeSynergyDictionaries()
    {
        synergyAbilityAcquired = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
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

        synergyLevels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
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


    private void InitializeSynergyCategoryToSprite()
    {
        if (synergyCategories.Length != synergyButtonSprites.Length)
        {
            Debug.LogError("synergyCategories�� synergyButtonSprites �迭�� ���̰� ��ġ���� �ʽ��ϴ�.");
            return;
        }

        for (int i = 0; i < synergyCategories.Length; i++)
        {
            if (!synergyCategoryToSprite.ContainsKey(synergyCategories[i]))
            {
                synergyCategoryToSprite.Add(synergyCategories[i], synergyButtonSprites[i]);
            }
            else
            {
                Debug.LogWarning($"�ߺ��� �ó��� ī�װ��� �����Ǿ����ϴ�: {synergyCategories[i]}");
            }
        }
    }

    public void CheckForSynergy(string category)
    {
        Debug.Log($"CheckForSynergy ȣ���. ī�װ�: {category}");

        // ���� synergyAbilityAcquired ��ųʸ��� Ű���� �α׷� ���
        if (synergyAbilityAcquired != null)
        {
            Debug.Log("synergyAbilityAcquired keys: " + string.Join(", ", synergyAbilityAcquired.Keys));
        }
        else
        {
            Debug.LogError("synergyAbilityAcquired ��ųʸ��� null�Դϴ�.");
        }

        // ���� synergyLevels ��ųʸ��� Ű���� �α׷� ���
        if (synergyLevels != null)
        {
            Debug.Log("synergyLevels keys: " + string.Join(", ", synergyLevels.Keys));
        }
        else
        {
            Debug.LogError("synergyLevels ��ųʸ��� null�Դϴ�.");
        }

        // �ó��� ȹ�� ���θ� Ȯ��
        if (hasAcquiredSynergy)
        {
            Debug.Log("�̹� �ó����� ȹ���߽��ϴ�. �޼��带 �����մϴ�.");
            return;
        }

        if (string.IsNullOrEmpty(category))
        {
            Debug.LogError("CheckForSynergy���� category�� null�̰ų� �� ���ڿ��Դϴ�.");
            return;
        }

        if (!synergyAbilityAcquired.ContainsKey(category) || !synergyLevels.ContainsKey(category))
        {
            Debug.LogError($"ī�װ� '{category}'�� synergyAbilityAcquired �Ǵ� synergyLevels ��ųʸ��� �������� �ʽ��ϴ�.");

            // �߰�: ���� ��ųʸ��� ���� ī�װ��� ��� �⺻���� �߰��Ͽ� ������ ����
            if (!synergyAbilityAcquired.ContainsKey(category))
            {
                synergyAbilityAcquired[category] = false;
                Debug.Log($"synergyAbilityAcquired ��ųʸ��� ī�װ� '{category}'�� �⺻������ �߰��߽��ϴ�.");
            }

            if (!synergyLevels.ContainsKey(category))
            {
                synergyLevels[category] = 0;
                Debug.Log($"synergyLevels ��ųʸ��� ī�װ� '{category}'�� �⺻������ �߰��߽��ϴ�.");
            }

            return;
        }

        int totalLevel = 0;
        foreach (var playerAbility in abilities)
        {
            if (playerAbility.ability.category.Equals(category, StringComparison.OrdinalIgnoreCase))
            {
                totalLevel += playerAbility.currentLevel;
            }
        }

        Debug.Log($"ī�װ�: {category}, �� ����: {totalLevel}");

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

        // �ó��� ������ ����Ǿ����Ƿ� PlayerDataManager�� ������Ʈ
        UpdateAbilitiesData();
    }



    private void AssignSynergyAbility(string category, int level)
    {
        string synergyAbilityName = $"{category}Synergy{level}";
        SynergyAbility synergyAbility = Resources.Load<SynergyAbility>($"SynergyAbilities/{synergyAbilityName}");
        if (synergyAbility != null)
        {
            Debug.Log($"Synergy ability acquired: {synergyAbilityName}");

            // �ó��� �ɷ� ����
            currentSynergyAbility = synergyAbility;

            // �ó��� �ɷ� UI ������Ʈ
            UpdateSynergyAbilityUI();

            // �߰��� �κ�: �ó��� ȹ�� ���θ� true�� ����
            hasAcquiredSynergy = true;

            // AbilityManager�� ��ư ��������Ʈ�� �����Ͽ� UI�� ������Ʈ
            AbilityManager abilityManager = FindObjectOfType<AbilityManager>();
            if (abilityManager != null)
            {
                if (synergyCategoryToSprite.TryGetValue(category, out Sprite buttonSprite))
                {
                    abilityManager.TriggerShowSynergyAbility(synergyAbility, buttonSprite);
                    Debug.Log($"AbilityManager���� �ó��� �����Ƽ '{synergyAbilityName}'�� ��������Ʈ '{buttonSprite.name}' ���޵�.");

                    StartCoroutine(abilityManager.DelayedShowSynergyAbility(synergyAbility));
                }
                else
                {
                    Debug.LogWarning($"ī�װ� '{category}'�� ���� ��ư ��������Ʈ ������ �������� �ʽ��ϴ�.");
                }
            }
            else
            {
                Debug.LogError("AbilityManager�� �������� �ʽ��ϴ�. �ó��� �����Ƽ UI�� ������Ʈ�� �� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogError($"Failed to load Synergy Ability: {synergyAbilityName}");
        }
    }

    public int GetCurrentLevel(string abilityName)
    {
        if (abilityNameToPlayerAbility.TryGetValue(abilityName, out PlayerAbility playerAbility))
        {
            return playerAbility.currentLevel;
        }
        else
        {
            return 0; // �÷��̾ �ش� �ɷ��� ������ ���� ����
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
            Debug.LogError("PlayerAbilityManager: synergyAbility�� null�Դϴ�.");
        }
    }

    public T GetAbilityOfType<T>() where T : Ability
    {
        foreach (var playerAbility in abilities)
        {
            if (playerAbility.ability is T)
            {
                return playerAbility.ability as T;
            }
        }
        return null;
    }

    /// <summary>
    /// �ó��� �ɷ��� Ȱ��ȭ�մϴ�.
    /// </summary>
    public void ActivateSynergyAbility()
    {
        if (currentSynergyAbility != null)
        {
            if (currentSynergyAbility.IsReady)
            {
                currentSynergyAbility.Activate(player);

                // ��Ÿ�� UI ������Ʈ
                UpdateSynergyCooldownUI();
            }
            else
            {
                Debug.Log("�ó��� �ɷ��� ���� ��Ÿ���Դϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("���� �ó��� �ɷ��� �����ϴ�.");
        }
    }
}
