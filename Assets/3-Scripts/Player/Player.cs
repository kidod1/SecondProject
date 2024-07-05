using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;

public class Player : MonoBehaviour
{
    [SerializeField]
    private PlayerData stat;
    public ObjectPool objectPool;

    // UI 관련 변수
    [SerializeField]
    private Transform healthPanel;
    [SerializeField]
    private GameObject healthHeartPrefab;
    [SerializeField]
    private GameObject shieldHeartPrefab;
    [SerializeField]
    private Sprite halfHeartSprite;

    // Movement 관련 변수
    private Vector2 moveInput;
    private Rigidbody2D rb;

    // Health 관련 변수
    private int currentHP;
    private int currentShield;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;

    // Shooting 관련 변수
    private int baseAttack;

    private float shootCooldown = 0.5f;
    private float lastShootTime;
    private bool isShooting = false;
    private Vector2 shootDirection;
    private Vector2 nextShootDirection;
    private bool hasNextShootDirection = false;

    // Input System
    private PlayerInput playerInput;

    // Shooting 이벤트
    public UnityEvent<Vector2, int> OnShoot;

    // 능력 관련 변수
    private List<Ability> acquiredAbilities = new List<Ability>();

    private bool isShieldOnLowHPEnabled = false;

    private bool isShieldRefillEnabled = false;
    private float shieldRefillInterval = 10f;

    private bool isAttackBoostWithShieldEnabled = false;
    private SpecialAbility currentSpecialAbility;

    // Save System
    private string saveFilePath;
    private Dictionary<string, Func<Ability>> abilityTypeRegistry;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerInput = new PlayerInput();

