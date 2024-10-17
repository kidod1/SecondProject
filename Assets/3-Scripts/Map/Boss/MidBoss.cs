using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Linq;

public class MidBoss : MonoBehaviour
{
    [Header("Mid-Boss Settings")]
    [InspectorName("최대 체력")]
    public int maxHealth = 100;
    private int currentHP;

    [Header("패턴 데이터")]
    [InspectorName("패턴 데이터")]
    public BossPatternData patternData;

    [SerializeField, InspectorName("공격 가능 여부")]
    private bool isAttackable = false;

    [SerializeField, InspectorName("패턴 부모 Transform")]
    private Transform patternParent;

    [Header("데미지 및 무적 설정")]
    [SerializeField, InspectorName("무적 여부")]
    private bool isInvincible = false;

    [SerializeField, InspectorName("사망 여부")]
    private bool isDead = false;

    [SerializeField, InspectorName("무적 지속 시간")]
    private float invincibilityDuration = 0.2f;

    [SerializeField, InspectorName("깜빡임 간격")]
    private float blinkInterval = 0.1f;

    [SerializeField, InspectorName("Mesh Renderer")]
    private MeshRenderer meshRenderer;

    [SerializeField, InspectorName("데미지 텍스트 프리팹 경로")]
    private string damageTextPrefabPath = "DamageTextPrefab";

    [SerializeField, InspectorName("Rigidbody2D")]
    private Rigidbody2D rb;

    [SerializeField, InspectorName("플레이어")]
    private Player player;

    [Header("UI Manager")]
    [SerializeField, InspectorName("PlayerUIManager")]
    private PlayerUIManager playerUIManager;

    private SlothMapManager slothMapManager;

    private void Start()
    {
        currentHP = maxHealth;
        Debug.Log($"중간 보스 등장! 체력: {currentHP}/{maxHealth}");

        if (playerUIManager != null)
        {
            playerUIManager.InitializeBossHealth(maxHealth);
        }
        else
        {
            Debug.LogError("MidBoss: PlayerUIManager가 할당되지 않았습니다.");
        }

        patternParent = new GameObject("BossPatterns").transform;
        isAttackable = false;

        meshRenderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody2D>();

        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("플레이어 오브젝트를 찾을 수 없습니다.");
        }

        if (patternData == null)
        {
            Debug.LogError("MidBoss: BossPatternData가 할당되지 않았습니다.");
        }

