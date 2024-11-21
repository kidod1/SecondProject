using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Linq;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine.Events;
using Spine;

public class MidBoss : Monster
{
    [Header("Mid-Boss Settings")]
    [InspectorName("�ִ� ü��")]
    public int maxHealth = 100;

    [Header("���� ������")]
    [InspectorName("���� ������")]
    public BossPatternData patternData;

    [SerializeField, InspectorName("���� ���� ����")]
    private bool isAttackable = false;

    [SerializeField, InspectorName("���� �θ� Transform")]
    private Transform patternParent;

    [Header("UI Manager")]
    [SerializeField, InspectorName("PlayerUIManager")]
    private PlayerUIManager playerUIManager;

    [SerializeField]
    private SlothMapManager slothMapManager;

    [SerializeField]
    private DeathAnimationHandler deathAnimationHandler;

    // ExecutePatterns �ڷ�ƾ�� �����ϱ� ���� ����
    private Coroutine executePatternsCoroutine;

    // ���� ��� ���� ���� ����Ʈ �ε���
    private int currentSpawnPointIndex = 0;

    [SerializeField]
    private GameObject bossFalse;

    [Header("źȯ ���� ����Ʈ ����")]
    [SerializeField, InspectorName("źȯ ���� ����Ʈ �迭")]
    [Tooltip("źȯ�� �߻�� ���� ��ġ�� Transform �迭")]
    private Transform[] bulletSpawnPoints;

    [Header("��� ������ ��ġ ����")]
    [SerializeField, InspectorName("��� ������ ��ġ �迭")]
    [Tooltip("��� ������ ������ �߻��� 5���� ��ġ")]
    private Transform[] warningLaserPositions;

    [SerializeField]
    private GameManager gameManager;

    // �ǰ� �� ���� ������ ���� �ʵ�
    private MeshRenderer redMeshRenderer;
    private Color bossOriginalColor;

    [Header("WWISE Sound Events")]
    [SerializeField]
    private AK.Wwise.Event bulletPatternSound;

    [SerializeField]
    private AK.Wwise.Event warningAttackPatternSound;

    [SerializeField]
    private AK.Wwise.Event warningLaserPatternSound;

    [SerializeField]
    private AK.Wwise.Event groundSmashPatternSound;

    // �÷��̾��� ���� ���¸� �����ϴ� ����
    private bool isPlayerDead = false;

    [Header("Spine Animation Settings")]
    [SerializeField]
    private SkeletonAnimation skeletonAnimation;

    [Header("Animation Names")]
    [SerializeField, SpineAnimation]
    private string idleAnimationName = "standard";

    [SerializeField, SpineAnimation]
    private string bulletPatternAnimationName;

    [SerializeField, SpineAnimation]
    private string warningAttackPatternAnimationName;

    [SerializeField, SpineAnimation]
    private string warningLaserPatternAnimationName;

    [SerializeField, SpineAnimation]
    private string groundSmashPatternAnimationName;

    [SerializeField, SpineAnimation]
    private string deathAnimationName;

    // �ִϸ��̼� ����� �������� Ȯ���ϱ� ���� �̺�Ʈ
    private bool isAnimationPlaying = false;

