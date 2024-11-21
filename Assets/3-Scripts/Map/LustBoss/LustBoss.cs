using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine.UI; // Import Spine Unity namespace

public class LustBoss : Monster
{
    // 피격 시 색상 변경을 위한 필드 추가
    private MeshRenderer redMeshRenderer;
    private Color bossOriginalColor;

    [Header("Animation Settings")]
    [SerializeField, SpineAnimation]
    private string idleAnimationName;

    [SerializeField, SpineAnimation]
    private string attackAnimationName;

    [SerializeField, Tooltip("Spine SkeletonAnimation component")]
    private SkeletonAnimation skeletonAnimation; // SkeletonAnimation 참조 추가

    [Header("Lust Boss Pattern Data")]
    [SerializeField]
    private LustBossPatternData lustPatternData;

    [Header("Pattern Parent")]
    [SerializeField, Tooltip("패턴 오브젝트들을 담을 부모 Transform")]
    private Transform patternParent;

    [Header("Spawn Points")]

    [Header("Circle Bullet Pattern Spawn Points")]
    [SerializeField, Tooltip("원형 탄환 패턴의 발사 지점들")]
    private Transform[] circleBulletSpawnPoints;

    [Header("Heart Bullet Pattern Spawn Points")]
    [SerializeField, Tooltip("하트 탄환 패턴의 발사 지점들")]
    private Transform[] heartBulletSpawnPoints;

    [Header("Angle Bullet Pattern Spawn Points")]
    [SerializeField, Tooltip("각도 탄환 패턴의 발사 지점들 및 대기 시간")]
    private AngleBulletSpawnData[] angleBulletSpawnData;

    [Header("Spawn Explosion Pattern Spawn Points")]
    [SerializeField, Tooltip("스폰 후 폭발 패턴의 발사 지점들")]
    private Transform[] spawnExplosionSpawnPoints;

    [Header("Specified Direction Pattern Settings")]
    [SerializeField, Tooltip("지정된 방향 패턴의 발사 지점들")]
    private Transform[] specifiedPatternSpawnPoints;

    [SerializeField, Tooltip("지정된 방향 패턴의 목표 지점들")]
    private Transform[] specifiedPatternTargetPoints;

    [Header("Specified Direction Pattern Sound Settings")]
    [SerializeField, Tooltip("지정된 방향 패턴의 사운드 이벤트")]
    private AK.Wwise.Event specifiedDirectionPatternSound;

    [Header("Circle Bullet Pattern Sound Settings")]
    [SerializeField, Tooltip("원형 탄환 패턴의 사운드 이벤트")]
    private AK.Wwise.Event circleBulletPatternSound;

    [Header("Heart Bullet Pattern Sound Settings")]
    [SerializeField, Tooltip("하트 탄환 패턴의 사운드 이벤트")]
    private AK.Wwise.Event heartBulletPatternSound;

    [Header("Angle Bullet Pattern Sound Settings")]
    [SerializeField, Tooltip("각도 탄환 패턴의 사운드 이벤트")]
    private AK.Wwise.Event angleBulletPatternSound;

    [Header("Spawn Explosion Pattern Sound Settings")]
    [SerializeField, Tooltip("스폰 후 폭발 패턴의 사운드 이벤트")]
    private AK.Wwise.Event spawnExplosionPatternSound;

    [Header("Death Transition Settings")]
    [SerializeField, Tooltip("페이드 인에 사용할 UI Image")]
    private Image fadeInImage; // 페이드 인을 위한 UI Image

    [SerializeField, Tooltip("보스 사망 시 재생할 Wwise 사운드 이벤트")]
    private AK.Wwise.Event bossDeathSound; // 보스 사망 사운드 이벤트

    [SerializeField, Tooltip("보스 사망 후 전환할 씬의 이름")]
    private string deathTransitionSceneName; // 보스 사망 후 전환할 씬 이름


    // 추가: PlayerUIManager 참조
    [Header("UI Manager")]
    [SerializeField, Tooltip("PlayerUIManager")]
    private PlayerUIManager playerUIManager;

