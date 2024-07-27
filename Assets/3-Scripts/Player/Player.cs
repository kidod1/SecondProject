using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;

public class Player : MonoBehaviour
{
    public PlayerData stat;
    public ObjectPool objectPool;

    // UI 관련 변수
    [SerializeField]
    private Transform smallHealthPanel;
    [SerializeField]
    private Transform largeHealthPanel;
    [SerializeField]
    private GameObject healthHeartPrefab;
    [SerializeField]
    private GameObject shieldHeartPrefab;
    [SerializeField]
    private Sprite halfHeartSprite;
    [SerializeField]
    private TMP_Text experienceText;
    [SerializeField]
    private TMP_Text levelText;
    [SerializeField]
    private Scrollbar experienceScrollbar;

    // UI 리소스 변수
    [SerializeField]
    private Sprite smallHealthHeartSprite;
    [SerializeField]
    private Sprite largeHealthHeartSprite;
    [SerializeField]
    private Sprite smallShieldHeartSprite;
    [SerializeField]
    private Sprite largeShieldHeartSprite;
    [SerializeField]
    private Sprite smallEmptyHeartSprite;
    [SerializeField]
    private Sprite largeEmptyHeartSprite;

    // Movement 관련 변수
    private Vector2 moveInput;
    private Rigidbody2D rb;

    // Health 관련 변수
    private int currentHP;
    private int currentShield;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;

    // Shooting 관련 변수
    public float shootCooldown = 0.5f;
    private float lastShootTime;
    private bool isShooting = false;
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

    // Input System
    private PlayerInput playerInput;

