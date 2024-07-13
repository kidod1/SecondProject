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
    protected SpriteRenderer spriteRenderer;
    protected MeshRenderer meshRenderer; // MeshRenderer 추가
    protected bool isInvincible = false;
    protected Player player;
    [SerializeField]
    private float invincibilityDuration = 0.5f; // 무적 상태 지속 시간
    [SerializeField]
    private float blinkInterval = 0.1f; // 깜빡임 간격

    public bool isInCooldown = false;

    public IMonsterState currentState;
    public IMonsterState idleState;
    public IMonsterState chaseState;
    public IMonsterState attackState;
    public IMonsterState cooldownState;

    protected virtual void Start()
    {
        currentHP = monsterBaseStat.maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        meshRenderer = GetComponent<MeshRenderer>(); // MeshRenderer 초기화
        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("플레이어 오브젝트를 찾을 수 없습니다.");
        }
        InitializeStates();
    }

    protected abstract void InitializeStates();

    private void Update()
    {
        currentState?.UpdateState();
    }

    public void TransitionToState(IMonsterState newState)
    {
        if (isInCooldown && newState != cooldownState)
        {
            return; // 쿨다운 중에는 다른 상태로 전환하지 않음
        }

        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }

    public bool IsPlayerInRange(float range)
    {
        return Vector2.Distance(transform.position, player.transform.position) <= range;
    }

    public void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * monsterBaseStat.monsterSpeed * Time.deltaTime;
    }

    public virtual void TakeDamage(int damage)
    {
        if (isInvincible) return;

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
        player.GainExperience(monsterBaseStat.experiencePoints);
        gameObject.SetActive(false);
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        for (float i = 0; i < invincibilityDuration; i += blinkInterval)
        {
            if (spriteRenderer != null) // SpriteRenderer가 있는 경우 깜빡임 효과 적용
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            else if (meshRenderer != null) // MeshRenderer가 있는 경우 깜빡임 효과 적용
            {
                meshRenderer.enabled = !meshRenderer.enabled;
            }
            yield return new WaitForSeconds(blinkInterval);
        }

        if (spriteRenderer != null) // SpriteRenderer가 있는 경우 원래 상태로 복원
        {
            spriteRenderer.enabled = true;
        }
        else if (meshRenderer != null) // MeshRenderer가 있는 경우 원래 상태로 복원
        {
            meshRenderer.enabled = true;
        }

        isInvincible = false;
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
