using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 추가

public class PlayerAbilityManager : MonoBehaviour
{
    private Player player;

    public List<PlayerAbility> abilities = new List<PlayerAbility>();
    private List<Ability> availableAbilities = new List<Ability>();

    // Ability 이름으로 PlayerAbility를 빠르게 찾기 위한 딕셔너리
    private Dictionary<string, PlayerAbility> abilityNameToPlayerAbility = new Dictionary<string, PlayerAbility>();

    // 시너지 관련 변수들
    private Dictionary<string, bool> synergyAbilityAcquired = new Dictionary<string, bool>();
    public Dictionary<string, int> synergyLevels = new Dictionary<string, int>();

    // 추가된 부분: 시너지 획득 여부를 추적하는 변수
    private bool hasAcquiredSynergy = false;

    // UI 관련 변수들
    private Coroutine selectionAnimationCoroutine;
    public event Action OnAbilitiesChanged;

    private int abilitiesAcquiredCount = 0;

    [SerializeField]
    private GameObject[] resultImages; // 인스펙터에서 할당된 이미지 배열

    [SerializeField]
    private TextMeshProUGUI[] levelTexts; // 인스펙터에서 할당된 레벨 텍스트 배열

    [Header("Synergy Button Resources")]
    [Tooltip("각 시너지 카테고리에 대응하는 버튼 스프라이트 배열.")]
    [SerializeField]
    private string[] synergyCategories; // 예: "Lust", "Envy", "Sloth", etc.

    [Tooltip("각 시너지 카테고리에 대응하는 버튼 스프라이트 배열.")]
    [SerializeField]
    private Sprite[] synergyButtonSprites; // 카테고리에 맞는 스프라이트 배열

    private Dictionary<string, Sprite> synergyCategoryToSprite = new Dictionary<string, Sprite>();

    // Ability 이름과 이미지 인덱스를 매핑하기 위한 딕셔너리
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

        // 플레이어 이벤트 리스너 설정
        player.OnHitEnemy.RemoveListener(ActivateAbilitiesOnHit);
        player.OnHitEnemy.AddListener(ActivateAbilitiesOnHit);

        InitializeSynergyCategoryToSprite();

