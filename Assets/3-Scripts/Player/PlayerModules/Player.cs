using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Spine.Unity;
using Cinemachine;
using System.Collections.Generic;
using AK.Wwise;
using UnityEditor.Rendering;

public class Player : MonoBehaviour
{
    // WWISE���� ������ ���� �Ķ���͸� �����ϱ� ���� ����
    public RTPC playerHealthRTPC;
    public RTPC playerHealthLow;
    [Tooltip("�÷��̾��� ���� ������")]
    public PlayerData stat;

    [Tooltip("���� �ó��� �ɷ�")]
    public SynergyAbility currentSynergyAbility;

    [Tooltip("������Ʈ Ǯ")]
    public ObjectPool objectPool;

    [Tooltip("�÷��̾��� �ɷ� �Ŵ���")]
    public PlayerAbilityManager abilityManager;

    [Tooltip("�� �ɷ�")]
    public Barrier barrierAbility;

    private List<UIShaker> healthBarShakers = new List<UIShaker>();

    [Header("Spine �ִϸ��̼�")]
    [Tooltip("Skeleton �ִϸ��̼�")]
    public SkeletonAnimation skeletonAnimation;

    private Spine.Skeleton skeleton;
    private Spine.AnimationState spineAnimationState;

    [SpineSkin, Tooltip("���� ��Ų �̸�")]
    public string frontSkinName;
    [SpineSkin, Tooltip("���� ��Ų �̸�")]
    public string backSkinName;
    [SpineSkin, Tooltip("���� ��Ų �̸�")]
    public string sideSkinName;
    [SpineSkin, Tooltip("���� �� ��Ų �̸�")]
    public string sideFrontSkinName;
    [SpineSkin, Tooltip("���� �� ��Ų �̸�")]
    public string sideBackSkinName;

    [SpineAnimation, Tooltip("�ڷ� �ȱ� �ִϸ��̼� �̸�")]
    public string walkBackAnimName;
    [SpineAnimation, Tooltip("�ڷ� �ȱ� ���� �ִϸ��̼� �̸�")]
    public string walkBackStopAnimName;
    [SpineAnimation, Tooltip("������ �ȱ� �ִϸ��̼� �̸�")]
    public string walkStraightAnimName;
    [SpineAnimation, Tooltip("������ �ȱ� ���� �ִϸ��̼� �̸�")]
    public string walkStraightStopAnimName;

    [SpineAnimation, Tooltip("��ü �ڷ� �ȱ� �ִϸ��̼� �̸�")]
    public string upperWalkBackAnimName;
    [SpineAnimation, Tooltip("��ü �ڷ� �ȱ� ���� �ִϸ��̼� �̸�")]
    public string upperWalkBackStopAnimName;
    [SpineAnimation, Tooltip("��ü ������ �ȱ� �ִϸ��̼� �̸�")]
    public string upperWalkStraightAnimName;
    [SpineAnimation, Tooltip("��ü ������ �ȱ� ���� �ִϸ��̼� �̸�")]
    public string upperWalkStraightStopAnimName;

    [SpineAnimation, Tooltip("�� ���� �ִϸ��̼� �̸�")]
    public string attackFrontAnimName;
    [SpineAnimation, Tooltip("�� ���� �ִϸ��̼� �̸�")]
    public string attackBackAnimName;
    [SpineAnimation, Tooltip("���� ���� �ִϸ��̼� �̸�")]
    public string attackSideAnimName;
    [SpineAnimation, Tooltip("���� �� ���� �ִϸ��̼� �̸�")]
    public string attackSideFrontAnimName;
    [SpineAnimation, Tooltip("���� �� ���� �ִϸ��̼� �̸�")]
    public string attackSideBackAnimName;

    [SpineAnimation, Tooltip("��ü ��� �ִϸ��̼� �̸�")]
    public string upperIdleAnimName;
    [SpineAnimation, Tooltip("��ü ��� �ִϸ��̼� �̸�")]
    public string lowerIdleAnimName;

    [Header("���� ���� �ִϸ��̼�")]
    [SpineAnimation, Tooltip("���� ���� �ִϸ��̼� 1")]
    public string gameStartAnim1;
    [SpineAnimation, Tooltip("���� ���� �ִϸ��̼� 2")]
    public string gameStartAnim2;
    [SpineAnimation, Tooltip("���� ���� �ִϸ��̼� 3")]
    public string gameStartAnim3;
    private bool isGameStartAnimationPlaying = false;

