using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Spine.Unity;
using Cinemachine;
using System.Collections.Generic;

public enum AbilityType
{
    JokerDraw,
    CardStrike,
    RicochetStrike,
    SharkStrike
}

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
    private Vector2 lastMoveDirection = Vector2.right;
    private bool hasNextShootDirection = false;

    private PlayerInput playerInput;

    public Transform shootPoint;

    public UnityEvent<Vector2, int> OnShoot;
    public UnityEvent OnLevelUp;
    public UnityEvent<Collider2D> OnMonsterEnter;
    public UnityEvent OnShootCanceled;
    public UnityEvent OnTakeDamage;
    public UnityEvent OnPlayerDeath;
    public UnityEvent OnMonsterKilled;
    public UnityEvent<Collider2D> OnHitEnemy;
    public UnityEvent OnHeal;
    public UnityEvent<int> OnGainExperience; // 경험치 획득 이벤트 추가

    private string saveFilePath;

    public Vector2 PlayerPosition => transform.position;

    [Header("Camera Shake")]
    public CinemachineImpulseSource impulseSource;

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
        OnMonsterKilled ??= new UnityEvent();
        OnHitEnemy ??= new UnityEvent<Collider2D>();
        OnHeal ??= new UnityEvent();
        OnGainExperience ??= new UnityEvent<int>(); // 이벤트 초기화

        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");

        skeleton = skeletonAnimation.Skeleton;
        spineAnimationState = skeletonAnimation.AnimationState;

        skeleton.SetSkin(frontSkinName);
        skeleton.SetSlotsToSetupPose();

        spineAnimationState.Complete += OnSpineAnimationComplete;
    }

    private void Start()
    {
        healthBarShakers.AddRange(FindObjectsOfType<UIShaker>());
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
            PlayShootAnimation();
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
            PlayShootAnimation();
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

            float healthPercentage = (float)stat.currentHP / stat.currentMaxHP;

            foreach (UIShaker shaker in healthBarShakers)
            {
                shaker.StartShake(healthPercentage);
            }

            if (impulseSource != null)
            {
                impulseSource.GenerateImpulse(); // 카메라 쉐이크 발생
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
        Debug.Log("몬스터 킬");
    }

    public void Heal(int amount)
    {
        stat.Heal(amount);
        OnHeal.Invoke(); // 체력 회복 시 이벤트 호출
        UpdateUI();
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

        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = moveInput.normalized;
        }

        UpdateAnimation();
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

                PlayShootAnimation();

                if (hasNextShootDirection)
                {
                    shootDirection = nextShootDirection;
                    hasNextShootDirection = false;
                }
            }

            yield return null;
        }
    }

    private void PlayShootAnimation()
    {
        int trackIndex = 2;

        if (shootDirection.y > 0)
        {
            skeleton.SetSkin(backSkinName);
            spineAnimationState.SetAnimation(trackIndex, shootBackUpperAnimName, false);
        }
        else if (shootDirection.y < 0)
        {
            skeleton.SetSkin(frontSkinName);
            spineAnimationState.SetAnimation(trackIndex, shootFrontUpperAnimName, false);
        }
        else
        {
            skeleton.SetSkin(sideSkinName);
            transform.localScale = new Vector3(shootDirection.x < 0 ? 0.1f : -0.1f, 0.1f, 0.1f);
            spineAnimationState.SetAnimation(trackIndex, shootSideUpperAnimName, false);
        }

        skeleton.SetSlotsToSetupPose();
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

        // 3방향으로 투사체 발사
        float angleOffset = 15f; // 각도 오프셋 설정
        Vector2[] shootDirections = new Vector2[3];

        // 중앙 방향
        shootDirections[0] = direction;

        // 왼쪽 방향
        shootDirections[1] = RotateVector(direction, angleOffset);

        // 오른쪽 방향
        shootDirections[2] = RotateVector(direction, -angleOffset);

        // 각 방향으로 투사체 생성
        foreach (var dir in shootDirections)
        {
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

                int adjustedDamage = Mathf.RoundToInt(stat.currentPlayerDamage * damageMultiplier);

                projScript.Initialize(stat, this, false, adjustedDamage);
                projScript.SetDirection(dir);
            }

            OnShoot.Invoke(dir, prefabIndex);
        }
    }
    private Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        float x = vector.x * cos - vector.y * sin;
        float y = vector.x * sin + vector.y * cos;

        return new Vector2(x, y);
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
            // 이동도 사격도 하지 않을 때 마지막 이동 방향을 반환
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
