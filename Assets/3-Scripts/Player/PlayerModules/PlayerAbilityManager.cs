using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro�� ����ϱ� ���� �߰�

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

    // �߰��� �κ�: �ó��� ȹ�� ���θ� �����ϴ� ����
    private bool hasAcquiredSynergy = false;

    // UI ���� ������
    private Coroutine selectionAnimationCoroutine;
    public event Action OnAbilitiesChanged;

    private int abilitiesAcquiredCount = 0;

    [SerializeField]
    private GameObject[] resultImages; // �ν����Ϳ��� �Ҵ�� �̹��� �迭

    [SerializeField]
    private TextMeshProUGUI[] levelTexts; // �ν����Ϳ��� �Ҵ�� ���� �ؽ�Ʈ �迭

    [Header("Synergy Button Resources")]
    [Tooltip("�� �ó��� ī�װ��� �����ϴ� ��ư ��������Ʈ �迭.")]
    [SerializeField]
    private string[] synergyCategories; // ��: "Lust", "Envy", "Sloth", etc.

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

        // ����� �ɷ� ������ �ε� �� ����
        PlayerDataManager dataManager = PlayerDataManager.Instance;
        if (dataManager != null)
        {
            List<AbilityData> savedAbilitiesData = dataManager.GetAbilitiesData();
            if (savedAbilitiesData != null && savedAbilitiesData.Count > 0)
            {
                ApplySavedAbilities(savedAbilitiesData);
            }
        }
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

                    // �ó��� �����Ƽ�� ��� ��ư ���ҽ� ����
                    if (playerAbility.ability is SynergyAbility synergyAbility)
                    {
                        SetSynergyButtonResource(synergyAbility);
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
    /// �ó��� �����Ƽ�� ��ư ���ҽ��� �����մϴ�.
    /// </summary>
    private void SetSynergyButtonResource(SynergyAbility synergyAbility)
    {
        string category = synergyAbility.category;

        if (synergyCategoryToSprite.ContainsKey(category))
        {
            Sprite buttonSprite = synergyCategoryToSprite[category];

            // AbilityManager�� ��ư ��������Ʈ�� �����Ͽ� UI�� ������Ʈ
            AbilityManager abilityManager = FindObjectOfType<AbilityManager>();
            if (abilityManager != null)
            {
                abilityManager.TriggerShowSynergyAbility(synergyAbility, buttonSprite);
                Debug.Log($"AbilityManager���� �ó��� �����Ƽ '{synergyAbility.abilityName}'�� ��������Ʈ '{buttonSprite.name}' ���޵�.");

                StartCoroutine(abilityManager.DelayedShowSynergyAbility(synergyAbility));
            }
            else
            {
                Debug.LogError("AbilityManager�� �������� �ʽ��ϴ�. �ó��� �����Ƽ UI�� ������Ʈ�� �� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning($"ī�װ� '{category}'�� ���� ��ư ��������Ʈ ������ �������� �ʽ��ϴ�.");
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

    public void CheckForSynergy(string category)
    {
        // �߰��� �κ�: �̹� �ó����� ȹ���� ��� �޼��� ����
        if (hasAcquiredSynergy)
        {
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
            return;
        }

        int totalLevel = 0;
        foreach (var playerAbility in abilities)
        {
            if (playerAbility.ability.category == category)
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
    }


    private void AssignSynergyAbility(string category, int level)
    {
        string synergyAbilityName = $"{category}Synergy{level}";
        SynergyAbility synergyAbility = Resources.Load<SynergyAbility>($"SynergyAbilities/{synergyAbilityName}");
        if (synergyAbility != null)
        {
            Debug.Log($"Synergy ability acquired: {synergyAbilityName}");

            // �߰��� �κ�: �ó��� ȹ�� ���θ� true�� ����
            hasAcquiredSynergy = true;

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
}
