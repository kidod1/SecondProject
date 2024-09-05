using System.Collections;
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
    protected internal Player player;

    [SerializeField]
    private float invincibilityDuration = 0.5f;
    [SerializeField]
    private float blinkInterval = 0.1f;
    public GameObject monsterDeathEffectPrefab;
    [SerializeField]
    private float deathEffectDuration = 0.2f;

    public bool isInCooldown = false;

    public IMonsterState currentState;
    public IMonsterState idleState;
    public IMonsterState chaseState;
    public IMonsterState attackState;
    public IMonsterState cooldownState;

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

        currentState?.UpdateState();
    }

    protected abstract void InitializeStates();

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
        // player가 null인지 확인하여 예외 방지
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

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("몬스터 사망");

        if (monsterDeathEffectPrefab != null)
        {
            GameObject deathEffect = Instantiate(monsterDeathEffectPrefab, transform.position, Quaternion.identity);
            deathEffect.transform.localScale = Vector3.one;

            Debug.Log("사망 이펙트 생성됨");
            Debug.Log($"Death Effect 활성 상태: {deathEffect.activeSelf}");

            ParticleSystem[] particleSystems = deathEffect.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                Debug.Log($"파티클 시스템 이름: {ps.gameObject.name}, 활성 상태: {ps.gameObject.activeSelf}, 실행 중: {ps.isPlaying}");
                ps.Clear();  // 초기화
                ps.Play();   // 재생
            }

            Destroy(deathEffect, deathEffectDuration + 0.5f);

        }

        DropExperienceItem();

        Destroy(gameObject);
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
            GameObject experienceItemPrefab = Resources.Load<GameObject>("ExperienceItem");
            if (experienceItemPrefab != null)
            {
                GameObject expItem = Instantiate(experienceItemPrefab, transform.position, Quaternion.identity);
                ExperienceItem expScript = expItem.GetComponent<ExperienceItem>();
                if (expScript != null)
                {
                    expScript.experienceAmount = monsterBaseStat.experiencePoints;
                }
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
}
