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
    [InspectorName("최대 체력")]
    public int maxHealth = 100;

    [Header("패턴 데이터")]
    [InspectorName("패턴 데이터")]
    public BossPatternData patternData;

    [SerializeField, InspectorName("공격 가능 여부")]
    private bool isAttackable = false;

    [SerializeField, InspectorName("패턴 부모 Transform")]
    private Transform patternParent;

    [Header("UI Manager")]
    [SerializeField, InspectorName("PlayerUIManager")]
    private PlayerUIManager playerUIManager;

    [SerializeField]
    private SlothMapManager slothMapManager;

    // ExecutePatterns 코루틴을 제어하기 위한 변수
    private Coroutine executePatternsCoroutine;

    // 현재 사용 중인 스폰 포인트 인덱스
    private int currentSpawnPointIndex = 0;

    [Header("탄환 스폰 포인트 설정")]
    [SerializeField, InspectorName("탄환 스폰 포인트 배열")]
    [Tooltip("탄환이 발사될 여러 위치의 Transform 배열")]
    private Transform[] bulletSpawnPoints;

    // 추가된 부분: 경고 레이저 위치 배열
    [Header("경고 레이저 위치 설정")]
    [SerializeField, InspectorName("경고 레이저 위치 배열")]
    [Tooltip("경고 레이저 공격이 발생할 5개의 위치")]
    private Transform[] warningLaserPositions;

    [SerializeField]
    private GameManager gameManager;
    protected override void Start()
    {
        base.Start();

        // 몬스터 기본 스탯 설정
        if (monsterBaseStat != null)
        {
            monsterBaseStat.maxHP = maxHealth;
            currentHP = maxHealth;
        }
        else
        {
            Debug.LogError("MidBoss: MonsterData(monsterBaseStat)가 할당되지 않았습니다.");
            currentHP = maxHealth;
        }

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

        // 스폰 포인트가 설정되지 않은 경우 경고 메시지 출력
        if (bulletSpawnPoints == null || bulletSpawnPoints.Length == 0)
        {
            Debug.LogWarning("MidBoss: 탄환 스폰 포인트 배열이 설정되지 않았습니다.");
        }

        // 경고 레이저 위치 배열이 설정되지 않은 경우 경고 메시지 출력
        if (warningLaserPositions == null || warningLaserPositions.Length < 5)
        {
            Debug.LogWarning("MidBoss: 경고 레이저 위치 배열이 올바르게 설정되지 않았습니다.");
        }
    }

    protected override void InitializeStates()
    {
        // MidBoss는 상태 시스템을 사용하지 않으므로 구현하지 않습니다.
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
            // ExecutePatterns 코루틴을 시작하고 참조를 저장합니다.
            executePatternsCoroutine = StartCoroutine(ExecutePatterns());
            Debug.Log("보스 몬스터가 공격을 시작합니다.");
        }
        else
        {
            // 공격 불가능 시 코루틴 정지
            if (executePatternsCoroutine != null)
            {
                StopCoroutine(executePatternsCoroutine);
                executePatternsCoroutine = null;
                Debug.Log("보스 몬스터의 공격이 중지되었습니다.");
            }
        }
    }

    public override void TakeDamage(int damage, Vector3 damageSourcePosition, bool Nun = false)
    {
        base.TakeDamage(damage, damageSourcePosition);

        Debug.Log($"중간 보스가 데미지를 입었습니다! 남은 체력: {currentHP}/{maxHealth}");

        if (playerUIManager != null)
        {
            playerUIManager.UpdateBossHealth(currentHP);
        }
        else
        {
            Debug.LogWarning("MidBoss: PlayerUIManager가 할당되지 않았습니다.");
        }
    }

    protected override void Die()
    {
        if (isDead) return;

        // 보스가 죽을 때 공격 패턴 코루틴을 정지합니다.
        if (executePatternsCoroutine != null)
        {
            StopCoroutine(executePatternsCoroutine);
            executePatternsCoroutine = null;
            Debug.Log("보스 몬스터의 공격 패턴이 정지되었습니다.");
        }

        base.Die();
        gameManager.ShowGameResultPanelTest();
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
            if (isDead)
            {
                yield break;
            }

            // 총 확률 계산 (레이저 패턴 확률 제거)
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
        Debug.Log("경고 레이저 패턴 시작");

        if (warningLaserPositions == null || warningLaserPositions.Length < 5)
        {
            Debug.LogError("경고 레이저 위치가 제대로 설정되지 않았습니다.");
            yield break;
        }

        // 5개의 위치 중 4개를 랜덤하게 선택
        List<Transform> positionsList = warningLaserPositions.ToList();
        List<Transform> selectedPositions = new List<Transform>();

        for (int i = 0; i < 4; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, positionsList.Count);
            selectedPositions.Add(positionsList[randomIndex]);
            positionsList.RemoveAt(randomIndex);
        }

        // 선택된 위치에서 순서대로 경고 표시
        foreach (Transform position in selectedPositions)
        {
            // 경고 프리팹 인스턴스화
            GameObject warning = Instantiate(patternData.warningLaserWarningPrefab, position.position, Quaternion.identity, patternParent);
            Destroy(warning, patternData.warningLaserWarningDuration);

            // 경고 지속 시간만큼 대기
            yield return new WaitForSeconds(patternData.warningLaserWarningDuration);
        }

        // 경고 표시 순서대로 레이저 공격 실행
        foreach (Transform position in selectedPositions)
        {
            // 레이저 공격 프리팹 인스턴스화
            GameObject laserAttack = Instantiate(patternData.warningLaserAttackPrefab, position.position, Quaternion.identity, patternParent);
            Destroy(laserAttack, patternData.warningLaserAttackDuration);

            // 데미지 적용을 위한 DamageArea 컴포넌트 추가
            DamageArea damageArea = laserAttack.AddComponent<DamageArea>();
            damageArea.damage = patternData.warningLaserAttackDamage;
            damageArea.duration = patternData.warningLaserAttackDuration;
            damageArea.isContinuous = true;

            // 레이저 공격 지속 시간만큼 대기
            yield return new WaitForSeconds(patternData.warningLaserAttackDuration);
        }

        Debug.Log("경고 레이저 패턴 완료");
    }

    // 레이저 패턴 제거
    /*
    private IEnumerator LaserPattern()
    {
        // 해당 패턴은 제거되었습니다.
    }
    */

    private IEnumerator BulletPattern()
    {
        Debug.Log("탄막 패턴 시작");
        for (int i = 0; i < patternData.bulletPatternRepeatCount; i++)
        {
            FireBullets();
            yield return new WaitForSeconds(patternData.bulletFireInterval);
        }
    }

    /// <summary>
    /// 현재 스폰 포인트에서 탄환을 발사합니다.
    /// </summary>
    private void FireBullets()
    {
        if (bulletSpawnPoints == null || bulletSpawnPoints.Length == 0)
        {
            Debug.LogError("MidBoss: 탄환 스폰 포인트 배열이 설정되지 않았습니다.");
            return;
        }

        // 현재 스폰 포인트 인덱스에 해당하는 Transform 가져오기
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
                Debug.LogError("Bullet prefab에 Bullet 컴포넌트가 없습니다.");
            }
        }

        // 다음 스폰 포인트로 인덱스 업데이트 (순환)
        currentSpawnPointIndex = (currentSpawnPointIndex + 1) % bulletSpawnPoints.Length;
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

            // 3. 경고 이펙트 지속 시간만큼 대기
            yield return new WaitForSeconds(patternData.warningDuration);

            // 4. 공격 이펙트 생성 및 데미지 적용
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

            // 5. 다음 반복까지 대기
            yield return new WaitForSeconds(patternData.warningStartInterval);
        }

        Debug.Log("경고 후 공격 패턴 완료");
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
            float angleRad = patternData.groundSmashAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0) * patternData.groundSmashSpawnRadius;
            Vector3 spawnPosition = targetPosition + offset;

            Quaternion rotation = Quaternion.Euler(0f, 0f, patternData.groundSmashObjectRotation);

            GameObject groundSmashObject = Instantiate(patternData.groundSmashMeteorPrefab, spawnPosition, rotation, patternParent);

            SpriteRenderer spriteRenderer = groundSmashObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("groundSmashMeteorPrefab에 SpriteRenderer가 없습니다.");
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

            // 탄환 스폰 포인트 시각화
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

            // 경고 레이저 위치 시각화
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
        // MidBoss는 이 메서드를 사용하지 않습니다.
    }
}