        // 저장된 능력 데이터 로드 및 적용
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
            Debug.LogError("synergyCategories와 synergyButtonSprites 배열의 길이가 일치하지 않습니다.");
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
                Debug.LogWarning($"중복된 시너지 카테고리가 감지되었습니다: {synergyCategories[i]}");
            }
        }
    }

    private void LoadAvailableAbilities()
    {
        // Resources 폴더에서 모든 Ability ScriptableObject를 로드
        Ability[] loadedAbilities = Resources.LoadAll<Ability>("Abilities");
        availableAbilities.AddRange(loadedAbilities);
    }
    public List<Ability> GetAvailableAbilities()
    {
        return availableAbilities;
    }

    /// <summary>
    /// 저장된 AbilityData 리스트를 기반으로 능력을 적용합니다.
    /// </summary>
    public void ApplySavedAbilities(List<AbilityData> savedAbilitiesData)
    {
        foreach (var abilityData in savedAbilitiesData)
        {
            Ability abilityTemplate = availableAbilities.Find(a => a.abilityName == abilityData.abilityName);
            if (abilityTemplate != null)
            {
                // Ability의 인스턴스를 생성하여 PlayerAbility로 관리
                PlayerAbility playerAbility = new PlayerAbility(abilityTemplate, abilityData.currentLevel);
                abilities.Add(playerAbility);
                abilityNameToPlayerAbility.Add(playerAbility.ability.abilityName, playerAbility);

                // 능력 적용
                playerAbility.Apply(player);

                // UI 업데이트를 위해 이미지 인덱스 매핑 및 이미지 활성화
                if (abilitiesAcquiredCount < resultImages.Length)
                {
                    abilityToImageIndex[playerAbility.ability.abilityName] = abilitiesAcquiredCount;
                    abilitiesAcquiredCount++;

                    // UI 업데이트 메서드 호출
                    ActivateNextResultImage(playerAbility);
                }
                else
                {
                    Debug.LogWarning("모든 결과 이미지가 이미 활성화되었습니다. 추가 능력에 대한 UI가 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning($"Ability '{abilityData.abilityName}'을(를) 찾을 수 없습니다.");
            }
        }

        OnAbilitiesChanged?.Invoke();
    }


    /// <summary>
    /// 새로운 능력을 선택하거나 기존 능력을 업그레이드합니다.
    /// </summary>
    public void SelectAbility(string abilityName)
    {
        Ability abilityTemplate = availableAbilities.Find(a => a.abilityName == abilityName);
        if (abilityTemplate == null)
        {
            Debug.LogError($"Ability '{abilityName}'이(가) 존재하지 않습니다.");
            return;
        }

        if (abilityNameToPlayerAbility.TryGetValue(abilityName, out PlayerAbility existingAbility))
        {
            existingAbility.Upgrade(player);

            // 기존 능력이 업그레이드된 경우, 해당 능력의 레벨 텍스트 업데이트
            UpdateAbilityLevelText(existingAbility);
        }
        else
        {
            PlayerAbility newAbility = new PlayerAbility(abilityTemplate, 1);
            abilities.Add(newAbility);
            abilityNameToPlayerAbility.Add(newAbility.ability.abilityName, newAbility);
            newAbility.Apply(player);

            // 능력과 이미지 인덱스를 매핑
            if (abilitiesAcquiredCount < resultImages.Length)
            {
                abilityToImageIndex[newAbility.ability.abilityName] = abilitiesAcquiredCount;
                abilitiesAcquiredCount++;
            }
            else
            {
                Debug.LogWarning("모든 결과 이미지가 이미 활성화되었습니다. 추가 능력에 대한 UI가 없습니다.");
            }

            // 새로운 능력 획득 시 이미지 활성화
            ActivateNextResultImage(newAbility);
        }

        CheckForSynergy(abilityTemplate.category);

        OnAbilitiesChanged?.Invoke();
    }

    /// <summary>
    /// 다음 결과 이미지를 활성화하고 레벨 텍스트를 설정합니다.
    /// </summary>
    private void ActivateNextResultImage(PlayerAbility playerAbility)
    {
        if (resultImages == null || resultImages.Length == 0)
        {
            Debug.LogWarning("resultImages 배열이 비어있거나 할당되지 않았습니다.");
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

                    // 레벨 텍스트 설정
                    if (levelTexts != null && imageIndex < levelTexts.Length)
                    {
                        if (levelTexts[imageIndex] != null)
                        {
                            levelTexts[imageIndex].text = $"Lv {playerAbility.currentLevel}";
                        }
                        else
                        {
                            Debug.LogWarning($"levelTexts[{imageIndex}]가 할당되지 않았습니다.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("levelTexts 배열이 비어있거나 인덱스가 초과되었습니다.");
                    }

                    // 시너지 어빌리티인 경우 버튼 리소스 설정
                    if (playerAbility.ability is SynergyAbility synergyAbility)
                    {
                        SetSynergyButtonResource(synergyAbility);
                    }
                }
                else
                {
                    Debug.LogWarning($"resultImages[{imageIndex}]가 할당되지 않았습니다.");
                }
            }
            else
            {
                Debug.LogWarning("해당 능력에 대한 이미지 인덱스가 유효하지 않습니다.");
            }
        }
        else
        {
            Debug.LogWarning("해당 능력에 대한 이미지 인덱스가 매핑되지 않았습니다.");
        }
    }

    /// <summary>
    /// 시너지 어빌리티의 버튼 리소스를 설정합니다.
    /// </summary>
    private void SetSynergyButtonResource(SynergyAbility synergyAbility)
    {
        string category = synergyAbility.category;

        if (synergyCategoryToSprite.ContainsKey(category))
        {
            Sprite buttonSprite = synergyCategoryToSprite[category];

            // AbilityManager에 버튼 스프라이트를 전달하여 UI를 업데이트
            AbilityManager abilityManager = FindObjectOfType<AbilityManager>();
            if (abilityManager != null)
            {
                abilityManager.TriggerShowSynergyAbility(synergyAbility, buttonSprite);
                Debug.Log($"AbilityManager에게 시너지 어빌리티 '{synergyAbility.abilityName}'과 스프라이트 '{buttonSprite.name}' 전달됨.");

                StartCoroutine(abilityManager.DelayedShowSynergyAbility(synergyAbility));
            }
            else
            {
                Debug.LogError("AbilityManager가 존재하지 않습니다. 시너지 어빌리티 UI를 업데이트할 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"카테고리 '{category}'에 대한 버튼 스프라이트 매핑이 존재하지 않습니다.");
        }
    }

    /// <summary>
    /// 기존 능력이 업그레이드될 때 해당 능력의 레벨 텍스트를 업데이트합니다.
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
                    Debug.Log($"Result Image {imageIndex + 1}의 레벨 텍스트가 'Lv {playerAbility.currentLevel}'으로 업데이트됨.");
                }
                else
                {
                    Debug.LogWarning($"levelTexts[{imageIndex}]가 할당되지 않았습니다.");
                }
            }
            else
            {
                Debug.LogWarning("levelTexts 배열이 비어있거나 인덱스가 초과되었습니다.");
            }
        }
        else
        {
            Debug.LogWarning("해당 능력에 대한 이미지 인덱스가 매핑되지 않았습니다.");
        }
    }

    /// <summary>
    /// 모든 능력을 초기화하고 결과 이미지를 리셋합니다.
    /// </summary>
    public void ResetAllAbilities()
    {
        abilities.Clear();
        abilityNameToPlayerAbility.Clear();
        abilitiesAcquiredCount = 0;

        ResetResultImages();

        // 추가된 부분: 시너지 획득 여부를 초기화
        hasAcquiredSynergy = false;

        OnAbilitiesChanged?.Invoke();
    }

    /// <summary>
    /// 모든 결과 이미지를 비활성화하고 인덱스를 초기화합니다.
    /// </summary>
    private void ResetResultImages()
    {
        if (resultImages == null || resultImages.Length == 0)
        {
            Debug.LogWarning("resultImages 배열이 비어있거나 할당되지 않았습니다.");
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
        Debug.Log("모든 결과 이미지가 비활성화되고 레벨 텍스트가 초기화되었습니다.");
    }

    /// <summary>
    /// 플레이어의 능력 데이터를 AbilityData 리스트로 반환합니다.
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
            // 각 능력에 대한 OnProjectileHit 또는 OnHitMonster 메서드 호출
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
    /// 몬스터가 사망했을 때 능력을 활성화합니다.
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
        // 추가된 부분: 이미 시너지를 획득한 경우 메서드 종료
        if (hasAcquiredSynergy)
        {
            return;
        }

        if (string.IsNullOrEmpty(category))
        {
            Debug.LogError("CheckForSynergy에서 category가 null이거나 빈 문자열입니다.");
            return;
        }

        if (!synergyAbilityAcquired.ContainsKey(category) || !synergyLevels.ContainsKey(category))
        {
            Debug.LogError($"카테고리 '{category}'가 synergyAbilityAcquired 또는 synergyLevels 딕셔너리에 존재하지 않습니다.");
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

        Debug.Log($"카테고리: {category}, 총 레벨: {totalLevel}");

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

            // 추가된 부분: 시너지 획득 여부를 true로 설정
            hasAcquiredSynergy = true;

            AbilityManager abilityManager = FindObjectOfType<AbilityManager>();
            if (abilityManager != null)
            {
                if (synergyCategoryToSprite.TryGetValue(category, out Sprite buttonSprite))
                {
                    abilityManager.TriggerShowSynergyAbility(synergyAbility, buttonSprite);
                    Debug.Log($"AbilityManager에게 시너지 어빌리티 '{synergyAbilityName}'과 스프라이트 '{buttonSprite.name}' 전달됨.");

                    StartCoroutine(abilityManager.DelayedShowSynergyAbility(synergyAbility));
                }
                else
                {
                    Debug.LogWarning($"카테고리 '{category}'에 대한 버튼 스프라이트 매핑이 존재하지 않습니다.");
                }
            }
            else
            {
                Debug.LogError("AbilityManager가 존재하지 않습니다. 시너지 어빌리티 UI를 업데이트할 수 없습니다.");
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
            return 0; // 플레이어가 해당 능력을 가지고 있지 않음
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
            Debug.LogError("PlayerAbilityManager: synergyAbility가 null입니다.");
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