    // 패턴 실행 코루틴을 제어하기 위한 변수
    private Coroutine executePatternsCoroutine;

    // 소환된 탄환들을 관리하기 위한 리스트
    private List<GameObject> spawnedCircleBullets = new List<GameObject>();
    private List<GameObject> spawnedHeartBullets = new List<GameObject>();
    private List<GameObject> spawnedExplosionBullets = new List<GameObject>();

    // AngleBulletSpawnData 클래스 정의
    [System.Serializable]
    public class AngleBulletSpawnData
    {
        [Tooltip("각도 탄환 패턴의 발사 지점")]
        public Transform spawnPoint;

        [Tooltip("발사 지점마다의 대기 시간 (초)")]
        public float waitTime = 0.5f;
    }

    protected override void Start()
    {
        base.Start();
        // LustBoss 전용 패턴 데이터를 설정합니다.
        if (lustPatternData == null)
        {
            Debug.LogError("LustBoss: LustBossPatternData가 할당되지 않았습니다.");
            return;
        }

        // 패턴 부모가 할당되지 않았다면, 새로 생성
        if (patternParent == null)
        {
            GameObject parentObj = new GameObject("BossPatterns");
            patternParent = parentObj.transform;
        }

        // 보스의 최대 체력 설정 및 UI 초기화
        if (monsterBaseStat != null)
        {
            monsterBaseStat.maxHP = monsterBaseStat.maxHP > 0 ? monsterBaseStat.maxHP : 1000; // 예시로 1000 설정
            currentHP = monsterBaseStat.maxHP;
        }
        else
        {
            Debug.LogError("LustBoss: MonsterData(monsterBaseStat)가 할당되지 않았습니다.");
            currentHP = 1000; // 기본 체력 설정
        }

        // PlayerUIManager 초기화
        if (playerUIManager != null)
        {
            playerUIManager.InitializeBossHealth(currentHP);
        }
        else
        {
            Debug.LogError("LustBoss: PlayerUIManager가 할당되지 않았습니다.");
        }

        // MeshRenderer와 원래 색상 저장
        redMeshRenderer = GetComponent<MeshRenderer>();
        if (redMeshRenderer != null)
        {
            // 메테리얼 인스턴스화
            redMeshRenderer.material = new Material(redMeshRenderer.material);
            bossOriginalColor = redMeshRenderer.material.color;
        }
        else
        {
            Debug.LogWarning("MeshRenderer를 찾을 수 없습니다.");
        }

        // Initialize SkeletonAnimation
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            if (skeletonAnimation == null)
            {
                Debug.LogError("LustBoss: SkeletonAnimation component is not assigned and not found on the GameObject.");
            }
        }
        if (fadeInImage != null)
        {
            Color tempColor = fadeInImage.color;
            tempColor.a = 0f; // 완전히 투명하게 설정
            fadeInImage.color = tempColor;
            fadeInImage.gameObject.SetActive(false); // 시작 시 비활성화
        }
        else
        {
            Debug.LogWarning("LustBoss: FadeInImage가 할당되지 않았습니다.");
        }

        // Play Idle animation initially
        PlayIdleAnimation();

