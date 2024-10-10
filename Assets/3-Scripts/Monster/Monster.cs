using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public interface IMonsterState
{
    void EnterState();
    void UpdateState();
    void ExitState();
}

public abstract class Monster : MonoBehaviour
{
    public MonsterData monsterBaseStat;
    protected Rigidbody2D rb;
    protected int currentHP;
    protected MeshRenderer meshRenderer;
    protected bool isInvincible = false;
    protected bool isDead = false;
    public bool IsDead => isDead;

    protected internal Player player;

    [SerializeField]
    private float invincibilityDuration = 0.5f;
    [SerializeField]
    private float blinkInterval = 0.1f;
    public GameObject monsterDeathEffectPrefab;
    [SerializeField]
    private float deathEffectDuration = 0.2f;

    public bool isElite = false;
    public bool isInCooldown = false;
    public bool isInfected = false;


    // 새로운 필드 추가: Betting 능력에 의해 한 번만 발동하도록 관리
    public bool HasBeenHitByBetting { get; set; } = false;

    public IMonsterState currentState;
    public IMonsterState idleState;
    public IMonsterState chaseState;
    public IMonsterState attackState;
    public IMonsterState cooldownState;

    public GameObject damageArea;
    private Collider2D areaCollider;
    private SpriteRenderer areaSpriteRenderer;

    [SerializeField]
    private int areaDamage = 10;
    [SerializeField]
    private float damageInterval = 1f;

    private bool isStunned = false;
    private float stunDuration = 2f;
    private float stunEndTime;

    protected virtual void Start()
    {
        currentHP = monsterBaseStat.maxHP;
        meshRenderer = GetComponent<MeshRenderer>();
        player = FindObjectOfType<Player>();
        rb = GetComponent<Rigidbody2D>();

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
    }

    private void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
            if (player != null)
            {
                InitializeStates();
            }
        }
        if (isStunned) return;
        currentState?.UpdateState();
    }

    protected abstract void InitializeStates();

    public void Stun(float duration)
    {
        if (!isStunned)
        {
            isStunned = true;
            stunEndTime = Time.time + duration;
            // 스턴 효과 활성화 (애니메이션, 상태 변경 등)
            Debug.Log($"{name} has been stunned for {duration} seconds.");
            StartCoroutine(StunCoroutine(duration));
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isStunned = false;
        // 스턴 효과 비활성화
        Debug.Log($"{name} is no longer stunned.");
    }

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

    public bool IsPlayerInRange(float range)
    {
        if (player == null)
        {
            Debug.LogWarning("Player가 설정되지 않았습니다. IsPlayerInRange 호출이 무시됩니다.");
            return false;
        }

        return Vector2.Distance(transform.position, player.transform.position) <= range;
    }

    public void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * monsterBaseStat.monsterSpeed * Time.deltaTime;
    }

    public virtual void TakeDamage(int damage, Vector3 damageSourcePosition)
    {
        if (isDead) return;

        currentHP -= damage;

        ShowDamageText(damage);

        ApplyKnockback(damageSourcePosition);

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }
    private void ApplyKnockback(Vector3 damageSourcePosition)
    {
        if (rb != null)
        {
            // 노크백 방향 계산 (몬스터 위치 - 공격자 위치)
            Vector2 knockbackDirection = (transform.position - damageSourcePosition).normalized;

            float knockbackForce = 2f;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        }
        else
        {
            Debug.LogWarning("Rigidbody2D가 몬스터에 없습니다.");
        }
    }

    private void ShowDamageText(int damage)
    {
        // 데미지 텍스트 프리팹 로드
        GameObject damageTextPrefab = Resources.Load<GameObject>("DamageTextPrefab");
        if (damageTextPrefab == null)
        {
            Debug.LogError("DamageTextPrefab을 Resources 폴더에서 찾을 수 없습니다.");
            return;
        }

        // 태그로 캔버스를 찾습니다.
        GameObject canvasObject = GameObject.FindGameObjectWithTag("DamageCanvas");
        if (canvasObject == null)
        {
            Debug.LogError("'DamageCanvas' 태그를 가진 캔버스를 찾을 수 없습니다.");
            return;
        }

        Canvas screenCanvas = canvasObject.GetComponent<Canvas>();
        if (screenCanvas == null)
        {
            Debug.LogError("'DamageCanvas' 오브젝트에서 Canvas 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        // 캔버스의 Render Camera 설정 확인 및 가져오기
        Camera uiCamera = screenCanvas.worldCamera;
        if (uiCamera == null)
        {
            uiCamera = Camera.main;
        }

        if (uiCamera == null)
        {
            Debug.LogError("UI 카메라를 찾을 수 없습니다.");
            return;
        }

        // 몬스터의 머리 위치 계산 (월드 좌표)
        float monsterHeightOffset = 1.0f; // 몬스터 머리 위로 얼마나 띄울지 조정 가능한 값
        Vector3 headPosition = transform.position + new Vector3(0, monsterHeightOffset, 0);

        // 머리 위치를 스크린 좌표로 변환
        Vector3 screenPosition = uiCamera.WorldToScreenPoint(headPosition);

        // 스크린 좌표를 캔버스 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            screenCanvas.transform as RectTransform,
            screenPosition,
            uiCamera,
            out Vector2 localPoint);

        // 데미지 텍스트 인스턴스 생성 및 부모 설정
        GameObject damageTextInstance = Instantiate(damageTextPrefab, screenCanvas.transform);

        // RectTransform의 로컬 좌표 설정
        RectTransform rectTransform = damageTextInstance.GetComponent<RectTransform>();
        rectTransform.localPosition = localPoint;

        // 데미지 텍스트 설정
        DamageText damageText = damageTextInstance.GetComponent<DamageText>();
        if (damageText != null)
        {
            // 폰트와 글자 크기 설정 (원하는 폰트로 변경 가능)
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Maplestory Light SDF");
            if (font == null)
            {
                Debug.LogWarning("폰트 에셋을 찾을 수 없습니다. 기본 폰트를 사용합니다.");
            }

            int fontSize = 22;
            Color color = Color.white;

            damageText.SetText(damage.ToString(), font, fontSize, color);
        }
        else
        {
            Debug.LogError("DamageText 컴포넌트를 찾을 수 없습니다.");
        }
    }


    public abstract void Attack();
    public int GetCurrentHP()
    {
        return currentHP;
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;

        if (player != null)
        {
            player.KillMonster();

            // 능력 매니저에서 모든 능력을 가져옵니다.
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

        DropExperienceItem();

        Destroy(gameObject);
    }

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


    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        for (float i = 0; i < invincibilityDuration; i += blinkInterval)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = !meshRenderer.enabled;
            }
            yield return new WaitForSeconds(blinkInterval);
        }

        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }

        isInvincible = false;
    }

    private void DropExperienceItem()
    {
        if (player != null && monsterBaseStat != null)
        {
            int experiencePointsToDrop = monsterBaseStat.experiencePoints;
            if (Random.value < monsterBaseStat.highExperienceDropChance)
            {
                experiencePointsToDrop = monsterBaseStat.highExperiencePoints;
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
                GameObject expItem = Instantiate(experienceItemPrefab, transform.position, Quaternion.identity);
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

    virtual protected void OnCollisionEnter2D(Collision2D collision)
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

    private IEnumerator BlinkAreaSprite()
    {
        if (areaSpriteRenderer == null) yield break;

        float blinkDuration = 0.2f;
        areaSpriteRenderer.enabled = false;
        yield return new WaitForSeconds(blinkDuration);
        areaSpriteRenderer.enabled = true;
    }
}
