using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Linq;

public class MidBoss : MonoBehaviour
{
    [Header("Mid-Boss Settings")]
    [InspectorName("�ִ� ü��")]
    public int maxHealth = 100;
    private int currentHP;

    [Header("���� ������")]
    [InspectorName("���� ������")]
    public BossPatternData patternData;

    [SerializeField, InspectorName("���� ���� ����")]
    private bool isAttackable = false;

    [SerializeField, InspectorName("���� �θ� Transform")]
    private Transform patternParent;

    [Header("������ �� ���� ����")]
    [SerializeField, InspectorName("���� ����")]
    private bool isInvincible = false;

    [SerializeField, InspectorName("��� ����")]
    private bool isDead = false;

    [SerializeField, InspectorName("���� ���� �ð�")]
    private float invincibilityDuration = 0.2f;

    [SerializeField, InspectorName("������ ����")]
    private float blinkInterval = 0.1f;

    [SerializeField, InspectorName("Mesh Renderer")]
    private MeshRenderer meshRenderer;

    [SerializeField, InspectorName("������ �ؽ�Ʈ ������ ���")]
    private string damageTextPrefabPath = "DamageTextPrefab";

    [SerializeField, InspectorName("Rigidbody2D")]
    private Rigidbody2D rb;

    [SerializeField, InspectorName("�÷��̾�")]
    private Player player;

    [Header("UI Manager")]
    [SerializeField, InspectorName("PlayerUIManager")]
    private PlayerUIManager playerUIManager;

    private SlothMapManager slothMapManager;

