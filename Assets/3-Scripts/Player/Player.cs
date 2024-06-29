using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;

public class Player : MonoBehaviour
{
    [SerializeField]
    private PlayerData stat;
    public ObjectPool objectPool;

    // Movement ���� ����
    private Vector2 moveInput;
    private Rigidbody2D rb;

    // Health ���� ����
    private int currentHP;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;

    // Shooting ���� ����
    private float shootCooldown = 0.5f;
    private float lastShootTime;
    private bool isShooting = false;
    private Vector2 shootDirection;
    private Vector2 nextShootDirection;
    private bool hasNextShootDirection = false;

    // Input System
    private PlayerInput playerInput;

    // Shooting �̺�Ʈ
    public UnityEvent<Vector2, int> OnShoot;

    // �ɷ� ���� ����
    private List<Ability> acquiredAbilities = new List<Ability>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerInput = new PlayerInput();

        if (OnShoot == null)
        {
            OnShoot = new UnityEvent<Vector2, int>();
        }
    }

    private void Start()
    {
        InitializePlayer();
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

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isInvincible) return;

        currentHP -= damage;

        StartCoroutine(KnockbackCoroutine(knockbackDirection));
        StartCoroutine(InvincibilityCoroutine());

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        // �÷��̾� ���� ó�� ���� �߰� (��: ���� ���� ȭ�� ǥ��, ����� ��)
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > stat.MaxHP)
        {
            currentHP = stat.MaxHP;
        }
    }

    public void IncreaseAttack(int amount)
    {
        stat.playerDamage += amount;
    }

    public void IncreaseRange(int amount)
    {
        stat.projectileRange += amount;
    }

    public void IncreaseAttackSpeed(float amount)
    {
        stat.knockbackSpeed += amount;
    }

    // ���ο� �ɷ� �߰� �޼���
    public void IncreasePride(int amount)
    {
        // ���� ����
    }

    public void IncreaseWrath(int amount)
    {
        stat.playerDamage += amount; // �г� - ���ݷ� ���� ����
    }

    public void IncreaseGluttony(int amount)
    {
        // ��Ž ����
    }

    public void IncreaseGreed(int amount)
    {
        // Ž�� ����
    }

    public void IncreaseSloth(int amount)
    {
        // ���� ����
    }

    public void IncreaseEnvy(int amount)
    {
        // ���� ����
    }

    public void IncreaseLust(int amount)
    {
        // ���� ����
    }

    // ������Ÿ�� �����
    public void ChangeProjectile(int newProjectileType)
    {
        stat.projectileType = newProjectileType;
        Debug.Log("������Ÿ�� ���� " + newProjectileType);
    }

    // �÷��̾� ȸ��
    public int GetCurrentHP()
    {
        return currentHP;
    }

    // �ǰݽ� �˹�
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

    // �ǰݽ� ����
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

    // �÷��̾� �ʱ�ȭ �Լ�
    private void InitializePlayer()
    {
        stat.InitializeStats();
        currentHP = stat.MaxHP;
    }

    public void AddAbility(Ability ability)
    {
        acquiredAbilities.Add(ability);
        ability.Apply(this);

        if (ability is IncreasePride || ability is IncreaseAttack || ability is IncreaseRange || ability is IncreaseAttackSpeed)
        {
            UpdateAbilityTreeProgress("Pride");
        }
        else if (ability is IncreaseWrath || ability is IncreaseSuperWrath || ability is IncreaseUltraWrath)  // �г� �ɷ� ����
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

        // ���൵�� 3�� �� Ư�� �ɷ� �߰�
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
        // ���൵�� 5�� �� Ư�� �ɷ� �߰�
        else if (progress == 5)
        {
            switch (treeName)
            {
                case "Pride":
                    AddSpecialAbility(new SpecialAbilitySuperPride());
                    break;
                case "Wrath":
                    AddSpecialAbility(new SpecialAbilitySuperWrath());
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
        // ���൵�� 7�� �� Ư�� �ɷ� �߰�
        else if (progress == 7)
        {
            switch (treeName)
            {
                case "Pride":
                    AddSpecialAbility(new SpecialAbilityUltraPride());
                    break;
                case "Wrath":
                    AddSpecialAbility(new SpecialAbilityUltraWrath());
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

    // ȹ���� �ɷ��� ���� ��ȯ�ϴ� �Լ�
    public int GetAcquiredAbilityCount()
    {
        return acquiredAbilities.Count;
    }
    // ��ũƮ�� ���൵
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

}
