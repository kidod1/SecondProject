using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Linq;
using Spine.Unity;
using System.Collections.Generic;

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

    // ExecutePatterns �ڷ�ƾ�� �����ϱ� ���� ����
    private Coroutine executePatternsCoroutine;

    // ���� ��� ���� ���� ����Ʈ �ε���
    private int currentSpawnPointIndex = 0;

    [Header("źȯ ���� ����Ʈ ����")]
    [SerializeField, InspectorName("źȯ ���� ����Ʈ �迭")]
    [Tooltip("źȯ�� �߻�� ���� ��ġ�� Transform �迭")]
    private Transform[] bulletSpawnPoints;

    // �߰��� �κ�: ��� ������ ��ġ �迭
    [Header("��� ������ ��ġ ����")]
    [SerializeField, InspectorName("��� ������ ��ġ �迭")]
    [Tooltip("��� ������ ������ �߻��� 5���� ��ġ")]
    private Transform[] warningLaserPositions;

    [SerializeField]
    private GameManager gameManager;
    protected override void Start()
    {
        base.Start();

        // ���� �⺻ ���� ����
        if (monsterBaseStat != null)
        {
            monsterBaseStat.maxHP = maxHealth;
            currentHP = maxHealth;
        }
        else
        {
            Debug.LogError("MidBoss: MonsterData(monsterBaseStat)�� �Ҵ���� �ʾҽ��ϴ�.");
            currentHP = maxHealth;
        }

        Debug.Log($"�߰� ���� ����! ü��: {currentHP}/{maxHealth}");

        if (playerUIManager != null)
        {
            playerUIManager.InitializeBossHealth(maxHealth);
        }
        else
        {
            Debug.LogError("MidBoss: PlayerUIManager�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        patternParent = new GameObject("BossPatterns").transform;
        isAttackable = false;

        if (patternData == null)
        {
            Debug.LogError("MidBoss: BossPatternData�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (patternData != null && slothMapManager == null)
        {
            slothMapManager = FindObjectsOfType<SlothMapManager>().FirstOrDefault();
            if (slothMapManager == null)
            {
                Debug.LogError("SlothMapManager�� ã�� �� �����ϴ�.");
            }
        }

        // ���� ����Ʈ�� �������� ���� ��� ��� �޽��� ���
        if (bulletSpawnPoints == null || bulletSpawnPoints.Length == 0)
        {
            Debug.LogWarning("MidBoss: źȯ ���� ����Ʈ �迭�� �������� �ʾҽ��ϴ�.");
        }

        // ��� ������ ��ġ �迭�� �������� ���� ��� ��� �޽��� ���
        if (warningLaserPositions == null || warningLaserPositions.Length < 5)
        {
            Debug.LogWarning("MidBoss: ��� ������ ��ġ �迭�� �ùٸ��� �������� �ʾҽ��ϴ�.");
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
            // ExecutePatterns �ڷ�ƾ�� �����ϰ� ������ �����մϴ�.
            executePatternsCoroutine = StartCoroutine(ExecutePatterns());
            Debug.Log("���� ���Ͱ� ������ �����մϴ�.");
        }
        else
        {
            // ���� �Ұ��� �� �ڷ�ƾ ����
            if (executePatternsCoroutine != null)
            {
                StopCoroutine(executePatternsCoroutine);
                executePatternsCoroutine = null;
                Debug.Log("���� ������ ������ �����Ǿ����ϴ�.");
            }
        }
    }

    public override void TakeDamage(int damage, Vector3 damageSourcePosition, bool Nun = false)
    {
        base.TakeDamage(damage, damageSourcePosition);

        Debug.Log($"�߰� ������ �������� �Ծ����ϴ�! ���� ü��: {currentHP}/{maxHealth}");

        if (playerUIManager != null)
        {
            playerUIManager.UpdateBossHealth(currentHP);
        }
        else
        {
            Debug.LogWarning("MidBoss: PlayerUIManager�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    protected override void Die()
    {
        if (isDead) return;

        // ������ ���� �� ���� ���� �ڷ�ƾ�� �����մϴ�.
        if (executePatternsCoroutine != null)
        {
            StopCoroutine(executePatternsCoroutine);
            executePatternsCoroutine = null;
            Debug.Log("���� ������ ���� ������ �����Ǿ����ϴ�.");
        }

        base.Die();
        gameManager.ShowGameResultPanelTest();
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
            StartCoroutine(PlayDeathAnimationsCoroutine());
        }
        else
        {
            Debug.LogError("SlothMapManager�� �Ҵ���� �ʾҽ��ϴ�.");
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
            if (isDead)
            {
                yield break;
            }

            // �� Ȯ�� ��� (������ ���� Ȯ�� ����)
            float totalProbability = patternData.bulletPatternProbability + patternData.warningAttackPatternProbability +
                                     patternData.warningLaserPatternProbability + patternData.groundSmashPatternProbability;

            float randomValue = UnityEngine.Random.Range(0f, totalProbability);

            if (randomValue < patternData.bulletPatternProbability)
            {
                yield return StartCoroutine(BulletPattern());
            }
            else if (randomValue < patternData.bulletPatternProbability + patternData.warningAttackPatternProbability)
            {
                yield return StartCoroutine(WarningAttackPattern());
            }
            else if (randomValue < patternData.bulletPatternProbability + patternData.warningAttackPatternProbability + patternData.warningLaserPatternProbability)
            {
                yield return StartCoroutine(WarningLaserPattern());
            }
            else
            {
                yield return StartCoroutine(GroundSmashPattern());
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator WarningLaserPattern()
    {
        Debug.Log("��� ������ ���� ����");

        if (warningLaserPositions == null || warningLaserPositions.Length < 5)
        {
            Debug.LogError("��� ������ ��ġ�� ����� �������� �ʾҽ��ϴ�.");
            yield break;
        }

        // 5���� ��ġ �� 4���� �����ϰ� ����
        List<Transform> positionsList = warningLaserPositions.ToList();
        List<Transform> selectedPositions = new List<Transform>();

        for (int i = 0; i < 4; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, positionsList.Count);
            selectedPositions.Add(positionsList[randomIndex]);
            positionsList.RemoveAt(randomIndex);
        }

        // ���õ� ��ġ���� ������� ��� ǥ��
        foreach (Transform position in selectedPositions)
        {
            // ��� ������ �ν��Ͻ�ȭ
            GameObject warning = Instantiate(patternData.warningLaserWarningPrefab, position.position, Quaternion.identity, patternParent);
            Destroy(warning, patternData.warningLaserWarningDuration);

            // ��� ���� �ð���ŭ ���
            yield return new WaitForSeconds(patternData.warningLaserWarningDuration);
        }

        // ��� ǥ�� ������� ������ ���� ����
        foreach (Transform position in selectedPositions)
        {
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

        Debug.Log("��� ������ ���� �Ϸ�");
    }

    // ������ ���� ����
    /*
    private IEnumerator LaserPattern()
    {
        // �ش� ������ ���ŵǾ����ϴ�.
    }
    */

    private IEnumerator BulletPattern()
    {
        Debug.Log("ź�� ���� ����");
        for (int i = 0; i < patternData.bulletPatternRepeatCount; i++)
        {
            FireBullets();
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
            Debug.LogError("MidBoss: źȯ ���� ����Ʈ �迭�� �������� �ʾҽ��ϴ�.");
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
            else
            {
                Debug.LogError("Bullet prefab�� Bullet ������Ʈ�� �����ϴ�.");
            }
        }

        // ���� ���� ����Ʈ�� �ε��� ������Ʈ (��ȯ)
        currentSpawnPointIndex = (currentSpawnPointIndex + 1) % bulletSpawnPoints.Length;
    }

    private IEnumerator WarningAttackPattern()
    {
        for (int i = 0; i < patternData.warningAttackRepeatCount; i++)
        {
            Vector3 targetPosition = GetPlayerPosition();

            // 1. ��� ����Ʈ ����
            if (patternData.warningEffectPrefab != null)
            {
                GameObject warning = Instantiate(patternData.warningEffectPrefab, targetPosition, Quaternion.identity, patternParent);
                Destroy(warning, patternData.warningDuration);
            }
            else
            {
                Debug.LogError("Warning Effect Prefab�� �Ҵ���� �ʾҽ��ϴ�.");
            }

            // 3. ��� ����Ʈ ���� �ð���ŭ ���
            yield return new WaitForSeconds(patternData.warningDuration);

            // 4. ���� ����Ʈ ���� �� ������ ����
            if (patternData.attackEffectPrefab != null)
            {
                GameObject attackEffect = Instantiate(patternData.attackEffectPrefab, targetPosition, Quaternion.identity, patternParent);
                Destroy(attackEffect, patternData.attackEffectDuration);

                // ���� ����Ʈ�� DamageArea ������Ʈ�� �߰��Ͽ� �������� �����մϴ�.
                DamageArea damageArea = attackEffect.AddComponent<DamageArea>();
                damageArea.damage = patternData.warningAttackDamage;
                damageArea.duration = patternData.attackEffectDuration;
                damageArea.isContinuous = true;
            }
            else
            {
                Debug.LogError("Attack Effect Prefab�� �Ҵ���� �ʾҽ��ϴ�.");
            }

            // 5. ���� �ݺ����� ���
            yield return new WaitForSeconds(patternData.warningStartInterval);
        }

        Debug.Log("��� �� ���� ���� �Ϸ�");
    }

    private IEnumerator GroundSmashPattern()
    {
        Debug.Log("�ٴ� ��� ���� ����");

        Vector3 targetPosition = GetPlayerPosition();
        GameObject warning = Instantiate(patternData.groundSmashWarningPrefab, targetPosition, Quaternion.identity, patternParent);
        Destroy(warning, patternData.groundSmashWarningDuration);
        yield return new WaitForSeconds(patternData.groundSmashWarningDuration);

        // ���ο� �ٴ� ��� ���� ����
        SpawnGroundSmashObjects(targetPosition);

        yield return new WaitForSeconds(patternData.groundSmashCooldown);
    }

    private void SpawnGroundSmashObjects(Vector3 targetPosition)
    {
        for (int i = 0; i < patternData.groundSmashMeteorCount; i++)
        {
            float angleRad = patternData.groundSmashAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0) * patternData.groundSmashSpawnRadius;
            Vector3 spawnPosition = targetPosition + offset;

            Quaternion rotation = Quaternion.Euler(0f, 0f, patternData.groundSmashObjectRotation);

            GameObject groundSmashObject = Instantiate(patternData.groundSmashMeteorPrefab, spawnPosition, rotation, patternParent);

            SpriteRenderer spriteRenderer = groundSmashObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("groundSmashMeteorPrefab�� SpriteRenderer�� �����ϴ�.");
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
        if (patternData.groundSmashBulletPrefab == null)
        {
            Debug.LogError("groundSmashBulletPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        int bulletCount = patternData.groundSmashBulletCount;
        float bulletSpeed = patternData.groundSmashBulletSpeed;
        int bulletDamage = patternData.groundSmashBulletDamage;

        for (int i = 0; i < bulletCount; i++)
        {
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
                else
                {
                    Debug.LogError("groundSmashBulletPrefab�� Rigidbody2D�� �����ϴ�.");
                }
            }
            else
            {
                Debug.LogError("groundSmashBulletPrefab�� Bullet ������Ʈ�� �����ϴ�.");
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
}