    [Header("�� ����Ʈ")]
    [SerializeField, Tooltip("�� ����Ʈ ������")]
    private GameObject healEffectPrefab;
    [SerializeField, Tooltip("�ǰ� ����Ʈ ��Ʈ�ѷ�")]
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
    [Tooltip("�߻� ��ġ")]
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

    // �߰��� UnityEvent��
    public UnityEvent OnPlayerInitialized;
    public UnityEvent OnUIUpdated;
    public UnityEvent OnInvincibilityStarted;
    public UnityEvent OnInvincibilityEnded;
    public UnityEvent OnPlayerStartShoot;
    public UnityEvent OnPlayerStopShoot;
    public UnityEvent OnPlayerDataLoaded;
    public UnityEvent OnPlayerDataSaved;
    public UnityEvent OnPlayerDeathComplete;

    private string saveFilePath;
    private bool isHealthLow = false;

    public Vector2 PlayerPosition => transform.position;

    [Header("ī�޶� ��鸲")]
    [Tooltip("Cinemachine ���޽� �ҽ�")]
    public CinemachineImpulseSource impulseSource;

    [Header("���� ����")]
    [SerializeField, Tooltip("�߻��� �Ѿ��� ����")]
    private int numberOfProjectiles = 5;
    [SerializeField, Tooltip("������Ʈ���� ������ �� ���� (�� ����)")]
    private float spreadAngle = 30f;
    [SerializeField, Tooltip("������Ʈ�� �ӵ��� �ּҰ�")]
    private float minProjectileSpeed = 8f;
    [SerializeField, Tooltip("������Ʈ�� �ӵ��� �ִ밪")]
    private float maxProjectileSpeed = 12f;
    [SerializeField, Tooltip("������Ʈ�� ���� �ð��� �ּҰ�")]
    private float minLifetime = 1f;
    [SerializeField, Tooltip("������Ʈ�� ���� �ð��� �ִ밪")]
    private float maxLifetime = 2f;

    private float nextShootTime;

    private bool isOverheated = false; // ���� ���¸� ��Ÿ���� ����
    public bool IsOverheated
    {
        get { return isOverheated; }
        set { isOverheated = value; }
    }

    /// <summary>
    /// �÷��̾��� ������Ʈ�� �ʱ�ȭ�մϴ�.
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

        // �߰��� UnityEvent �ʱ�ȭ
        OnPlayerInitialized ??= new UnityEvent();
        OnUIUpdated ??= new UnityEvent();
        OnInvincibilityStarted ??= new UnityEvent();
        OnInvincibilityEnded ??= new UnityEvent();
        OnPlayerStartShoot ??= new UnityEvent();
        OnPlayerStopShoot ??= new UnityEvent();
        OnPlayerDataLoaded ??= new UnityEvent();
        OnPlayerDataSaved ??= new UnityEvent();

        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");

        skeleton = skeletonAnimation.Skeleton;
        spineAnimationState = skeletonAnimation.AnimationState;

        skeleton.SetSkin(sideSkinName);
        skeleton.SetSlotsToSetupPose();

