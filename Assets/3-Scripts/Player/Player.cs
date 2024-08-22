using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;

public class Player : MonoBehaviour
{
    public PlayerData stat;
    public ObjectPool objectPool;
    private MapBoundary mapBoundary;

    // UI 관련 변수
    [SerializeField]
    private TMP_Text experienceText;
    [SerializeField]
    private TMP_Text levelText;
    [SerializeField]
    private Scrollbar experienceScrollbar;
    [SerializeField]
    private RectTransform maskRectTransform;
    [SerializeField]
    private Image healthFillImage;
    [SerializeField]
    private TMP_Text healthText;
    [SerializeField]
    private TMP_Text currencyText;

    // Movement 관련 변수
    private Vector2 moveInput;
    private Rigidbody2D rb;

    // Health 관련 변수
    private int currentHP;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;

    // Shooting 관련 변수
    private float lastShootTime;
    public bool isShooting = false;
    private Vector2 shootDirection;
    private Vector2 nextShootDirection;
    private bool hasNextShootDirection = false;

    // 경험치 및 레벨 관련 변수
    private int experience;
    private int level;
    private Coroutine experienceBarCoroutine;

    // 능력 관련 변수
    private List<Ability> abilities = new List<Ability>();
    private List<Ability> availableAbilities = new List<Ability>();
    private Dictionary<string, bool> synergyAbilityAcquired = new Dictionary<string, bool>();
    private Dictionary<string, int> synergyLevels = new Dictionary<string, int>();
    private Coroutine buffCoroutine;
    private SynergyAbility synergyAbility;
    private bool hasSynergyAbility = false;

    // 재화 관련 변수
    private int currentCurrency = 0;

    // Input System
    private PlayerInput playerInput;

    // 이벤트
    public UnityEvent<Vector2, int> OnShoot;
    public UnityEvent OnLevelUp;
    public UnityEvent<Collider2D> OnMonsterEnter;
    public UnityEvent OnShootCanceled;
    public UnityEvent OnTakeDamage;

    // Save System
    private string saveFilePath;

    public Vector2 PlayerPosition => transform.position;

    private void Awake()
    {
        LoadAvailableAbilities();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerInput = new PlayerInput();

        if (OnShoot == null)
        {
            OnShoot = new UnityEvent<Vector2, int>();
        }
        if (OnMonsterEnter == null)
        {
            OnMonsterEnter = new UnityEvent<Collider2D>();
        }
        if (OnShootCanceled == null)
        {
            OnShootCanceled = new UnityEvent();
        }
        if (OnTakeDamage == null)
        {
            OnTakeDamage = new UnityEvent();
        }


        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");
    }
    private void Start()
    {
        InitializePlayer();
        LoadPlayerData();
        UpdateHealthUI();
        ResetPlayerData();
        //UpdateCurrencyUI();

        InitializeSynergyDictionaries(); // 시너지 딕셔너리 초기화

        mapBoundary = FindObjectOfType<MapBoundary>();
        if (mapBoundary == null)
        {
            Debug.LogError("MapBoundary 오브젝트를 찾을 수 없습니다.");
        }
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
        playerInput.Player.Move.performed += OnMovePerformed;
        playerInput.Player.Move.canceled += OnMoveCanceled;
        playerInput.Player.Shoot.started += OnShootStarted;
        playerInput.Player.Shoot.performed += OnShootPerformed;
        playerInput.Player.Shoot.canceled += OnShootCanceledInputAction; // 이벤트 바인딩
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();
        playerInput.Player.Move.performed -= OnMovePerformed;
        playerInput.Player.Move.canceled -= OnMoveCanceled;
        playerInput.Player.Shoot.started -= OnShootStarted;
        playerInput.Player.Shoot.performed -= OnShootPerformed;
        playerInput.Player.Shoot.canceled -= OnShootCanceledInputAction; // 이벤트 바인딩 해제
    }