    // 이벤트
    public UnityEvent<Vector2, int> OnShoot;
    public UnityEvent OnLevelUp;
    public UnityEvent<Collider2D> OnMonsterEnter;

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

        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");
    }

    private void Start()
    {
        InitializePlayer();
        LoadPlayerData();
        UpdateHealthUI();
        ResetPlayerData();

        synergyAbilityAcquired.Add("Attack", false);
        synergyAbilityAcquired.Add("Defense", false);
        synergyAbilityAcquired.Add("Speed", false);
        synergyAbilityAcquired.Add("Health", false);
        synergyAbilityAcquired.Add("Magic", false);
        synergyAbilityAcquired.Add("Support", false);
        synergyAbilityAcquired.Add("Utility", false);

        synergyLevels.Add("Attack", 0);
        synergyLevels.Add("Defense", 0);
        synergyLevels.Add("Speed", 0);
        synergyLevels.Add("Health", 0);
        synergyLevels.Add("Magic", 0);
        synergyLevels.Add("Support", 0);
        synergyLevels.Add("Utility", 0);
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
        playerInput.Player.Move.performed += OnMovePerformed;
        playerInput.Player.Move.canceled += OnMoveCanceled;
        playerInput.Player.Shoot.started += OnShootStarted;
        playerInput.Player.Shoot.performed += OnShootPerformed;
        playerInput.Player.Shoot.canceled += OnShootCanceled;
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();
        playerInput.Player.Move.performed -= OnMovePerformed;
        playerInput.Player.Move.canceled -= OnMoveCanceled;
        playerInput.Player.Shoot.started -= OnShootStarted;
        playerInput.Player.Shoot.performed -= OnShootPerformed;
        playerInput.Player.Shoot.canceled -= OnShootCanceled;
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

                if (Time.time >= lastShootTime + shootCooldown)
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

    private void OnShootCanceled(InputAction.CallbackContext context)
    {
        isShooting = false;
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

    public void TakeHP(int damage, Vector2 knockbackDirection)
    {
        if (isInvincible) return;

        currentHP -= damage;
        StartCoroutine(ShowLargeHealthUI());

        StartCoroutine(InvincibilityCoroutine());

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }

        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
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

        StartCoroutine(ShowLargeHealthUI());
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
        // 플레이어 죽음 처리 로직 추가
        ResetPlayerData();
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

    public void IncreaseShield(int amount)
    {
        currentShield += amount;
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
        experience += amount;
        if (experienceBarCoroutine != null)
        {
            StopCoroutine(experienceBarCoroutine);
        }
        experienceBarCoroutine = StartCoroutine(AnimateExperienceBar());
        CheckLevelUp();
    }

    private IEnumerator AnimateExperienceBar()
    {
        float currentExp = experienceScrollbar.size * stat.experienceThresholds[level];
        float targetExp = experience;
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newExp = Mathf.Lerp(currentExp, targetExp, elapsed / duration);
            experienceScrollbar.size = newExp / stat.experienceThresholds[level];
            yield return null;
        }

        experienceScrollbar.size = targetExp / stat.experienceThresholds[level];
        UpdateExperienceUI();
    }

    private void CheckLevelUp()
    {
        if (level < stat.experienceThresholds.Length && experience >= stat.experienceThresholds[level])
        {
            level++;
            OnLevelUp.Invoke();
            UpdateExperienceUI();
        }
    }

    private void UpdateExperienceUI()
    {
        levelText.text = "Level: " + level;
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

    private void CheckForSynergy(string category)
    {
        if (synergyAbilityAcquired[category]) return;

        int totalLevel = 0;

        foreach (var ability in abilities)
        {
            if (ability.category == category)
            {
                totalLevel += ability.currentLevel;
            }
        }

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
            StartCoroutine(ShowSynergyAbilityWithDelay(synergyAbility, 0.5f));
            synergyAbilityAcquired[category] = true;
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
        currentShield = stat.defaultShield;
        currentHP = stat.maxHP;
        currentShield = 0;
        ResetAbilities();
        UpdateExperienceUI();
    }


    private void UpdateSmallHealthUI()
    {
        smallHealthPanel.gameObject.SetActive(true);
        largeHealthPanel.gameObject.SetActive(false);

        foreach (Transform child in smallHealthPanel)
        {
            Destroy(child.gameObject);
        }

        int fullHearts = currentHP / 2;
        bool hasHalfHeart = (currentHP % 2) != 0;
        int totalHearts = stat.maxHP / 2;

        for (int i = 0; i < totalHearts; i++)
        {
            GameObject heart = Instantiate(healthHeartPrefab, smallHealthPanel);
            Image heartImage = heart.GetComponent<Image>();
            RectTransform heartRectTransform = heart.GetComponent<RectTransform>();
            heartRectTransform.sizeDelta = new Vector2(30, 30);

            if (i < fullHearts)
            {
                heartImage.sprite = smallHealthHeartSprite;
            }
            else if (i == fullHearts && hasHalfHeart)
            {
                heartImage.sprite = halfHeartSprite;
            }
            else
            {
                heartImage.sprite = smallEmptyHeartSprite;
            }
        }

        for (int i = 0; i < currentShield / 2; i++)
        {
            GameObject shield = Instantiate(shieldHeartPrefab, smallHealthPanel);
            Image shieldImage = shield.GetComponent<Image>();
            RectTransform shieldRectTransform = shield.GetComponent<RectTransform>();
            shieldRectTransform.sizeDelta = new Vector2(30, 30);
            shieldImage.sprite = smallShieldHeartSprite;
        }
    }

    private void UpdateLargeHealthUI()
    {
        smallHealthPanel.gameObject.SetActive(false);
        largeHealthPanel.gameObject.SetActive(true);

        foreach (Transform child in largeHealthPanel)
        {
            Destroy(child.gameObject);
        }

        int fullHearts = currentHP / 2;
        bool hasHalfHeart = (currentHP % 2) != 0;
        int totalHearts = stat.maxHP / 2;

        for (int i = 0; i < totalHearts; i++)
        {
            GameObject heart = Instantiate(healthHeartPrefab, largeHealthPanel);
            Image heartImage = heart.GetComponent<Image>();
            RectTransform heartRectTransform = heart.GetComponent<RectTransform>();
            heartRectTransform.sizeDelta = new Vector2(55, 55);

            if (i < fullHearts)
            {
                heartImage.sprite = largeHealthHeartSprite;
            }
            else if (i == fullHearts && hasHalfHeart)
            {
                heartImage.sprite = halfHeartSprite;
            }
            else
            {
                heartImage.sprite = largeEmptyHeartSprite;
            }
        }

        for (int i = 0; i < currentShield / 2; i++)
        {
            GameObject shield = Instantiate(shieldHeartPrefab, largeHealthPanel);
            Image shieldImage = shield.GetComponent<Image>();
            RectTransform shieldRectTransform = shield.GetComponent<RectTransform>();
            shieldRectTransform.sizeDelta = new Vector2(55, 55);
            shieldImage.sprite = largeShieldHeartSprite;
        }
    }

    private IEnumerator ShowLargeHealthUI()
    {
        UpdateLargeHealthUI();

        yield return new WaitForSeconds(2f);

        largeHealthPanel.gameObject.SetActive(false);
        UpdateSmallHealthUI();
    }

    public void UpdateHealthUI()
    {
        UpdateSmallHealthUI();
        StartCoroutine(ShowLargeHealthUI());
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
            knockbackSpeed = stat.knockbackSpeed
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
            currentShield = data.currentShield;
            stat.playerDamage = data.playerDamage;
            stat.projectileRange = data.projectileRange;
            stat.knockbackSpeed = data.knockbackSpeed;
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
        currentShield = data.currentShield;
        stat.playerDamage = data.playerDamage;
        stat.projectileRange = data.projectileRange;
        stat.knockbackSpeed = data.knockbackSpeed;
        UpdateHealthUI();
    }

    private void OnApplicationQuit()
    {
        SavePlayerData();
    }
}


[System.Serializable]
public class PlayerDataToJson
{
    public int maxHP = 10;
    public int currentHP;
    public int currentShield;
    public int playerDamage;
    public float knockbackSpeed;
    public float projectileRange;

    public void InitializeDefaultValues()
    {
        currentHP = maxHP;
        currentShield = 0;
        playerDamage = 5;
        knockbackSpeed = 5.0f;
        projectileRange = 2;
    }
}