    protected override void Start()
    {
        base.Start();
        bossFalse.gameObject.SetActive(false);

        // ���� �⺻ ���� ����
        if (monsterBaseStat != null)
        {
            monsterBaseStat.maxHP = maxHealth;
            currentHP = maxHealth;
        }
        else
        {
            currentHP = maxHealth;
        }

        if (playerUIManager != null)
        {
            playerUIManager.InitializeBossHealth(maxHealth);
        }

        patternParent = new GameObject("BossPatterns").transform;
        isAttackable = false;

        if (patternData != null && slothMapManager == null)
        {
            slothMapManager = FindObjectsOfType<SlothMapManager>().FirstOrDefault();
        }

        // DeathAnimationHandler ����
        if (deathAnimationHandler == null && slothMapManager != null)
        {
            deathAnimationHandler = slothMapManager.GetComponent<DeathAnimationHandler>();
        }

        // ���� ����Ʈ�� �������� ���� ���
        if (bulletSpawnPoints == null || bulletSpawnPoints.Length == 0)
        {
            // ���� ����Ʈ �迭�� ������� ���� ó��
        }

        // ��� ������ ��ġ �迭�� �������� ���� ���
        if (warningLaserPositions == null || warningLaserPositions.Length < 5)
        {
            // ��� ������ ��ġ �迭�� ������� ���� ���� ó��
        }

        // MeshRenderer�� ���� ���� ����
        redMeshRenderer = GetComponent<MeshRenderer>();
        if (redMeshRenderer != null)
        {
            // ���׸��� �ν��Ͻ�ȭ
            redMeshRenderer.material = new Material(redMeshRenderer.material);
            bossOriginalColor = redMeshRenderer.material.color;
        }

        // �÷��̾��� ���� �̺�Ʈ�� �����մϴ�.
        if (player != null)
        {
            player.OnPlayerDeath.AddListener(OnPlayerDeathHandler);
        }

        // Spine �ִϸ��̼� �ʱ�ȭ �� Idle �ִϸ��̼� ���
        if (skeletonAnimation != null)
        {
            PlayAnimation(idleAnimationName, loop: true);
        }

        // ���� ���� ����
        SetAttackable(true);
    }

    private void OnEnable()
    {
        // �ִϸ��̼� �̺�Ʈ �ڵ鷯 ��� ����
    }

    private void OnDisable()
    {
        // �ִϸ��̼� �̺�Ʈ �ڵ鷯 ���� ����
    }

    private void Update()
    {
        if (isDead || isPlayerDead) return;

        currentState?.UpdateState();
    }

    // �÷��̾ �׾��� �� ȣ��Ǵ� �޼���
    private void OnPlayerDeathHandler()
    {
        isPlayerDead = true;

        // ���� �Ұ��� ���·� ����
        SetAttackable(false);

        // ���� �ʱ�ȭ
        if (redMeshRenderer != null && redMeshRenderer.material != null)
        {
            redMeshRenderer.material.color = bossOriginalColor;
        }

        // Idle �ִϸ��̼� ���
        PlayAnimation(idleAnimationName, loop: true);
    }