    private void Update()
    {
        if (isShooting && Time.time >= lastShootTime + stat.shotCooldown)
        {
            Shoot(shootDirection, stat.projectileType);
            lastShootTime = Time.time;

            if (hasNextShootDirection)
            {
                shootDirection = nextShootDirection;
                hasNextShootDirection = false;
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 movement = moveInput * stat.playerSpeed * Time.fixedDeltaTime;
        Vector2 newPosition = rb.position + movement;

        if (mapBoundary != null)
        {
            newPosition = mapBoundary.ClampPosition(newPosition);
        }

        rb.MovePosition(newPosition);
    }
    public void SetInvincibility(bool value)
    {
        isInvincible = value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnMonsterEnter.Invoke(other);
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnShootStarted(InputAction.CallbackContext context)
    {
        if (context.control != null)
        {
            Vector2 newDirection = context.ReadValue<Vector2>();
            if (newDirection != Vector2.zero)
            {
                shootDirection = newDirection;
                isShooting = true;

                if (Time.time >= lastShootTime + stat.shotCooldown)
                {
                    Shoot(shootDirection, stat.projectileType);
                    lastShootTime = Time.time;
                }
            }
        }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        Vector2 newDirection = context.ReadValue<Vector2>();
        if (newDirection != Vector2.zero)
        {
            nextShootDirection = newDirection;
            hasNextShootDirection = true;
        }
    }

    private void OnShootCanceledInputAction(InputAction.CallbackContext context)
    {
        isShooting = false;
        OnShootCanceled.Invoke(); // 공격 중단 이벤트 호출
    }

    public void Shoot(Vector2 direction, int prefabIndex)
    {
        GameObject projectile = objectPool.GetObject(prefabIndex);
        projectile.transform.position = transform.position;

        Projectile projScript = projectile.GetComponent<Projectile>();
        projScript.Initialize(stat);
        projScript.SetDirection(direction);

        OnShoot.Invoke(direction, prefabIndex);
    }

    public void RebindMoveKey(Action<RebindingOperation> callback)
    {
        playerInput.Player.Move.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .OnComplete(operation =>
            {
                operation.Dispose();
                callback(operation);
            })
            .Start();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        int finalDamage = Mathf.Max(0, damage - stat.defense);
        currentHP -= finalDamage;

        OnTakeDamage.Invoke();

        StartCoroutine(InvincibilityCoroutine());

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }

        UpdateHealthUI();
    }

    // 피격시 깜빡
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float invincibilityDuration = 1f;
        float blinkInterval = 0.1f;

        for (float i = 0; i < invincibilityDuration; i += blinkInterval)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }

        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    private void Die()
    {
        SavePlayerData();
        InitializePlayer();
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > stat.maxHP)
        {
            currentHP = stat.maxHP;
        }

        UpdateHealthUI();
    }

    public void ChangeProjectile(int newProjectileType)
    {
        stat.projectileType = newProjectileType;
    }

    public int GetCurrentHP()
    {
        return currentHP;
    }

    private void LoadAvailableAbilities()
    {
        // Resources 폴더에서 모든 능력을 로드
        Ability[] loadedAbilities = Resources.LoadAll<Ability>("Abilities");
        availableAbilities.AddRange(loadedAbilities); // 수정: availableAbilities에 추가

        // 각 능력을 불러올 때 디버그 로그 출력
        foreach (var ability in loadedAbilities)
        {
            Debug.Log($"Loaded ability: {ability.abilityName}");
        }
    }

    public void ResetAllAbilities()
    {
        foreach (var ability in availableAbilities)
        {
            ability.ResetLevel();
        }
        abilities.Clear();
        OnShoot.RemoveAllListeners();
    }

    public void GainExperience(int amount)
    {
        int adjustedAmount = Mathf.RoundToInt(amount * stat.experienceMultiplier);
        experience += adjustedAmount;

        if (experienceBarCoroutine != null)
        {
            StopCoroutine(experienceBarCoroutine);
        }
        experienceBarCoroutine = StartCoroutine(AnimateExperienceBar());
        CheckLevelUp();
    }

    private IEnumerator AnimateExperienceBar()
    {
        float currentExp = experienceScrollbar.size * stat.experienceThresholds[Mathf.Clamp(level, 0, stat.experienceThresholds.Length - 1)];
        float targetExp = experience;
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newExp = Mathf.Lerp(currentExp, targetExp, elapsed / duration);
            experienceScrollbar.size = newExp / stat.experienceThresholds[Mathf.Clamp(level, 0, stat.experienceThresholds.Length - 1)];
            yield return null;
        }