        spineAnimationState.Complete += OnSpineAnimationComplete;
    }

    /// <summary>
    /// �÷��̾ �ʱ�ȭ�ϰ� UI�� ������Ʈ�մϴ�.
    /// </summary>
    private void Start()
    {
        healthBarShakers.AddRange(FindObjectsOfType<UIShaker>());
        InitializePlayer();
        UpdateUI();
        PlayRandomGameStartAnimation();
    }

    /// <summary>
    /// ������ ���� ���� �ִϸ��̼��� ����մϴ�.
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
    /// ���� ���� �ִϸ��̼� �Ϸ� �� ȣ��Ǵ� �ݹ��Դϴ�.
    /// </summary>
    /// <param name="trackEntry">Ʈ�� ��Ʈ��</param>
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
    /// ������Ʈ�� Ȱ��ȭ�� �� ȣ��˴ϴ�.
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
    /// ������Ʈ�� ��Ȱ��ȭ�� �� ȣ��˴ϴ�.
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
    /// �� �����Ӹ��� ȣ��Ǿ� �Է� �� �ִϸ��̼��� ������Ʈ�մϴ�.
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
    /// ���� ������Ʈ�� ó���մϴ�.
    /// </summary>
    private void FixedUpdate()
    {
        Vector2 movement = moveInput.normalized * stat.buffedPlayerSpeed * Time.fixedDeltaTime;
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
    /// �÷��̾��� �ִϸ��̼��� ������Ʈ�մϴ�.
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
    /// ��ü�� Idle �ִϸ��̼� �̸��� �����ɴϴ�.
    /// </summary>
    /// <param name="direction">���� ����</param>
    /// <returns>��ü Idle �ִϸ��̼� �̸�</returns>
    private string GetUpperIdleAnimationName(Direction8 direction)
    {
        return upperIdleAnimName;
    }

    /// <summary>
    /// ��ü�� Idle �ִϸ��̼� �̸��� �����ɴϴ�.
    /// </summary>
    /// <param name="direction">���� ����</param>
    /// <returns>��ü Idle �ִϸ��̼� �̸�</returns>
    private string GetLowerIdleAnimationName(Direction8 direction)
    {
        return lowerIdleAnimName;
    }

    /// <summary>
    /// ���� �ִϸ��̼��� ����մϴ�.
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
    /// Spine �ִϸ��̼��� �Ϸ�Ǿ��� �� ȣ��Ǵ� �ݹ��Դϴ�.
    /// </summary>
    /// <param name="trackEntry">Ʈ�� ��Ʈ��</param>
    private void OnSpineAnimationComplete(Spine.TrackEntry trackEntry)
    {
        int upperBodyTrackIndex = 1;

        if (trackEntry.TrackIndex == upperBodyTrackIndex && !isShooting)
        {
            UpdateAnimation();
        }
    }

    /// <summary>
    /// �־��� ������ 8���� ���������� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="direction">���� ����</param>
    /// <returns>8���� ������ ��</returns>
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
    /// ���� ���⿡ ���� ��Ų �̸��� �����ɴϴ�.
    /// </summary>
    /// <param name="direction">���� ����</param>
    /// <returns>��Ų �̸�</returns>
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
    /// ���� ���⿡ ���� �¿� ������ �ʿ����� ���θ� �����մϴ�.
    /// </summary>
    /// <param name="direction">���� ����</param>
    /// <returns>�¿� ���� �ʿ� ����</returns>
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
    /// ���� ���⿡ ���� ���� �ִϸ��̼� �̸��� �����ɴϴ�.
    /// </summary>
    /// <param name="direction">���� ����</param>
    /// <returns>���� �ִϸ��̼� �̸�</returns>
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
    /// ���� ���� �� ������ ���¿� ���� ��ü �ִϸ��̼� �̸��� �����ɴϴ�.
    /// </summary>
    /// <param name="direction">���� ����</param>
    /// <param name="isMoving">������ ����</param>
    /// <returns>��ü �ִϸ��̼� �̸�</returns>
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
    /// ���� ���� �� ������ ���¿� ���� ��ü �ִϸ��̼� �̸��� �����ɴϴ�.
    /// </summary>
    /// <param name="direction">���� ����</param>
    /// <param name="isMoving">������ ����</param>
    /// <returns>��ü �ִϸ��̼� �̸�</returns>
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
    /// ���� ���⿡ ���� ��ü �ȱ� �ִϸ��̼� �̸��� �����ɴϴ�.
    /// </summary>
    /// <param name="direction">���� ����</param>
    /// <returns>��ü �ȱ� �ִϸ��̼� �̸�</returns>
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
    /// ���� ���⿡ ���� ��ü �ȱ� �ִϸ��̼� �̸��� �����ɴϴ�.
    /// </summary>
    /// <param name="direction">���� ����</param>
    /// <returns>��ü �ȱ� �ִϸ��̼� �̸�</returns>
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
    /// ���ο� �ó��� �ɷ��� ȹ���մϴ�.
    /// </summary>
    /// <param name="newAbility">ȹ���� �ó��� �ɷ�</param>
    public void AcquireSynergyAbility(SynergyAbility newAbility)
    {
        currentSynergyAbility = newAbility;
    }

    /// <summary>
    /// ���� ���¸� �����մϴ�.
    /// </summary>
    /// <param name="value">���� ���� ����</param>
    public void SetInvincibility(bool value)
    {
        isInvincible = value;
    }

    private void UpdatePlayerHealthRTPC()
    {
        // ü�� ����� ���
        float healthPercentage = (float)stat.currentHP / stat.currentMaxHP * 100f;

        // WWISE�� ���� �Ķ���� ������Ʈ
        playerHealthRTPC.SetGlobalValue(healthPercentage);

        // ü���� 30% �������� Ȯ��
        if (healthPercentage <= 30f && !isHealthLow)
        {
            playerHealthLow.SetGlobalValue(1f); // ��ü�� ���� Ȱ��ȭ
            isHealthLow = true;
            Debug.Log("Player Health is Low!");
        }
        else if (healthPercentage > 30f && isHealthLow)
        {
            playerHealthLow.SetGlobalValue(0f); // ��ü�� ���� ��Ȱ��ȭ
            isHealthLow = false;
            Debug.Log("Player Health is Normal.");
        }
    }

    /// <summary>
    /// �������� �ް� ü���� ���ҽ�ŵ�ϴ�.
    /// </summary>
    /// <param name="damage">���� ������ ��</param>
    public void TakeDamage(int damage)
    {
        // "Barrier" �±׸� ���� ��� ������Ʈ�� ã���ϴ�.
        GameObject[] barriers = GameObject.FindGameObjectsWithTag("Barrier");

        if (barriers.Length > 0)
        {
            foreach (GameObject barrier in barriers)
            {
                // Shield ������Ʈ�� �����ɴϴ�.
                Shield shieldComponent = barrier.GetComponent<Shield>();
                if (shieldComponent != null)
                {
                    // Shield�� BreakShield �޼��� ȣ��
                    shieldComponent.BreakShield();
                }
                else
                {
                    Debug.LogWarning($"Barrier �±׸� ���� ������Ʈ '{barrier.name}'�� Shield ������Ʈ�� �����ϴ�.");
                }
            }

            // Barrier�� ���������Ƿ� �������� �����ϰ� �޼��带 �����մϴ�.
            return;
        }

        // Barrier�� Ȱ��ȭ�Ǿ� ���� ������ ������ ������ ������ �����մϴ�.
        if (!isInvincible)
        {
            stat.TakeDamage(damage);
            UpdatePlayerHealthRTPC();
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
    /// ���� ���¸� ���� �ð� �����ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <returns>�ڷ�ƾ</returns>
    private IEnumerator InvincibilityCoroutine()
    {
        OnInvincibilityStarted.Invoke(); // ���� ���� ���� �̺�Ʈ ȣ��

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

        OnInvincibilityEnded.Invoke(); // ���� ���� ���� �̺�Ʈ ȣ��
    }

    /// <summary>
    /// �÷��̾ ������� �� ȣ��˴ϴ�.
    /// </summary>
    private void Die()
    {
        PlayManager.I.StopAllSounds();
        OnPlayerDeath.Invoke();
        PlayManager.I.isPlayerDie();
        PlayerDataManager.Instance.ResetPlayerData();
        StartCoroutine(InvokeDeathCompleteEventAfterDelay(1.5f));
    }

    /// <summary>
    /// ������ �ð� �Ŀ� OnPlayerDeathComplete �̺�Ʈ�� ȣ���ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <param name="delaySeconds">���� �ð�(��)</param>
    /// <returns></returns>
    private IEnumerator InvokeDeathCompleteEventAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        OnPlayerDeathComplete?.Invoke();
    }
    /// <summary>
    /// ���͸� óġ���� �� ȣ��˴ϴ�.
    /// </summary>
    public void KillMonster()
    {
        OnMonsterKilled.Invoke();
    }

    /// <summary>
    /// �÷��̾��� ü���� ȸ���մϴ�.
    /// </summary>
    /// <param name="amount">ȸ���� ü�� ��</param>
    public void Heal(int amount)
    {
        stat.Heal(amount);
        OnHeal.Invoke();
        UpdatePlayerHealthRTPC();
        UpdateUI();

        if (healEffectPrefab != null)
        {
            GameObject healEffect = Instantiate(healEffectPrefab, transform);
            healEffect.transform.localRotation = Quaternion.identity;
            Destroy(healEffect, 1f);
        }
        else
        {
            Debug.LogWarning("healEffectPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// ���� ü���� �����ɴϴ�.
    /// </summary>
    /// <returns>���� ü��</returns>
    public int GetCurrentHP()
    {
        return stat.currentHP;
    }

    /// <summary>
    /// ����ġ�� ȹ���ϰ� ������ ���θ� �Ǵ��մϴ�.
    /// </summary>
    /// <param name="amount">ȹ���� ����ġ ��</param>
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
    /// �÷��̾��� UI�� ������Ʈ�մϴ�.
    /// </summary>
    public void UpdateUI()
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
            Debug.LogWarning("PlayerUIManager�� ã�� �� �����ϴ�.");
        }

        OnUIUpdated.Invoke(); // UI ������Ʈ �̺�Ʈ ȣ��
    }

    /// <summary>
    /// �÷��̾��� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    private void InitializePlayer()
    {
        stat.InitializeStats();
        UpdatePlayerHealthRTPC();
        OnPlayerInitialized.Invoke(); // �÷��̾� �ʱ�ȭ �̺�Ʈ ȣ��
    }

    /// <summary>
    /// �̵� �Է��� �߻����� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="context">�Է� ���ؽ�Ʈ</param>
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
    /// �̵� �Է��� ��ҵǾ��� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="context">�Է� ���ؽ�Ʈ</param>
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
    /// ���� �Է��� �߻����� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="context">�Է� ���ؽ�Ʈ</param>
    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        isShooting = true;
        OnPlayerStartShoot.Invoke(); // ���� ���� �̺�Ʈ ȣ��
        UpdateShootDirection();
    }

    /// <summary>
    /// ���� ������ ������Ʈ�մϴ�.
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
    /// ���� �Է��� ��ҵǾ��� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="context">�Է� ���ؽ�Ʈ</param>
    private void OnShootCanceledInputAction(InputAction.CallbackContext context)
    {
        isShooting = false;
        OnShootCanceled.Invoke();
        OnPlayerStopShoot.Invoke(); // ���� ���� �̺�Ʈ ȣ��
        UpdateAnimation();
    }
    /// <summary>
    /// �־��� �������� �Ѿ��� �߻��մϴ�.
    /// </summary>
    /// <param name="direction">�߻��� ����</param>
    /// <param name="prefabIndex">������ �ε���</param>
    private void Shoot(Vector2 direction, int prefabIndex)
    {
        if (objectPool == null)
        {
            Debug.LogError("objectPool�� �Ҵ���� �ʾҽ��ϴ�.");
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
                Debug.LogError("Projectile�� �ν��Ͻ�ȭ�� �� �����ϴ�.");
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

                projScript.Initialize(stat, this, false, damageMultiplier, stat.buffedPlayerDamage, randomSpeed, randomLifetime);
                projScript.SetDirection(shootDir.normalized);
            }
            else
            {
                Debug.LogError("Projectile ��ũ��Ʈ�� ã�� �� �����ϴ�.");
            }
            OnShoot.Invoke(shootDir.normalized, prefabIndex, projectile);
        }

        PlayShootAnimation();
    }

    /// <summary>
    /// �÷��̾ ���� �ɷ��� ������ �ִ��� Ȯ���մϴ�.
    /// </summary>
    /// <returns>���� ���� ����</returns>
    public bool CanStun()
    {
        StunAbility stunAbility = abilityManager.GetAbilityOfType<StunAbility>();

        return stunAbility != null;
    }

    /// <summary>
    /// �÷��̾ �ٶ󺸴� ������ �����ɴϴ�.
    /// </summary>
    /// <returns>�ٶ󺸴� ���� ����</returns>
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
    /// �÷��̾� ��Ʈ���� Ȱ��ȭ�մϴ�.
    /// </summary>
    public void EnableControls()
    {
        playerInput.Player.Enable();
    }

    /// <summary>
    /// �÷��̾� ��Ʈ���� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    public void DisableControls()
    {
        playerInput.Player.Disable();
    }
}