        if (patternData != null && slothMapManager == null)
        {
            slothMapManager = FindObjectsOfType<SlothMapManager>().FirstOrDefault();
            if (slothMapManager == null)
            {
                Debug.LogError("SlothMapManager를 찾을 수 없습니다.");
            }
        }
    }

    /// <summary>
    /// 보스가 공격 가능 상태로 설정합니다.
    /// </summary>
    /// <param name="value">공격 가능 여부</param>
    public void SetAttackable(bool value)
    {
        isAttackable = value;
        if (isAttackable)
        {
            StartCoroutine(ExecutePatterns());
            Debug.Log("보스 몬스터가 공격을 시작합니다.");
        }
    }

    /// <summary>
    /// 데미지를 입었을 때 호출됩니다.
    /// </summary>
    /// <param name="damage">입은 데미지 양</param>
    /// <param name="damageSourcePosition">데미지를 준 위치</param>
    public void TakeDamage(int damage, Vector3 damageSourcePosition)
    {
        if (isDead || isInvincible)
            return;

        ShowDamageText(damage);
        ApplyKnockback(damageSourcePosition);

        currentHP -= damage;
        Debug.Log($"중간 보스가 데미지를 입었습니다! 남은 체력: {currentHP}/{maxHealth}");

        if (playerUIManager != null)
        {
            playerUIManager.UpdateBossHealth(currentHP);
        }
        else
        {
            Debug.LogWarning("MidBoss: PlayerUIManager가 할당되지 않았습니다.");
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
        // 단일 데미지 텍스트 생성
        GameObject damageTextPrefab = Resources.Load<GameObject>(damageTextPrefabPath);
        if (damageTextPrefab == null)
        {
            Debug.LogError("DamageTextPrefab을 Resources 폴더에서 찾을 수 없습니다.");
            yield break;
        }

        GameObject canvasObject = GameObject.FindGameObjectWithTag("DamageCanvas");
        if (canvasObject == null)
        {
            Debug.LogError("'DamageCanvas' 태그를 가진 캔버스를 찾을 수 없습니다.");
            yield break;
        }

        Canvas screenCanvas = canvasObject.GetComponent<Canvas>();
        if (screenCanvas == null)
        {
            Debug.LogError("'DamageCanvas' 오브젝트에서 Canvas 컴포넌트를 찾을 수 없습니다.");
            yield break;
        }

        Camera uiCamera = screenCanvas.worldCamera;
        if (uiCamera == null)
        {
            uiCamera = Camera.main;
        }

        if (uiCamera == null)
        {
            Debug.LogError("UI 카메라를 찾을 수 없습니다.");
            yield break;
        }

        // 데미지 텍스트 위치 설정 (보스 머리 위)
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
            // 데미지 양에 따라 글자 크기 설정
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
                Debug.LogWarning("폰트 에셋을 찾을 수 없습니다. 기본 폰트를 사용합니다.");
            }

            Color color = Color.white;
            damageText.SetText(damage.ToString(), font, fontSize, color);
        }
        else
        {
            Debug.LogError("DamageText 컴포넌트를 찾을 수 없습니다.");
        }

        // 데미지 텍스트가 사라지도록 대기 (예: 1초 후 제거)
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
            Debug.LogWarning("Rigidbody2D가 보스에 없습니다.");
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
        Debug.Log("중간 보스가 쓰러졌습니다!");

        if (playerUIManager != null)
        {
            playerUIManager.UpdateBossHealth(0);
            playerUIManager.HideBossHealthUI(); // 보스 체력 UI 패널 비활성화
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
            Debug.LogError("SlothMapManager가 할당되지 않았습니다.");
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
        Debug.Log("탄막 패턴 시작");
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
                Debug.LogError("Bullet prefab에 Bullet 컴포넌트가 없습니다.");
            }
        }
    }

    private IEnumerator WarningAttackPattern()
    {
        for (int i = 0; i < patternData.warningAttackRepeatCount; i++)
        {
            Vector3 targetPosition = GetPlayerPosition();

            // 1. 경고 이펙트 생성
            if (patternData.warningEffectPrefab != null)
            {
                GameObject warning = Instantiate(patternData.warningEffectPrefab, targetPosition, Quaternion.identity, patternParent);
                Destroy(warning, patternData.warningDuration);
            }
            else
            {
                Debug.LogError("Warning Effect Prefab이 할당되지 않았습니다.");
            }

            // 2. 경고 이펙트 지속 시간만큼 대기
            yield return new WaitForSeconds(patternData.warningDuration);

            // 3. 공격 이펙트 생성 및 데미지 적용
            if (patternData.attackEffectPrefab != null)
            {
                GameObject attackEffect = Instantiate(patternData.attackEffectPrefab, targetPosition, Quaternion.identity, patternParent);
                Destroy(attackEffect, patternData.attackEffectDuration);

                // 공격 이펙트에 DamageArea 컴포넌트를 추가하여 데미지를 적용합니다.
                DamageArea damageArea = attackEffect.AddComponent<DamageArea>();
                damageArea.damage = patternData.warningAttackDamage;
                damageArea.duration = patternData.attackEffectDuration;
                damageArea.isContinuous = true;
            }
            else
            {
                Debug.LogError("Attack Effect Prefab이 할당되지 않았습니다.");
            }

            // 4. 다음 반복까지 대기
            yield return new WaitForSeconds(patternData.warningStartInterval);
        }

        Debug.Log("경고 후 공격 패턴 완료");
    }

    private IEnumerator LaserPattern()
    {
        Debug.Log("레이저 패턴 시작");
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
        Debug.Log("바닥 찍기 패턴 시작");

        Vector3 targetPosition = GetPlayerPosition();
        GameObject warning = Instantiate(patternData.groundSmashWarningPrefab, targetPosition, Quaternion.identity, patternParent);
        Destroy(warning, patternData.groundSmashWarningDuration);
        yield return new WaitForSeconds(patternData.groundSmashWarningDuration);

        // 새로운 바닥 찍기 패턴 생성
        SpawnGroundSmashObjects(targetPosition);

        yield return new WaitForSeconds(patternData.groundSmashCooldown);
    }

    private void SpawnGroundSmashObjects(Vector3 targetPosition)
    {
        for (int i = 0; i < patternData.groundSmashMeteorCount; i++)
        {
            // 패턴 데이터에서 지정한 각도와 스폰 반경을 사용하여 스폰 위치 계산
            float angleRad = patternData.groundSmashAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0) * patternData.groundSmashSpawnRadius;
            Vector3 spawnPosition = targetPosition + offset;

            // 패턴 데이터에서 지정한 각도로 회전 설정
            Quaternion rotation = Quaternion.Euler(0f, 0f, patternData.groundSmashObjectRotation);

            // 바닥 찍기 오브젝트 생성
            GameObject groundSmashObject = Instantiate(patternData.groundSmashMeteorPrefab, spawnPosition, rotation, patternParent);

            // 바닥 찍기 오브젝트에 SpriteRenderer가 있는지 확인
            SpriteRenderer spriteRenderer = groundSmashObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("groundSmashMeteorPrefab에 SpriteRenderer가 없습니다.");
                Destroy(groundSmashObject);
                continue;
            }

            // 바닥 찍기 오브젝트에 이동 속도 설정
            float fallSpeed = patternData.groundSmashMeteorFallSpeed;

            // 바닥 찍기 오브젝트가 타겟 위치에 도달하면 피해를 주고, 위로 올라가도록 변경
            StartCoroutine(FallAndApplyDamage(groundSmashObject, targetPosition, fallSpeed));
        }
    }

    private IEnumerator FallAndApplyDamage(GameObject groundSmashObject, Vector3 targetPosition, float fallSpeed)
    {
        while (groundSmashObject != null)
        {
            // 오브젝트가 타겟 위치에 도달할 때까지 아래로 이동
            if (Vector3.Distance(groundSmashObject.transform.position, targetPosition) > 0.1f)
            {
                groundSmashObject.transform.position = Vector3.MoveTowards(groundSmashObject.transform.position, targetPosition, fallSpeed * Time.deltaTime);
                yield return null;
            }
            else
            {
                // 타겟 위치에 도달했을 때 피해를 주기
                ApplyGroundSmashDamage(targetPosition);

                yield return new WaitForSeconds(0.5f);
                // 바닥 찍기 오브젝트를 위로 이동시키기 시작
                StartCoroutine(MoveUpAndDestroy(groundSmashObject, patternData.groundSmashSpeed, 3f));

                yield break;
            }
        }
    }

    private void ApplyGroundSmashDamage(Vector3 position)
    {
        // 충돌 반경 내의 적들에게 피해 적용
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, patternData.groundSmashMeteorRadius);
        foreach (Collider2D hit in hits)
        {
            Monster monster = hit.GetComponent<Monster>();
            if (monster != null && !monster.IsDead)
            {
                monster.TakeDamage(patternData.groundSmashDamage, position);
                // 추가 효과나 이펙트 적용 가능
            }
        }

        // 바닥 찍기 공격 시 주변으로 탄환 발사
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
            Debug.LogError("groundSmashBulletPrefab이 할당되지 않았습니다.");
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

            // 탄환 스폰 위치 (바닥 찍기 공격이 터진 위치)
            Vector3 spawnPosition = position;

            // 탄환 회전 설정
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            // 탄환 생성
            GameObject bullet = Instantiate(patternData.groundSmashBulletPrefab, spawnPosition, rotation, patternParent);

            // Bullet 스크립트 설정
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
                    Debug.LogError("groundSmashBulletPrefab에 Rigidbody2D가 없습니다.");
                }
            }
            else
            {
                Debug.LogError("groundSmashBulletPrefab에 Bullet 컴포넌트가 없습니다.");
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
            // Ground Smash 충돌 반경 시각화
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetPlayerPosition(), patternData.groundSmashMeteorRadius);

            // Ground Smash 탄환 발사 각도 시각화
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