        experienceScrollbar.size = targetExp / stat.experienceThresholds[Mathf.Clamp(level, 0, stat.experienceThresholds.Length - 1)];
        UpdateExperienceUI();
    }

    private void CheckLevelUp()
    {
        while (level < stat.experienceThresholds.Length && experience >= stat.experienceThresholds[level])
        {
            level++;
            OnLevelUp.Invoke();
            UpdateExperienceUI();
        }
    }

    private void UpdateExperienceUI()
    {
        levelText.text = "Lv. " + level;
        experienceText.text = "EXP: " + experience + " / " + stat.experienceThresholds[level];

        float expRatio = (float)experience / stat.experienceThresholds[level];
        experienceScrollbar.size = expRatio;
    }

    public void SelectAbility(Ability ability)
    {
        ability.Apply(this);

        if (ability.currentLevel == 0)
        {
            ability.currentLevel = 1;
            abilities.Add(ability);
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
    }

    // 시너지 딕셔너리 초기화 메서드
    private void InitializeSynergyDictionaries()
    {
        // 시너지 능력 초기화
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

        // 시너지 레벨 초기화
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

    private void CheckForSynergy(string category)
    {
        // 딕셔너리에서 키 존재 여부 확인
        if (!synergyAbilityAcquired.ContainsKey(category) || !synergyLevels.ContainsKey(category))
        {
            Debug.LogError($"Category '{category}' not found in synergyAbilityAcquired or synergyLevels dictionary");
            return;
        }

        int totalLevel = 0;

        // 카테고리의 총 레벨을 계산
        foreach (var ability in abilities)
        {
            if (ability.category == category)
            {
                totalLevel += ability.currentLevel;
            }
        }

        Debug.Log($"Category: {category}, Total Level: {totalLevel}");

        // 15레벨 시너지 능력 할당
        if (totalLevel >= 15 && synergyLevels[category] < 15)
        {
            AssignSynergyAbility(category, 15);
            synergyLevels[category] = 15; // 15레벨 시너지 능력 획득 기록
        }
        // 10레벨 시너지 능력 할당
        else if (totalLevel >= 10 && synergyLevels[category] < 10)
        {
            AssignSynergyAbility(category, 10);
            synergyLevels[category] = 10; // 10레벨 시너지 능력 획득 기록
        }
        // 5레벨 시너지 능력 할당
        else if (totalLevel >= 5 && synergyLevels[category] < 5)
        {
            AssignSynergyAbility(category, 5);
            synergyLevels[category] = 5; // 5레벨 시너지 능력 획득 기록
        }
    }




    private void AssignSynergyAbility(string category, int level)
    {
        Debug.Log($"Assigning Synergy Ability for {category} at level {level}");
        string synergyAbilityName = $"{category}Synergy{level}";
        SynergyAbility synergyAbility = Resources.Load<SynergyAbility>($"SynergyAbilities/{synergyAbilityName}");
        if (synergyAbility != null)
        {
            Debug.Log($"Synergy ability acquired: {synergyAbilityName}");
            StartCoroutine(ShowSynergyAbilityWithDelay(synergyAbility, 0.5f));
            ApplyAuraEffect(category, level);
        }
        else
        {
            Debug.LogError($"Failed to load Synergy Ability: {synergyAbilityName}");
        }
    }

    private void ApplyAuraEffect(string category, int level)
    {
        // 기존 오오라 제거
        RemoveCurrentAura();

        // 새로운 오오라 이펙트 로드
        string auraEffectPath = $"Effects/{category}Synergy{level}";
        GameObject auraEffectPrefab = Resources.Load<GameObject>(auraEffectPath);

        if (auraEffectPrefab != null)
        {
            // 오오라 이펙트 생성 및 플레이어에게 붙이기
            GameObject auraEffect = Instantiate(auraEffectPrefab, transform);
            auraEffect.name = $"{category}SynergyAura"; // 생성된 오오라 이름 지정
        }
    }
    private void RemoveCurrentAura()
    {
        // 기존에 붙어있던 오오라 이펙트 제거
        foreach (Transform child in transform)
        {
            if (child.name.Contains("SynergyAura"))
            {
                Destroy(child.gameObject);
            }
        }
    }
    private IEnumerator ShowSynergyAbilityWithDelay(SynergyAbility synergyAbility, float delay)
    {
        yield return new WaitForSeconds(delay);
        FindObjectOfType<AbilityManager>().ShowSynergyAbility(synergyAbility);
    }
    public void ApplySynergyAbility(SynergyAbility synergyAbility)
    {
        synergyAbility.Apply(this);
    }

    public List<Ability> GetAvailableAbilities()
    {
        return availableAbilities;
    }

    private void InitializePlayer()
    {
        stat.InitializeStats();
        currentHP = stat.maxHP;
        ResetAbilities();
        UpdateExperienceUI();
    }
    private void UpdateHealthUI()
    {
        // HP 비율 계산
        float healthPercentage = (float)currentHP / stat.maxHP;

        // Right 패딩을 HP 비율에 맞게 조정 (체력 비율에 따라 240씩 증가하도록)
        float rightPadding = (1 - healthPercentage) * 240 * 4; // 240으로 수정

        // RectMask2D가 적용된 부모 오브젝트의 offsetMax 값 조정
        maskRectTransform.offsetMax = new Vector2(-rightPadding, maskRectTransform.offsetMax.y);

        // HP 비율 텍스트 업데이트
        healthText.text = $"{healthPercentage * 100:F0}%"; // 백분율로 표시
    }

    private void UpdateCurrencyUI()
    {
        if (currencyText != null)
        {
            currencyText.text = "현재 재화: " + currentCurrency;
        }
        else
        {
            Debug.LogWarning("currencyText is not assigned in the inspector.");
        }
    }

    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
        UpdateCurrencyUI();
    }


    public void SavePlayerData()
    {
        PlayerDataToJson data = new PlayerDataToJson
        {
            maxHP = stat.maxHP,
            currentHP = currentHP,
            playerDamage = stat.playerDamage,
            projectileRange = stat.projectileRange,
            knockbackSpeed = stat.knockbackSpeed,
            currency = currentCurrency
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(saveFilePath, json);
    }


    public void LoadPlayerData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerDataToJson data = JsonUtility.FromJson<PlayerDataToJson>(json);

            stat.maxHP = data.maxHP;
            currentHP = data.currentHP;
            stat.playerDamage = data.playerDamage;
            stat.projectileRange = data.projectileRange;
            stat.knockbackSpeed = data.knockbackSpeed;
            currentCurrency = data.currency;

            UpdateCurrencyUI(); // LoadPlayerData 호출 후 UI 업데이트
        }
        else
        {
            Debug.LogWarning("Save file not found, using default player data.");
        }
    }


    public void ResetPlayerData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerDataToJson data = JsonUtility.FromJson<PlayerDataToJson>(json);
            data.InitializeDefaultValues();
            data.currency = currentCurrency;
            ApplyPlayerData(data);
        }
        else
        {
            PlayerDataToJson data = new PlayerDataToJson();
            data.InitializeDefaultValues();
            ApplyPlayerData(data);
        }
    }

    public void ResetAbilities()
    {
        foreach (var ability in availableAbilities)
        {
            ability.ResetLevel();
        }
        abilities.Clear();
        OnShoot.RemoveAllListeners();
    }

    private void ApplyPlayerData(PlayerDataToJson data)
    {
        stat.maxHP = data.maxHP;
        currentHP = data.currentHP;
        stat.playerDamage = data.playerDamage;
        stat.projectileRange = data.projectileRange;
        stat.knockbackSpeed = data.knockbackSpeed;
        currentCurrency = data.currency;
        UpdateHealthUI();
        UpdateCurrencyUI();
    }

    private void OnApplicationQuit()
    {
        SavePlayerData();
    }
}

[System.Serializable]
public class PlayerDataToJson
{
    public int maxHP = 100;
    public int currentHP;
    public int playerDamage;
    public float knockbackSpeed;
    public float projectileRange;
    public int currency;

    public void InitializeDefaultValues()
    {
        currentHP = maxHP;
        playerDamage = 5;
        knockbackSpeed = 5.0f;
        projectileRange = 2;
        currency = 0;
    }
}
