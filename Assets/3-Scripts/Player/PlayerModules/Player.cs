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
    [Tooltip("플레이어의 스탯 데이터")]
    public PlayerData stat;

    [Tooltip("현재 시너지 능력")]
    public SynergyAbility currentSynergyAbility;

    [Tooltip("오브젝트 풀")]
    public ObjectPool objectPool;

    [Tooltip("플레이어의 능력 매니저")]
    public PlayerAbilityManager abilityManager;

    [Tooltip("방어막 능력")]
    public Barrier barrierAbility;

    private List<UIShaker> healthBarShakers = new List<UIShaker>();

    [Header("Spine 애니메이션")]
    [Tooltip("Skeleton 애니메이션")]
    public SkeletonAnimation skeletonAnimation;

    private Spine.Skeleton skeleton;
    private Spine.AnimationState spineAnimationState;

    [SpineSkin, Tooltip("앞쪽 스킨 이름")]
    public string frontSkinName;
    [SpineSkin, Tooltip("뒤쪽 스킨 이름")]
    public string backSkinName;
    [SpineSkin, Tooltip("측면 스킨 이름")]
    public string sideSkinName;
    [SpineSkin, Tooltip("측면 앞 스킨 이름")]
    public string sideFrontSkinName;
    [SpineSkin, Tooltip("측면 뒤 스킨 이름")]
    public string sideBackSkinName;

    [SpineAnimation, Tooltip("뒤로 걷기 애니메이션 이름")]
    public string walkBackAnimName;
    [SpineAnimation, Tooltip("뒤로 걷기 멈춤 애니메이션 이름")]
    public string walkBackStopAnimName;
    [SpineAnimation, Tooltip("앞으로 걷기 애니메이션 이름")]
    public string walkStraightAnimName;
    [SpineAnimation, Tooltip("앞으로 걷기 멈춤 애니메이션 이름")]
    public string walkStraightStopAnimName;

    [SpineAnimation, Tooltip("상체 뒤로 걷기 애니메이션 이름")]
    public string upperWalkBackAnimName;
    [SpineAnimation, Tooltip("상체 뒤로 걷기 멈춤 애니메이션 이름")]
    public string upperWalkBackStopAnimName;
    [SpineAnimation, Tooltip("상체 앞으로 걷기 애니메이션 이름")]
    public string upperWalkStraightAnimName;
    [SpineAnimation, Tooltip("상체 앞으로 걷기 멈춤 애니메이션 이름")]
    public string upperWalkStraightStopAnimName;

    [SpineAnimation, Tooltip("앞 공격 애니메이션 이름")]
    public string attackFrontAnimName;
    [SpineAnimation, Tooltip("뒤 공격 애니메이션 이름")]
    public string attackBackAnimName;
    [SpineAnimation, Tooltip("측면 공격 애니메이션 이름")]
    public string attackSideAnimName;
    [SpineAnimation, Tooltip("측면 앞 공격 애니메이션 이름")]
    public string attackSideFrontAnimName;
    [SpineAnimation, Tooltip("측면 뒤 공격 애니메이션 이름")]
    public string attackSideBackAnimName;

    [SpineAnimation, Tooltip("상체 대기 애니메이션 이름")]
    public string upperIdleAnimName;
    [SpineAnimation, Tooltip("하체 대기 애니메이션 이름")]
    public string lowerIdleAnimName;

    [Header("게임 시작 애니메이션")]
    [SpineAnimation, Tooltip("게임 시작 애니메이션 1")]
    public string gameStartAnim1;
    [SpineAnimation, Tooltip("게임 시작 애니메이션 2")]
    public string gameStartAnim2;
    [SpineAnimation, Tooltip("게임 시작 애니메이션 3")]
    public string gameStartAnim3;
    private bool isGameStartAnimationPlaying = false;

    [Header("힐 이펙트")]
    [SerializeField, Tooltip("힐 이펙트 프리팹")]
    private GameObject healEffectPrefab;
    [SerializeField, Tooltip("피격 이펙트 컨트롤러")]
    private HitEffectController hitEffectController;

    private bool isMoving = false;
    private Vector2 lastAttackDirection = Vector2.zero;
    private Vector2 moveInput;
    private Rigidbody2D rb;

    private bool isInvincible = false;
    private MeshRenderer meshRenderer;

    public bool isShooting = false;
    private Vector2 shootDirection;
    private Vector2 lastMoveDirection = Vector2.zero;

    private PlayerInput playerInput;
    [Tooltip("발사 위치")]
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
    public UnityEvent OnStartPlayerAnimationComplete;
    public UnityEvent OnPlayerStartMove;
    public UnityEvent OnPlayerStopMove;


    private string saveFilePath;

    public Vector2 PlayerPosition => transform.position;

    [Header("카메라 흔들림")]
    [Tooltip("Cinemachine 임펄스 소스")]
    public CinemachineImpulseSource impulseSource;

    [Header("샷건 설정")]
    [SerializeField, Tooltip("발사할 총알의 개수")]
    private int numberOfProjectiles = 5;
    [SerializeField, Tooltip("프로젝트일이 퍼지는 총 각도 (도 단위)")]
    private float spreadAngle = 30f;
    [SerializeField, Tooltip("프로젝트일 속도의 최소값")]
    private float minProjectileSpeed = 8f;
    [SerializeField, Tooltip("프로젝트일 속도의 최대값")]
    private float maxProjectileSpeed = 12f;
    [SerializeField, Tooltip("프로젝트일 생명 시간의 최소값")]
    private float minLifetime = 1f;
    [SerializeField, Tooltip("프로젝트일 생명 시간의 최대값")]
    private float maxLifetime = 2f;

    private float nextShootTime;

    private bool isOverheated = false; // 과열 상태를 나타내는 변수
    public bool IsOverheated
    {
        get { return isOverheated; }
        set { isOverheated = value; }
    }

    /// <summary>
    /// 플레이어의 컴포넌트를 초기화합니다.
    /// </summary>
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
        OnStartPlayerAnimationComplete ??= new UnityEvent();
        OnPlayerStartMove ??= new UnityEvent();
        OnPlayerStopMove ??= new UnityEvent();

        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");

        skeleton = skeletonAnimation.Skeleton;
        spineAnimationState = skeletonAnimation.AnimationState;

        skeleton.SetSkin(sideSkinName);
        skeleton.SetSlotsToSetupPose();

        spineAnimationState.Complete += OnSpineAnimationComplete;
    }

    /// <summary>
    /// 플레이어를 초기화하고 UI를 업데이트합니다.
    /// </summary>
    private void Start()
    {
        healthBarShakers.AddRange(FindObjectsOfType<UIShaker>());
        InitializePlayer();
        UpdateUI();
        SavePlayerData();

        PlayRandomGameStartAnimation();
    }

    /// <summary>
    /// 랜덤한 게임 시작 애니메이션을 재생합니다.
    /// </summary>
    private void PlayRandomGameStartAnimation()
    {
        string[] gameStartAnimations = { gameStartAnim1, gameStartAnim2, gameStartAnim3 };
        int randomIndex = UnityEngine.Random.Range(0, gameStartAnimations.Length);
        string selectedAnimation = gameStartAnimations[randomIndex];

        isGameStartAnimationPlaying = true;

        DisableControls();
        skeletonAnimation.enabled = false;

        int trackIndex = 1;
        spineAnimationState.SetAnimation(trackIndex, selectedAnimation, false);

        Spine.TrackEntry trackEntry = spineAnimationState.GetCurrent(trackIndex);
        if (trackEntry != null)
        {
            trackEntry.Complete += OnGameStartAnimationComplete;
        }
    }

    /// <summary>
    /// 게임 시작 애니메이션 완료 시 호출되는 콜백입니다.
    /// </summary>
    /// <param name="trackEntry">트랙 엔트리</param>
    private void OnGameStartAnimationComplete(Spine.TrackEntry trackEntry)
    {
        isGameStartAnimationPlaying = false;
        trackEntry.Complete -= OnGameStartAnimationComplete;

        skeletonAnimation.enabled = true;

        EnableControls();
        if (!PlayManager.I.isPlayerDied)
        {
            OnStartPlayerAnimationComplete.Invoke();
        }

        playerInput.Player.Move.performed += OnMovePerformed;
        playerInput.Player.Move.canceled += OnMoveCanceled;
        playerInput.Player.Shoot.performed += OnShootPerformed;
        playerInput.Player.Shoot.canceled += OnShootCanceledInputAction;

        UpdateAnimation();

    }

    /// <summary>
    /// 컴포넌트가 활성화될 때 호출됩니다.
    /// </summary>
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

    /// <summary>
    /// 컴포넌트가 비활성화될 때 호출됩니다.
    /// </summary>
    private void OnDisable()
    {
        playerInput.Player.Disable();

        playerInput.Player.Move.performed -= OnMovePerformed;
        playerInput.Player.Move.canceled -= OnMoveCanceled;
        playerInput.Player.Shoot.performed -= OnShootPerformed;
        playerInput.Player.Shoot.canceled -= OnShootCanceledInputAction;

        spineAnimationState.Complete -= OnSpineAnimationComplete;
    }

    /// <summary>
    /// 매 프레임마다 호출되어 입력 및 애니메이션을 업데이트합니다.
    /// </summary>
    private void Update()
    {
        if (isGameStartAnimationPlaying)
        {
            skeletonAnimation.Update(Time.deltaTime);
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
                    currentSynergyAbility.Activate(this);
                }
            }
        }
        if (isShooting)
        {
            UpdateShootDirection();
            TryShoot();
        }
    }
    private void TryShoot()
    {
        if (isOverheated)
            return;

        if (Time.time >= nextShootTime)
        {
            Shoot(shootDirection, stat.currentProjectileType);
            nextShootTime = Time.time + (1f / stat.currentAttackSpeed);
            PlayShootAnimation();
        }
    }

    public void ResetNextShootTime()
    {
        nextShootTime = Time.time + (1f / stat.currentAttackSpeed);
    }
    /// <summary>
    /// 물리 업데이트를 처리합니다.
    /// </summary>
    private void FixedUpdate()
    {
        Vector2 movement = moveInput.normalized * stat.currentPlayerSpeed * Time.fixedDeltaTime;
        Vector2 newPosition = rb.position + movement;
        rb.MovePosition(newPosition);
    }

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

    /// <summary>
    /// 플레이어의 애니메이션을 업데이트합니다.
    /// </summary>
    private void UpdateAnimation()
    {
        if (isGameStartAnimationPlaying)
            return;

        Vector2 currentDirection;

        if (moveInput != Vector2.zero)
        {
            currentDirection = moveInput;
            lastMoveDirection = moveInput.normalized;
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
            currentDirection = Vector2.down;
        }

        Direction8 direction = GetDirection8(currentDirection);

        string skinName = GetSkinName(direction);
        bool flipX = ShouldFlipX(direction);

        skeleton.SetSkin(skinName);
        skeleton.SetSlotsToSetupPose();

        transform.localScale = new Vector3(flipX ? 0.15f : -0.15f, 0.15f, 0.15f);

        int upperBodyTrackIndex = 1;
        int lowerBodyTrackIndex = 0;

        if (moveInput.magnitude > 0)
        {
            string upperAnimationName = GetUpperBodyAnimationName(direction, true);
            string lowerAnimationName = GetLowerBodyAnimationName(direction, true);

            if (!isShooting)
            {
                if (!spineAnimationState.GetCurrent(upperBodyTrackIndex)?.Animation?.Name.Equals(upperAnimationName) ?? true)
                {
                    spineAnimationState.SetAnimation(upperBodyTrackIndex, upperAnimationName, true);
                }
            }

            if (!spineAnimationState.GetCurrent(lowerBodyTrackIndex)?.Animation?.Name.Equals(lowerAnimationName) ?? true)
            {
                spineAnimationState.SetAnimation(lowerBodyTrackIndex, lowerAnimationName, true);
            }
        }
        else
        {
            string upperAnimationName = GetUpperIdleAnimationName(direction);
            string lowerAnimationName = GetLowerIdleAnimationName(direction);

            if (!isShooting)
            {
                if (!spineAnimationState.GetCurrent(upperBodyTrackIndex)?.Animation?.Name.Equals(upperAnimationName) ?? true)
                {
                    spineAnimationState.SetAnimation(upperBodyTrackIndex, upperIdleAnimName, true);
                }
            }

            if (!spineAnimationState.GetCurrent(lowerBodyTrackIndex)?.Animation?.Name.Equals(lowerAnimationName) ?? true)
            {
                spineAnimationState.SetAnimation(lowerBodyTrackIndex, lowerIdleAnimName, true);
            }
        }
    }

    /// <summary>
    /// 상체의 Idle 애니메이션 이름을 가져옵니다.
    /// </summary>
    /// <param name="direction">현재 방향</param>
    /// <returns>상체 Idle 애니메이션 이름</returns>
    private string GetUpperIdleAnimationName(Direction8 direction)
    {
        return upperIdleAnimName;
    }

    /// <summary>
    /// 하체의 Idle 애니메이션 이름을 가져옵니다.
    /// </summary>
    /// <param name="direction">현재 방향</param>
    /// <returns>하체 Idle 애니메이션 이름</returns>
    private string GetLowerIdleAnimationName(Direction8 direction)
    {
        return lowerIdleAnimName;
    }

    /// <summary>
    /// 공격 애니메이션을 재생합니다.
    /// </summary>
    private void PlayShootAnimation()
    {
        if (isGameStartAnimationPlaying)
            return;

        Direction8 direction = GetDirection8(shootDirection);

        lastAttackDirection = shootDirection.normalized;

        string skinName = GetSkinName(direction);
        string attackAnimationName = GetAttackAnimationName(direction);
        bool flipX = ShouldFlipX(direction);

        skeleton.SetSkin(skinName);
        skeleton.SetSlotsToSetupPose();

        transform.localScale = new Vector3(flipX ? 0.15f : -0.15f, 0.15f, 0.15f);

        int upperBodyTrackIndex = 1;

        spineAnimationState.SetAnimation(upperBodyTrackIndex, attackAnimationName, false).Complete += delegate
        {
            if (!isShooting)
            {
                string upperIdleAnimationName = GetUpperIdleAnimationName(direction);
                spineAnimationState.SetAnimation(upperBodyTrackIndex, upperIdleAnimationName, true);
            }
        };
    }

    /// <summary>
    /// Spine 애니메이션이 완료되었을 때 호출되는 콜백입니다.
    /// </summary>
    /// <param name="trackEntry">트랙 엔트리</param>
    private void OnSpineAnimationComplete(Spine.TrackEntry trackEntry)
    {
        int upperBodyTrackIndex = 1;

        if (trackEntry.TrackIndex == upperBodyTrackIndex && !isShooting)
        {
            UpdateAnimation();
        }
    }

    /// <summary>
    /// 주어진 방향을 8방향 열거형으로 변환합니다.
    /// </summary>
    /// <param name="direction">벡터 방향</param>
    /// <returns>8방향 열거형 값</returns>
    private Direction8 GetDirection8(Vector2 direction)
    {
        if (direction == Vector2.zero)
            return Direction8.South;

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

    /// <summary>
    /// 현재 방향에 따른 스킨 이름을 가져옵니다.
    /// </summary>
    /// <param name="direction">현재 방향</param>
    /// <returns>스킨 이름</returns>
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

    /// <summary>
    /// 현재 방향에 따라 좌우 반전이 필요한지 여부를 결정합니다.
    /// </summary>
    /// <param name="direction">현재 방향</param>
    /// <returns>좌우 반전 필요 여부</returns>
    private bool ShouldFlipX(Direction8 direction)
    {
        switch (direction)
        {
            case Direction8.East:
            case Direction8.NorthEast:
            case Direction8.SouthEast:
                return false;
            case Direction8.West:
            case Direction8.NorthWest:
            case Direction8.SouthWest:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 현재 방향에 따른 공격 애니메이션 이름을 가져옵니다.
    /// </summary>
    /// <param name="direction">현재 방향</param>
    /// <returns>공격 애니메이션 이름</returns>
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

    /// <summary>
    /// 현재 방향 및 움직임 상태에 따른 상체 애니메이션 이름을 가져옵니다.
    /// </summary>
    /// <param name="direction">현재 방향</param>
    /// <param name="isMoving">움직임 여부</param>
    /// <returns>상체 애니메이션 이름</returns>
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

    /// <summary>
    /// 현재 방향 및 움직임 상태에 따른 하체 애니메이션 이름을 가져옵니다.
    /// </summary>
    /// <param name="direction">현재 방향</param>
    /// <param name="isMoving">움직임 여부</param>
    /// <returns>하체 애니메이션 이름</returns>
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

    /// <summary>
    /// 현재 방향에 따른 상체 걷기 애니메이션 이름을 가져옵니다.
    /// </summary>
    /// <param name="direction">현재 방향</param>
    /// <returns>상체 걷기 애니메이션 이름</returns>
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

    /// <summary>
    /// 현재 방향에 따른 하체 걷기 애니메이션 이름을 가져옵니다.
    /// </summary>
    /// <param name="direction">현재 방향</param>
    /// <returns>하체 걷기 애니메이션 이름</returns>
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

    /// <summary>
    /// 새로운 시너지 능력을 획득합니다.
    /// </summary>
    /// <param name="newAbility">획득할 시너지 능력</param>
    public void AcquireSynergyAbility(SynergyAbility newAbility)
    {
        currentSynergyAbility = newAbility;
    }

    /// <summary>
    /// 무적 상태를 설정합니다.
    /// </summary>
    /// <param name="value">무적 상태 여부</param>
    public void SetInvincibility(bool value)
    {
        isInvincible = value;
    }

    /// <summary>
    /// 데미지를 받고 체력을 감소시킵니다.
    /// </summary>
    /// <param name="damage">받을 데미지 양</param>
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

            if (hitEffectController != null)
            {
                hitEffectController.PlayRandomHitAnimation();
            }

            if (stat.currentHP <= 0)
            {
                Die();
            }
        }
    }

    /// <summary>
    /// 무적 상태를 일정 시간 유지하는 코루틴입니다.
    /// </summary>
    /// <returns>코루틴</returns>
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

    /// <summary>
    /// 플레이어가 사망했을 때 호출됩니다.
    /// </summary>
    private void Die()
    {
        SaveCurrencyOnly();
        OnPlayerDeath.Invoke();
        PlayManager.I.isPlayerDie();
    }

    /// <summary>
    /// 몬스터를 처치했을 때 호출됩니다.
    /// </summary>
    public void KillMonster()
    {
        OnMonsterKilled.Invoke();
    }

    /// <summary>
    /// 플레이어의 체력을 회복합니다.
    /// </summary>
    /// <param name="amount">회복할 체력 양</param>
    public void Heal(int amount)
    {
        stat.Heal(amount);
        OnHeal.Invoke();
        UpdateUI();

        if (healEffectPrefab != null)
        {
            GameObject healEffect = Instantiate(healEffectPrefab, transform);
            healEffect.transform.localRotation = Quaternion.identity;
            Destroy(healEffect, 1f);
        }
        else
        {
            Debug.LogWarning("healEffectPrefab이 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// 현재 체력을 가져옵니다.
    /// </summary>
    /// <returns>현재 체력</returns>
    public int GetCurrentHP()
    {
        return stat.currentHP;
    }

    /// <summary>
    /// 경험치를 획득하고 레벨업 여부를 판단합니다.
    /// </summary>
    /// <param name="amount">획득할 경험치 양</param>
    public void GainExperience(int amount)
    {
        if (stat.GainExperience(amount))
        {
            OnLevelUp?.Invoke();
        }

        OnGainExperience.Invoke(amount);
        UpdateUI();
    }

    /// <summary>
    /// 플레이어의 UI를 업데이트합니다.
    /// </summary>
    private void UpdateUI()
    {
        PlayerUIManager uiManager = FindObjectOfType<PlayerUIManager>();
        if (uiManager != null)
        {
            uiManager.UpdateExperienceUI();
            uiManager.Initialize(this);
            uiManager.UpdateHealthUI();
            uiManager.UpdateCurrencyUI(stat.currentCurrency);
        }
        else
        {
            Debug.LogWarning("PlayerUIManager를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 플레이어의 스탯을 초기화합니다.
    /// </summary>
    private void InitializePlayer()
    {
        stat.InitializeStats();
    }

    /// <summary>
    /// 이동 입력이 발생했을 때 호출됩니다.
    /// </summary>
    /// <param name="context">입력 컨텍스트</param>
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 previousMoveInput = moveInput;
        moveInput = context.ReadValue<Vector2>();

        if (moveInput != Vector2.zero && !isMoving)
        {
            isMoving = true;
            OnPlayerStartMove.Invoke();
        }

        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = moveInput.normalized;
        }

        UpdateAnimation();
    }

    /// <summary>
    /// 이동 입력이 취소되었을 때 호출됩니다.
    /// </summary>
    /// <param name="context">입력 컨텍스트</param>
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;

        if (isMoving)
        {
            isMoving = false;
            OnPlayerStopMove.Invoke();
        }

        UpdateAnimation();
    }

    /// <summary>
    /// 공격 입력이 발생했을 때 호출됩니다.
    /// </summary>
    /// <param name="context">입력 컨텍스트</param>
    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        isShooting = true;
        UpdateShootDirection();
    }

    /// <summary>
    /// 공격 방향을 업데이트합니다.
    /// </summary>
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

    /// <summary>
    /// 공격 입력이 취소되었을 때 호출됩니다.
    /// </summary>
    /// <param name="context">입력 컨텍스트</param>
    private void OnShootCanceledInputAction(InputAction.CallbackContext context)
    {
        isShooting = false;
        OnShootCanceled.Invoke();
        UpdateAnimation();
    }
    /// <summary>
    /// 주어진 방향으로 총알을 발사합니다.
    /// </summary>
    /// <param name="direction">발사할 방향</param>
    /// <param name="prefabIndex">프리팹 인덱스</param>
    private void Shoot(Vector2 direction, int prefabIndex)
    {
        if (objectPool == null)
        {
            Debug.LogError("objectPool가 할당되지 않았습니다.");
            return;
        }

        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        for (int i = 0; i < numberOfProjectiles; i++)
        {
            float randomAngle = baseAngle + UnityEngine.Random.Range(-spreadAngle / 2f, spreadAngle / 2f);
            Vector2 shootDir = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;

            float randomSpeed = UnityEngine.Random.Range(minProjectileSpeed, maxProjectileSpeed);
            float randomLifetime = UnityEngine.Random.Range(minLifetime, maxLifetime);

            GameObject projectile = objectPool.GetObject(prefabIndex);
            if (projectile == null)
            {
                Debug.LogError("Projectile을 인스턴스화할 수 없습니다.");
                continue;
            }

            projectile.transform.position = shootPoint.position;

            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                float damageMultiplier = 1f;
                FieryBloodToastAbility fieryAbility = abilityManager.GetAbilityOfType<FieryBloodToastAbility>();
                if (fieryAbility != null)
                {
                    damageMultiplier = fieryAbility.GetDamageMultiplier();
                }

                projScript.Initialize(stat, this, false, 1.0f, stat.buffedPlayerDamage, randomSpeed, randomLifetime);
                projScript.SetDirection(shootDir.normalized);
            }
            else
            {
                Debug.LogError("Projectile 스크립트를 찾을 수 없습니다.");
            }
            OnShoot.Invoke(shootDir.normalized, prefabIndex, projectile);
        }

        PlayShootAnimation();
    }

    /// <summary>
    /// 플레이어가 스턴 능력을 가지고 있는지 확인합니다.
    /// </summary>
    /// <returns>스턴 가능 여부</returns>
    public bool CanStun()
    {
        StunAbility stunAbility = abilityManager.GetAbilityOfType<StunAbility>();

        return stunAbility != null;
    }

    /// <summary>
    /// 플레이어가 바라보는 방향을 가져옵니다.
    /// </summary>
    /// <returns>바라보는 방향 벡터</returns>
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

    /// <summary>
    /// 통화 정보만 저장합니다.
    /// </summary>
    private void SaveCurrencyOnly()
    {
        PlayerCurrencyToJson data = new PlayerCurrencyToJson
        {
            currentCurrency = stat.currentCurrency
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(saveFilePath, json);
    }

    /// <summary>
    /// 플레이어 데이터를 저장합니다.
    /// </summary>
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
            currentShootCooldown = stat.currentAttackSpeed,
            currentDefense = stat.currentDefense,
            currentExperience = stat.currentExperience,
            currentCurrency = stat.currentCurrency,
            currentLevel = stat.currentLevel
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(saveFilePath, json);
    }

    /// <summary>
    /// 플레이어 데이터를 로드합니다.
    /// </summary>
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
            stat.currentAttackSpeed = data.currentShootCooldown;
            stat.currentDefense = data.currentDefense;
            stat.currentExperience = data.currentExperience;
            stat.currentCurrency = data.currentCurrency;
            stat.currentLevel = data.currentLevel;
        }
        else
        {
            Debug.LogWarning("저장 파일을 찾을 수 없어 기본 플레이어 데이터 사용.");
        }
    }
    /// <summary>
    /// 플레이어 컨트롤을 활성화합니다.
    /// </summary>
    public void EnableControls()
    {
        playerInput.Player.Enable();
    }

    /// <summary>
    /// 플레이어 컨트롤을 비활성화합니다.
    /// </summary>
    public void DisableControls()
    {
        playerInput.Player.Disable();
    }
    private void OnApplicationQuit()
    {
        SavePlayerData();
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
}
