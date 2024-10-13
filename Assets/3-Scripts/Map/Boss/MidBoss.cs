using UnityEngine;
using System.Collections;
using TMPro;

public class MidBoss : MonoBehaviour
{
    [Header("Mid-Boss Settings")]
    [Tooltip("중간 보스의 최대 체력")]
    public int maxHealth = 100;
    private int currentHP;

    [Header("패턴 데이터")]
    public BossPatternData patternData;

    private bool isAttackable = false; // 모든 웨이브가 끝나야 공격 가능

    // 패턴 오브젝트들의 부모
    private Transform patternParent;

    // 데미지 및 무적 관련 변수
    private bool isInvincible = false;
    private bool isDead = false;
    private float invincibilityDuration = 0.5f;
    private float blinkInterval = 0.1f;
    private MeshRenderer meshRenderer;

    // 데미지 텍스트를 위한 변수
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
        // 현재 체력을 설정합니다.
        currentHP = maxHealth;
        Debug.Log($"중간 보스 등장! 체력: {currentHP}/{maxHealth}");

        // 패턴 오브젝트들의 부모 생성
        patternParent = new GameObject("BossPatterns").transform;

        // 초기에는 공격 불가능
        isAttackable = false;

        // 컴포넌트 초기화
        meshRenderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody2D>();

        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("플레이어 오브젝트를 찾을 수 없습니다.");
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
        // StartCoroutine을 통해 코루틴 시작
        StartCoroutine(ShowDamageTextCoroutine(damage));
    }

    private IEnumerator ShowDamageTextCoroutine(int damage)
    {
        // 데미지 분할 횟수 및 폰트 크기 결정
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

        // 각 분할된 데미지 계산
        int splitDamage = Mathf.CeilToInt((float)damage / splitCount);

        // 머리 위치 계산 (월드 좌표)
        float monsterHeightOffset = 1.0f; // 몬스터 머리 위로 얼마나 띄울지 조정 가능한 값
        Vector3 headPosition = transform.position + new Vector3(0, monsterHeightOffset, 0);

        for (int i = 0; i < splitCount; i++)
        {
            // DamageText 프리팹 로드
            GameObject damageTextPrefab = Resources.Load<GameObject>(damageTextPrefabPath);
            if (damageTextPrefab == null)
            {
                Debug.LogError("DamageTextPrefab을 Resources 폴더에서 찾을 수 없습니다.");
                yield break;
            }

            // 태그로 캔버스를 찾습니다.
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

            // 캔버스의 Render Camera 설정 확인 및 가져오기
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

            // 머리 위치를 스크린 좌표로 변환 (직선으로 위로 이동)
            // 각 텍스트마다 Y축으로 20픽셀씩 위로 이동
            Vector3 offset = new Vector3(0, i * 20f, 0);
            Vector3 screenPosition = uiCamera.WorldToScreenPoint(headPosition + offset);

            // 스크린 좌표를 캔버스 로컬 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                screenCanvas.transform as RectTransform,
                screenPosition,
                uiCamera,
                out Vector2 localPoint);

            // 데미지 텍스트 인스턴스 생성 및 부모 설정
            GameObject damageTextInstance = Instantiate(damageTextPrefab, screenCanvas.transform);

            // RectTransform의 로컬 좌표 설정
            RectTransform rectTransform = damageTextInstance.GetComponent<RectTransform>();
            rectTransform.localPosition = localPoint;

            // 데미지 텍스트 설정
            DamageText damageText = damageTextInstance.GetComponent<DamageText>();
            if (damageText != null)
            {
                // 폰트와 글자 크기 설정 (원하는 폰트로 변경 가능)
                TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Maplestory Light SDF");
                if (font == null)
                {
                    Debug.LogWarning("폰트 에셋을 찾을 수 없습니다. 기본 폰트를 사용합니다.");
                }

                Color color = Color.white;

                // 폰트 크기 설정
                damageText.SetText(splitDamage.ToString(), font, fontSize, color);
            }
            else
            {
                Debug.LogError("DamageText 컴포넌트를 찾을 수 없습니다.");
            }

            // 0.05초 대기 후 다음 텍스트 생성
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void ApplyKnockback(Vector3 damageSourcePosition)
    {
        if (rb != null)
        {
            // 노크백 방향 계산 (보스 위치 - 공격자 위치)
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
    /// 중간 보스가 사망했을 때 호출됩니다.
    /// </summary>
    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("중간 보스가 쓰러졌습니다!");
        // 사망 처리 로직 추가 (애니메이션, 드롭 아이템 등)
        Destroy(gameObject);
    }

    // 이하 패턴 관련 메서드 (ExecutePatterns, BulletPattern 등) 패턴에 데미지 적용된 버전
    /// <summary>
    /// 패턴을 랜덤하게 실행합니다.
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
                yield return StartCoroutine(GroundSmashPattern()); // 바닥 찍기 패턴 실행
            }

            // 패턴 사이의 딜레이
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// 탄막 패턴을 실행합니다.
    /// </summary>
    private IEnumerator BulletPattern()
    {
        Debug.Log("탄막 패턴 시작");
        for (int i = 0; i < patternData.bulletPatternRepeatCount; i++)
        {
            // 보스 위치에서 탄환 발사
            FireBullets(transform.position);
            yield return new WaitForSeconds(patternData.bulletFireInterval);
        }
    }

    private void FireBullets(Vector3 spawnPosition)
    {
        // 플레이어 위치를 향해 샷건처럼 3발 발사
        Vector3 playerPosition = GetPlayerPosition();
        Vector3 direction = (playerPosition - spawnPosition).normalized;

        float spreadAngle = 15f; // 샷건 퍼짐 각도
        int bulletCount = 3;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = (-spreadAngle) + (spreadAngle * i);
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 bulletDirection = rotation * direction;

            GameObject bullet = Instantiate(patternData.bulletPrefab, spawnPosition, Quaternion.identity, patternParent);

            // Bullet 컴포넌트 설정
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetAttackDamage(bulletDamage);
                // Rigidbody2D의 velocity 설정
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.velocity = bulletDirection * 5f;
                }
            }
            else
            {
                Debug.LogError("Bullet prefab에 Bullet 컴포넌트가 없습니다.");
            }
        }
    }

    /// <summary>
    /// 경고 후 공격 패턴을 실행합니다.
    /// </summary>
    private IEnumerator WarningAttackPattern()
    {
        Debug.Log("경고 후 공격 패턴 시작");

        for (int i = 0; i < patternData.warningAttackRepeatCount; i++)
        {
            Vector3 targetPosition = GetPlayerPosition();

            // 경고 이펙트 생성
            GameObject warning = Instantiate(patternData.warningEffectPrefab, targetPosition, Quaternion.identity, patternParent);
            Destroy(warning, patternData.warningDuration);

            // 공격 예약
            StartCoroutine(DelayedAttack(targetPosition, patternData.warningDuration));

            // 다음 경고까지의 간격 (warningStartInterval)
            yield return new WaitForSeconds(patternData.warningStartInterval);
        }
    }

    /// <summary>
    /// 지연된 공격을 실행합니다.
    /// </summary>
    /// <param name="position">공격할 위치</param>
    /// <param name="delay">지연 시간</param>
    private IEnumerator DelayedAttack(Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 공격 이펙트 생성
        GameObject attackEffect = Instantiate(patternData.attackEffectPrefab, position, Quaternion.identity, patternParent);
        Destroy(attackEffect, patternData.attackEffectDuration);

        // 공격 이펙트에 DamageArea 스크립트를 추가하여 플레이어에게 데미지를 입힐 수 있도록 합니다.
        DamageArea damageArea = attackEffect.AddComponent<DamageArea>();
        damageArea.damage = warningAttackDamage;
        damageArea.duration = patternData.attackEffectDuration;
        damageArea.isContinuous = false; // 일회성 데미지 적용
    }

    /// <summary>
    /// 레이저 패턴을 실행합니다.
    /// </summary>
    private IEnumerator LaserPattern()
    {
        Debug.Log("레이저 패턴 시작");

        // 레이저가 나타날 위치를 보스 위치를 기준으로 설정
        Vector3 laserPosition = transform.position + Vector3.right * 2f;

        // 레이저 경고 표시 생성
        GameObject warning = Instantiate(patternData.laserWarningPrefab, laserPosition, Quaternion.identity, patternParent);
        Destroy(warning, patternData.laserWarningDuration);

        yield return new WaitForSeconds(patternData.laserWarningDuration);

        // 레이저 활성화
        GameObject laser = Instantiate(patternData.laserPrefab, laserPosition, Quaternion.identity, patternParent);
        Destroy(laser, patternData.laserDuration);

        // 레이저에 DamageArea 스크립트를 추가하여 지속적인 데미지를 입힐 수 있도록 합니다.
        DamageArea damageArea = laser.AddComponent<DamageArea>();
        damageArea.damage = laserDamagePerSecond;
        damageArea.duration = patternData.laserDuration;
        damageArea.isContinuous = true; // 지속적인 데미지 적용
    }

    /// <summary>
    /// 바닥 찍기 패턴을 실행합니다.
    /// </summary>
    private IEnumerator GroundSmashPattern()
    {
        Debug.Log("바닥 찍기 패턴 시작");

        // 플레이어 위치에 경고 표시 생성
        Vector3 targetPosition = GetPlayerPosition();
        GameObject warning = Instantiate(patternData.groundSmashWarningPrefab, targetPosition, Quaternion.identity, patternParent);
        Destroy(warning, patternData.groundSmashWarningDuration);

        yield return new WaitForSeconds(patternData.groundSmashWarningDuration);

        // 기계 팔 내려찍기 이펙트 생성
        GameObject smashEffect = Instantiate(patternData.groundSmashEffectPrefab, targetPosition, Quaternion.identity, patternParent);
        Destroy(smashEffect, patternData.groundSmashEffectDuration);

        // 내려찍기 이펙트에 DamageArea 스크립트를 추가하여 데미지를 입힐 수 있도록 합니다.
        DamageArea damageArea = smashEffect.AddComponent<DamageArea>();
        damageArea.damage = groundSmashDamage;
        damageArea.duration = patternData.groundSmashEffectDuration;
        damageArea.isContinuous = false; // 일회성 데미지 적용

        // 탄막 발사
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

            // Bullet 컴포넌트 설정
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetAttackDamage(bulletDamage);
                // Rigidbody2D의 velocity 설정
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.velocity = bulletDirection * patternData.groundSmashBulletSpeed;
                }
            }
            else
            {
                Debug.LogError("Bullet prefab에 Bullet 컴포넌트가 없습니다.");
            }
        }
    }

    /// <summary>
    /// 플레이어의 현재 위치를 반환합니다.
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
