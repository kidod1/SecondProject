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

    // UI ���� ����
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

    // UI ���ҽ� ����
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

    // Movement ���� ����
    private Vector2 moveInput;
    private Rigidbody2D rb;

    // Health ���� ����
    private int currentHP;
    private int currentShield;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;

    // Shooting ���� ����
    private int baseAttack;

    private float shootCooldown = 0.5f;
    private float lastShootTime;
    private bool isShooting = false;
    private Vector2 shootDirection;
    private Vector2 nextShootDirection;
    private bool hasNextShootDirection = false;

    // ����ġ �� ���� ���� ����
    private int experience;
    private int level;
    private Coroutine experienceBarCoroutine;

    // �ɷ� ���� ����
    private List<Ability> abilities = new List<Ability>();
    private List<Ability> availableAbilities = new List<Ability>();
    private SynergyAbility synergyAbility;
    private bool hasSynergyAbility = false;

    // Input System
    private PlayerInput playerInput;

    // �̺�Ʈ
    public UnityEvent<Vector2, int> OnShoot;
    public UnityEvent OnLevelUp;

    // Save System
    private string saveFilePath;

    public Vector2 PlayerPosition => transform.position; // ��ġ ������ �����ϴ� ������Ƽ

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

        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");
    }

    private void Start()
    {
        Debug.Log("Save File Path: " + saveFilePath);
        InitializePlayer();
        LoadPlayerData();
        UpdateHealthUI();
        ResetPlayerData();
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

    public void Shoot(Vector2 direction, int prefabIndex)
    {
        GameObject projectile = objectPool.GetObject(prefabIndex);
        projectile.transform.position = transform.position;

        Projectile projScript = projectile.GetComponent<Projectile>();
        projScript.Initialize(stat);
        projScript.SetDirection(direction);

        OnShoot.Invoke(direction, prefabIndex);

        Debug.Log($"Shoot called with direction: {direction}, prefabIndex: {prefabIndex}");
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
        StartCoroutine(ShowLargeHealthUI()); // ü�� UI Ȯ�� ȿ�� ����

        StartCoroutine(InvincibilityCoroutine());

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }

        UpdateHealthUI(); // UI ������Ʈ
    }

    // �������� �޾��� �� ȣ��Ǵ� �Լ�
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

        StartCoroutine(ShowLargeHealthUI()); // ü�� UI Ȯ�� ȿ�� ����
        StartCoroutine(InvincibilityCoroutine());

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }

        UpdateHealthUI(); // UI ������Ʈ
    }

    // �ǰݽ� ����
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float invincibilityDuration = 1f; // ���� �ð� 1��
        float blinkInterval = 0.1f; // �����̴� ����

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
        // �÷��̾� ���� ó�� ���� �߰� (��: ���� ���� ȭ�� ǥ��, ����� ��)
        ResetPlayerData();
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > stat.maxHP)
        {
            currentHP = stat.maxHP;
        }

        UpdateHealthUI(); // UI ������Ʈ
    }

    public void IncreaseShield(int amount)
    {
        currentShield += amount;
        UpdateHealthUI(); // UI ������Ʈ
    }

    public void ChangeProjectile(int newProjectileType)
    {
        stat.projectileType = newProjectileType;
        Debug.Log("������Ÿ�� ���� " + newProjectileType);
    }

    public int GetCurrentHP()
    {
        return currentHP;
    }


    private void LoadAvailableAbilities()
    {
        // ����: Resources �������� �ɷ� �ε�
        availableAbilities.AddRange(Resources.LoadAll<Ability>("Abilities"));
        ResetAllAbilities();
    }
    private void ResetAllAbilities()
    {
        foreach (var ability in availableAbilities)
        {
            ability.ResetLevel();
        }
    }

    // ����ġ�� ��� �Լ�
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
        Debug.Log($"Selecting ability: {ability.abilityName}");
        ability.Apply(this); // �ɷ� ����

        if (ability.currentLevel == 0)
        {
            ability.currentLevel = 1;
            Debug.Log($"{ability.abilityName} initialized to level 1");
            abilities.Add(ability);
        }
        else
        {
            ability.Upgrade(); // �ɷ� ���׷��̵�
        }

        if (ability.currentLevel >= 5)
        {
            availableAbilities.Remove(ability);
        }

        CheckForSynergy(); // �ó��� �ɷ� üũ
    }

    // �ó��� �ɷ��� üũ�ϴ� �Լ�
    private void CheckForSynergy()
    {
        if (hasSynergyAbility) return;

        // �� �ɷ� �з��� �� ������ üũ
        var abilityCategories = new Dictionary<string, int>
    {
        { "Attack", 0 },
        { "Defense", 0 },
        { "Speed", 0 },
        { "Health", 0 },
        { "Magic", 0 },
        { "Support", 0 },
        { "Utility", 0 }
    };

        foreach (var ability in abilities)
        {
            abilityCategories[ability.category] += ability.currentLevel;
        }

        foreach (var category in abilityCategories)
        {
            // ���� �ɷ� �з��� �ش� �з��� �� ������ ���
            Debug.Log($"Category: {category.Key}, Total Level: {category.Value}");
            if (category.Value >= 15) // 5, 10, 15 ���� �޼� Ȯ��
            {
                AssignSynergyAbility(category.Key);
                break;
            }
        }
    }

    // �ó��� �ɷ��� �Ҵ��ϴ� �Լ�
    private void AssignSynergyAbility(string category)
    {
        hasSynergyAbility = true;
        // ����: Resources �������� �ó��� �ɷ� �ε�
        synergyAbility = Resources.Load<SynergyAbility>($"SynergyAbilities/{category}Synergy");
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
        baseAttack = stat.playerDamage;
        ResetAbilities();
        UpdateExperienceUI();
    }

    // ���� ��Ʈ UI ������Ʈ
    private void UpdateSmallHealthUI()
    {
        Debug.Log("Updating small health UI.");

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
            heartRectTransform.sizeDelta = new Vector2(30, 30); // ���� ��Ʈ ũ�� ����

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
            shieldRectTransform.sizeDelta = new Vector2(30, 30); // ���� �ǵ� ũ�� ����
            shieldImage.sprite = smallShieldHeartSprite;
            Debug.Log("Added small shield heart.");
        }
    }

    // ū ��Ʈ UI ������Ʈ
    private void UpdateLargeHealthUI()
    {
        Debug.Log("Updating large health UI.");

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
            heartRectTransform.sizeDelta = new Vector2(55, 55); // ū ��Ʈ ũ�� ����

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
            shieldRectTransform.sizeDelta = new Vector2(55, 55); // ū �ǵ� ũ�� ����
            shieldImage.sprite = largeShieldHeartSprite;
        }
    }

    // ū ��Ʈ UI�� ��� �����ִ� �ڷ�ƾ
    private IEnumerator ShowLargeHealthUI()
    {
        Debug.Log("Showing large health UI.");
        UpdateLargeHealthUI();

        yield return new WaitForSeconds(2f);

        Debug.Log("Hiding large health UI.");
        largeHealthPanel.gameObject.SetActive(false);
        UpdateSmallHealthUI();  // ���� ��Ʈ UI ������Ʈ
    }

    // ��ü ü�� UI ������Ʈ
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
        foreach (var ability in abilities)
        {
            ability.ResetLevel();
        }
        abilities.Clear();
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
