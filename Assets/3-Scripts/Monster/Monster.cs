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
    [Header("���� ����")]
    [Tooltip("������ �⺻ ���� �����͸� �����մϴ�.")]
    public MonsterData monsterBaseStat;

    protected Rigidbody2D rb;
    protected int currentHP;
    protected MeshRenderer meshRenderer;
    private Color originalColor;
    protected bool isDead = false;
    public bool IsDead => isDead;

    protected internal Player player;

    [Header("�˹� ����")]
    [SerializeField]
    [Tooltip("�˹��� �ּ� ���� �����մϴ�.")]
    private float minKnockbackForce = 5f;

    [SerializeField]
    [Tooltip("�˹��� �ִ� ���� �����մϴ�.")]
    private float maxKnockbackForce = 15f;

    [SerializeField]
    [Tooltip("������ ������ �����մϴ�.")]
    private float blinkInterval = 0.1f;

    [SerializeField]
    [Tooltip("������ ���� �ð��� �����մϴ�.")]
    private float blinkDuration = 0.5f;

    [Tooltip("���� ��� �� ������ ����Ʈ �������Դϴ�.")]
    public GameObject monsterDeathEffectPrefab;

    [SerializeField]
    [Tooltip("��� ����Ʈ�� ���� �ð��� �����մϴ�.")]
    private float deathEffectDuration = 0.2f;

    [SerializeField]
    [Tooltip("�ɷ¿� ���� ��� ����Ʈ�� ���� �ð��� �����մϴ�.")]
    private float abilityDeathEffectDuration = 1f;

    [SerializeField]
    [Tooltip("��� ����Ʈ �������Դϴ�.")]
    private GameObject monsterInstantKillEffectPrefab;

    [Tooltip("���Ͱ� ����Ʈ���� ���θ� ��Ÿ���ϴ�.")]
    public bool isElite = false;

    [Tooltip("��ٿ� �������� ���θ� ��Ÿ���ϴ�.")]
    public bool isInCooldown = false;

    [Tooltip("���� �������� ���θ� ��Ÿ���ϴ�.")]
    public bool isInfected = false;

    private string damageTextPrefabPath = "DamageTextPrefab";
    protected SkeletonAnimation skeletonAnimations;

    [Tooltip("���� �ɷ¿� ���� �� ���� �ߵ��ϵ��� �����մϴ�.")]
    public bool HasBeenHitByBetting { get; set; } = false;

    public IMonsterState currentState;
    public IMonsterState idleState;
    public IMonsterState chaseState;
    public IMonsterState attackState;
    public IMonsterState cooldownState;

    [Tooltip("������ ���� ������Ʈ�Դϴ�.")]
    public GameObject damageArea;

    private Collider2D areaCollider;
    private SpriteRenderer areaSpriteRenderer;

    [SerializeField]
    [Tooltip("���� �������� �����մϴ�.")]
    private int areaDamage = 10;

    [SerializeField]
    [Tooltip("������ ������ �����մϴ�.")]
    private float damageInterval = 1f;
    protected bool isPlayerDead = false;
    private bool isStunned = false;
    public bool IsStunned => isStunned;
    private float stunEndTime;

    private bool originalIsKinematic;

    [Header("���� ȿ��")]
    [Tooltip("���� ������ �� ǥ���� ����Ʈ ������")]
    public GameObject stunEffectPrefab;
    private GameObject currentStunEffect;

    [Tooltip("��� ���θ� �����մϴ�.")]
    public bool isInstantKilled = false;

    private static Transform experienceItemsParent;

    /// <summary>
    /// ������ ������Ʈ�� �ʱ�ȭ�ϰ� �ʱ� ���¸� �����մϴ�.
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
            Debug.LogWarning("MeshRenderer�� ã�� �� �����ϴ�.");
        }

        rb = GetComponent<Rigidbody2D>();

        player = FindObjectOfType<Player>();
        InitializeExperienceItemsParent();
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
        player.OnPlayerDeath.AddListener(OnPlayerDeathHandler);
    }

    /// <summary>
    /// �� �����Ӹ��� ���͸� ������Ʈ�ϰ� �÷��̾� ������ Ȯ���մϴ�.
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
    /// ������ ���¸� �ʱ�ȭ�մϴ�. ����Ŭ�������� �����ؾ� �մϴ�.
    /// </summary>
    protected abstract void InitializeStates();

    /// <summary>
    /// ������ �ð� ���� ���͸� ������ŵ�ϴ�.
    /// </summary>
    /// <param name="duration">���� ���� �ð�(��)</param>
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
    /// ���� ���� �ð��� ȸ���� ó���ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <param name="duration">���� ���� �ð�(��)</param>
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
    /// ���� ���� ���� ����Ʈ�� ǥ���մϴ�.
    /// </summary>
    private void ShowStunEffect()
    {
        if (stunEffectPrefab == null)
        {
            Debug.LogWarning("StunEffectPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        Vector3 stunPosition = transform.position + new Vector3(0, 1.0f, 0);
        currentStunEffect = Instantiate(stunEffectPrefab, stunPosition, Quaternion.identity, transform);
    }

    /// <summary>
    /// ���Ϳ��� ���� ����Ʈ�� �����մϴ�.
    /// </summary>
    private void RemoveStunEffect()
    {
        if (currentStunEffect != null)
        {
            Destroy(currentStunEffect);
        }
    }

    /// <summary>
    /// ���͸� ���ο� ���·� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="newState">��ȯ�� ���ο� ����</param>
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
    /// �÷��̾ ������ ���� ���� �ִ��� Ȯ���մϴ�.
    /// </summary>
    /// <param name="range">Ȯ���� ����</param>
    /// <returns>�÷��̾ ���� ���� ������ true�� ��ȯ</returns>
    public bool IsPlayerInRange(float range)
    {
        if (player == null)
        {
            Debug.LogWarning("Player�� �������� �ʾҽ��ϴ�. IsPlayerInRange ȣ���� ���õ˴ϴ�.");
            return false;
        }

        return Vector2.Distance(transform.position, player.transform.position) <= range;
    }

    /// <summary>
    /// ���͸� ������ ��ǥ ��ġ�� �̵���ŵ�ϴ�.
    /// </summary>
    /// <param name="target">�̵��� ��ǥ ��ġ</param>
    public void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * monsterBaseStat.monsterSpeed * Time.deltaTime;
    }

    /// <summary>
    /// ���Ϳ��� �������� �����ϰ� ������ �˹��� ó���մϴ�.
    /// </summary>
    /// <param name="damage">������ ������ ��</param>
    /// <param name="damageSourcePosition">�������� �߻��� ��ġ</param>
    /// <param name="deathAbilityKill">�������� ���� �ɷ¿� ���� ������ ����</param>
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
    /// ���� ���� ������ �ؽ�Ʈ�� ǥ���մϴ�.
    /// </summary>
    /// <param name="damage">ǥ���� ������ ��</param>
    public void ShowDamageText(int damage)
    {
        StartCoroutine(ShowDamageTextCoroutine(damage));
    }

    /// <summary>
    /// ������ �ؽ�Ʈ�� ǥ�� �� ���Ÿ� ó���ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <param name="damage">ǥ���� ������ ��</param>
    /// <returns>�ڷ�ƾ�� IEnumerator</returns>
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
            Debug.LogError("DamageTextPrefab�� Resources �������� ã�� �� �����ϴ�.");
            yield break;
        }

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
                Debug.LogWarning("��Ʈ ������ ã�� �� �����ϴ�. �⺻ ��Ʈ�� ����մϴ�.");
            }

            Color color = Color.white;
            damageText.SetText(damage.ToString(), font, fontSize, color);
        }
        else
        {
            Debug.LogError("DamageText ������Ʈ�� ã�� �� �����ϴ�.");
        }

        float totalDuration = 1f + additionalTime;

        yield return new WaitForSeconds(totalDuration);
        Destroy(damageTextInstance);
    }

    /// <summary>
    /// ������ �������� �־������� ���Ϳ��� �˹��� �����մϴ�.
    /// </summary>
    /// <param name="damageSourcePosition">�������� �߻��� ��ġ</param>
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
            Debug.LogWarning("Rigidbody2D�� ���Ϳ� �����ϴ�.");
        }
    }

    /// <summary>
    /// ������ ���� ������ �����մϴ�. ����Ŭ�������� �����ؾ� �մϴ�.
    /// </summary>
    public abstract void Attack();

    /// <summary>
    /// ������ ���� ü���� �����ɴϴ�.
    /// </summary>
    /// <returns>���� ü�� ��</returns>
    public int GetCurrentHP()
    {
        return currentHP;
    }

    /// <summary>
    /// ������ ������ ó���ϰ� ����Ʈ, ����ġ ���, ������Ʈ �ı��� �����մϴ�.
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

        // �ʿ��� ��� ������ �������� ���߰ų� �ִϸ��̼��� �����մϴ�.
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (skeletonAnimations != null)
        {
            skeletonAnimations.timeScale = 0f; // �ִϸ��̼� ����
        }
    }

    /// <summary>
    /// ���Ͱ� �����Ǿ��� ��� ���� �� ������� �����մϴ�.
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
            Debug.LogWarning("��� ���� �������� ã�� �� �����ϴ�: ParasitePrefab");
        }
    }

    /// <summary>
    /// ���Ͱ� �������� ���� �� �Ӱ� �����̴� ȿ���� ó���ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <returns>�ڷ�ƾ�� IEnumerator</returns>
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
    /// ����ġ ������ �θ� Ʈ�������� �ʱ�ȭ�մϴ�.
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
    /// ���Ͱ� ���� �� ����ġ �������� ����մϴ�.
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

            // ����ġ ȹ�淮�� 0�̸� �������� �������� ����
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
                Debug.LogWarning($"���ҽ��� ã�� �� �����ϴ�: {prefabName}");
            }
        }
    }


    /// <summary>
    /// ���Ͱ� �ٸ� �ݶ��̴��� �浹�� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="collision">�浹 ����</param>
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
    /// ���Ͱ� ����Ʈ�� ��� ���� �������� ó���ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <returns>�ڷ�ƾ�� IEnumerator</returns>
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
    /// �÷��̾ ���� �������� ���� �� ���� ��������Ʈ�� �����̴� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <returns>�ڷ�ƾ�� IEnumerator</returns>
    private IEnumerator BlinkAreaSprite()
    {
        if (areaSpriteRenderer == null) yield break;

        float blinkDuration = 0.2f;
        areaSpriteRenderer.enabled = false;
        yield return new WaitForSeconds(blinkDuration);
        areaSpriteRenderer.enabled = true;
    }
}
