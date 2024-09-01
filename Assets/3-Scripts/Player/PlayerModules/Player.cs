using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Spine.Unity;

public class Player : MonoBehaviour
{
    public PlayerData stat;
    public ObjectPool objectPool;
    public PlayerAbilityManager abilityManager;
    public Barrier barrierAbility;
    private UIShaker healthBarShaker;

    [Header("Spine Animation")]
    public SkeletonAnimation skeletonAnimation;
    private Spine.Skeleton skeleton;
    private Spine.AnimationState spineAnimationState;

    [SpineSkin] public string frontSkinName;
    [SpineSkin] public string backSkinName;
    [SpineSkin] public string sideSkinName;

    [SpineAnimation] public string walkLowerAnimName;
    [SpineAnimation] public string idleLowerAnimName;
    [SpineAnimation] public string shootFrontUpperAnimName;
    [SpineAnimation] public string shootBackUpperAnimName;
    [SpineAnimation] public string shootSideUpperAnimName;
    [SpineAnimation] public string idleUpperAnimName;

    private bool isMoving = false;

    private Vector2 moveInput;
    private Rigidbody2D rb;

    private bool isInvincible = false;
    private MeshRenderer meshRenderer;

    private float lastShootTime;
    public bool isShooting = false;
    private Vector2 shootDirection;
    private Vector2 nextShootDirection;
    private bool hasNextShootDirection = false;

    private PlayerInput playerInput;

    // ShootPoint 추가
    public Transform shootPoint; // 투사체가 발사될 위치

    public UnityEvent<Vector2, int> OnShoot;
    public UnityEvent OnLevelUp;
    public UnityEvent<Collider2D> OnMonsterEnter;
    public UnityEvent OnShootCanceled;
    public UnityEvent OnTakeDamage;
    public UnityEvent OnPlayerDeath;

    private string saveFilePath;

    public Vector2 PlayerPosition => transform.position;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        meshRenderer = GetComponent<MeshRenderer>();
        playerInput = new PlayerInput();

        OnShoot ??= new UnityEvent<Vector2, int>();
        OnMonsterEnter ??= new UnityEvent<Collider2D>();
        OnShootCanceled ??= new UnityEvent();
        OnTakeDamage ??= new UnityEvent();
        OnPlayerDeath ??= new UnityEvent();

        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");

        skeleton = skeletonAnimation.Skeleton;
        spineAnimationState = skeletonAnimation.AnimationState;

        skeleton.SetSkin(frontSkinName);
        skeleton.SetSlotsToSetupPose();