        // 패턴 실행을 시작합니다.
        SetAttackable(true);
    }

    public override void TakeDamage(int damage, Vector3 damageSourcePosition, bool Nun = false)
    {
        if (isDead)
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
            StartCoroutine(FlashRedCoroutine());
        }

        Debug.Log($"LustBoss가 데미지를 입었습니다! 남은 체력: {currentHP}/{monsterBaseStat.maxHP}");

        // 보스 체력 UI 업데이트
        if (playerUIManager != null)
        {
            playerUIManager.UpdateBossHealth(currentHP);
        }
        else
        {
            Debug.LogWarning("LustBoss: PlayerUIManager가 할당되지 않았습니다.");
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        // 보스 체력 UI 숨김
        if (playerUIManager != null)
        {
            playerUIManager.UpdateBossHealth(0);
            playerUIManager.HideBossHealthUI(); // 보스 체력 UI 패널 비활성화
        }

        // 보스 사망 시 페이드 인 및 씬 전환 코루틴 시작
        if (fadeInImage != null && bossDeathSound != null && !string.IsNullOrEmpty(deathTransitionSceneName))
        {
            StartCoroutine(FadeInAndTransition());
        }
        else
        {
            Debug.LogWarning("LustBoss: 페이드 인이나 씬 전환에 필요한 설정이 누락되었습니다.");
            // 필요한 설정이 없을 경우 즉시 씬 전환
            LoadDeathScene();
        }
    }
    /// <summary>
    /// 페이드 인 효과를 수행하고 사운드를 재생한 후 씬을 전환하는 코루틴입니다.
    /// </summary>
    /// <returns>코루틴용 IEnumerator</returns>
    private IEnumerator FadeInAndTransition()
    {
        // 페이드 인 이미지 활성화
        fadeInImage.gameObject.SetActive(true);

        // 페이드 인 사운드 재생
        bossDeathSound?.Post(gameObject);

        // 페이드 인 시간 설정 (예: 2초)
        float fadeDuration = 2f;
        float elapsedTime = 0f;

        Color color = fadeInImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            color.a = alpha;
            fadeInImage.color = color;
            yield return null;
        }

        // 완전히 페이드 인된 상태
        color.a = 1f;
        fadeInImage.color = color;

        // 3초 대기
        yield return new WaitForSeconds(3f);

        // 씬 전환
        LoadDeathScene();
    }

    /// <summary>
    /// 사망 후 지정된 씬을 로드하는 메서드입니다.
    /// </summary>
    private void LoadDeathScene()
    {
        if (string.IsNullOrEmpty(deathTransitionSceneName))
        {
            Debug.LogError("LustBoss: DeathTransitionSceneName이 설정되지 않았습니다.");
            return;
        }

        // 씬 로드
        UnityEngine.SceneManagement.SceneManager.LoadScene(deathTransitionSceneName);
    }
    /// <summary>
    /// 몬스터가 데미지를 받을 때 붉게 깜빡이는 효과를 처리하는 코루틴입니다.
    /// </summary>
    /// <returns>코루틴용 IEnumerator</returns>
    private IEnumerator FlashRedCoroutine()
    {
        float elapsed = 0f;
        bool isRed = false;

        while (elapsed < 0.5f)
        {
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.color = isRed ? bossOriginalColor : Color.red;
                isRed = !isRed;
            }

            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.color = bossOriginalColor;
        }
    }

    /// <summary>
    /// 몬스터의 상태를 초기화합니다. LustBoss는 상태 시스템을 사용하지 않으므로 빈 구현.
    /// </summary>
    protected override void InitializeStates()
    {
        // LustBoss는 상태 시스템을 사용하지 않습니다.
    }

    /// <summary>
    /// LustBoss의 공격 동작을 구현합니다. 현재는 패턴에 의존하므로 빈 구현.
    /// </summary>
    public override void Attack()
    {
        // LustBoss는 패턴을 통해 공격을 수행합니다.
    }

    /// <summary>
    /// 패턴 실행 코루틴을 오버라이드하여 LustBoss의 패턴을 실행합니다.
    /// </summary>
    /// <returns>코루틴용 IEnumerator</returns>
    private IEnumerator ExecutePatterns()
    {
        while (true)
        {
            if (isDead)
            {
                yield break;
            }

            // Play Attack animation
            PlayAttackAnimation();

            // Wait for the Attack animation to complete
            // This relies on the OnAttackAnimationComplete callback to switch back to Idle

            // Execute the selected pattern
            float randomValue = Random.value; // 0.0부터 1.0 사이의 랜덤 값
            float cumulativeProbability = 0f;

            // 각 패턴의 확률을 비교하여 패턴을 선택합니다.
            if (randomValue < (cumulativeProbability += lustPatternData.circlePatternProbability))
            {
                yield return StartCoroutine(CircleBulletPattern());
            }
            else if (randomValue < (cumulativeProbability += lustPatternData.heartPatternProbability))
            {
                yield return StartCoroutine(HeartBulletPattern());
            }
            else if (randomValue < (cumulativeProbability += lustPatternData.anglePatternProbability))
            {
                yield return StartCoroutine(AngleBulletPattern());
            }
            else if (randomValue < (cumulativeProbability += lustPatternData.spawnExplosionPatternProbability))
            {
                yield return StartCoroutine(SpawnExplosionPattern());
            }
            else if (randomValue < (cumulativeProbability += lustPatternData.specifiedPatternProbability))
            {
                yield return StartCoroutine(SpecifiedDirectionPattern());
            }
            else
            {
                Debug.LogWarning("LustBoss: Unknown pattern index.");
            }

            // Wait for 1 second between patterns
            yield return new WaitForSeconds(1f);
        }
    }

    // 1번 패턴: 원형 탄환 패턴
    private IEnumerator CircleBulletPattern()
    {
        Debug.Log("원형 탄환 패턴 시작");

        int repeatCount = lustPatternData.circlePatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            circleBulletPatternSound?.Post(gameObject);
            foreach (Transform spawnPoint in circleBulletSpawnPoints)
            {
                // 탄환 소환
                SpawnCircleBullets(spawnPoint);

                // 0.1초 대기
                yield return new WaitForSeconds(0.1f);

                // 탄환 발사
                ActivateCircleBullets();
            }
        }
    }

    private void SpawnCircleBullets(Transform spawnPoint)
    {
        int bulletCount = lustPatternData.circleBulletCount;
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            GameObject bullet = Instantiate(lustPatternData.circleBulletPrefab, spawnPoint.position, rotation, patternParent);

            spawnedCircleBullets.Add(bullet);
        }
    }

    private void ActivateCircleBullets()
    {
        Vector3 playerPosition = GetPlayerPosition();

        foreach (GameObject bullet in spawnedCircleBullets)
        {
            if (bullet != null)
            {
                bullet.SetActive(true);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    // 탄환이 플레이어를 향해 날아가도록 방향 설정
                    Vector2 direction = (playerPosition - bullet.transform.position).normalized;
                    bulletRb.velocity = direction * lustPatternData.circleBulletSpeed;
                }
                else
                {
                    Debug.LogError("Circle Bullet에 Rigidbody2D가 없습니다.");
                }
            }
        }

        // 리스트를 초기화합니다.
        spawnedCircleBullets.Clear();
    }

    // 2번 패턴: 하트 탄환 패턴
    private IEnumerator HeartBulletPattern()
    {
        Debug.Log("하트 탄환 패턴 시작");

        int repeatCount = lustPatternData.heartPatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            // 하트 탄환 패턴의 발사 지점 중 4개를 랜덤하게 선택
            List<Transform> randomSpawnPoints = heartBulletSpawnPoints.OrderBy(x => Random.value).Take(4).ToList();

            foreach (Transform spawnPoint in randomSpawnPoints)
            {
                SpawnHeartBullets(spawnPoint);
            }

            yield return new WaitForSeconds(2f); // 모든 탄환이 소환된 후 대기 시간
            heartBulletPatternSound?.Post(gameObject);
            ActivateHeartBullets();

            yield return new WaitForSeconds(0.5f); // 다음 반복 전 대기 시간
        }
    }

    private void SpawnHeartBullets(Transform spawnPoint)
    {
        GameObject bullet = Instantiate(lustPatternData.heartBulletPrefab, spawnPoint.position, Quaternion.identity, patternParent);
        bullet.SetActive(false); // 일단 비활성화하여 정지 상태로 둡니다.

        spawnedHeartBullets.Add(bullet);
    }

    private void ActivateHeartBullets()
    {
        foreach (GameObject bullet in spawnedHeartBullets)
        {
            if (bullet != null)
            {
                bullet.SetActive(true);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    // 탄환이 아래로 떨어지도록 속도를 설정합니다.
                    bulletRb.velocity = Vector2.down * lustPatternData.heartBulletSpeed;
                }
                else
                {
                    Debug.LogError("Heart Bullet에 Rigidbody2D가 없습니다.");
                }
            }
        }

        // 리스트를 초기화합니다.
        spawnedHeartBullets.Clear();
    }

    // 3번 패턴: 각도 탄환 패턴
    private IEnumerator AngleBulletPattern()
    {
        int repeatCount = lustPatternData.anglePatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            angleBulletPatternSound?.Post(gameObject);
            foreach (AngleBulletSpawnData spawnData in angleBulletSpawnData)
            {
                FireAngleBullets(spawnData.spawnPoint);
                yield return new WaitForSeconds(spawnData.waitTime); // 각 발사 지점마다 대기 시간
            }

            yield return new WaitForSeconds(0.5f); // 다음 반복 전 대기 시간
        }
    }

    private void FireAngleBullets(Transform spawnPoint)
    {
        Vector3 playerPosition = GetPlayerPosition();
        Vector3 directionToPlayer = (playerPosition - spawnPoint.position).normalized;
        float baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        int bulletCount = lustPatternData.angleBulletCount;
        float angleOffset = 15f; // 각 탄환 간 각도 차이

        for (int j = 0; j < bulletCount; j++)
        {
            float angle = baseAngle + angleOffset * (j - bulletCount / 2);
            float angleRad = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;

            GameObject bullet = Instantiate(lustPatternData.angleBulletPrefab, spawnPoint.position, Quaternion.identity, patternParent);

            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = direction * lustPatternData.angleBulletSpeed;
            }
            else
            {
                Debug.LogError("Angle Bullet에 Rigidbody2D가 없습니다.");
            }

            bullet.SetActive(true);
            // Optional: 각 탄환 발사 간 대기 시간 추가
        }
    }

    // 4번 패턴: 스폰 후 폭발 패턴
    private IEnumerator SpawnExplosionPattern()
    {
        int repeatCount = lustPatternData.spawnExplosionPatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            foreach (Transform spawnPoint in spawnExplosionSpawnPoints)
            {
                SpawnExplosionBullets(spawnPoint);
            }

            yield return new WaitForSeconds(1f); // 모든 탄환이 소환된 후 대기 시간
            spawnExplosionPatternSound?.Post(gameObject);
            ActivateExplosionBullets();

            yield return new WaitForSeconds(0.5f); // 다음 반복 전 대기 시간
        }
    }

    private void SpawnExplosionBullets(Transform spawnPoint)
    {
        int bulletCount = lustPatternData.spawnExplosionBulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            GameObject bullet = Instantiate(lustPatternData.spawnExplosionPrefab, spawnPoint.position, Quaternion.identity, patternParent);
            bullet.SetActive(false); // 일단 비활성화하여 정지 상태로 둡니다.

            spawnedExplosionBullets.Add(bullet);
        }
    }

    private void ActivateExplosionBullets()
    {
        foreach (GameObject bullet in spawnedExplosionBullets)
        {
            if (bullet != null)
            {
                bullet.SetActive(true);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    // 탄환을 랜덤한 방향으로 폭발시키도록 속도를 설정합니다.
                    float angle = Random.Range(0f, 360f);
                    Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
                    bulletRb.velocity = direction * lustPatternData.spawnExplosionBulletSpeed;
                }
                else
                {
                    Debug.LogError("Explosion Bullet에 Rigidbody2D가 없습니다.");
                }
            }
        }

        // 리스트를 초기화합니다.
        spawnedExplosionBullets.Clear();
    }

    // 5번 패턴: 지정된 방향 패턴
    private IEnumerator SpecifiedDirectionPattern()
    {
        Debug.Log("지정된 방향 패턴 시작");

        float elapsedTime = 0f;

        while (elapsedTime < lustPatternData.specifiedPatternDuration)
        {
            if (isDead)
            {
                yield break;
            }
            specifiedDirectionPatternSound?.Post(gameObject);
            FireSpecifiedBullets();

            yield return new WaitForSeconds(lustPatternData.specifiedPatternFireInterval);

            elapsedTime += lustPatternData.specifiedPatternFireInterval;
        }

        Debug.Log("지정된 방향 패턴 종료");
    }

    private void FireSpecifiedBullets()
    {
        if (specifiedPatternSpawnPoints == null || specifiedPatternTargetPoints == null)
        {
            Debug.LogWarning("발사 지점 또는 목표 지점들이 설정되지 않았습니다.");
            return;
        }

        if (specifiedPatternSpawnPoints.Length != specifiedPatternTargetPoints.Length)
        {
            Debug.LogWarning("발사 지점과 목표 지점의 개수가 일치하지 않습니다.");
            return;
        }

        for (int i = 0; i < specifiedPatternSpawnPoints.Length; i++)
        {
            Transform spawnPoint = specifiedPatternSpawnPoints[i];
            Transform targetPoint = specifiedPatternTargetPoints[i];

            if (spawnPoint == null || targetPoint == null)
            {
                Debug.LogWarning($"인덱스 {i}의 발사 지점 또는 목표 지점이 null입니다.");
                continue;
            }

            // 탄환 생성
            GameObject bullet = Instantiate(lustPatternData.specifiedPatternBulletPrefab, spawnPoint.position, Quaternion.identity, patternParent);

            // 방향 설정
            Vector2 direction = (targetPoint.position - spawnPoint.position).normalized;

            // 탄환에 속도 적용
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = direction * lustPatternData.specifiedPatternBulletSpeed;
            }
            else
            {
                Debug.LogError("지정된 패턴 탄환에 Rigidbody2D가 없습니다.");
            }

            bullet.SetActive(true);
        }
    }

    /// <summary>
    /// 패턴 실행을 시작하거나 중지합니다.
    /// </summary>
    /// <param name="value">공격 가능 여부</param>
    public void SetAttackable(bool value)
    {
        if (value)
        {
            if (executePatternsCoroutine == null)
            {
                executePatternsCoroutine = StartCoroutine(ExecutePatterns());
                Debug.Log("LustBoss가 공격을 시작합니다.");
            }
        }
        else
        {
            if (executePatternsCoroutine != null)
            {
                StopCoroutine(executePatternsCoroutine);
                executePatternsCoroutine = null;
                Debug.Log("LustBoss의 공격이 중지되었습니다.");
            }
        }
    }

    /// <summary>
    /// 플레이어의 위치를 가져옵니다.
    /// </summary>
    /// <returns>플레이어의 위치 벡터</returns>
    private Vector3 GetPlayerPosition()
    {
        if (player != null)
        {
            return player.transform.position;
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Plays the Idle animation.
    /// </summary>
    private void PlayIdleAnimation()
    {
        if (skeletonAnimation != null && !string.IsNullOrEmpty(idleAnimationName))
        {
            skeletonAnimation.AnimationState.SetAnimation(0, idleAnimationName, true);
        }
        else
        {
            Debug.LogWarning("LustBoss: Idle animation name is not set or SkeletonAnimation is missing.");
        }
    }

    /// <summary>
    /// Plays the Attack animation and returns to Idle after completion.
    /// </summary>
    private void PlayAttackAnimation()
    {
        if (skeletonAnimation != null && !string.IsNullOrEmpty(attackAnimationName))
        {
            skeletonAnimation.AnimationState.SetAnimation(0, attackAnimationName, false).Complete += OnAttackAnimationComplete;
        }
        else
        {
            Debug.LogWarning("LustBoss: Attack animation name is not set or SkeletonAnimation is missing.");
        }
    }

    /// <summary>
    /// Callback when Attack animation completes.
    /// </summary>
    /// <param name="trackEntry">The completed track entry.</param>
    private void OnAttackAnimationComplete(Spine.TrackEntry trackEntry)
    {
        PlayIdleAnimation();
    }
}
