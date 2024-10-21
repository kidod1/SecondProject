using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Spine.Unity;
using Cinemachine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public PlayerData stat;
    public SynergyAbility currentSynergyAbility;
    public ObjectPool objectPool;
    public PlayerAbilityManager abilityManager;
    public Barrier barrierAbility;
    private List<UIShaker> healthBarShakers = new List<UIShaker>();

    [Header("Spine Animation")]
    public SkeletonAnimation skeletonAnimation;
    private Spine.Skeleton skeleton;
    private Spine.AnimationState spineAnimationState;

    // ��Ų �̸���
    [SpineSkin] public string frontSkinName;
    [SpineSkin] public string backSkinName;
    [SpineSkin] public string sideSkinName;
    [SpineSkin] public string sideFrontSkinName;
    [SpineSkin] public string sideBackSkinName;

    // �ִϸ��̼� �̸���
    [SpineAnimation] public string walkBackAnimName;
    [SpineAnimation] public string walkBackStopAnimName;
    [SpineAnimation] public string walkStraightAnimName;
    [SpineAnimation] public string walkStraightStopAnimName;

    [SpineAnimation] public string upperWalkBackAnimName;
    [SpineAnimation] public string upperWalkBackStopAnimName;
    [SpineAnimation] public string upperWalkStraightAnimName;
    [SpineAnimation] public string upperWalkStraightStopAnimName;

    [SpineAnimation] public string attackFrontAnimName;
    [SpineAnimation] public string attackBackAnimName;
    [SpineAnimation] public string attackSideAnimName;
    [SpineAnimation] public string attackSideFrontAnimName;
    [SpineAnimation] public string attackSideBackAnimName;

    [SpineAnimation] public string upperIdleAnimName;
    [SpineAnimation] public string lowerIdleAnimName;

    [Header("Game Start Animations")]
    // === �߰��� �κ� ���� ===
    [SpineAnimation] public string gameStartAnim1;
    [SpineAnimation] public string gameStartAnim2;
    [SpineAnimation] public string gameStartAnim3;
    private bool isGameStartAnimationPlaying = false;
    // === �߰��� �κ� �� ===

    [Header("Heal Effect")]
    [SerializeField]
    private GameObject healEffectPrefab;

    private bool isMoving = false;
    private Vector2 lastAttackDirection = Vector2.zero;
    private Vector2 moveInput;
    private Rigidbody2D rb;

    private bool isInvincible = false;
    private MeshRenderer meshRenderer;

    private float lastShootTime;
    public bool isShooting = false;
    private Vector2 shootDirection;
    private Vector2 lastMoveDirection = Vector2.right;

    private PlayerInput playerInput;

    public Transform shootPoint;

    public UnityEvent<Vector2, int, GameObject> OnShoot;
    public UnityEvent OnLevelUp;
    public UnityEvent<Collider2D> OnMonsterEnter;
    public UnityEvent OnShootCanceled;
    public UnityEvent OnTakeDamage;
    public UnityEvent OnPlayerDeath;
    public UnityEvent OnMonsterKilled;
    public UnityEvent<Collider2D> OnHitEnemy;
    public UnityEvent OnHeal;
    public UnityEvent<int> OnGainExperience;

    private string saveFilePath;

    public Vector2 PlayerPosition => transform.position;

    [Header("Camera Shake")]
    public CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        meshRenderer = GetComponent<MeshRenderer>();
        playerInput = new PlayerInput();

        OnShoot ??= new UnityEvent<Vector2, int, GameObject>();
        OnMonsterEnter ??= new UnityEvent<Collider2D>();
        OnShootCanceled ??= new UnityEvent();
        OnTakeDamage ??= new UnityEvent();
        OnPlayerDeath ??= new UnityEvent();
        OnMonsterKilled ??= new UnityEvent();
        OnHitEnemy ??= new UnityEvent<Collider2D>();
        OnHeal ??= new UnityEvent();
        OnGainExperience ??= new UnityEvent<int>();

        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");

        skeleton = skeletonAnimation.Skeleton;
        spineAnimationState = skeletonAnimation.AnimationState;

        skeleton.SetSkin(sideSkinName);
        skeleton.SetSlotsToSetupPose();

        spineAnimationState.Complete += OnSpineAnimationComplete;
    }

    private void Start()
    {
        healthBarShakers.AddRange(FindObjectsOfType<UIShaker>());
        InitializePlayer();
        UpdateUI();
        SavePlayerData();

        // === �߰��� �κ� ���� ===
        PlayRandomGameStartAnimation();
        // === �߰��� �κ� �� ===
    }

    // === �߰��� �κ� ���� ===
    private void PlayRandomGameStartAnimation()
    {
        string[] gameStartAnimations = { gameStartAnim1, gameStartAnim2, gameStartAnim3 };
        int randomIndex = UnityEngine.Random.Range(0, gameStartAnimations.Length);
        string selectedAnimation = gameStartAnimations[randomIndex];

        isGameStartAnimationPlaying = true;

        // �÷��̾� �Է� ��Ȱ��ȭ
        playerInput.Player.Disable();

        // SkeletonAnimation�� �ڵ� ������Ʈ ��Ȱ��ȭ
        skeletonAnimation.enabled = false;

        // ���� Ʈ�� �ε������� �ִϸ��̼� ���
        int trackIndex = 1; // Ʈ�� �ε����� �ִ��� ���� ����
        spineAnimationState.SetAnimation(trackIndex, selectedAnimation, false);

        // �ִϸ��̼� �Ϸ� �� �ݹ� ����
        Spine.TrackEntry trackEntry = spineAnimationState.GetCurrent(trackIndex);
        if (trackEntry != null)
        {
            trackEntry.Complete += OnGameStartAnimationComplete;
        }
    }


    private void OnGameStartAnimationComplete(Spine.TrackEntry trackEntry)
    {
        isGameStartAnimationPlaying = false;
        trackEntry.Complete -= OnGameStartAnimationComplete;

        // SkeletonAnimation�� �ڵ� ������Ʈ ��Ȱ��ȭ
        skeletonAnimation.enabled = true;

        // �÷��̾� �Է� Ȱ��ȭ
        playerInput.Player.Enable();

        // �Է� �̺�Ʈ �ڵ鷯 ����
        playerInput.Player.Move.performed += OnMovePerformed;
        playerInput.Player.Move.canceled += OnMoveCanceled;
        playerInput.Player.Shoot.performed += OnShootPerformed;
        playerInput.Player.Shoot.canceled += OnShootCanceledInputAction;

        // �ִϸ��̼� ������Ʈ
        UpdateAnimation();
    }



    // === �߰��� �κ� �� ===

    private void OnEnable()
    {
        if (!isGameStartAnimationPlaying)
        {
            playerInput.Player.Enable();

            playerInput.Player.Move.performed += OnMovePerformed;
            playerInput.Player.Move.canceled += OnMoveCanceled;
            playerInput.Player.Shoot.performed += OnShootPerformed;
            playerInput.Player.Shoot.canceled += OnShootCanceledInputAction;
        }

        spineAnimationState.Complete += OnSpineAnimationComplete;
    }


    private void OnDisable()
    {
        playerInput.Player.Disable();

        playerInput.Player.Move.performed -= OnMovePerformed;
        playerInput.Player.Move.canceled -= OnMoveCanceled;
        playerInput.Player.Shoot.performed -= OnShootPerformed;
        playerInput.Player.Shoot.canceled -= OnShootCanceledInputAction;

        spineAnimationState.Complete -= OnSpineAnimationComplete;
    }


    private void Update()
    {
        if (isGameStartAnimationPlaying)
        {
            // �������� SkeletonAnimation ������Ʈ
            skeletonAnimation.Update(Time.unscaledDeltaTime);
            skeletonAnimation.LateUpdate();
        }
        else
        {
            if (!isShooting)
            {
                UpdateAnimation();
            }

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                if (currentSynergyAbility != null)
                {
                    Debug.Log($"Activating ability: {currentSynergyAbility.abilityName}");
                    currentSynergyAbility.Activate(this);
                }
                else
                {
                    Debug.Log("No synergy ability assigned.");
                }
            }
        }
    }


    private void FixedUpdate()
    {
        Vector2 movement = moveInput.normalized * stat.currentPlayerSpeed * Time.fixedDeltaTime;
        Vector2 newPosition = rb.position + movement;
        rb.MovePosition(newPosition);
    }

    // 8������ ��Ÿ���� ������ ����
    private enum Direction8
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }

    private void UpdateAnimation()
    {
        if (isGameStartAnimationPlaying)
            return;
        Vector2 currentDirection;

        if (moveInput != Vector2.zero)
        {
            currentDirection = moveInput;
            lastMoveDirection = moveInput.normalized; // ������ �̵� ���� ������Ʈ
        }
        else if (lastAttackDirection != Vector2.zero)
        {
            currentDirection = lastAttackDirection;
        }
        else if (lastMoveDirection != Vector2.zero)
        {
            currentDirection = lastMoveDirection;
        }
        else
        {
            currentDirection = Vector2.down; // �⺻���� �Ʒ������� ����
        }

        Direction8 direction = GetDirection8(currentDirection);

        string skinName = GetSkinName(direction);
        bool flipX = ShouldFlipX(direction);

        skeleton.SetSkin(skinName);
        skeleton.SetSlotsToSetupPose();

        // �¿� ������ ���� ������Ʈ�� ���� �����Ϸ� ó��
        transform.localScale = new Vector3(flipX ? 0.15f : -0.15f, 0.15f, 0.15f);

        int upperBodyTrackIndex = 1; // ��ü �ִϸ��̼� Ʈ��
        int lowerBodyTrackIndex = 0; // ��ü �ִϸ��̼� Ʈ��

        if (moveInput.magnitude > 0)
        {
            // �̵� ���� ��
            string upperAnimationName = GetUpperBodyAnimationName(direction, true);
            string lowerAnimationName = GetLowerBodyAnimationName(direction, true);

            // ��ü �ִϸ��̼� ���� (Idle �Ǵ� �̵� ��ü �ִϸ��̼�)
            if (!isShooting)
            {
                if (!spineAnimationState.GetCurrent(upperBodyTrackIndex)?.Animation?.Name.Equals(upperAnimationName) ?? true)
                {
                    spineAnimationState.SetAnimation(upperBodyTrackIndex, upperAnimationName, true);
                }
            }

            // ��ü �ִϸ��̼� ����
            if (!spineAnimationState.GetCurrent(lowerBodyTrackIndex)?.Animation?.Name.Equals(lowerAnimationName) ?? true)
            {
                spineAnimationState.SetAnimation(lowerBodyTrackIndex, lowerAnimationName, true);
            }
        }
        else
        {
            // ���� ���� ��
            string upperAnimationName = GetUpperIdleAnimationName(direction);
            string lowerAnimationName = GetLowerIdleAnimationName(direction);

            // ��ü �ִϸ��̼� ���� (Idle �ִϸ��̼�)
            if (!isShooting)
            {
                if (!spineAnimationState.GetCurrent(upperBodyTrackIndex)?.Animation?.Name.Equals(upperAnimationName) ?? true)
                {
                    spineAnimationState.SetAnimation(upperBodyTrackIndex, upperAnimationName, true);
                }
            }

            // ��ü �ִϸ��̼� ����
            if (!spineAnimationState.GetCurrent(lowerBodyTrackIndex)?.Animation?.Name.Equals(lowerAnimationName) ?? true)
            {
                spineAnimationState.SetAnimation(lowerBodyTrackIndex, lowerAnimationName, true);
            }
        }
    }

    private string GetUpperIdleAnimationName(Direction8 direction)
    {
        return upperIdleAnimName;
    }

    private string GetLowerIdleAnimationName(Direction8 direction)
    {
        return lowerIdleAnimName;
    }

    private void PlayShootAnimation()
    {
        if (isGameStartAnimationPlaying)
            return;
        Direction8 direction = GetDirection8(shootDirection);

        lastAttackDirection = shootDirection.normalized; // Update last attack direction

        string skinName = GetSkinName(direction);
        string attackAnimationName = GetAttackAnimationName(direction);
        bool flipX = ShouldFlipX(direction);

        skeleton.SetSkin(skinName);
        skeleton.SetSlotsToSetupPose();

        // Handle flipping by adjusting local scale
        transform.localScale = new Vector3(flipX ? 0.15f : -0.15f, 0.15f, 0.15f);

        int upperBodyTrackIndex = 1; // Upper body animation track

        // Set attack animation on the upper body track
        spineAnimationState.SetAnimation(upperBodyTrackIndex, attackAnimationName, false).Complete += delegate {
            // Called when attack animation completes
            if (!isShooting)
            {
                // Transition to upper body idle animation
                string upperIdleAnimationName = GetUpperIdleAnimationName(direction);
                spineAnimationState.SetAnimation(upperBodyTrackIndex, upperIdleAnimationName, true);
            }
        };
    }

    private void OnSpineAnimationComplete(Spine.TrackEntry trackEntry)
    {
        int upperBodyTrackIndex = 1; // ��ü �ִϸ��̼� Ʈ��

        if (trackEntry.TrackIndex == upperBodyTrackIndex && !isShooting)
        {
            UpdateAnimation();
        }
    }

    private Direction8 GetDirection8(Vector2 direction)
    {
        if (direction == Vector2.zero)
            return Direction8.South; // �⺻��

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle = (angle + 360f) % 360f;

        if (angle >= 22.5f && angle < 67.5f)
            return Direction8.NorthEast;
        else if (angle >= 67.5f && angle < 112.5f)
            return Direction8.North;
        else if (angle >= 112.5f && angle < 157.5f)
            return Direction8.NorthWest;
        else if (angle >= 157.5f && angle < 202.5f)
            return Direction8.West;
        else if (angle >= 202.5f && angle < 247.5f)
            return Direction8.SouthWest;
        else if (angle >= 247.5f && angle < 292.5f)
            return Direction8.South;
        else if (angle >= 292.5f && angle < 337.5f)
            return Direction8.SouthEast;
        else
            return Direction8.East;
    }

    private string GetSkinName(Direction8 direction)
    {
        switch (direction)
        {
            case Direction8.North:
                return backSkinName;
            case Direction8.NorthEast:
            case Direction8.NorthWest:
                return sideBackSkinName;
            case Direction8.SouthEast:
            case Direction8.SouthWest:
                return sideFrontSkinName;
            case Direction8.East:
            case Direction8.West:
                return sideSkinName;
            case Direction8.South:
                return frontSkinName;
            default:
                return frontSkinName;
        }
    }

    private bool ShouldFlipX(Direction8 direction)
    {
        switch (direction)
        {
            case Direction8.East:
            case Direction8.NorthEast:
            case Direction8.SouthEast:
                return false; // �������� ���� ��
            case Direction8.West:
            case Direction8.NorthWest:
            case Direction8.SouthWest:
                return true; // ������ ���� ��
            default:
                return false;
        }
    }

    private string GetAttackAnimationName(Direction8 direction)
    {
        switch (direction)
        {
            case Direction8.North:
                return attackBackAnimName;
            case Direction8.NorthEast:
            case Direction8.NorthWest:
                return attackSideBackAnimName;
            case Direction8.SouthEast:
            case Direction8.SouthWest:
                return attackSideFrontAnimName;
            case Direction8.East:
            case Direction8.West:
                return attackSideAnimName;
            case Direction8.South:
                return attackFrontAnimName;
            default:
                return attackFrontAnimName;
        }
    }

    private string GetUpperBodyAnimationName(Direction8 direction, bool isMoving)
    {
        if (isMoving)
        {
            return GetUpperWalkAnimationName(direction);
        }
        else
        {
            return GetUpperIdleAnimationName(direction);
        }
    }

    private string GetLowerBodyAnimationName(Direction8 direction, bool isMoving)
    {
        if (isMoving)
        {
            return GetLowerWalkAnimationName(direction);
        }
        else
        {
            return GetLowerIdleAnimationName(direction);
        }
    }

    private string GetUpperWalkAnimationName(Direction8 direction)
    {
        switch (direction)
        {
            case Direction8.North:
            case Direction8.NorthEast:
            case Direction8.NorthWest:
                return upperWalkBackAnimName;
            default:
                return upperWalkStraightAnimName;
        }
    }

    private string GetLowerWalkAnimationName(Direction8 direction)
    {
        switch (direction)
        {
            case Direction8.North:
            case Direction8.NorthEast:
            case Direction8.NorthWest:
                return walkBackAnimName;
            default:
                return walkStraightAnimName;
        }
    }

    public void AcquireSynergyAbility(SynergyAbility newAbility)
    {
        currentSynergyAbility = newAbility;
    }

    public void SetInvincibility(bool value)
    {
        isInvincible = value;
    }

    public void TakeDamage(int damage)
    {
        if (barrierAbility != null && barrierAbility.IsShieldActive())
        {
            barrierAbility.DeactivateBarrierVisual();
            return;
        }

        if (!isInvincible)
        {
            stat.TakeDamage(damage);
            OnTakeDamage.Invoke();

            float healthPercentage = (float)stat.currentHP / stat.currentMaxHP;

            foreach (UIShaker shaker in healthBarShakers)
            {
                shaker.StartShake(healthPercentage);
            }

            if (impulseSource != null)
            {
                impulseSource.GenerateImpulse();
            }
            else
            {
                Debug.LogWarning("ImpulseSource�� �Ҵ���� �ʾҽ��ϴ�.");
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
            yield return new WaitForSecondsRealtime(blinkInterval);
        }

        meshRenderer.enabled = true;
        isInvincible = false;
    }

    private void Die()
    {
        SaveCurrencyOnly();
        Debug.Log("�÷��̾� ���");

        OnPlayerDeath.Invoke();
    }

    public void KillMonster()
    {
        OnMonsterKilled.Invoke();
    }

    public void Heal(int amount)
    {
        stat.Heal(amount);
        OnHeal.Invoke(); // ü�� ȸ�� �� �̺�Ʈ ȣ��
        UpdateUI();

        // �� ����Ʈ ����
        if (healEffectPrefab != null)
        {
            // �÷��̾��� �ڽ����� �� ����Ʈ�� �ν��Ͻ�ȭ
            GameObject healEffect = Instantiate(healEffectPrefab, transform);

            healEffect.transform.localRotation = Quaternion.identity; // �⺻ ȸ��

            Destroy(healEffect, 1f);
        }
        else
        {
            Debug.LogWarning("healEffectPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
        }
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

        OnGainExperience.Invoke(amount); // ����ġ ȹ�� �� �̺�Ʈ ȣ��
        UpdateUI();
    }

    private void UpdateUI()
    {
        PlayerUIManager uiManager = FindObjectOfType<PlayerUIManager>();
        if (uiManager != null)
        {
            uiManager.UpdateExperienceUI();
            uiManager.Initialize(this); // UI �Ŵ��� �ʱ�ȭ ȣ��
            uiManager.UpdateHealthUI();
            uiManager.UpdateCurrencyUI(stat.currentCurrency);
        }
        else
        {
            Debug.LogWarning("PlayerUIManager�� ã�� �� �����ϴ�.");
        }
    }

    private void InitializePlayer()
    {
        stat.InitializeStats();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = moveInput.normalized;
        }

        UpdateAnimation();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
        UpdateAnimation(); // �̵��� ������ �� �ִϸ��̼� ������Ʈ
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        if (!isShooting)
        {
            isShooting = true;
            StartCoroutine(DelayedShootStart());
        }
    }

    private IEnumerator DelayedShootStart()
    {
        // ª�� �ð� ���� ����Ͽ� �Է��� �����մϴ�.
        yield return new WaitForSeconds(0.05f); // 50�и��� ���

        UpdateShootDirection(); // ��� �Ŀ� ���� ���� ������Ʈ
        StartCoroutine(ShootContinuously());
    }

    private IEnumerator ShootContinuously()
    {
        while (isShooting)
        {
            if (Time.time >= lastShootTime + stat.currentShootCooldown)
            {
                UpdateShootDirection();
                Shoot(shootDirection, stat.currentProjectileType);
                lastShootTime = Time.time;

                PlayShootAnimation();
            }

            yield return null;
        }
    }

    private void UpdateShootDirection()
    {
        Vector2 newDirection = playerInput.Player.Shoot.ReadValue<Vector2>();
        if (newDirection == Vector2.zero)
        {
            newDirection = moveInput;
        }

        if (newDirection != Vector2.zero)
        {
            shootDirection = newDirection.normalized;
        }
        else
        {
            shootDirection = GetFacingDirection();
        }
    }

    private void OnShootCanceledInputAction(InputAction.CallbackContext context)
    {
        isShooting = false;
        OnShootCanceled.Invoke();
        UpdateAnimation();
    }

    public void Shoot(Vector2 direction, int prefabIndex)
    {
        if (objectPool == null)
        {
            Debug.LogError("objectPool is not assigned.");
            return;
        }

        // �߾� ����� �¿�� 10�� ȸ���� ���� ���
        Vector2[] shootDirections = new Vector2[3];
        shootDirections[0] = direction; // �߾�
        shootDirections[1] = RotateVector(direction, -10f); // ���� 10��
        shootDirections[2] = RotateVector(direction, 10f);  // ���� 10��

        foreach (Vector2 dir in shootDirections)
        {
            // ����ü �߻�
            GameObject projectile = objectPool.GetObject(prefabIndex);
            if (projectile == null)
            {
                Debug.LogError("Projectile could not be instantiated.");
                continue;
            }

            projectile.transform.position = shootPoint.position;

            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                // FieryBloodToastAbility�� ȿ�� ����
                float damageMultiplier = 1f;
                FieryBloodToastAbility fieryAbility = abilityManager.GetAbilityOfType<FieryBloodToastAbility>();
                if (fieryAbility != null)
                {
                    damageMultiplier = fieryAbility.GetDamageMultiplier();
                }

                // �ɷ��� �������� �ƴ� �⺻ ������ ������ ����
                int adjustedDamage = Mathf.RoundToInt(stat.currentPlayerDamage * damageMultiplier);

                // Projectile.Initialize�� 5�� �Ű������� �°� ����
                projScript.Initialize(stat, this, false, 1.0f, stat.currentPlayerDamage);
                projScript.SetDirection(dir.normalized);
            }
            else
            {
                Debug.LogError("Projectile ��ũ��Ʈ�� ã�� �� �����ϴ�.");
            }

            OnShoot.Invoke(dir.normalized, prefabIndex, projectile);
        }
    }

    private Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    public bool CanStun()
    {
        StunAbility stunAbility = abilityManager.GetAbilityOfType<StunAbility>();

        return stunAbility != null;
    }

    public Vector2 GetFacingDirection()
    {
        if (isShooting && shootDirection != Vector2.zero)
        {
            return shootDirection.normalized;
        }
        else if (moveInput != Vector2.zero)
        {
            return moveInput.normalized;
        }
        else
        {
            return lastMoveDirection;
        }
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