    private void Start()
    {
        currentHP = maxHealth;
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

        meshRenderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody2D>();

        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("�÷��̾� ������Ʈ�� ã�� �� �����ϴ�.");
        }

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
            StartCoroutine(ExecutePatterns());
            Debug.Log("���� ���Ͱ� ������ �����մϴ�.");
        }
    }

    /// <summary>
    /// �������� �Ծ��� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="damage">���� ������ ��</param>
    /// <param name="damageSourcePosition">�������� �� ��ġ</param>
    public void TakeDamage(int damage, Vector3 damageSourcePosition)
    {
        if (isDead || isInvincible)
            return;

        ShowDamageText(damage);
        ApplyKnockback(damageSourcePosition);

        currentHP -= damage;
        Debug.Log($"�߰� ������ �������� �Ծ����ϴ�! ���� ü��: {currentHP}/{maxHealth}");

        if (playerUIManager != null)
        {
            playerUIManager.UpdateBossHealth(currentHP);
        }
        else
        {
            Debug.LogWarning("MidBoss: PlayerUIManager�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    private void ShowDamageText(int damage)
    {
        StartCoroutine(ShowDamageTextCoroutine(damage));
    }

    private IEnumerator ShowDamageTextCoroutine(int damage)
    {
        // ���� ������ �ؽ�Ʈ ����
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

        // ������ �ؽ�Ʈ ��ġ ���� (���� �Ӹ� ��)
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
            // ������ �翡 ���� ���� ũ�� ����
            int fontSize;
            if (damage >= 100)
            {
                fontSize = 40;
            }
            else if (damage >= 50)
            {
                fontSize = 32;
            }
            else
            {
                fontSize = 18;
            }

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

        // ������ �ؽ�Ʈ�� ��������� ��� (��: 1�� �� ����)
        yield return new WaitForSeconds(1f);
        Destroy(damageTextInstance);
    }

    private void ApplyKnockback(Vector3 damageSourcePosition)
    {
        if (rb != null)
        {
            Vector2 knockbackDirection = (transform.position - damageSourcePosition).normalized;
            float knockbackForce = 2f;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogWarning("Rigidbody2D�� ������ �����ϴ�.");
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
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
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("�߰� ������ ���������ϴ�!");

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
            float randomValue = UnityEngine.Random.Range(0f, 100f);

            if (randomValue < patternData.bulletPatternProbability)
            {
                yield return StartCoroutine(BulletPattern());
            }
            else if (randomValue < patternData.bulletPatternProbability + patternData.warningAttackPatternProbability)
            {
                yield return StartCoroutine(WarningAttackPattern());
            }
            else if (randomValue < patternData.bulletPatternProbability + patternData.warningAttackPatternProbability + patternData.laserPatternProbability)
            {
                yield return StartCoroutine(LaserPattern());
            }
            else
            {
                yield return StartCoroutine(GroundSmashPattern());
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator BulletPattern()
    {
        Debug.Log("ź�� ���� ����");
        for (int i = 0; i < patternData.bulletPatternRepeatCount; i++)
        {
            FireBullets(transform.position);
            yield return new WaitForSeconds(patternData.bulletFireInterval);
        }
    }

    private void FireBullets(Vector3 spawnPosition)
    {
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

            // 2. ��� ����Ʈ ���� �ð���ŭ ���
            yield return new WaitForSeconds(patternData.warningDuration);

            // 3. ���� ����Ʈ ���� �� ������ ����
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

            // 4. ���� �ݺ����� ���
            yield return new WaitForSeconds(patternData.warningStartInterval);
        }

        Debug.Log("��� �� ���� ���� �Ϸ�");
    }

    private IEnumerator LaserPattern()
    {
        Debug.Log("������ ���� ����");
        Vector3 laserPosition = transform.position + Vector3.down * 4f;
        GameObject warning = Instantiate(patternData.laserWarningPrefab, laserPosition, Quaternion.identity, patternParent);
        Destroy(warning, patternData.laserWarningDuration);
        yield return new WaitForSeconds(patternData.laserWarningDuration);

        GameObject laser = Instantiate(patternData.laserPrefab, laserPosition, Quaternion.identity, patternParent);
        Destroy(laser, patternData.laserDuration);

        DamageArea damageArea = laser.AddComponent<DamageArea>();
        damageArea.damage = patternData.laserDamagePerSecond;
        damageArea.duration = patternData.laserDuration;
        damageArea.isContinuous = true;
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
            // ���� �����Ϳ��� ������ ������ ���� �ݰ��� ����Ͽ� ���� ��ġ ���
            float angleRad = patternData.groundSmashAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0) * patternData.groundSmashSpawnRadius;
            Vector3 spawnPosition = targetPosition + offset;

            // ���� �����Ϳ��� ������ ������ ȸ�� ����
            Quaternion rotation = Quaternion.Euler(0f, 0f, patternData.groundSmashObjectRotation);

            // �ٴ� ��� ������Ʈ ����
            GameObject groundSmashObject = Instantiate(patternData.groundSmashMeteorPrefab, spawnPosition, rotation, patternParent);

            // �ٴ� ��� ������Ʈ�� SpriteRenderer�� �ִ��� Ȯ��
            SpriteRenderer spriteRenderer = groundSmashObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("groundSmashMeteorPrefab�� SpriteRenderer�� �����ϴ�.");
                Destroy(groundSmashObject);
                continue;
            }

            // �ٴ� ��� ������Ʈ�� �̵� �ӵ� ����
            float fallSpeed = patternData.groundSmashMeteorFallSpeed;

            // �ٴ� ��� ������Ʈ�� Ÿ�� ��ġ�� �����ϸ� ���ظ� �ְ�, ���� �ö󰡵��� ����
            StartCoroutine(FallAndApplyDamage(groundSmashObject, targetPosition, fallSpeed));
        }
    }

    private IEnumerator FallAndApplyDamage(GameObject groundSmashObject, Vector3 targetPosition, float fallSpeed)
    {
        while (groundSmashObject != null)
        {
            // ������Ʈ�� Ÿ�� ��ġ�� ������ ������ �Ʒ��� �̵�
            if (Vector3.Distance(groundSmashObject.transform.position, targetPosition) > 0.1f)
            {
                groundSmashObject.transform.position = Vector3.MoveTowards(groundSmashObject.transform.position, targetPosition, fallSpeed * Time.deltaTime);
                yield return null;
            }
            else
            {
                // Ÿ�� ��ġ�� �������� �� ���ظ� �ֱ�
                ApplyGroundSmashDamage(targetPosition);

                yield return new WaitForSeconds(0.5f);
                // �ٴ� ��� ������Ʈ�� ���� �̵���Ű�� ����
                StartCoroutine(MoveUpAndDestroy(groundSmashObject, patternData.groundSmashSpeed, 3f));

                yield break;
            }
        }
    }

    private void ApplyGroundSmashDamage(Vector3 position)
    {
        // �浹 �ݰ� ���� ���鿡�� ���� ����
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, patternData.groundSmashMeteorRadius);
        foreach (Collider2D hit in hits)
        {
            Monster monster = hit.GetComponent<Monster>();
            if (monster != null && !monster.IsDead)
            {
                monster.TakeDamage(patternData.groundSmashDamage, position);
                // �߰� ȿ���� ����Ʈ ���� ����
            }
        }

        // �ٴ� ��� ���� �� �ֺ����� źȯ �߻�
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

            // źȯ ���� ��ġ (�ٴ� ��� ������ ���� ��ġ)
            Vector3 spawnPosition = position;

            // źȯ ȸ�� ����
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            // źȯ ����
            GameObject bullet = Instantiate(patternData.groundSmashBulletPrefab, spawnPosition, rotation, patternParent);

            // Bullet ��ũ��Ʈ ����
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
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            return playerObj.transform.position;
        }
        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (patternData != null)
        {
            // Ground Smash �浹 �ݰ� �ð�ȭ
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetPlayerPosition(), patternData.groundSmashMeteorRadius);

            // Ground Smash źȯ �߻� ���� �ð�ȭ
            Gizmos.color = Color.blue;
            float angleStep = 360f / patternData.groundSmashBulletCount;
            for (int i = 0; i < patternData.groundSmashBulletCount; i++)
            {
                float angle = i * angleStep;
                float angleRad = angle * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0).normalized;
                Gizmos.DrawLine(GetPlayerPosition(), GetPlayerPosition() + direction * 2f);
            }
        }
    }
}
