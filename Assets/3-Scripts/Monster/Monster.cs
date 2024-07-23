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
    protected MeshRenderer meshRenderer;
    protected bool isInvincible = false;
    protected Player player;
    [SerializeField]
    private float invincibilityDuration = 0.5f;
    [SerializeField]
    private float blinkInterval = 0.1f;

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
        meshRenderer = GetComponent<MeshRenderer>();
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
            return;
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


    // 타격감을 위한 깜빡임
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        for (float i = 0; i < invincibilityDuration; i += blinkInterval)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            else if (meshRenderer != null)
            {
                meshRenderer.enabled = !meshRenderer.enabled;
            }
            yield return new WaitForSeconds(blinkInterval);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        else if (meshRenderer != null)
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
