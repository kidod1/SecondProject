using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Linq;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine.Events;
using Spine;
using Cinemachine; // Cinemachine 네임스페이스 추가

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

    [SerializeField]
    private DeathAnimationHandler deathAnimationHandler;

    // ExecutePatterns 코루틴을 제어하기 위한 변수
    private Coroutine executePatternsCoroutine;

    // 현재 사용 중인 스폰 포인트 인덱스
    private int currentSpawnPointIndex = 0;

    [SerializeField]
    private GameObject bossFalse;

    [Header("탄환 스폰 포인트 설정")]
    [SerializeField, InspectorName("탄환 스폰 포인트 배열")]
    [Tooltip("탄환이 발사될 여러 위치의 Transform 배열")]
    private Transform[] bulletSpawnPoints;

    [Header("경고 레이저 위치 설정")]
    [SerializeField, InspectorName("경고 레이저 위치 배열")]
    [Tooltip("경고 레이저 공격이 발생할 5개의 위치")]
    private Transform[] warningLaserPositions;

    [SerializeField]
    private GameManager gameManager;

    // 피격 시 색상 변경을 위한 필드
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

    // 플레이어의 죽음 상태를 추적하는 변수
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

    // 애니메이션 재생이 끝났는지 확인하기 위한 이벤트
    private bool isAnimationPlaying = false;

    // **추가된 부분**: Cinemachine 카메라 참조
    [Header("Cinemachine Cameras")]
    [SerializeField]
    private CinemachineVirtualCamera playerCamera;

    [SerializeField]
    private CinemachineVirtualCamera bossCamera;

    protected override void Start()
    {
        base.Start();
        bossFalse.gameObject.SetActive(false);

        // 몬스터 기본 스탯 설정
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

        // DeathAnimationHandler 설정
        if (deathAnimationHandler == null && slothMapManager != null)
        {
            deathAnimationHandler = slothMapManager.GetComponent<DeathAnimationHandler>();
        }

        // MeshRenderer와 원래 색상 저장
        redMeshRenderer = GetComponent<MeshRenderer>();
        if (redMeshRenderer != null)
        {
            // 메테리얼 인스턴스화
            redMeshRenderer.material = new Material(redMeshRenderer.material);
            bossOriginalColor = redMeshRenderer.material.color;
        }

        // 플레이어의 죽음 이벤트를 구독합니다.
        if (player != null)
        {
            player.OnPlayerDeath.AddListener(OnPlayerDeathHandler);
        }

        // Spine 애니메이션 초기화 및 Idle 애니메이션 재생
        if (skeletonAnimation != null)
        {
            PlayAnimation(idleAnimationName, loop: true);
        }

        // 패턴 실행 시작
        SetAttackable(true);

        // SlothMapManager의 OnDeathAnimationsCompleted 이벤트에 구독 추가
        if (slothMapManager != null)
        {
            slothMapManager.OnDeathAnimationsCompleted += HandleDeathAnimationsCompleted;
        }
    }

    private void OnEnable()
    {
        // 애니메이션 이벤트 핸들러 등록 제거
    }

    private void Update()
    {
        if (isDead || isPlayerDead) return;

        currentState?.UpdateState();
    }

    // 플레이어가 죽었을 때 호출되는 메서드
    private void OnPlayerDeathHandler()
    {
        isPlayerDead = true;

        // 공격 불가능 상태로 설정
        SetAttackable(false);

        // 색상 초기화
        if (redMeshRenderer != null && redMeshRenderer.material != null)
        {
            redMeshRenderer.material.color = bossOriginalColor;
        }

        // Idle 애니메이션 재생
        PlayAnimation(idleAnimationName, loop: true);
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnPlayerDeath.RemoveListener(OnPlayerDeathHandler);
        }

        if (slothMapManager != null)
        {
            slothMapManager.OnDeathAnimationsCompleted -= HandleDeathAnimationsCompleted;
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
            if (executePatternsCoroutine == null)
            {
                // ExecutePatterns 코루틴을 시작하고 참조를 저장합니다.
                executePatternsCoroutine = StartCoroutine(ExecutePatterns());
            }
        }
        else
        {
            // 공격 불가능 시 코루틴 정지
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
            // 피격 시 색상 초기화
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
            yield return new WaitForSeconds(0.1f); // 빨간색 유지 시간
            redMeshRenderer.material.color = bossOriginalColor;
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("MidBoss Die() 호출됨.");

        // 공격 패턴 코루틴 정지
        SetAttackable(false);

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
                Debug.Log("SlothMapManager 활성화됨.");
            }

            PlayManager.I.RestrictPlayerMovement();
            StartCoroutine(CameraTransitionCoroutine());

            // 죽음 애니메이션 재생
            PlayAnimation(deathAnimationName, loop: false);
            Debug.Log($"죽음 애니메이션 재생됨: {deathAnimationName}");

            // Death 애니메이션이 완료될 때까지 대기
            WaitForDeathAnimation();
        }
        else
        {
            Debug.LogError("MidBoss Die(): slothMapManager가 할당되지 않았습니다.");
            Destroy(gameObject);
        }
    }

    private void WaitForDeathAnimation()
    {
        Debug.Log("Death 애니메이션 완료됨. Death 애니메이션 후 처리 시작.");

        // Death 애니메이션이 끝난 후 처리
        if (slothMapManager != null)
        {
            StartCoroutine(PlayDeathAnimationsCoroutine());
        }
        else
        {
            Debug.LogError("WaitForDeathAnimation: slothMapManager가 할당되지 않았습니다.");
        }
    }

    private IEnumerator PlayDeathAnimationsCoroutine()
    {
        Debug.Log("PlayDeathAnimationsCoroutine 시작.");
        yield return new WaitForEndOfFrame();
        slothMapManager.PlayDeathAnimations();
    }

    private void HandleDeathAnimationsCompleted()
    {
        Debug.Log("HandleDeathAnimationsCompleted 호출됨. 포탈 활성화 및 보스 오브젝트 파괴.");

        // isAnimationPlaying을 false로 설정하여 코루틴 종료
        isAnimationPlaying = false;
        Debug.Log("isAnimationPlaying을 false로 설정.");

        if (slothMapManager != null)
        {
            slothMapManager.OnDeathAnimationsCompleted -= HandleDeathAnimationsCompleted;
            Debug.Log("SlothMapManager의 OnDeathAnimationsCompleted 이벤트 구독 해제됨.");
        }
    }

    private IEnumerator ExecutePatterns()
    {
        while (true)
        {
            if (isDead || isPlayerDead)
            {
                yield break;
            }

            // 총 확률 계산
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

            // 패턴 실행 후 idle 애니메이션 재생
            PlayAnimation(idleAnimationName, loop: true);

            // 다음 패턴까지 대기 시간 (1초)
            yield return new WaitForSeconds(1f);
        }
    }

    // 각 패턴 메서드에서도 isPlayerDead를 확인하여 조기 종료
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

        // 5개의 위치 중 4개를 랜덤하게 선택
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

            // 경고 프리팹 인스턴스화
            GameObject warning = Instantiate(patternData.warningLaserWarningPrefab, position.position, Quaternion.identity, patternParent);
            Destroy(warning, patternData.warningLaserWarningDuration);

            // 경고 지속 시간만큼 대기
            yield return new WaitForSeconds(patternData.warningLaserWarningDuration);
        }

        // 경고 표시 순서대로 레이저 공격 실행
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
                yield break; // 예외 발생 시 코루틴 종료
            }

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
        }

        // 다음 스폰 포인트로 인덱스 업데이트 (순환)
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

            // 경고 이펙트 생성
            if (patternData.warningEffectPrefab != null)
            {
                GameObject warning = Instantiate(patternData.warningEffectPrefab, targetPosition, Quaternion.identity, patternParent);
                Destroy(warning, patternData.warningDuration);
            }

            // 경고 이펙트 지속 시간만큼 대기
            yield return new WaitForSeconds(patternData.warningDuration);

            if (isDead || isPlayerDead)
            {
                yield break;
            }

            // 공격 이펙트 생성 및 데미지 적용
            if (patternData.attackEffectPrefab != null)
            {
                if (warningAttackPatternSound != null)
                {
                    warningAttackPatternSound.Post(gameObject);
                }
                GameObject attackEffect = Instantiate(patternData.attackEffectPrefab, targetPosition, Quaternion.identity, patternParent);
                Destroy(attackEffect, patternData.attackEffectDuration);

                // 공격 이펙트에 DamageArea 컴포넌트를 추가하여 데미지를 적용합니다.
                DamageArea damageArea = attackEffect.AddComponent<DamageArea>();
                damageArea.damage = patternData.warningAttackDamage;
                damageArea.duration = patternData.attackEffectDuration;
                damageArea.isContinuous = true;
            }

            // 다음 반복까지 대기
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

        // 새로운 바닥 찍기 패턴 생성
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

    /// <summary>
    /// 스파인 애니메이션을 재생합니다.
    /// </summary>
    /// <param name="animationName">재생할 애니메이션의 이름</param>
    /// <param name="loop">애니메이션 반복 여부</param>
    private void PlayAnimation(string animationName, bool loop)
    {
        if (skeletonAnimation == null)
        {
            Debug.LogError("PlayAnimation: skeletonAnimation이 할당되지 않았습니다.");
            return;
        }

        // 현재 재생 중인 애니메이션이 같다면 재생하지 않음
        var current = skeletonAnimation.AnimationState.GetCurrent(0);
        if (current != null && current.Animation.Name == animationName)
        {
            Debug.Log($"PlayAnimation: 현재 애니메이션이 '{animationName}'와 같아 재생하지 않음.");
            return;
        }

        skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
        isAnimationPlaying = !loop;
        Debug.Log($"PlayAnimation: 애니메이션 '{animationName}' 재생됨. Loop: {loop}");
    }

    // **추가된 부분**: 카메라 전환 코루틴
    private IEnumerator CameraTransitionCoroutine()
    {
        // 카메라 참조가 유효한지 확인
        if (playerCamera == null || bossCamera == null)
        {
            Debug.LogError("CameraTransitionCoroutine: playerCamera 또는 bossCamera가 할당되지 않았습니다.");
            yield break;
        }

        // 보스 카메라의 우선 순위를 높여 활성화
        bossCamera.Priority = 20;
        playerCamera.Priority = 10;

        // 카메라 확대 (OrthographicSize 조절)
        float originalSize = bossCamera.m_Lens.OrthographicSize;
        float zoomedSize = originalSize * 0.8f; // 원하는 확대 배율로 조절
        float zoomDuration = 2f; // 확대에 걸리는 시간
        float elapsedTime = 0f;

        // 카메라 확대 애니메이션
        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            bossCamera.m_Lens.OrthographicSize = Mathf.Lerp(originalSize, zoomedSize, elapsedTime / zoomDuration);
            yield return null;
        }
        bossCamera.m_Lens.OrthographicSize = zoomedSize;

        // 보스 사망 애니메이션 대기
        yield return new WaitForSeconds(2f); // 필요한 대기 시간으로 조절

        // 카메라 축소 애니메이션
        elapsedTime = 0f;
        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            bossCamera.m_Lens.OrthographicSize = Mathf.Lerp(zoomedSize, originalSize, elapsedTime / zoomDuration);
            yield return null;
        }
        bossCamera.m_Lens.OrthographicSize = originalSize;

        // 플레이어 카메라의 우선 순위를 높여 다시 활성화
        bossCamera.Priority = 10;
        playerCamera.Priority = 20;

        // 플레이어 움직임 허용
        PlayManager.I.AllowPlayerMovement();
        Destroy(gameObject);
    }
}
