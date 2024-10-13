using UnityEngine;
using System.Collections;
using TMPro;

public class MidBoss : MonoBehaviour
{
    [Header("Mid-Boss Settings")]
    [Tooltip("�߰� ������ �ִ� ü��")]
    public int maxHealth = 100;
    private int currentHP;

    [Header("���� ������")]
    public BossPatternData patternData;

    private bool isAttackable = false; // ��� ���̺갡 ������ ���� ����

    // ���� ������Ʈ���� �θ�
    private Transform patternParent;

    // ������ �� ���� ���� ����
    private bool isInvincible = false;
    private bool isDead = false;
    private float invincibilityDuration = 0.5f;
    private float blinkInterval = 0.1f;
    private MeshRenderer meshRenderer;

    // ������ �ؽ�Ʈ�� ���� ����
    private string damageTextPrefabPath = "DamageTextPrefab";

    private Rigidbody2D rb;
    private Player player;

    [Header("Attack Damage Values")]
    public int bulletDamage = 10;
    public int warningAttackDamage = 20;
    public int laserDamagePerSecond = 5;
    public int groundSmashDamage = 15;

    private void Start()
    {
        // ���� ü���� �����մϴ�.
        currentHP = maxHealth;
        Debug.Log($"�߰� ���� ����! ü��: {currentHP}/{maxHealth}");

        // ���� ������Ʈ���� �θ� ����
        patternParent = new GameObject("BossPatterns").transform;

        // �ʱ⿡�� ���� �Ұ���
        isAttackable = false;

        // ������Ʈ �ʱ�ȭ
        meshRenderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody2D>();

        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("�÷��̾� ������Ʈ�� ã�� �� �����ϴ�.");
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
        // StartCoroutine�� ���� �ڷ�ƾ ����
        StartCoroutine(ShowDamageTextCoroutine(damage));
    }

    private IEnumerator ShowDamageTextCoroutine(int damage)
    {
        // ������ ���� Ƚ�� �� ��Ʈ ũ�� ����
        int splitCount = 1;
        int fontSize = 18;

        if (damage >= 100)
        {
            splitCount = 5;
            fontSize = 40;
        }
        else if (damage >= 50)
        {
            splitCount = 3;
            fontSize = 32;
        }

        // �� ���ҵ� ������ ���
        int splitDamage = Mathf.CeilToInt((float)damage / splitCount);

        // �Ӹ� ��ġ ��� (���� ��ǥ)
        float monsterHeightOffset = 1.0f; // ���� �Ӹ� ���� �󸶳� ����� ���� ������ ��
        Vector3 headPosition = transform.position + new Vector3(0, monsterHeightOffset, 0);

        for (int i = 0; i < splitCount; i++)
        {
            // DamageText ������ �ε�
            GameObject damageTextPrefab = Resources.Load<GameObject>(damageTextPrefabPath);
            if (damageTextPrefab == null)
            {
                Debug.LogError("DamageTextPrefab�� Resources �������� ã�� �� �����ϴ�.");
                yield break;
            }

            // �±׷� ĵ������ ã���ϴ�.
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

            // ĵ������ Render Camera ���� Ȯ�� �� ��������
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

            // �Ӹ� ��ġ�� ��ũ�� ��ǥ�� ��ȯ (�������� ���� �̵�)
            // �� �ؽ�Ʈ���� Y������ 20�ȼ��� ���� �̵�
            Vector3 offset = new Vector3(0, i * 20f, 0);
            Vector3 screenPosition = uiCamera.WorldToScreenPoint(headPosition + offset);

            // ��ũ�� ��ǥ�� ĵ���� ���� ��ǥ�� ��ȯ
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                screenCanvas.transform as RectTransform,
                screenPosition,
                uiCamera,
                out Vector2 localPoint);

            // ������ �ؽ�Ʈ �ν��Ͻ� ���� �� �θ� ����
            GameObject damageTextInstance = Instantiate(damageTextPrefab, screenCanvas.transform);

            // RectTransform�� ���� ��ǥ ����
            RectTransform rectTransform = damageTextInstance.GetComponent<RectTransform>();
            rectTransform.localPosition = localPoint;

            // ������ �ؽ�Ʈ ����
            DamageText damageText = damageTextInstance.GetComponent<DamageText>();
            if (damageText != null)
            {
                // ��Ʈ�� ���� ũ�� ���� (���ϴ� ��Ʈ�� ���� ����)
                TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Maplestory Light SDF");
                if (font == null)
                {
                    Debug.LogWarning("��Ʈ ������ ã�� �� �����ϴ�. �⺻ ��Ʈ�� ����մϴ�.");
                }

                Color color = Color.white;

                // ��Ʈ ũ�� ����
                damageText.SetText(splitDamage.ToString(), font, fontSize, color);
            }
            else
            {
                Debug.LogError("DamageText ������Ʈ�� ã�� �� �����ϴ�.");
            }

            // 0.05�� ��� �� ���� �ؽ�Ʈ ����
            yield return new WaitForSeconds(0.05f);
        }
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
            Debug.LogWarning("Rigidbody2D�� ������ �����ϴ�.");
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

