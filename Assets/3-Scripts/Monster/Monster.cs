using Spine.Unity;
using System.Collections;
using TMPro;
using UnityEngine;

public interface IMonsterState
{
    void EnterState();
    void UpdateState();
    void ExitState();
}

public abstract class Monster : MonoBehaviour
{
    [Header("몬스터 설정")]
    [Tooltip("몬스터의 기본 스탯 데이터를 저장합니다.")]
    public MonsterData monsterBaseStat;

    protected Rigidbody2D rb;
    protected int currentHP;
    protected MeshRenderer meshRenderer;
    private Color originalColor;
    protected bool isDead = false;
    public bool IsDead => isDead;

    protected internal Player player;

    [Header("넉백 설정")]
    [SerializeField]
    [Tooltip("넉백의 최소 힘을 설정합니다.")]
    private float minKnockbackForce = 5f;

    [SerializeField]
    [Tooltip("넉백의 최대 힘을 설정합니다.")]
    private float maxKnockbackForce = 15f;

    [SerializeField]
    [Tooltip("깜빡임 간격을 설정합니다.")]
    private float blinkInterval = 0.1f;

    [SerializeField]
    [Tooltip("깜빡임 지속 시간을 설정합니다.")]
    private float blinkDuration = 0.5f;

    [Tooltip("몬스터 사망 시 생성될 이펙트 프리팹입니다.")]
    public GameObject monsterDeathEffectPrefab;

    [SerializeField]
    [Tooltip("사망 이펙트의 지속 시간을 설정합니다.")]
    private float deathEffectDuration = 0.2f;

    [SerializeField]
    [Tooltip("능력에 의한 사망 이펙트의 지속 시간을 설정합니다.")]
    private float abilityDeathEffectDuration = 1f;

    [SerializeField]
    [Tooltip("즉사 이펙트 프리팹입니다.")]
    private GameObject monsterInstantKillEffectPrefab;

    [Tooltip("몬스터가 엘리트인지 여부를 나타냅니다.")]
    public bool isElite = false;

    [Tooltip("쿨다운 상태인지 여부를 나타냅니다.")]
    public bool isInCooldown = false;

    [Tooltip("감염 상태인지 여부를 나타냅니다.")]
    public bool isInfected = false;

    private string damageTextPrefabPath = "DamageTextPrefab";
    protected SkeletonAnimation skeletonAnimations;

    [Tooltip("베팅 능력에 의해 한 번만 발동하도록 관리합니다.")]
    public bool HasBeenHitByBetting { get; set; } = false;

    public IMonsterState currentState;
    public IMonsterState idleState;
    public IMonsterState chaseState;
    public IMonsterState attackState;
    public IMonsterState cooldownState;

    [Tooltip("데미지 영역 오브젝트입니다.")]
    public GameObject damageArea;

    private Collider2D areaCollider;
    private SpriteRenderer areaSpriteRenderer;

    [SerializeField]
    [Tooltip("영역 데미지를 설정합니다.")]
    private int areaDamage = 10;

    [SerializeField]
    [Tooltip("데미지 간격을 설정합니다.")]
    private float damageInterval = 1f;
    protected bool isPlayerDead = false;
    private bool isStunned = false;
    public bool IsStunned => isStunned;
    private float stunEndTime;

    private bool originalIsKinematic;

    [Header("스턴 효과")]
    [Tooltip("스턴 상태일 때 표시할 이펙트 프리팹")]
    public GameObject stunEffectPrefab;
    private GameObject currentStunEffect;

    [Tooltip("즉사 여부를 추적합니다.")]
    public bool isInstantKilled = false;

    private static Transform experienceItemsParent;

    /// <summary>
    /// 몬스터의 컴포넌트를 초기화하고 초기 상태를 설정합니다.
    /// </summary>
    protected virtual void Start()
    {
        skeletonAnimations = GetComponent<SkeletonAnimation>();
        currentHP = monsterBaseStat.maxHP;
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            meshRenderer.material = new Material(meshRenderer.material);
            originalColor = meshRenderer.material.color;
        }
        else
        {
            Debug.LogWarning("MeshRenderer를 찾을 수 없습니다.");
        }