        spineAnimationState.Complete += OnSpineAnimationComplete;
    }

    private void Start()
    {
        healthBarShaker = FindObjectOfType<UIShaker>();
        InitializePlayer();
        UpdateUI();
        SavePlayerData();
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
        playerInput.Player.Move.performed += OnMovePerformed;
        playerInput.Player.Move.canceled += OnMoveCanceled;
        playerInput.Player.Shoot.started += OnShootStarted;
        playerInput.Player.Shoot.performed += OnShootPerformed;
        playerInput.Player.Shoot.canceled += OnShootCanceledInputAction;
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();
        playerInput.Player.Move.performed -= OnMovePerformed;
        playerInput.Player.Move.canceled -= OnMoveCanceled;
        playerInput.Player.Shoot.started -= OnShootStarted;
        playerInput.Player.Shoot.performed -= OnShootPerformed;
        playerInput.Player.Shoot.canceled -= OnShootCanceledInputAction;

        spineAnimationState.Complete -= OnSpineAnimationComplete;
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        Vector2 movement = moveInput * stat.currentPlayerSpeed * Time.fixedDeltaTime;
        Vector2 newPosition = rb.position + movement;

        rb.MovePosition(newPosition);
    }

    private void UpdateAnimation()
    {
        if (isShooting)
        {
            if (shootDirection.y > 0)
            {
                SetSkinAndAnimation(backSkinName, shootBackUpperAnimName, false);
            }
            else if (shootDirection.y < 0)
            {
                SetSkinAndAnimation(frontSkinName, shootFrontUpperAnimName, false);
            }
            else
            {
                SetSkinAndAnimation(sideSkinName, shootSideUpperAnimName, false, shootDirection.x < 0);
            }
        }
        else
        {
            if (moveInput.y > 0)
            {
                SetSkinAndAnimation(backSkinName, idleUpperAnimName, true);
            }
            else if (moveInput.y < 0)
            {
                SetSkinAndAnimation(frontSkinName, idleUpperAnimName, true);
            }
            else if (moveInput.x != 0)
            {
                SetSkinAndAnimation(sideSkinName, idleUpperAnimName, true, moveInput.x < 0);
            }
            else
            {
                spineAnimationState.SetAnimation(1, idleUpperAnimName, true);
            }
        }

        skeleton.SetSlotsToSetupPose();

        if (moveInput.magnitude > 0)
        {
            if (!isMoving)
            {
                spineAnimationState.SetAnimation(0, walkLowerAnimName, true);
                isMoving = true;
            }
        }
        else
        {
            if (isMoving)
            {
                spineAnimationState.SetAnimation(0, idleLowerAnimName, true);
                isMoving = false;
            }
        }
    }

    private void SetSkinAndAnimation(string skinName, string animationName, bool loop, bool flipX = false)
    {
        skeleton.SetSkin(skinName);
        transform.localScale = new Vector3(flipX ? 0.1f : -0.1f, 0.1f, 0.1f);
        spineAnimationState.SetAnimation(1, animationName, loop);
    }

    private void OnSpineAnimationComplete(Spine.TrackEntry trackEntry)
    {
        if (isShooting)
        {
            if (shootDirection.y > 0)
            {
                spineAnimationState.SetAnimation(1, shootBackUpperAnimName, false);
            }
            else if (shootDirection.y < 0)
            {
                spineAnimationState.SetAnimation(1, shootFrontUpperAnimName, false);
            }
            else
            {
                spineAnimationState.SetAnimation(1, shootSideUpperAnimName, false);
            }
        }
    }

    public void SetInvincibility(bool value)
    {
        isInvincible = value;
    }

    public void TakeDamage(int damage)
    {
        GameObject activeBarrier = GameObject.FindGameObjectWithTag("Barrier");

        if (activeBarrier != null)
        {
            Destroy(activeBarrier);
            barrierAbility.DeactivateBarrierVisual();
            barrierAbility.StartCooldown();
            return;
        }

        if (!isInvincible)
        {
            stat.TakeDamage(damage);
            OnTakeDamage.Invoke();

            if (healthBarShaker != null)
            {
                healthBarShaker.StartShake();
            }

            StartCoroutine(InvincibilityCoroutine());

            if (stat.currentHP <= 0)
            {
                Die();
            }
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float invincibilityDuration = 1f;
        float blinkInterval = 0.1f;

        for (float i = 0; i < invincibilityDuration; i += blinkInterval)
        {
            meshRenderer.enabled = !meshRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }

        meshRenderer.enabled = true;
        isInvincible = false;
    }

    private void Die()
    {
        SaveCurrencyOnly();
        Debug.Log("플레이어 사망");

        OnPlayerDeath.Invoke();
    }

    public void Heal(int amount)
    {
        stat.Heal(amount);
    }

    public int GetCurrentHP()
    {
        return stat.currentHP;
    }

    public void GainExperience(int amount)
    {
        if (stat.GainExperience(amount))
        {
            OnLevelUp?.Invoke();
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        PlayerUIManager uiManager = FindObjectOfType<PlayerUIManager>();
        if (uiManager != null)
        {
            uiManager.Initialize(this);
            uiManager.UpdateExperienceUI();
            uiManager.UpdateHealthUI();
            uiManager.UpdateCurrencyUI(stat.currentCurrency);
        }
        else
        {
            Debug.LogWarning("PlayerUIManager를 찾을 수 없습니다.");
        }
    }

    private void InitializePlayer()
    {
        stat.InitializeStats();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        UpdateAnimation();  // 방향 변경 시 즉시 애니메이션 업데이트
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
            newDirection = ConvertToFourDirections(newDirection);
            if (newDirection != Vector2.zero)
            {
                shootDirection = newDirection;
                isShooting = true;
                StartCoroutine(ShootContinuously());
            }
        }
    }

    private Vector2 ConvertToFourDirections(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return new Vector2(Mathf.Sign(direction.x), 0);
        }
        else
        {
            return new Vector2(0, Mathf.Sign(direction.y));
        }
    }

    private IEnumerator ShootContinuously()
    {
        while (isShooting)
        {
            if (Time.time >= lastShootTime + stat.currentShootCooldown)
            {
                Shoot(shootDirection, stat.currentProjectileType);
                lastShootTime = Time.time;

                UpdateAnimation();

                if (hasNextShootDirection)
                {
                    shootDirection = nextShootDirection;
                    hasNextShootDirection = false;
                }
            }
            yield return null;
        }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        Vector2 newDirection = context.ReadValue<Vector2>();
        newDirection = ConvertToFourDirections(newDirection);
        if (newDirection != Vector2.zero)
        {
            nextShootDirection = newDirection;
            hasNextShootDirection = true;
        }
    }

    private void OnShootCanceledInputAction(InputAction.CallbackContext context)
    {
        isShooting = false;
        OnShootCanceled.Invoke();
    }

    public void Shoot(Vector2 direction, int prefabIndex)
    {
        if (objectPool == null)
        {
            Debug.LogError("objectPool is not assigned.");
            return;
        }

        GameObject projectile = objectPool.GetObject(prefabIndex);
        if (projectile == null)
        {
            Debug.LogError("Projectile could not be instantiated.");
            return;
        }

        // ShootPoint 위치에서 투사체를 생성
        projectile.transform.position = shootPoint.position;

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.Initialize(stat);
            projScript.SetDirection(direction);
        }
        else
        {
            Debug.LogError("Projectile does not have a Projectile component.");
        }

        OnShoot.Invoke(direction, prefabIndex);
    }

    private void SaveCurrencyOnly()
    {
        PlayerCurrencyToJson data = new PlayerCurrencyToJson
        {
            currentCurrency = stat.currentCurrency
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(saveFilePath, json);
    }

    public void SavePlayerData()
    {
        PlayerDataToJson data = new PlayerDataToJson
        {
            currentPlayerSpeed = stat.currentPlayerSpeed,
            currentPlayerDamage = stat.currentPlayerDamage,
            currentProjectileSpeed = stat.currentProjectileSpeed,
            currentProjectileRange = stat.currentProjectileRange,
            currentProjectileType = stat.currentProjectileType,
            currentMaxHP = stat.currentMaxHP,
            currentHP = stat.currentHP,
            currentShield = stat.currentShield,
            currentShootCooldown = stat.currentShootCooldown,
            currentDefense = stat.currentDefense,
            currentExperience = stat.currentExperience,
            currentCurrency = stat.currentCurrency,
            currentLevel = stat.currentLevel
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

            stat.currentPlayerSpeed = data.currentPlayerSpeed;
            stat.currentPlayerDamage = data.currentPlayerDamage;
            stat.currentProjectileSpeed = data.currentProjectileSpeed;
            stat.currentProjectileRange = data.currentProjectileRange;
            stat.currentProjectileType = data.currentProjectileType;
            stat.currentMaxHP = data.currentMaxHP;
            stat.currentHP = data.currentHP;
            stat.currentShield = data.currentShield;
            stat.currentShootCooldown = data.currentShootCooldown;
            stat.currentDefense = data.currentDefense;
            stat.currentExperience = data.currentExperience;
            stat.currentCurrency = data.currentCurrency;
            stat.currentLevel = data.currentLevel;
        }
        else
        {
            Debug.LogWarning("Save file not found, using default player data.");
        }
    }

    private void OnApplicationQuit()
    {
        SavePlayerData();
    }
}

[System.Serializable]
public class PlayerCurrencyToJson
{
    public int currentCurrency;
}

[System.Serializable]
public class PlayerDataToJson
{
    public float currentPlayerSpeed;
    public int currentPlayerDamage;
    public float currentProjectileSpeed;
    public float currentProjectileRange;
    public int currentProjectileType;
    public int currentMaxHP;
    public int currentHP;
    public int currentShield;
    public float currentShootCooldown;
    public int currentDefense;
    public int currentExperience;
    public int currentCurrency;
    public int currentLevel;
}