    /// <summary>
    /// �߰� ������ ������� �� ȣ��˴ϴ�.
    /// </summary>
    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("�߰� ������ ���������ϴ�!");
        // ��� ó�� ���� �߰� (�ִϸ��̼�, ��� ������ ��)
        Destroy(gameObject);
    }

    // ���� ���� ���� �޼��� (ExecutePatterns, BulletPattern ��) ���Ͽ� ������ ����� ����
    /// <summary>
    /// ������ �����ϰ� �����մϴ�.
    /// </summary>
    private IEnumerator ExecutePatterns()
    {
        while (true)
        {
            float randomValue = Random.Range(0f, 100f);

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
                yield return StartCoroutine(GroundSmashPattern()); // �ٴ� ��� ���� ����
            }

            // ���� ������ ������
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// ź�� ������ �����մϴ�.
    /// </summary>
    private IEnumerator BulletPattern()
    {
        Debug.Log("ź�� ���� ����");
        for (int i = 0; i < patternData.bulletPatternRepeatCount; i++)
        {
            // ���� ��ġ���� źȯ �߻�
            FireBullets(transform.position);
            yield return new WaitForSeconds(patternData.bulletFireInterval);
        }
    }

    private void FireBullets(Vector3 spawnPosition)
    {
        // �÷��̾� ��ġ�� ���� ����ó�� 3�� �߻�
        Vector3 playerPosition = GetPlayerPosition();
        Vector3 direction = (playerPosition - spawnPosition).normalized;

        float spreadAngle = 15f; // ���� ���� ����
        int bulletCount = 3;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = (-spreadAngle) + (spreadAngle * i);
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 bulletDirection = rotation * direction;

            GameObject bullet = Instantiate(patternData.bulletPrefab, spawnPosition, Quaternion.identity, patternParent);

            // Bullet ������Ʈ ����
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetAttackDamage(bulletDamage);
                // Rigidbody2D�� velocity ����
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.velocity = bulletDirection * 5f;
                }
            }
            else
            {
                Debug.LogError("Bullet prefab�� Bullet ������Ʈ�� �����ϴ�.");
            }
        }
    }

    /// <summary>
    /// ��� �� ���� ������ �����մϴ�.
    /// </summary>
    private IEnumerator WarningAttackPattern()
    {
        Debug.Log("��� �� ���� ���� ����");

        for (int i = 0; i < patternData.warningAttackRepeatCount; i++)
        {
            Vector3 targetPosition = GetPlayerPosition();

            // ��� ����Ʈ ����
            GameObject warning = Instantiate(patternData.warningEffectPrefab, targetPosition, Quaternion.identity, patternParent);
            Destroy(warning, patternData.warningDuration);

            // ���� ����
            StartCoroutine(DelayedAttack(targetPosition, patternData.warningDuration));

            // ���� �������� ���� (warningStartInterval)
            yield return new WaitForSeconds(patternData.warningStartInterval);
        }
    }

    /// <summary>
    /// ������ ������ �����մϴ�.
    /// </summary>
    /// <param name="position">������ ��ġ</param>
    /// <param name="delay">���� �ð�</param>
    private IEnumerator DelayedAttack(Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);

        // ���� ����Ʈ ����
        GameObject attackEffect = Instantiate(patternData.attackEffectPrefab, position, Quaternion.identity, patternParent);
        Destroy(attackEffect, patternData.attackEffectDuration);

        // ���� ����Ʈ�� DamageArea ��ũ��Ʈ�� �߰��Ͽ� �÷��̾�� �������� ���� �� �ֵ��� �մϴ�.
        DamageArea damageArea = attackEffect.AddComponent<DamageArea>();
        damageArea.damage = warningAttackDamage;
        damageArea.duration = patternData.attackEffectDuration;
        damageArea.isContinuous = false; // ��ȸ�� ������ ����
    }

    /// <summary>
    /// ������ ������ �����մϴ�.
    /// </summary>
    private IEnumerator LaserPattern()
    {
        Debug.Log("������ ���� ����");

        // �������� ��Ÿ�� ��ġ�� ���� ��ġ�� �������� ����
        Vector3 laserPosition = transform.position + Vector3.right * 2f;

        // ������ ��� ǥ�� ����
        GameObject warning = Instantiate(patternData.laserWarningPrefab, laserPosition, Quaternion.identity, patternParent);
        Destroy(warning, patternData.laserWarningDuration);

        yield return new WaitForSeconds(patternData.laserWarningDuration);

        // ������ Ȱ��ȭ
        GameObject laser = Instantiate(patternData.laserPrefab, laserPosition, Quaternion.identity, patternParent);
        Destroy(laser, patternData.laserDuration);

        // �������� DamageArea ��ũ��Ʈ�� �߰��Ͽ� �������� �������� ���� �� �ֵ��� �մϴ�.
        DamageArea damageArea = laser.AddComponent<DamageArea>();
        damageArea.damage = laserDamagePerSecond;
        damageArea.duration = patternData.laserDuration;
        damageArea.isContinuous = true; // �������� ������ ����
    }

    /// <summary>
    /// �ٴ� ��� ������ �����մϴ�.
    /// </summary>
    private IEnumerator GroundSmashPattern()
    {
        Debug.Log("�ٴ� ��� ���� ����");

        // �÷��̾� ��ġ�� ��� ǥ�� ����
        Vector3 targetPosition = GetPlayerPosition();
        GameObject warning = Instantiate(patternData.groundSmashWarningPrefab, targetPosition, Quaternion.identity, patternParent);
        Destroy(warning, patternData.groundSmashWarningDuration);

        yield return new WaitForSeconds(patternData.groundSmashWarningDuration);

        // ��� �� ������� ����Ʈ ����
        GameObject smashEffect = Instantiate(patternData.groundSmashEffectPrefab, targetPosition, Quaternion.identity, patternParent);
        Destroy(smashEffect, patternData.groundSmashEffectDuration);

        // ������� ����Ʈ�� DamageArea ��ũ��Ʈ�� �߰��Ͽ� �������� ���� �� �ֵ��� �մϴ�.
        DamageArea damageArea = smashEffect.AddComponent<DamageArea>();
        damageArea.damage = groundSmashDamage;
        damageArea.duration = patternData.groundSmashEffectDuration;
        damageArea.isContinuous = false; // ��ȸ�� ������ ����

        // ź�� �߻�
        FireRadialBullets(targetPosition);

        yield return new WaitForSeconds(patternData.groundSmashCooldown);
    }

    private void FireRadialBullets(Vector3 spawnPosition)
    {
        int bulletCount = patternData.groundSmashBulletCount;
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
            Vector3 bulletDirection = rotation * Vector3.right;

            GameObject bullet = Instantiate(patternData.bulletPrefab, spawnPosition, Quaternion.identity, patternParent);

            // Bullet ������Ʈ ����
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetAttackDamage(bulletDamage);
                // Rigidbody2D�� velocity ����
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.velocity = bulletDirection * patternData.groundSmashBulletSpeed;
                }
            }
            else
            {
                Debug.LogError("Bullet prefab�� Bullet ������Ʈ�� �����ϴ�.");
            }
        }
    }

    /// <summary>
    /// �÷��̾��� ���� ��ġ�� ��ȯ�մϴ�.
    /// </summary>
    private Vector3 GetPlayerPosition()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            return playerObj.transform.position;
        }
        return Vector3.zero;
    }
}