    // OnDestroy���� �̺�Ʈ ���� ���� ����
    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnPlayerDeath.RemoveListener(OnPlayerDeathHandler);
        }
    }

    protected override void InitializeStates()
    {
        // MidBoss�� ���� �ý����� ������� �����Ƿ� �������� �ʽ��ϴ�.
    }

    /// <summary>
    /// ������ ���� ���� ���·� �����մϴ�.
    /// </summary>
    /// <param name="value">���� ���� ����</param>
    public void SetAttackable(bool value)
    {
        isAttackable = value;
        if (isAttackable)
        {
            if (executePatternsCoroutine == null)
            {
                // ExecutePatterns �ڷ�ƾ�� �����ϰ� ������ �����մϴ�.
                executePatternsCoroutine = StartCoroutine(ExecutePatterns());
            }
        }
        else
        {
            // ���� �Ұ��� �� �ڷ�ƾ ����
            if (executePatternsCoroutine != null)
            {
                StopCoroutine(executePatternsCoroutine);
                executePatternsCoroutine = null;
            }
        }
    }

    public override void TakeDamage(int damage, Vector3 damageSourcePosition, bool Nun = false)
    {
        if (isDead || isPlayerDead)
        {
            return;
        }

        ShowDamageText(damage);

        currentHP -= damage;

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
        else
        {
            // �ǰ� �� ���� �ʱ�ȭ
            if (redMeshRenderer != null && redMeshRenderer.material != null)
            {
                redMeshRenderer.material.color = Color.red;
                StartCoroutine(FlashRedCoroutine());
            }
        }

        if (playerUIManager != null)
        {
            playerUIManager.UpdateBossHealth(currentHP);
        }
    }

    private IEnumerator FlashRedCoroutine()
    {
        if (redMeshRenderer != null && redMeshRenderer.material != null)
        {
            redMeshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f); // ������ ���� �ð�
            redMeshRenderer.material.color = bossOriginalColor;
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        // ���� ���� �ڷ�ƾ ����
        SetAttackable(false);

        if (playerUIManager != null)
        {
            playerUIManager.UpdateBossHealth(0);
            playerUIManager.HideBossHealthUI(); // ���� ü�� UI �г� ��Ȱ��ȭ
        }

        if (slothMapManager != null)
        {
            if (!slothMapManager.gameObject.activeInHierarchy)
            {
                slothMapManager.gameObject.SetActive(true);
            }

            slothMapManager.OnDeathAnimationsCompleted += HandleDeathAnimationsCompleted;
            gameManager.AddBossKill();
            gameManager.AddFloorClear();

            // ���� �ִϸ��̼� ���
            PlayAnimation(deathAnimationName, loop: false);

            // Death �ִϸ��̼��� �Ϸ�� ������ ���
            StartCoroutine(WaitForDeathAnimation());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator WaitForDeathAnimation()
    {
        // �ִϸ��̼��� ���� ������ ���
        while (isAnimationPlaying)
        {
            yield return null;
        }

        // Death �ִϸ��̼��� ���� �� ó��
        if (slothMapManager != null)
        {
            StartCoroutine(PlayDeathAnimationsCoroutine());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator PlayDeathAnimationsCoroutine()
    {
        yield return new WaitForEndOfFrame();
        slothMapManager.PlayDeathAnimations();
    }

    private void HandleDeathAnimationsCompleted()
    {
        if (slothMapManager != null)
        {
            slothMapManager.OnDeathAnimationsCompleted -= HandleDeathAnimationsCompleted;
        }

        Destroy(gameObject);
    }

    private IEnumerator ExecutePatterns()
    {
        while (true)
        {
            if (isDead || isPlayerDead)
            {
                yield break;
            }

            // �� Ȯ�� ���
            float totalProbability = patternData.bulletPatternProbability + patternData.warningAttackPatternProbability +
                                     patternData.warningLaserPatternProbability + patternData.groundSmashPatternProbability;

            float randomValue = UnityEngine.Random.Range(0f, totalProbability);

            if (randomValue < patternData.bulletPatternProbability)
            {
                PlayAnimation(bulletPatternAnimationName, loop: false);
                yield return StartCoroutine(BulletPattern());
            }
            else if (randomValue < patternData.bulletPatternProbability + patternData.warningAttackPatternProbability)
            {
                PlayAnimation(warningAttackPatternAnimationName, loop: false);
                yield return StartCoroutine(WarningAttackPattern());
            }
            else if (randomValue < patternData.bulletPatternProbability + patternData.warningAttackPatternProbability + patternData.warningLaserPatternProbability)
            {
                PlayAnimation(warningLaserPatternAnimationName, loop: false);
                yield return StartCoroutine(WarningLaserPattern());
            }
            else
            {
                PlayAnimation(groundSmashPatternAnimationName, loop: false);
                yield return StartCoroutine(GroundSmashPattern());
            }

            // ���� ���� �� idle �ִϸ��̼� ���
            PlayAnimation(idleAnimationName, loop: true);

            // ���� ���ϱ��� ��� �ð� (1��)
            yield return new WaitForSeconds(1f);
        }
    }

    // �� ���� �޼��忡���� isPlayerDead�� Ȯ���Ͽ� ���� ����
    private IEnumerator WarningLaserPattern()
    {
        if (isDead || isPlayerDead)
        {
            yield break;
        }

        if (warningLaserPositions == null || warningLaserPositions.Length < 5)
        {
            yield break;
        }

        // 5���� ��ġ �� 4���� �����ϰ� ����
        List<Transform> positionsList = warningLaserPositions.ToList();
        List<Transform> selectedPositions = new List<Transform>();

        for (int i = 0; i < 4; i++)
        {
            if (positionsList.Count == 0) break;
            int randomIndex = UnityEngine.Random.Range(0, positionsList.Count);
            selectedPositions.Add(positionsList[randomIndex]);
            positionsList.RemoveAt(randomIndex);
        }

        foreach (Transform position in selectedPositions)
        {
            if (isDead || isPlayerDead)
            {
                yield break;
            }

            // ��� ������ �ν��Ͻ�ȭ
            GameObject warning = Instantiate(patternData.warningLaserWarningPrefab, position.position, Quaternion.identity, patternParent);
            Destroy(warning, patternData.warningLaserWarningDuration);

            // ��� ���� �ð���ŭ ���
            yield return new WaitForSeconds(patternData.warningLaserWarningDuration);
        }

        // ��� ǥ�� ������� ������ ���� ����
        foreach (Transform position in selectedPositions)
        {
            if (isDead || isPlayerDead)
            {
                yield break;
            }

            if (warningLaserPatternSound != null)
            {
                warningLaserPatternSound.Post(gameObject);
            }

            // ������ ���� ������ �ν��Ͻ�ȭ
            GameObject laserAttack = Instantiate(patternData.warningLaserAttackPrefab, position.position, Quaternion.identity, patternParent);
            Destroy(laserAttack, patternData.warningLaserAttackDuration);

            // ������ ������ ���� DamageArea ������Ʈ �߰�
            DamageArea damageArea = laserAttack.AddComponent<DamageArea>();
            damageArea.damage = patternData.warningLaserAttackDamage;
            damageArea.duration = patternData.warningLaserAttackDuration;
            damageArea.isContinuous = true;

            // ������ ���� ���� �ð���ŭ ���
            yield return new WaitForSeconds(patternData.warningLaserAttackDuration);
        }
    }

    private IEnumerator BulletPattern()
    {
        if (isDead || isPlayerDead)
        {
            yield break;
        }

        for (int i = 0; i < patternData.bulletPatternRepeatCount; i++)
        {
            if (isDead || isPlayerDead)
            {
                yield break;
            }

            try
            {
                FireBullets();
            }
            catch (System.Exception ex)
            {
                yield break; // ���� �߻� �� �ڷ�ƾ ����
            }

            yield return new WaitForSeconds(patternData.bulletFireInterval);
        }
    }

    /// <summary>
    /// ���� ���� ����Ʈ���� źȯ�� �߻��մϴ�.
    /// </summary>
    private void FireBullets()
    {
        if (bulletSpawnPoints == null || bulletSpawnPoints.Length == 0)
        {
            return;
        }

        // ���� ���� ����Ʈ �ε����� �ش��ϴ� Transform ��������
        Transform spawnPoint = bulletSpawnPoints[currentSpawnPointIndex];
        Vector3 spawnPosition = spawnPoint.position;

        Vector3 playerPosition = GetPlayerPosition();
        Vector3 direction = (playerPosition - spawnPosition).normalized;
        float spreadAngle = 12f;
        int bulletCount = 55;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = (-spreadAngle) + (spreadAngle * i);
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 bulletDirection = rotation * direction;

            GameObject bullet = Instantiate(patternData.bulletPrefab, spawnPosition, Quaternion.identity, patternParent);

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetAttackDamage(patternData.bulletDamage);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.velocity = bulletDirection * 5;
                }
            }
        }

        // ���� ���� ����Ʈ�� �ε��� ������Ʈ (��ȯ)
        currentSpawnPointIndex = (currentSpawnPointIndex + 1) % bulletSpawnPoints.Length;
    }

    private IEnumerator WarningAttackPattern()
    {
        if (isDead || isPlayerDead)
        {
            yield break;
        }

        for (int i = 0; i < patternData.warningAttackRepeatCount; i++)
        {
            if (isDead || isPlayerDead)
            {
                yield break;
            }

            Vector3 targetPosition = GetPlayerPosition();

            // ��� ����Ʈ ����
            if (patternData.warningEffectPrefab != null)
            {
                GameObject warning = Instantiate(patternData.warningEffectPrefab, targetPosition, Quaternion.identity, patternParent);
                Destroy(warning, patternData.warningDuration);
            }

            // ��� ����Ʈ ���� �ð���ŭ ���
            yield return new WaitForSeconds(patternData.warningDuration);

            if (isDead || isPlayerDead)
            {
                yield break;
            }

            // ���� ����Ʈ ���� �� ������ ����
            if (patternData.attackEffectPrefab != null)
            {
                if (warningAttackPatternSound != null)
                {
                    warningAttackPatternSound.Post(gameObject);
                }
                GameObject attackEffect = Instantiate(patternData.attackEffectPrefab, targetPosition, Quaternion.identity, patternParent);
                Destroy(attackEffect, patternData.attackEffectDuration);

                // ���� ����Ʈ�� DamageArea ������Ʈ�� �߰��Ͽ� �������� �����մϴ�.
                DamageArea damageArea = attackEffect.AddComponent<DamageArea>();
                damageArea.damage = patternData.warningAttackDamage;
                damageArea.duration = patternData.attackEffectDuration;
                damageArea.isContinuous = true;
            }

            // ���� �ݺ����� ���
            yield return new WaitForSeconds(patternData.warningStartInterval);
        }
    }

    private IEnumerator GroundSmashPattern()
    {
        if (isDead || isPlayerDead)
        {
            yield break;
        }

        Vector3 targetPosition = GetPlayerPosition();
        GameObject warning = Instantiate(patternData.groundSmashWarningPrefab, targetPosition, Quaternion.identity, patternParent);
        Destroy(warning, patternData.groundSmashWarningDuration);

        yield return new WaitForSeconds(patternData.groundSmashWarningDuration);

        if (isDead || isPlayerDead)
        {
            yield break;
        }

        // ���ο� �ٴ� ��� ���� ����
        SpawnGroundSmashObjects(targetPosition);

        if (groundSmashPatternSound != null)
        {
            groundSmashPatternSound.Post(gameObject);
        }

        yield return new WaitForSeconds(patternData.groundSmashCooldown);
    }

    private void SpawnGroundSmashObjects(Vector3 targetPosition)
    {
        for (int i = 0; i < patternData.groundSmashMeteorCount; i++)
        {
            if (isDead || isPlayerDead)
            {
                return;
            }

            float angleRad = patternData.groundSmashAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0) * patternData.groundSmashSpawnRadius;
            Vector3 spawnPosition = targetPosition + offset;

            Quaternion rotation = Quaternion.Euler(0f, 0f, patternData.groundSmashObjectRotation);

            GameObject groundSmashObject = Instantiate(patternData.groundSmashMeteorPrefab, spawnPosition, rotation, patternParent);

            SpriteRenderer spriteRenderer = groundSmashObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Destroy(groundSmashObject);
                continue;
            }

            float fallSpeed = patternData.groundSmashMeteorFallSpeed;

            StartCoroutine(FallAndApplyDamage(groundSmashObject, targetPosition, fallSpeed));
        }
    }

    private IEnumerator FallAndApplyDamage(GameObject groundSmashObject, Vector3 targetPosition, float fallSpeed)
    {
        while (groundSmashObject != null)
        {
            if (isDead || isPlayerDead)
            {
                Destroy(groundSmashObject);
                yield break;
            }

            if (Vector3.Distance(groundSmashObject.transform.position, targetPosition) > 0.1f)
            {
                groundSmashObject.transform.position = Vector3.MoveTowards(groundSmashObject.transform.position, targetPosition, fallSpeed * Time.deltaTime);
                yield return null;
            }
            else
            {
                ApplyGroundSmashDamage(targetPosition);

                yield return new WaitForSeconds(0.5f);
                StartCoroutine(MoveUpAndDestroy(groundSmashObject, patternData.groundSmashSpeed, 3f));

                yield break;
            }
        }
    }

    private void ApplyGroundSmashDamage(Vector3 position)
    {
        if (isDead || isPlayerDead)
        {
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(position, patternData.groundSmashMeteorRadius);

        foreach (Collider2D hit in hits)
        {
            Monster monster = hit.GetComponent<Monster>();
            if (monster != null && !monster.IsDead)
            {
                monster.TakeDamage(patternData.groundSmashDamage, position);
            }
        }

        SpawnGroundSmashBullets(position);
    }

    private IEnumerator MoveUpAndDestroy(GameObject obj, float speed, float duration)
    {
        float elapsed = 0f;
        Vector3 upwardDirection = Vector3.up;

        while (elapsed < duration && obj != null)
        {
            if (isDead || isPlayerDead)
            {
                Destroy(obj);
                yield break;
            }

            obj.transform.position += upwardDirection * speed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (obj != null)
        {
            Destroy(obj);
        }
    }

    private void SpawnGroundSmashBullets(Vector3 position)
    {
        if (isDead || isPlayerDead)
        {
            return;
        }

        if (patternData.groundSmashBulletPrefab == null)
        {
            return;
        }

        int bulletCount = patternData.groundSmashBulletCount;
        float bulletSpeed = patternData.groundSmashBulletSpeed;
        int bulletDamage = patternData.groundSmashBulletDamage;

        for (int i = 0; i < bulletCount; i++)
        {
            if (isDead || isPlayerDead)
            {
                return;
            }

            float angle = i * (360f / bulletCount);
            float angleRad = angle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0).normalized;

            Vector3 spawnPosition = position;

            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            GameObject bullet = Instantiate(patternData.groundSmashBulletPrefab, spawnPosition, rotation, patternParent);

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetAttackDamage(bulletDamage);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.velocity = direction * bulletSpeed;
                }
            }
        }
    }

    private Vector3 GetPlayerPosition()
    {
        if (player != null)
        {
            return player.transform.position;
        }
        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (patternData != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetPlayerPosition(), patternData.groundSmashMeteorRadius);

            Gizmos.color = Color.blue;
            float angleStep = 360f / patternData.groundSmashBulletCount;
            for (int i = 0; i < patternData.groundSmashBulletCount; i++)
            {
                float angle = i * angleStep;
                float angleRad = angle * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0).normalized;
                Gizmos.DrawLine(GetPlayerPosition(), GetPlayerPosition() + direction * 2f);
            }

            // źȯ ���� ����Ʈ �ð�ȭ
            if (bulletSpawnPoints != null && bulletSpawnPoints.Length > 0)
            {
                Gizmos.color = Color.yellow;
                foreach (Transform spawnPoint in bulletSpawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    }
                }
            }

            // ��� ������ ��ġ �ð�ȭ
            if (warningLaserPositions != null && warningLaserPositions.Length > 0)
            {
                Gizmos.color = Color.magenta;
                foreach (Transform position in warningLaserPositions)
                {
                    if (position != null)
                    {
                        Gizmos.DrawWireSphere(position.position, 0.5f);
                    }
                }
            }
        }
    }

    public override void Attack()
    {
        // MidBoss�� �� �޼��带 ������� �ʽ��ϴ�.
    }

    /// <summary>
    /// ������ �ִϸ��̼��� ����մϴ�.
    /// </summary>
    /// <param name="animationName">����� �ִϸ��̼��� �̸�</param>
    /// <param name="loop">�ִϸ��̼� �ݺ� ����</param>
    private void PlayAnimation(string animationName, bool loop)
    {
        if (skeletonAnimation == null)
        {
            return;
        }

        // ���� ��� ���� �ִϸ��̼��� ���ٸ� ������� ����
        var current = skeletonAnimation.AnimationState.GetCurrent(0);
        if (current != null && current.Animation.Name == animationName)
        {
            return;
        }

        skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
        isAnimationPlaying = !loop;
    }
}
