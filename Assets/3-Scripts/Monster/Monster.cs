using Spine.Unity;
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
    [Header("Monster Settings")]
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

    private string damageTextPrefabPath = "DamageTextPrefab";
    protected SkeletonAnimation skeletonAnimations;

    // ���ο� �ʵ� �߰�: Betting �ɷ¿� ���� �� ���� �ߵ��ϵ��� ����
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
    public bool IsStunned => isStunned;
    private float stunEndTime;

    // ������ isKinematic ���¸� �����ϱ� ���� �ʵ� �߰�
    private bool originalIsKinematic;

    [Header("Stun Effect")]
    [Tooltip("���� ������ �� ǥ���� ����Ʈ ������")]
    public GameObject stunEffectPrefab;
    private GameObject currentStunEffect;

    protected virtual void Start()
    {
        skeletonAnimations = GetComponent<SkeletonAnimation>();
        currentHP = monsterBaseStat.maxHP;
        meshRenderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody2D>();

        player = FindObjectOfType<Player>();

        if (player == null)
        {
            Debug.LogError("�÷��̾� ������Ʈ�� ã�� �� �����ϴ�. Update �޼��忡�� ��õ��մϴ�.");
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
                    Debug.LogError("Damage area collider�� ã�� �� �����ϴ�. Collider2D�� �����ϴ��� Ȯ���ϼ���.");
                }

                if (areaSpriteRenderer == null)
                {
                    Debug.LogError("Damage area�� SpriteRenderer�� ã�� �� �����ϴ�. SpriteRenderer�� �����ϴ��� Ȯ���ϼ���.");
                }

                if (areaCollider != null && areaSpriteRenderer != null)
                {
                    StartCoroutine(AreaDamageCoroutine());
                }
            }
            else
            {
                Debug.LogWarning("damageArea�� �������� �ʾҽ��ϴ�. ���� ������ �ý����� ��Ȱ��ȭ�մϴ�.");
            }
        }
    }

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

        if (isStunned) return;
        currentState?.UpdateState();
    }

    protected abstract void InitializeStates();

    public virtual void Stun(float duration)
    {
        if (!isStunned)
        {
            isStunned = true;
            stunEndTime = Time.time + duration;

            // �ִϸ��̼� ����
            if (skeletonAnimations != null)
            {
                skeletonAnimations.timeScale = 0f;
            }

            // Rigidbody ����
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                // ������ isKinematic ���� ����
                originalIsKinematic = rb.isKinematic;
                rb.isKinematic = true;
            }

            // ���� ����Ʈ ǥ��
            ShowStunEffect();

            StartCoroutine(StunCoroutine(duration));
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isStunned = false;

        // �ִϸ��̼� �簳
        if (skeletonAnimations != null)
        {
            skeletonAnimations.timeScale = 1f;
        }

        // Rigidbody ���� ���·� ����
        if (rb != null)
        {
            rb.isKinematic = originalIsKinematic;
        }

        // ���� ����Ʈ ����
        RemoveStunEffect();
    }

    private void ShowStunEffect()
    {
        if (stunEffectPrefab == null)
        {
            Debug.LogWarning("StunEffectPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // ���� ����Ʈ�� ������ �Ӹ� ���� ��ġ��ŵ�ϴ�.
        Vector3 stunPosition = transform.position + new Vector3(0, 1.0f, 0); // �Ӹ� ���� 1.0f ������
        currentStunEffect = Instantiate(stunEffectPrefab, stunPosition, Quaternion.identity, transform);
    }

    private void RemoveStunEffect()
    {
        if (currentStunEffect != null)
        {
            Destroy(currentStunEffect);
        }
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
            Debug.LogWarning("Player�� �������� �ʾҽ��ϴ�. IsPlayerInRange ȣ���� ���õ˴ϴ�.");
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
        if (isDead || isInvincible)
        {
            return;
        }
        ShowDamageText(damage);
        ApplyKnockback(damageSourcePosition);

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

    private void ShowDamageText(int damage)
    {
        // StartCoroutine�� ���� �ڷ�ƾ ����
        StartCoroutine(ShowDamageTextCoroutine(damage));
    }

    private IEnumerator ShowDamageTextCoroutine(int damage)
    {
        Debug.Log($"������ �ؽ�Ʈ ǥ��: {damage}");

        // ������ �翡 ���� ���� ũ�� �� �߰� �ð� ����
        int fontSize;
        float additionalTime = 0f;

        if (damage >= 100)
        {
            fontSize = 40;
            additionalTime = 2f; // +2��
        }
        else if (damage >= 50)
        {
            fontSize = 32;
            additionalTime = 0.5f; // +0.5��
        }
        else
        {
            fontSize = 18;
            additionalTime = 0f;
        }

        // DamageText ������ �ε�
        GameObject damageTextPrefab = Resources.Load<GameObject>(damageTextPrefabPath);
        if (damageTextPrefab == null)
        {
            Debug.LogError("DamageTextPrefab�� Resources �������� ã�� �� �����ϴ�.");
            yield break;
        }

        // DamageCanvas �±׷� ĵ���� ã��
        GameObject canvasObject = GameObject.FindGameObjectWithTag("DamageCanvas");
        if (canvasObject == null)
        {
            Debug.LogError("'DamageCanvas' �±׸� ���� ĵ������ ã�� �� �����ϴ�.");
            yield break;
        }

        Canvas screenCanvas = canvasObject.GetComponent<Canvas>();
        if (screenCanvas == null)
        {
            Debug.LogError("'DamageCanvas' ������Ʈ���� Canvas ������Ʈ�� ã�� �� �����ϴ�.");
            yield break;
        }

        // UI ī�޶� ��������
        Camera uiCamera = screenCanvas.worldCamera;
        if (uiCamera == null)
        {
            uiCamera = Camera.main;
        }

        if (uiCamera == null)
        {
            Debug.LogError("UI ī�޶� ã�� �� �����ϴ�.");
            yield break;
        }

        // ������ �ؽ�Ʈ ��ġ ���� (���� �Ӹ� ��)
        float monsterHeightOffset = 1.0f; // ���� �Ӹ� ���� �󸶳� ����� ���� ������ ��
        Vector3 headPosition = transform.position + new Vector3(0, monsterHeightOffset, 0);
        Vector3 screenPosition = uiCamera.WorldToScreenPoint(headPosition);

        // ��ũ�� ��ǥ�� ĵ���� ���� ��ǥ�� ��ȯ
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            screenCanvas.transform as RectTransform,
            screenPosition,
            uiCamera,
            out Vector2 localPoint);

        // ������ �ؽ�Ʈ �ν��Ͻ� ����
        GameObject damageTextInstance = Instantiate(damageTextPrefab, screenCanvas.transform);
        RectTransform rectTransform = damageTextInstance.GetComponent<RectTransform>();
        rectTransform.localPosition = localPoint;

        // DamageText ������Ʈ ����
        DamageText damageText = damageTextInstance.GetComponent<DamageText>();
        if (damageText != null)
        {
            // ��Ʈ �ε�
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Maplestory Light SDF");
            if (font == null)
            {
                Debug.LogWarning("��Ʈ ������ ã�� �� �����ϴ�. �⺻ ��Ʈ�� ����մϴ�.");
            }

            Color color = Color.white;

            // �ؽ�Ʈ ����
            damageText.SetText(damage.ToString(), font, fontSize, color);
        }
        else
        {
            Debug.LogError("DamageText ������Ʈ�� ã�� �� �����ϴ�.");
        }

        // �ؽ�Ʈ ���� �ð� ���
        float totalDuration = 1f + additionalTime;

        // �ؽ�Ʈ�� ��������� ��� (���� �ð� + �߰� �ð�)
        yield return new WaitForSeconds(totalDuration);
        Destroy(damageTextInstance);
    }

    private void ApplyKnockback(Vector3 damageSourcePosition)
    {
        if (rb != null)
        {
            // ��ũ�� ���� ��� (���� ��ġ - ������ ��ġ)
            Vector2 knockbackDirection = (transform.position - damageSourcePosition).normalized;

            float knockbackForce = 2f;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogWarning("Rigidbody2D�� ���Ϳ� �����ϴ�.");
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
        Debug.Log("���Ͱ� ���������ϴ�!");

        if (player != null)
        {
            player.KillMonster();

            // �ɷ� �Ŵ������� ��� �ɷ��� �����ɴϴ�.
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
            Debug.LogWarning("��� ���� �������� ã�� �� �����ϴ�: ParasitePrefab");
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        Debug.Log("���� ���� ����");

        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = !meshRenderer.enabled;
            }
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }

        isInvincible = false;
        Debug.Log("���� ���� ����");
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
                Debug.LogWarning($"���ҽ��� ã�� �� �����ϴ�: {prefabName}");
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
