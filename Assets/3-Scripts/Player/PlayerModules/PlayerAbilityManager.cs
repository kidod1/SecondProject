using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro�� ����ϱ� ���� �߰�

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

    // �ɷ� ���� �� ȣ���� C# �̺�Ʈ�� ����
    public event System.Action OnAbilitiesChanged;

    // �ɷ� ȹ�� Ƚ���� �����ϱ� ���� ����
    private int abilitiesAcquiredCount = 0;

    [SerializeField]
    private GameObject[] resultImages; // �ν����Ϳ��� �Ҵ�� �̹��� �迭

    // **�߰��� �κ�: �� �̹����� �����ϴ� ���� �ؽ�Ʈ �迭**
    [SerializeField]
    private TextMeshProUGUI[] levelTexts; // �ν����Ϳ��� �Ҵ�� ���� �ؽ�Ʈ �迭

    // **�߰��� �κ�: �ó��� ī�װ��� ��ư ��������Ʈ ����**
    [Header("Synergy Button Resources")]
    [Tooltip("�� �ó��� ī�װ��� �����ϴ� ��ư ��������Ʈ �迭.")]
    [SerializeField]
    private string[] synergyCategories; // ��: "Lust", "Envy", "Sloth", etc.

    [Tooltip("�� �ó��� ī�װ��� �����ϴ� ��ư ��������Ʈ �迭.")]
    [SerializeField]
    private Sprite[] synergyButtonSprites; // ī�װ��� �´� ��������Ʈ �迭

    // Dictionary�� ���� �ɷ°� UI�� ����
    private Dictionary<Ability, int> abilityToImageIndex = new Dictionary<Ability, int>();

    // **�߰��� �κ�: ī�װ��� ��ư ��������Ʈ ��ųʸ�**
    private Dictionary<string, Sprite> synergyCategoryToSprite = new Dictionary<string, Sprite>();

    public void Initialize(Player player)
    {
        this.player = player;

        LoadAvailableAbilities();
        InitializeSynergyDictionaries();
        ResetAllAbilities();

        // ������ �ߺ� ��� ������ ���� ���� ������ �� �߰�
        player.OnHitEnemy.RemoveListener(ActivateAbilitiesOnHit);
        player.OnHitEnemy.AddListener(ActivateAbilitiesOnHit);

        // **�߰��� �κ�: �ó��� ī�װ��� ��������Ʈ ���� �ʱ�ȭ**
        InitializeSynergyCategoryToSprite();
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
                Debug.LogWarning($"Duplicate synergy category detected: {synergyCategories[i]}. Skipping.");
            }
        }
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
        bool isNewAbility = false; // ���ο� �ɷ� ���θ� �����ϴ� �÷���

        ability.Apply(player);

        if (ability.currentLevel == 0)
        {
            ability.currentLevel = 1;
            abilities.Add(ability);
            isNewAbility = true; // ���ο� �ɷ��� ȹ�������� ǥ��

            // �ɷ°� �̹��� �ε����� ����
            if (abilitiesAcquiredCount < resultImages.Length)
            {
                abilityToImageIndex[ability] = abilitiesAcquiredCount;
                abilitiesAcquiredCount++;
            }
            else
            {
                Debug.LogWarning("��� ��� �̹����� �̹� Ȱ��ȭ�Ǿ����ϴ�. �߰� �ɷ¿� ���� UI�� �����ϴ�.");
            }
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

        // �ɷ��� ����Ǿ����� �˸� (C# �̺�Ʈ ȣ��)
        OnAbilitiesChanged?.Invoke();

        // **�߰��� �κ�: ���ο� �ɷ� ȹ�� �� �̹��� Ȱ��ȭ**
        if (isNewAbility)
        {
            ActivateNextResultImage(ability);
        }
        else
        {
            // ���� �ɷ��� ���׷��̵�� ���, �ش� �ɷ��� ���� �ؽ�Ʈ ������Ʈ
            UpdateAbilityLevelText(ability);
        }

        // ������ �� �α� ��� (����� �뵵)
#if UNITY_EDITOR
        int listenerCount = OnAbilitiesChanged?.GetInvocationList().Length ?? 0;
        Debug.Log($"OnAbilitiesChanged Listener Count: {listenerCount}");
#endif
    }

    /// <summary>
    /// ���� ��� �̹����� Ȱ��ȭ�ϰ� ���� �ؽ�Ʈ�� �����ϴ� �޼���
    /// </summary>
    private void ActivateNextResultImage(Ability ability)
    {
        if (resultImages == null || resultImages.Length == 0)
        {
            Debug.LogWarning("resultImages �迭�� ����ְų� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        int imageIndex = abilityToImageIndex.ContainsKey(ability) ? abilityToImageIndex[ability] : -1;

        if (imageIndex >= 0 && imageIndex < resultImages.Length)
        {
            if (resultImages[imageIndex] != null)
            {
                resultImages[imageIndex].SetActive(true);

                // **���� �ؽ�Ʈ ����: "Lv " ���λ� �߰�**
                if (levelTexts != null && imageIndex < levelTexts.Length)
                {
                    if (levelTexts[imageIndex] != null)
                    {
                        levelTexts[imageIndex].text = $"Lv {ability.currentLevel}";
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

                // **�߰��� �κ�: �ó��� �����Ƽ�� ��� ��ư ���ҽ� ����**
                if (ability is SynergyAbility synergyAbility)
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

    /// <summary>
    /// �ó��� �����Ƽ�� ��ư ���ҽ��� �����ϴ� �޼���
    /// </summary>
    /// <param name="synergyAbility">�ó��� �����Ƽ ��ü</param>
    private void SetSynergyButtonResource(SynergyAbility synergyAbility)
    {
        string category = synergyAbility.category;

        if (synergyCategoryToSprite.ContainsKey(category))
        {
            Sprite buttonSprite = synergyCategoryToSprite[category];

            // AbilityManager�� ��ư ��������Ʈ�� �����Ͽ� UI�� ������Ʈ�ϵ��� �մϴ�.
            AbilityManager abilityManager = FindObjectOfType<AbilityManager>();
            if (abilityManager != null)
            {
                abilityManager.TriggerShowSynergyAbility(synergyAbility, buttonSprite);
                Debug.Log($"AbilityManager���� �ó��� �����Ƽ '{synergyAbility.abilityName}'�� ��������Ʈ '{buttonSprite.name}' ���޵�.");
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
    /// ���� �ɷ��� ���׷��̵�� �� �ش� �ɷ��� ���� �ؽ�Ʈ�� ������Ʈ�ϴ� �޼���
    /// </summary>
    private void UpdateAbilityLevelText(Ability ability)
    {
        if (abilityToImageIndex.ContainsKey(ability))
        {
            int imageIndex = abilityToImageIndex[ability];

            if (levelTexts != null && imageIndex < levelTexts.Length)
            {
                if (levelTexts[imageIndex] != null)
                {
                    levelTexts[imageIndex].text = $"Lv {ability.currentLevel}";
                    Debug.Log($"Result Image {imageIndex + 1}�� ���� �ؽ�Ʈ�� 'Lv {ability.currentLevel}'���� ������Ʈ��.");
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
    /// ��� �ɷ��� �ʱ�ȭ�ϰ� ��� �̹����� �����ϴ� �޼���
    /// </summary>
    public void ResetAllAbilities()
    {
        foreach (var ability in availableAbilities)
        {
            ability.ResetLevel();
        }
        abilities.Clear();

        // �̹��� ����
        ResetResultImages();

        // �ɷ��� ����Ǿ����� �˸�
        OnAbilitiesChanged?.Invoke();
    }

    /// <summary>
    /// ��� ��� �̹����� ��Ȱ��ȭ�ϰ� �ε����� �ʱ�ȭ�ϴ� �޼���
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

        // **���� �ؽ�Ʈ�� �ʱ�ȭ: "Lv " ���λ� ���� �� ���ڿ��� ����**
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

        abilitiesAcquiredCount = 0;
        abilityToImageIndex.Clear();
        Debug.Log("��� ��� �̹����� ��Ȱ��ȭ�ǰ� ���� �ؽ�Ʈ�� �ʱ�ȭ�Ǿ����ϴ�.");
    }

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
                parasiticNestAbility.OnProjectileHit(enemy); // ParasiticNest�� OnProjectileHit ȣ��
            }
        }
    }

    /// <summary>
    /// ���Ͱ� ������� �� �ɷ��� Ȱ��ȭ�ϴ� �޼���
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
            // �߰� �ʱ�ȭ�� �ʿ��� ��� ���⿡ �ۼ�
        }
    }

    public List<Ability> GetAvailableAbilities()
    {
        return availableAbilities;
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
                // �ó��� �����Ƽ�� ī�װ��� �´� ��������Ʈ ��������
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
