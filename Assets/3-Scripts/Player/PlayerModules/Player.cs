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

    // 스킨 이름들
    [SpineSkin] public string frontSkinName;
    [SpineSkin] public string backSkinName;
    [SpineSkin] public string sideSkinName;
    [SpineSkin] public string sideFrontSkinName;
    [SpineSkin] public string sideBackSkinName;

    // 애니메이션 이름들
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
    // === 추가된 부분 시작 ===
    [SpineAnimation] public string gameStartAnim1;
    [SpineAnimation] public string gameStartAnim2;
    [SpineAnimation] public string gameStartAnim3;
    private bool isGameStartAnimationPlaying = false;
    // === 추가된 부분 끝 ===

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

        // === 추가된 부분 시작 ===
        PlayRandomGameStartAnimation();
        // === 추가된 부분 끝 ===
    }

    // === 추가된 부분 시작 ===
    private void PlayRandomGameStartAnimation()
    {
        string[] gameStartAnimations = { gameStartAnim1, gameStartAnim2, gameStartAnim3 };
        int randomIndex = UnityEngine.Random.Range(0, gameStartAnimations.Length);
        string selectedAnimation = gameStartAnimations[randomIndex];

        isGameStartAnimationPlaying = true;

        // 플레이어 입력 비활성화
        playerInput.Player.Disable();

        // SkeletonAnimation의 자동 업데이트 비활성화
        skeletonAnimation.enabled = false;

        // 높은 트랙 인덱스에서 애니메이션 재생
        int trackIndex = 1; // 트랙 인덱스를 최대한 높게 설정
        spineAnimationState.SetAnimation(trackIndex, selectedAnimation, false);

        // 애니메이션 완료 시 콜백 설정
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

        // SkeletonAnimation의 자동 업데이트 재활성화
        skeletonAnimation.enabled = true;

        // 플레이어 입력 활성화
        playerInput.Player.Enable();

        // 입력 이벤트 핸들러 연결
        playerInput.Player.Move.performed += OnMovePerformed;
        playerInput.Player.Move.canceled += OnMoveCanceled;
        playerInput.Player.Shoot.performed += OnShootPerformed;
        playerInput.Player.Shoot.canceled += OnShootCanceledInputAction;

        // 애니메이션 업데이트
        UpdateAnimation();
    }



    // === 추가된 부분 끝 ===

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
            // 수동으로 SkeletonAnimation 업데이트
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

    // 8방향을 나타내는 열거형 정의
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
            lastMoveDirection = moveInput.normalized; // 마지막 이동 방향 업데이트
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
            currentDirection = Vector2.down; // 기본값을 아래쪽으로 설정
        }

        Direction8 direction = GetDirection8(currentDirection);

        string skinName = GetSkinName(direction);
        bool flipX = ShouldFlipX(direction);

        skeleton.SetSkin(skinName);
        skeleton.SetSlotsToSetupPose();

        // 좌우 반전을 게임 오브젝트의 로컬 스케일로 처리
        transform.localScale = new Vector3(flipX ? 0.15f : -0.15f, 0.15f, 0.15f);

        int upperBodyTrackIndex = 1; // 상체 애니메이션 트랙
        int lowerBodyTrackIndex = 0; // 하체 애니메이션 트랙

        if (moveInput.magnitude > 0)
        {
            // 이동 중일 때
            string upperAnimationName = GetUpperBodyAnimationName(direction, true);
            string lowerAnimationName = GetLowerBodyAnimationName(direction, true);

            // 상체 애니메이션 설정 (Idle 또는 이동 상체 애니메이션)
            if (!isShooting)
            {
                if (!spineAnimationState.GetCurrent(upperBodyTrackIndex)?.Animation?.Name.Equals(upperAnimationName) ?? true)
                {
                    spineAnimationState.SetAnimation(upperBodyTrackIndex, upperAnimationName, true);
                }
            }

            // 하체 애니메이션 설정
            if (!spineAnimationState.GetCurrent(lowerBodyTrackIndex)?.Animation?.Name.Equals(lowerAnimationName) ?? true)
            {
                spineAnimationState.SetAnimation(lowerBodyTrackIndex, lowerAnimationName, true);
            }
        }
        else
        {
            // 멈춰 있을 때
            string upperAnimationName = GetUpperIdleAnimationName(direction);
            string lowerAnimationName = GetLowerIdleAnimationName(direction);

            // 상체 애니메이션 설정 (Idle 애니메이션)
            if (!isShooting)
            {
                if (!spineAnimationState.GetCurrent(upperBodyTrackIndex)?.Animation?.Name.Equals(upperAnimationName) ?? true)
                {
                    spineAnimationState.SetAnimation(upperBodyTrackIndex, upperAnimationName, true);
                }
            }

            // 하체 애니메이션 설정
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
        int upperBodyTrackIndex = 1; // 상체 애니메이션 트랙

        if (trackEntry.TrackIndex == upperBodyTrackIndex && !isShooting)
        {
            UpdateAnimation();
        }
    }

    private Direction8 GetDirection8(Vector2 direction)
    {
        if (direction == Vector2.zero)
            return Direction8.South; // 기본값

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
                return false; // 오른쪽을 향할 때
            case Direction8.West:
            case Direction8.NorthWest:
            case Direction8.SouthWest:
                return true; // 왼쪽을 향할 때
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
                Debug.LogWarning("ImpulseSource가 할당되지 않았습니다.");
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
        Debug.Log("플레이어 사망");

        OnPlayerDeath.Invoke();
    }

    public void KillMonster()
    {
        OnMonsterKilled.Invoke();
    }

    public void Heal(int amount)
    {
        stat.Heal(amount);
        OnHeal.Invoke(); // 체력 회복 시 이벤트 호출
        UpdateUI();

        // 힐 이펙트 생성
        if (healEffectPrefab != null)
        {
            // 플레이어의 자식으로 힐 이펙트를 인스턴스화
            GameObject healEffect = Instantiate(healEffectPrefab, transform);

            healEffect.transform.localRotation = Quaternion.identity; // 기본 회전

            Destroy(healEffect, 1f);
        }
        else
        {
            Debug.LogWarning("healEffectPrefab이 할당되지 않았습니다.");
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

        OnGainExperience.Invoke(amount); // 경험치 획득 시 이벤트 호출
        UpdateUI();
    }

    private void UpdateUI()
    {
        PlayerUIManager uiManager = FindObjectOfType<PlayerUIManager>();
        if (uiManager != null)
        {
            uiManager.UpdateExperienceUI();
            uiManager.Initialize(this); // UI 매니저 초기화 호출
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

        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = moveInput.normalized;
        }

        UpdateAnimation();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
        UpdateAnimation(); // 이동이 멈췄을 때 애니메이션 업데이트
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
        // 짧은 시간 동안 대기하여 입력을 수집합니다.
        yield return new WaitForSeconds(0.05f); // 50밀리초 대기

        UpdateShootDirection(); // 대기 후에 공격 방향 업데이트
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

        // 중앙 방향과 좌우로 10도 회전된 방향 계산
        Vector2[] shootDirections = new Vector2[3];
        shootDirections[0] = direction; // 중앙
        shootDirections[1] = RotateVector(direction, -10f); // 좌측 10도
        shootDirections[2] = RotateVector(direction, 10f);  // 우측 10도

        foreach (Vector2 dir in shootDirections)
        {
            // 투사체 발사
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
                // FieryBloodToastAbility의 효과 적용
                float damageMultiplier = 1f;
                FieryBloodToastAbility fieryAbility = abilityManager.GetAbilityOfType<FieryBloodToastAbility>();
                if (fieryAbility != null)
                {
                    damageMultiplier = fieryAbility.GetDamageMultiplier();
                }

                // 능력의 데미지가 아닌 기본 공격의 데미지 설정
                int adjustedDamage = Mathf.RoundToInt(stat.currentPlayerDamage * damageMultiplier);

                // Projectile.Initialize의 5개 매개변수에 맞게 전달
                projScript.Initialize(stat, this, false, 1.0f, stat.currentPlayerDamage);
                projScript.SetDirection(dir.normalized);
            }
            else
            {
                Debug.LogError("Projectile 스크립트를 찾을 수 없습니다.");
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
