using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 추가

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

    // 능력 변경 시 호출할 C# 이벤트로 변경
    public event System.Action OnAbilitiesChanged;

    // 능력 획득 횟수를 추적하기 위한 변수
    private int abilitiesAcquiredCount = 0;

    [SerializeField]
    private GameObject[] resultImages; // 인스펙터에서 할당된 이미지 배열

    // **추가된 부분: 각 이미지에 대응하는 레벨 텍스트 배열**
    [SerializeField]
    private TextMeshProUGUI[] levelTexts; // 인스펙터에서 할당된 레벨 텍스트 배열

    // **추가된 부분: 시너지 카테고리별 버튼 스프라이트 매핑**
    [Header("Synergy Button Resources")]
    [Tooltip("각 시너지 카테고리에 대응하는 버튼 스프라이트 배열.")]
    [SerializeField]
    private string[] synergyCategories; // 예: "Lust", "Envy", "Sloth", etc.

    [Tooltip("각 시너지 카테고리에 대응하는 버튼 스프라이트 배열.")]
    [SerializeField]
    private Sprite[] synergyButtonSprites; // 카테고리에 맞는 스프라이트 배열

    // Dictionary를 통해 능력과 UI를 매핑
    private Dictionary<Ability, int> abilityToImageIndex = new Dictionary<Ability, int>();

    // **추가된 부분: 카테고리별 버튼 스프라이트 딕셔너리**
    private Dictionary<string, Sprite> synergyCategoryToSprite = new Dictionary<string, Sprite>();

    public void Initialize(Player player)
    {
        this.player = player;

        LoadAvailableAbilities();
        InitializeSynergyDictionaries();
        ResetAllAbilities();

        // 리스너 중복 등록 방지를 위해 먼저 제거한 후 추가
        player.OnHitEnemy.RemoveListener(ActivateAbilitiesOnHit);
        player.OnHitEnemy.AddListener(ActivateAbilitiesOnHit);

        // **추가된 부분: 시너지 카테고리와 스프라이트 매핑 초기화**
        InitializeSynergyCategoryToSprite();
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
        bool isNewAbility = false; // 새로운 능력 여부를 추적하는 플래그

        ability.Apply(player);

        if (ability.currentLevel == 0)
        {
            ability.currentLevel = 1;
            abilities.Add(ability);
            isNewAbility = true; // 새로운 능력을 획득했음을 표시

            // 능력과 이미지 인덱스를 매핑
            if (abilitiesAcquiredCount < resultImages.Length)
            {
                abilityToImageIndex[ability] = abilitiesAcquiredCount;
                abilitiesAcquiredCount++;
            }
            else
            {
                Debug.LogWarning("모든 결과 이미지가 이미 활성화되었습니다. 추가 능력에 대한 UI가 없습니다.");
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

        // 능력이 변경되었음을 알림 (C# 이벤트 호출)
        OnAbilitiesChanged?.Invoke();

        // **추가된 부분: 새로운 능력 획득 시 이미지 활성화**
        if (isNewAbility)
        {
            ActivateNextResultImage(ability);
        }
        else
        {
            // 기존 능력이 업그레이드된 경우, 해당 능력의 레벨 텍스트 업데이트
            UpdateAbilityLevelText(ability);
        }

        // 리스너 수 로그 출력 (디버깅 용도)
#if UNITY_EDITOR
        int listenerCount = OnAbilitiesChanged?.GetInvocationList().Length ?? 0;
        Debug.Log($"OnAbilitiesChanged Listener Count: {listenerCount}");
#endif
    }

    /// <summary>
    /// 다음 결과 이미지를 활성화하고 레벨 텍스트를 설정하는 메서드
    /// </summary>
    private void ActivateNextResultImage(Ability ability)
    {
        if (resultImages == null || resultImages.Length == 0)
        {
            Debug.LogWarning("resultImages 배열이 비어있거나 할당되지 않았습니다.");
            return;
        }

        int imageIndex = abilityToImageIndex.ContainsKey(ability) ? abilityToImageIndex[ability] : -1;

        if (imageIndex >= 0 && imageIndex < resultImages.Length)
        {
            if (resultImages[imageIndex] != null)
            {
                resultImages[imageIndex].SetActive(true);

                // **레벨 텍스트 설정: "Lv " 접두사 추가**
                if (levelTexts != null && imageIndex < levelTexts.Length)
                {
                    if (levelTexts[imageIndex] != null)
                    {
                        levelTexts[imageIndex].text = $"Lv {ability.currentLevel}";
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

                // **추가된 부분: 시너지 어빌리티인 경우 버튼 리소스 설정**
                if (ability is SynergyAbility synergyAbility)
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

    /// <summary>
    /// 시너지 어빌리티의 버튼 리소스를 설정하는 메서드
    /// </summary>
    /// <param name="synergyAbility">시너지 어빌리티 객체</param>
    private void SetSynergyButtonResource(SynergyAbility synergyAbility)
    {
        string category = synergyAbility.category;

        if (synergyCategoryToSprite.ContainsKey(category))
        {
            Sprite buttonSprite = synergyCategoryToSprite[category];

            // AbilityManager에 버튼 스프라이트를 전달하여 UI를 업데이트하도록 합니다.
            AbilityManager abilityManager = FindObjectOfType<AbilityManager>();
            if (abilityManager != null)
            {
                abilityManager.TriggerShowSynergyAbility(synergyAbility, buttonSprite);
                Debug.Log($"AbilityManager에게 시너지 어빌리티 '{synergyAbility.abilityName}'과 스프라이트 '{buttonSprite.name}' 전달됨.");
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
    /// 기존 능력이 업그레이드될 때 해당 능력의 레벨 텍스트를 업데이트하는 메서드
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
                    Debug.Log($"Result Image {imageIndex + 1}의 레벨 텍스트가 'Lv {ability.currentLevel}'으로 업데이트됨.");
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
    /// 모든 능력을 초기화하고 결과 이미지를 리셋하는 메서드
    /// </summary>
    public void ResetAllAbilities()
    {
        foreach (var ability in availableAbilities)
        {
            ability.ResetLevel();
        }
        abilities.Clear();

        // 이미지 리셋
        ResetResultImages();

        // 능력이 변경되었음을 알림
        OnAbilitiesChanged?.Invoke();
    }

    /// <summary>
    /// 모든 결과 이미지를 비활성화하고 인덱스를 초기화하는 메서드
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

        // **레벨 텍스트도 초기화: "Lv " 접두사 없이 빈 문자열로 설정**
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
        Debug.Log("모든 결과 이미지가 비활성화되고 레벨 텍스트가 초기화되었습니다.");
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
                parasiticNestAbility.OnProjectileHit(enemy); // ParasiticNest의 OnProjectileHit 호출
            }
        }
    }

    /// <summary>
    /// 몬스터가 사망했을 때 능력을 활성화하는 메서드
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
            // 추가 초기화가 필요한 경우 여기에 작성
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
                // 시너지 어빌리티의 카테고리에 맞는 스프라이트 가져오기
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