        if (OnShoot == null)
        {
            OnShoot = new UnityEvent<Vector2, int>();
        }

        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");
    }

    private void Start()
    {
        Debug.Log("Save File Path: " + saveFilePath);
        InitializePlayer();
        LoadPlayerData();
        UpdateHealthUI();
        RegisterAbilityTypes();
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
        playerInput.Player.Move.performed += OnMovePerformed;
        playerInput.Player.Move.canceled += OnMoveCanceled;
        playerInput.Player.Shoot.started += OnShootStarted;
        playerInput.Player.Shoot.performed += OnShootPerformed;
        playerInput.Player.Shoot.canceled += OnShootCanceled;
        playerInput.Player.SpecialAbility.performed += OnSpecialAbilityPerformed;
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();
        playerInput.Player.Move.performed -= OnMovePerformed;
        playerInput.Player.Move.canceled -= OnMoveCanceled;
        playerInput.Player.Shoot.started -= OnShootStarted;
        playerInput.Player.Shoot.performed -= OnShootPerformed;
        playerInput.Player.Shoot.canceled -= OnShootCanceled;
        playerInput.Player.SpecialAbility.performed -= OnSpecialAbilityPerformed;
    }

    private void Update()
    {
        if (isShooting && Time.time >= lastShootTime + shootCooldown)
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
        rb.MovePosition(rb.position + movement);
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
        Vector2 newDirection = context.ReadValue<Vector2>();
        if (newDirection != Vector2.zero)
        {
            shootDirection = newDirection;
            isShooting = true;

            if (Time.time >= lastShootTime + shootCooldown)
            {
                Shoot(shootDirection, stat.projectileType);
                lastShootTime = Time.time;
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

    private void OnShootCanceled(InputAction.CallbackContext context)
    {
        isShooting = false;
    }

    private void OnSpecialAbilityPerformed(InputAction.CallbackContext context)
    {
        if (currentSpecialAbility != null)
        {
            currentSpecialAbility.Activate(this);
        }
    }

    public void SetSpecialAbility(SpecialAbility specialAbility)
    {
        currentSpecialAbility = specialAbility;
    }

    private void Shoot(Vector2 direction, int prefabIndex)
    {
        GameObject projectile = objectPool.GetObject(prefabIndex);
        projectile.transform.position = transform.position;
        projectile.GetComponent<Projectile>().SetDirection(direction);

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

    public void TakeHP(int damage, Vector2 knockbackDirection)
    {
        if (isInvincible) return;

        currentHP -= damage;

        CheckHPForShield();
        UpdateAttackBoost(); // 실드 상태 변경 시 공격력 부스트 업데이트
        StartCoroutine(InvincibilityCoroutine());
        StartCoroutine(KnockbackCoroutine(knockbackDirection));

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }

        UpdateHealthUI(); // UI 업데이트
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isInvincible) return;

        if (currentShield > 0)
        {
            int remainingDamage = damage - currentShield;
            currentShield = Mathf.Max(currentShield - damage, 0);

            if (remainingDamage > 0)
            {
                currentHP -= remainingDamage;
            }
        }
        else
        {
            currentHP -= damage;
        }

        CheckHPForShield(); // 실드 체크 추가
        UpdateAttackBoost(); // 실드 상태 변경 시 공격력 부스트 업데이트

        StartCoroutine(KnockbackCoroutine(knockbackDirection));
        StartCoroutine(InvincibilityCoroutine());

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }

        UpdateHealthUI(); // UI 업데이트
    }

    // 피격시 넉백
    private IEnumerator KnockbackCoroutine(Vector2 direction)
    {
        float timer = 0;

        while (timer < stat.knockbackDuration)
        {
            timer += Time.deltaTime;
            transform.Translate(direction * stat.knockbackSpeed * Time.deltaTime);
            yield return null;
        }
    }

    // 피격시 깜빡
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float invincibilityDuration = 0.5f;
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
        Debug.Log("Player Died!");
        // 플레이어 죽음 처리 로직 추가 (예: 게임 오버 화면 표시, 재시작 등)
        ResetPlayerData();
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > stat.maxHP)
        {
            currentHP = stat.maxHP;
        }

        UpdateHealthUI(); // UI 업데이트
    }

    public void IncreaseShield(int amount)
    {
        currentShield += amount;
        UpdateHealthUI(); // UI 업데이트
    }

    private void CheckHPForShield()
    {
        if (isShieldOnLowHPEnabled && currentHP == 2)
        {
            currentShield += 2;
        }
    }

    public void EnableShieldOnLowHP()
    {
        isShieldOnLowHPEnabled = true;
    }

    public void ReduceMaxHPAndStartShieldRefill()
    {
        stat.maxHP -= 2;
        if (currentHP > stat.maxHP)
        {
            currentHP = stat.maxHP;
        }
        isShieldRefillEnabled = true;
        StartCoroutine(ShieldRefillCoroutine());
    }

    private IEnumerator ShieldRefillCoroutine()
    {
        while (isShieldRefillEnabled)
        {
            yield return new WaitForSeconds(shieldRefillInterval);
            if (currentShield <= 0)
            {
                IncreaseShield(2);
            }
        }
    }

    public void ReduceMaxHPAndIncreaseAttack()
    {
        stat.maxHP -= 2;
        if (currentHP > stat.maxHP)
        {
            currentHP = stat.maxHP;
        }
        IncreaseAttack(10);
    }

    public void EnableAttackBoostWithShield()
    {
        isAttackBoostWithShieldEnabled = true;
        baseAttack = stat.playerDamage;
        UpdateAttackBoost();
    }

    private void UpdateAttackBoost()
    {
        if (isAttackBoostWithShieldEnabled)
        {
            if (currentShield > 0)
            {
                stat.playerDamage = baseAttack + 10;
                Debug.Log("실드 존재 - 공격력 증가");
            }
            else
            {
                stat.playerDamage = baseAttack;
                Debug.Log("실드 없음 - 기본 공격력");
            }
        }
    }

    public void IncreaseAttack(int amount)
    {
        stat.playerDamage += amount;
        baseAttack = stat.playerDamage;
    }

    public void IncreaseRange(int amount)
    {
        stat.projectileRange += amount;
    }

    public void IncreaseAttackSpeed(float amount)
    {
        stat.knockbackSpeed += amount;
    }

    // 새로운 능력 추가 메서드
    public void IncreasePride(int amount)
    {
        // 오만 로직
    }

    public void IncreaseWrath(int amount)
    {
        stat.playerDamage += amount; // 분노 - 공격력 증가 로직
    }

    public void IncreaseGluttony(int amount)
    {
        // 식탐 로직
    }

    public void IncreaseGreed(int amount)
    {
        // 탐욕 로직
    }

    public void IncreaseSloth(int amount)
    {
        // 나태 로직
    }

    public void IncreaseEnvy(int amount)
    {
        // 질투 로직
    }

    public void IncreaseLust(int amount)
    {
        // 색욕 로직
    }

    // 프로젝타일 변경시
    public void ChangeProjectile(int newProjectileType)
    {
        stat.projectileType = newProjectileType;
        Debug.Log("프로젝타일 변경 " + newProjectileType);
    }

    // 플레이어 회복
    public int GetCurrentHP()
    {
        return currentHP;
    }

    // 플레이어 초기화 함수
    private void InitializePlayer()
    {
        stat.InitializeStats();
        currentShield = stat.defaultShield;
        currentHP = stat.maxHP;
        currentShield = 0;
        isShieldOnLowHPEnabled = false;
        baseAttack = stat.playerDamage;
        currentSpecialAbility = null;
    }

    public void AddAbility(Ability ability)
    {
        acquiredAbilities.Add(ability);
        ability.Apply(this);

        if (ability is IncreasePride || ability is IncreaseAttack || ability is IncreaseRange)
        {
            UpdateAbilityTreeProgress("Pride");
        }
        else if (ability is ShieldOnLowHP || ability is ReduceMaxHPAndRefillShield || ability is ReduceMaxHPIncreaseAttack || ability is IncreaseAttackWithShield || ability is IncreaseAttackSpeed)  // 분노 능력 적용
        {
            UpdateAbilityTreeProgress("Wrath");
        }
        else if (ability is IncreaseGluttony)
        {
            UpdateAbilityTreeProgress("Gluttony");
        }
        else if (ability is IncreaseGreed)
        {
            UpdateAbilityTreeProgress("Greed");
        }
        else if (ability is IncreaseSloth)
        {
            UpdateAbilityTreeProgress("Sloth");
        }
        else if (ability is IncreaseEnvy)
        {
            UpdateAbilityTreeProgress("Envy");
        }
        else if (ability is IncreaseLust)
        {
            UpdateAbilityTreeProgress("Lust");
        }
    }

    private void UpdateAbilityTreeProgress(string treeName)
    {
        if (abilityTreeProgress.ContainsKey(treeName))
        {
            abilityTreeProgress[treeName]++;
            CheckForSpecialAbility(treeName);
        }
    }

    private void CheckForSpecialAbility(string treeName)
    {
        int progress = abilityTreeProgress[treeName];

        // 진행도가 3일 때 특수 능력 추가
        if (progress == 3)
        {
            switch (treeName)
            {
                case "Pride":
                    AddSpecialAbility(new SpecialAbilityPride());
                    break;
                case "Wrath":
                    AddSpecialAbility(new SpecialAbilityWrath());
                    break;
                case "Gluttony":
                    AddSpecialAbility(new SpecialAbilityGluttony());
                    break;
                case "Greed":
                    AddSpecialAbility(new SpecialAbilityGreed());
                    break;
                case "Sloth":
                    AddSpecialAbility(new SpecialAbilitySloth());
                    break;
                case "Envy":
                    AddSpecialAbility(new SpecialAbilityEnvy());
                    break;
                case "Lust":
                    AddSpecialAbility(new SpecialAbilityLust());
                    break;
            }
        }
        // 진행도가 5일 때 특수 능력 추가
        else if (progress == 5)
        {
            switch (treeName)
            {
                case "Pride":
                    AddSpecialAbility(new SpecialAbilitySuperPride());
                    break;
                case "Wrath":
                    AddSpecialAbility(new SpecialAbilitySuperWrath());
                    currentSpecialAbility = new SpecialAbilitySuperWrath();
                    break;
                case "Gluttony":
                    // AddSuperSpecialAbility(new SpecialAbilitySuperGluttony());
                    break;
                case "Greed":
                    // AddSuperSpecialAbility(new SpecialAbilitySuperGreed());
                    break;
                case "Sloth":
                    // AddSuperSpecialAbility(new SpecialAbilitySuperSloth());
                    break;
                case "Envy":
                    // AddSuperSpecialAbility(new SpecialAbilitySuperEnvy());
                    break;
                case "Lust":
                    // AddSuperSpecialAbility(new SpecialAbilitySuperLust());
                    break;
            }
        }
        // 진행도가 7일 때 특수 능력 추가
        else if (progress == 7)
        {
            switch (treeName)
            {
                case "Pride":
                    AddSpecialAbility(new SpecialAbilityUltraPride());
                    break;
                case "Wrath":
                    AddSpecialAbility(new SpecialAbilityUltraWrath());
                    currentSpecialAbility = new SpecialAbilityUltraWrath();
                    break;
                case "Gluttony":
                    // AddUltraSpecialAbility(new SpecialAbilityUltraGluttony());
                    break;
                case "Greed":
                    // AddUltraSpecialAbility(new SpecialAbilityUltraGreed());
                    break;
                case "Sloth":
                    // AddUltraSpecialAbility(new SpecialAbilityUltraSloth());
                    break;
                case "Envy":
                    // AddUltraSpecialAbility(new SpecialAbilityUltraEnvy());
                    break;
                case "Lust":
                    // AddUltraSpecialAbility(new SpecialAbilityUltraLust());
                    break;
            }
        }
    }

    private void AddSpecialAbility(SpecialAbility specialAbility)
    {
        acquiredAbilities.Add(specialAbility);
        specialAbility.Apply(this);
    }

    public int GetAcquiredAbilityCount()
    {
        return acquiredAbilities.Count;
    }

    private Dictionary<string, int> abilityTreeProgress = new Dictionary<string, int>
    {
        {"Pride", 0},
        {"Wrath", 0},
        {"Gluttony", 0},
        {"Greed", 0},
        {"Sloth", 0},
        {"Envy", 0},
        {"Lust", 0}
    };

    public bool HasAbility(Ability ability)
    {
        return acquiredAbilities.Contains(ability);
    }

    private void UpdateHealthUI()
    {
        foreach (Transform child in healthPanel)
        {
            Destroy(child.gameObject);
        }

        int fullHearts = currentHP / 2;
        bool hasHalfHeart = (currentHP % 2) != 0;

        for (int i = 0; i < fullHearts; i++)
        {
            Instantiate(healthHeartPrefab, healthPanel);
        }

        if (hasHalfHeart)
        {
            var halfHeart = Instantiate(healthHeartPrefab, healthPanel);
            halfHeart.GetComponent<Image>().sprite = halfHeartSprite;
        }

        for (int i = 0; i < currentShield / 2; i++)
        {
            Instantiate(shieldHeartPrefab, healthPanel);
        }
    }

    public void SavePlayerData()
    {
        PlayerDataToJson data = new PlayerDataToJson
        {
            maxHP = stat.maxHP,
            currentHP = currentHP,
            currentShield = currentShield,
            playerDamage = stat.playerDamage,
            projectileRange = stat.projectileRange,
            knockbackSpeed = stat.knockbackSpeed,
            acquiredAbilities = new List<string>(),
            abilityTreeProgress = new Dictionary<string, int>(abilityTreeProgress)
        };

        foreach (var ability in acquiredAbilities)
        {
            data.acquiredAbilities.Add(ability.Name);
        }

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
            currentShield = data.currentShield;
            stat.playerDamage = data.playerDamage;
            stat.projectileRange = data.projectileRange;
            stat.knockbackSpeed = data.knockbackSpeed;
            acquiredAbilities.Clear();
            foreach (var abilityName in data.acquiredAbilities)
            {
                Ability ability = CreateAbilityByName(abilityName);
                if (ability != null)
                {
                    acquiredAbilities.Add(ability);
                    ability.Apply(this);
                }
            }
            if (data.abilityTreeProgress == null)
            {
                data.abilityTreeProgress = new Dictionary<string, int>
                {
                    {"Pride", 0},
                    {"Wrath", 0},
                    {"Gluttony", 0},
                    {"Greed", 0},
                    {"Sloth", 0},
                    {"Envy", 0},
                    {"Lust", 0}
                };
            }
            abilityTreeProgress = new Dictionary<string, int>(data.abilityTreeProgress);
        }
    }

    public void ResetPlayerData()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }

        PlayerDataToJson data = new PlayerDataToJson();
        data.InitializeDefaultValues();
        ApplyPlayerData(data);
    }

    private void ApplyPlayerData(PlayerDataToJson data)
    {
        stat.maxHP = data.maxHP;
        currentHP = data.currentHP;
        currentShield = data.currentShield;
        stat.playerDamage = data.playerDamage;
        stat.projectileRange = data.projectileRange;
        stat.knockbackSpeed = data.knockbackSpeed;
        acquiredAbilities.Clear();
        abilityTreeProgress = new Dictionary<string, int>(data.abilityTreeProgress);
        UpdateHealthUI();
    }

    private Ability CreateAbilityByName(string name)
    {
        if (abilityTypeRegistry.ContainsKey(name))
        {
            return abilityTypeRegistry[name]();
        }
        return null;
    }

    private void RegisterAbilityTypes()
    {
        abilityTypeRegistry = new Dictionary<string, Func<Ability>>
    {
        { "IncreasePride", () => new IncreasePride() },
        { "IncreaseGluttony", () => new IncreaseGluttony() },
        { "IncreaseGreed", () => new IncreaseGreed() },
        { "IncreaseSloth", () => new IncreaseSloth() },
        { "IncreaseEnvy", () => new IncreaseEnvy() },
        { "IncreaseLust", () => new IncreaseLust() },
        { "IncreaseAttack", () => new IncreaseAttack() },
        { "IncreaseRange", () => new IncreaseRange() },
        { "IncreaseAttackSpeed", () => new IncreaseAttackSpeed() },
        { "ShieldOnLowHP", () => new ShieldOnLowHP() },
        { "ReduceMaxHPIncreaseAttack", () => new ReduceMaxHPIncreaseAttack() },
        { "ReduceMaxHPAndRefillShield", () => new ReduceMaxHPAndRefillShield() },
        { "IncreaseAttackWithShield", () => new IncreaseAttackWithShield() }
    };
    }

    private void OnApplicationQuit()
    {
        SavePlayerData();
    }
}

[System.Serializable]
public class PlayerDataToJson
{
    public int maxHP;
    public int currentHP;
    public int currentShield;
    public int playerDamage;
    public float knockbackSpeed;
    public float projectileRange;
    public List<string> acquiredAbilities;
    public Dictionary<string, int> abilityTreeProgress;

    public void InitializeDefaultValues()
    {
        maxHP = 10;
        currentHP = maxHP;
        currentShield = 0;
        playerDamage = 5;
        knockbackSpeed = 5.0f;
        projectileRange = 2;
        acquiredAbilities = new List<string>();
        abilityTreeProgress = new Dictionary<string, int>
        {
            {"Pride", 0},
            {"Wrath", 0},
            {"Gluttony", 0},
            {"Greed", 0},
            {"Sloth", 0},
            {"Envy", 0},
            {"Lust", 0}
        };
    }
}