        rb = GetComponent<Rigidbody2D>();

        player = FindObjectOfType<Player>();
        InitializeExperienceItemsParent();
        if (player == null)
        {
            Debug.LogError("플레이어 오브젝트를 찾을 수 없습니다. Update 메서드에서 재시도합니다.");
        }
        else
        {
            InitializeStates();
        }

        if (isElite)
        {
            if (damageArea != null)
            {
                areaCollider = damageArea.GetComponent<Collider2D>();
                areaSpriteRenderer = damageArea.GetComponent<SpriteRenderer>();

                if (areaCollider == null)
                {
                    Debug.LogError("Damage area collider를 찾을 수 없습니다. Collider2D가 존재하는지 확인하세요.");
                }

                if (areaSpriteRenderer == null)
                {
                    Debug.LogError("Damage area의 SpriteRenderer를 찾을 수 없습니다. SpriteRenderer가 존재하는지 확인하세요.");
                }

                if (areaCollider != null && areaSpriteRenderer != null)
                {
                    StartCoroutine(AreaDamageCoroutine());
                }
            }
            else
            {
                Debug.LogWarning("damageArea가 설정되지 않았습니다. 장판 데미지 시스템을 비활성화합니다.");
            }
        }
        player.OnPlayerDeath.AddListener(OnPlayerDeathHandler);
    }

    /// <summary>
    /// 매 프레임마다 몬스터를 업데이트하고 플레이어 참조를 확인합니다.
    /// </summary>
    protected virtual void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
            if (player != null)
            {
                InitializeStates();
            }
        }
        if (isStunned || isPlayerDead) return;
        currentState?.UpdateState();
    }
    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnPlayerDeath.RemoveListener(OnPlayerDeathHandler);
        }
    }


    /// <summary>
    /// 몬스터의 상태를 초기화합니다. 서브클래스에서 구현해야 합니다.
    /// </summary>
    protected abstract void InitializeStates();

    /// <summary>
    /// 지정된 시간 동안 몬스터를 기절시킵니다.
    /// </summary>
    /// <param name="duration">기절 지속 시간(초)</param>
    public virtual void Stun(float duration)
    {
        if (isElite) return;

        if (!isStunned)
        {
            isStunned = true;
            stunEndTime = Time.time + duration;

            if (skeletonAnimations != null)
            {
                skeletonAnimations.timeScale = 0f;
            }

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                originalIsKinematic = rb.isKinematic;
                rb.isKinematic = true;
            }

            ShowStunEffect();

            StartCoroutine(StunCoroutine(duration));
        }
    }

    /// <summary>
    /// 기절 지속 시간과 회복을 처리하는 코루틴입니다.
    /// </summary>
    /// <param name="duration">기절 지속 시간(초)</param>
    private IEnumerator StunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isStunned = false;

        if (skeletonAnimations != null)
        {
            skeletonAnimations.timeScale = 1f;
        }

        if (rb != null)
        {
            rb.isKinematic = originalIsKinematic;
        }

        RemoveStunEffect();
    }

    /// <summary>
    /// 몬스터 위에 기절 이펙트를 표시합니다.
    /// </summary>
    private void ShowStunEffect()
    {
        if (stunEffectPrefab == null)
        {
            Debug.LogWarning("StunEffectPrefab이 할당되지 않았습니다.");
            return;
        }

        Vector3 stunPosition = transform.position + new Vector3(0, 1.0f, 0);
        currentStunEffect = Instantiate(stunEffectPrefab, stunPosition, Quaternion.identity, transform);
    }

    /// <summary>
    /// 몬스터에서 기절 이펙트를 제거합니다.
    /// </summary>
    private void RemoveStunEffect()
    {
        if (currentStunEffect != null)
        {
            Destroy(currentStunEffect);
        }
    }

    /// <summary>
    /// 몬스터를 새로운 상태로 전환합니다.
    /// </summary>
    /// <param name="newState">전환할 새로운 상태</param>
    public void TransitionToState(IMonsterState newState)
    {
        if (isInCooldown && newState != cooldownState)
        {
            return;
        }

        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }

    /// <summary>
    /// 플레이어가 지정된 범위 내에 있는지 확인합니다.
    /// </summary>
    /// <param name="range">확인할 범위</param>
    /// <returns>플레이어가 범위 내에 있으면 true를 반환</returns>
    public bool IsPlayerInRange(float range)
    {
        if (player == null)
        {
            Debug.LogWarning("Player가 설정되지 않았습니다. IsPlayerInRange 호출이 무시됩니다.");
            return false;
        }

        return Vector2.Distance(transform.position, player.transform.position) <= range;
    }

    /// <summary>
    /// 몬스터를 지정된 목표 위치로 이동시킵니다.
    /// </summary>
    /// <param name="target">이동할 목표 위치</param>
    public void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * monsterBaseStat.monsterSpeed * Time.deltaTime;
    }

    /// <summary>
    /// 몬스터에게 데미지를 적용하고 죽음과 넉백을 처리합니다.
    /// </summary>
    /// <param name="damage">적용할 데미지 양</param>
    /// <param name="damageSourcePosition">데미지가 발생한 위치</param>
    /// <param name="deathAbilityKill">데미지가 죽음 능력에 의한 것인지 여부</param>
    public virtual void TakeDamage(int damage, Vector3 damageSourcePosition, bool deathAbilityKill = false)
    {
        if (isDead)
        {
            return;
        }

        ShowDamageText(damage);
        ApplyKnockback(damageSourcePosition);

        if (isInstantKilled)
        {
            currentHP = 0;
        }
        else
        {
            currentHP -= damage;
        }

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
        else
        {
            StartCoroutine(BlinkRedCoroutine());
        }
    }

    /// <summary>
    /// 몬스터 위에 데미지 텍스트를 표시합니다.
    /// </summary>
    /// <param name="damage">표시할 데미지 양</param>
    public void ShowDamageText(int damage)
    {
        StartCoroutine(ShowDamageTextCoroutine(damage));
    }

    /// <summary>
    /// 데미지 텍스트의 표시 및 제거를 처리하는 코루틴입니다.
    /// </summary>
    /// <param name="damage">표시할 데미지 양</param>
    /// <returns>코루틴용 IEnumerator</returns>
    private IEnumerator ShowDamageTextCoroutine(int damage)
    {
        int fontSize;
        float additionalTime = 0f;

        if (damage >= 100)
        {
            fontSize = 40;
            additionalTime = 2f;
        }
        else if (damage >= 50)
        {
            fontSize = 32;
            additionalTime = 0.5f;
        }
        else
        {
            fontSize = 18;
            additionalTime = 0f;
        }

        GameObject damageTextPrefab = Resources.Load<GameObject>(damageTextPrefabPath);
        if (damageTextPrefab == null)
        {
            Debug.LogError("DamageTextPrefab을 Resources 폴더에서 찾을 수 없습니다.");
            yield break;
        }

        GameObject canvasObject = GameObject.FindGameObjectWithTag("DamageCanvas");
        if (canvasObject == null)
        {
            Debug.LogError("'DamageCanvas' 태그를 가진 캔버스를 찾을 수 없습니다.");
            yield break;
        }

        Canvas screenCanvas = canvasObject.GetComponent<Canvas>();
        if (screenCanvas == null)
        {
            Debug.LogError("'DamageCanvas' 오브젝트에서 Canvas 컴포넌트를 찾을 수 없습니다.");
            yield break;
        }

        Camera uiCamera = screenCanvas.worldCamera;
        if (uiCamera == null)
        {
            uiCamera = Camera.main;
        }

        if (uiCamera == null)
        {
            Debug.LogError("UI 카메라를 찾을 수 없습니다.");
            yield break;
        }

        float monsterHeightOffset = 1.0f;
        Vector3 headPosition = transform.position + new Vector3(0, monsterHeightOffset, 0);
        Vector3 screenPosition = uiCamera.WorldToScreenPoint(headPosition);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            screenCanvas.transform as RectTransform,
            screenPosition,
            uiCamera,
            out Vector2 localPoint);

        GameObject damageTextInstance = Instantiate(damageTextPrefab, screenCanvas.transform);
        RectTransform rectTransform = damageTextInstance.GetComponent<RectTransform>();
        rectTransform.localPosition = localPoint;

        DamageText damageText = damageTextInstance.GetComponent<DamageText>();
        if (damageText != null)
        {
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Maplestory Light SDF");
            if (font == null)
            {
                Debug.LogWarning("폰트 에셋을 찾을 수 없습니다. 기본 폰트를 사용합니다.");
            }

            Color color = Color.white;
            damageText.SetText(damage.ToString(), font, fontSize, color);
        }
        else
        {
            Debug.LogError("DamageText 컴포넌트를 찾을 수 없습니다.");
        }

        float totalDuration = 1f + additionalTime;

        yield return new WaitForSeconds(totalDuration);
        Destroy(damageTextInstance);
    }

    /// <summary>
    /// 데미지 원점에서 멀어지도록 몬스터에게 넉백을 적용합니다.
    /// </summary>
    /// <param name="damageSourcePosition">데미지가 발생한 위치</param>
    private void ApplyKnockback(Vector3 damageSourcePosition)
    {
        if (rb != null)
        {
            Vector2 knockbackDirection = (transform.position - damageSourcePosition).normalized;
            float knockbackForce = Random.Range(minKnockbackForce, maxKnockbackForce);
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogWarning("Rigidbody2D가 몬스터에 없습니다.");
        }
    }

    /// <summary>
    /// 몬스터의 공격 동작을 수행합니다. 서브클래스에서 구현해야 합니다.
    /// </summary>
    public abstract void Attack();

    /// <summary>
    /// 몬스터의 현재 체력을 가져옵니다.
    /// </summary>
    /// <returns>현재 체력 값</returns>
    public int GetCurrentHP()
    {
        return currentHP;
    }

    /// <summary>
    /// 몬스터의 죽음을 처리하고 이펙트, 경험치 드롭, 오브젝트 파괴를 수행합니다.
    /// </summary>
    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (!isElite)
        {
            gameManager.AddMonsterKill();
        }
        else
        {
            gameManager.AddMonsterKill();
        }
        if (player != null)
        {
            player.KillMonster();

            PlayerAbilityManager abilityManager = player.GetComponent<PlayerAbilityManager>();
            if (abilityManager != null)
            {
                abilityManager.ActivateAbilitiesOnMonsterDeath(this);
            }
        }

        if (isInfected)
        {
            SpawnParasite();
        }

        if (monsterDeathEffectPrefab != null)
        {
            GameObject deathEffect = Instantiate(monsterDeathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(deathEffect, deathEffectDuration);
        }

        if (isInstantKilled && monsterInstantKillEffectPrefab != null)
        {
            GameObject instantKillEffect = Instantiate(monsterInstantKillEffectPrefab, transform.position, Quaternion.identity);
            Destroy(instantKillEffect, abilityDeathEffectDuration);
        }

        DropExperienceItem();

        Destroy(gameObject);
    }

    private void OnPlayerDeathHandler()
    {
        isPlayerDead = true;

        // 필요한 경우 몬스터의 움직임을 멈추거나 애니메이션을 변경합니다.
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (skeletonAnimations != null)
        {
            skeletonAnimations.timeScale = 0f; // 애니메이션 정지
        }
    }

    /// <summary>
    /// 몬스터가 감염되었을 경우 죽을 때 기생충을 생성합니다.
    /// </summary>
    private void SpawnParasite()
    {
        GameObject parasitePrefab = Resources.Load<GameObject>("ParasitePrefab");
        if (parasitePrefab != null)
        {
            GameObject parasite = Instantiate(parasitePrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("기생 벌레 프리팹을 찾을 수 없습니다: ParasitePrefab");
        }
    }

    /// <summary>
    /// 몬스터가 데미지를 받을 때 붉게 깜빡이는 효과를 처리하는 코루틴입니다.
    /// </summary>
    /// <returns>코루틴용 IEnumerator</returns>
    private IEnumerator BlinkRedCoroutine()
    {
        float elapsed = 0f;
        bool isRed = false;

        while (elapsed < blinkDuration)
        {
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.color = isRed ? originalColor : Color.red;
                isRed = !isRed;
            }

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.color = originalColor;
        }
    }

    /// <summary>
    /// 경험치 아이템 부모 트랜스폼을 초기화합니다.
    /// </summary>
    private void InitializeExperienceItemsParent()
    {
        if (experienceItemsParent == null)
        {
            GameObject parentObj = GameObject.Find("ExperienceItems");
            if (parentObj == null)
            {
                parentObj = new GameObject("ExperienceItems");
            }
            experienceItemsParent = parentObj.transform;
        }
    }

    /// <summary>
    /// 몬스터가 죽을 때 경험치 아이템을 드롭합니다.
    /// </summary>
    protected virtual void DropExperienceItem()
    {
        if (player != null && monsterBaseStat != null)
        {
            int experiencePointsToDrop = monsterBaseStat.experiencePoints;
            if (Random.value < monsterBaseStat.highExperienceDropChance)
            {
                experiencePointsToDrop = monsterBaseStat.highExperiencePoints;
            }

            // 경험치 획득량이 0이면 프리팹을 생성하지 않음
            if (experiencePointsToDrop <= 0)
            {
                return;
            }

            string prefabName = "ExperienceItem";
            if (experiencePointsToDrop > 50 && experiencePointsToDrop <= 100)
            {
                prefabName = "ExperienceItem2";
            }
            else if (experiencePointsToDrop > 100 && experiencePointsToDrop <= 150)
            {
                prefabName = "ExperienceItem3";
            }
            else if (experiencePointsToDrop > 150)
            {
                prefabName = "ExperienceItem4";
            }

            GameObject experienceItemPrefab = Resources.Load<GameObject>(prefabName);
            if (experienceItemPrefab != null)
            {
                if (experienceItemsParent == null)
                {
                    InitializeExperienceItemsParent();
                }

                GameObject expItem = Instantiate(experienceItemPrefab, transform.position, Quaternion.identity, experienceItemsParent);
                ExperienceItem expScript = expItem.GetComponent<ExperienceItem>();
                if (expScript != null)
                {
                    expScript.experienceAmount = experiencePointsToDrop;
                }
            }
            else
            {
                Debug.LogWarning($"리소스를 찾을 수 없습니다: {prefabName}");
            }
        }
    }


    /// <summary>
    /// 몬스터가 다른 콜라이더와 충돌할 때 호출됩니다.
    /// </summary>
    /// <param name="collision">충돌 정보</param>
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(monsterBaseStat.contectDamage);
            }
        }
    }

    /// <summary>
    /// 몬스터가 엘리트일 경우 영역 데미지를 처리하는 코루틴입니다.
    /// </summary>
    /// <returns>코루틴용 IEnumerator</returns>
    private IEnumerator AreaDamageCoroutine()
    {
        while (!isDead)
        {
            if (player != null && areaCollider != null && areaCollider.enabled)
            {
                if (areaCollider.bounds.Contains(player.transform.position))
                {
                    player.TakeDamage(areaDamage);
                    StartCoroutine(BlinkAreaSprite());
                }
            }
            yield return new WaitForSeconds(damageInterval);
        }
    }

    /// <summary>
    /// 플레이어가 영역 데미지를 받을 때 영역 스프라이트를 깜빡이는 코루틴입니다.
    /// </summary>
    /// <returns>코루틴용 IEnumerator</returns>
    private IEnumerator BlinkAreaSprite()
    {
        if (areaSpriteRenderer == null) yield break;

        float blinkDuration = 0.2f;
        areaSpriteRenderer.enabled = false;
        yield return new WaitForSeconds(blinkDuration);
        areaSpriteRenderer.enabled = true;
    }
}
