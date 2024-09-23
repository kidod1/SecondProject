using System.Collections;
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

    protected virtual void Start()
    {
        currentHP = monsterBaseStat.maxHP;
        meshRenderer = GetComponent<MeshRenderer>();
        player = FindObjectOfType<Player>();

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

    public void Stun()
    {
        if (isStunned) return;
        isStunned = true;

        // 몬스터가 기절했다는 메시지 출력
        Debug.Log($"몬스터 {gameObject.name} 이(가) 기절했습니다! 기절 지속 시간: {stunDuration}초");

        // 기절 상태가 끝나는 Coroutine 시작
        StartCoroutine(StunCoroutine());
    }

    private IEnumerator StunCoroutine()
    {
        yield return new WaitForSecondsRealtime(stunDuration);
        isStunned = false;

        // 기절이 끝났다는 메시지 출력
        Debug.Log($"몬스터 {gameObject.name} 의 기절 상태가 종료되었습니다.");
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
        transform.position += direction * monsterBaseStat.monsterSpeed * Time.unscaledDeltaTime;
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;

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

    public abstract void Attack();
    public int GetCurrentHP()
    {
        return currentHP;
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("몬스터 사망");

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
            // ...
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
            yield return new WaitForSecondsRealtime(blinkInterval);
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
            yield return new WaitForSecondsRealtime(damageInterval);
        }
    }

    private IEnumerator BlinkAreaSprite()
    {
        if (areaSpriteRenderer == null) yield break;

        float blinkDuration = 0.2f;
        areaSpriteRenderer.enabled = false;
        yield return new WaitForSecondsRealtime(blinkDuration);
        areaSpriteRenderer.enabled = true;
    }
}
