using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LustBoss : Monster
{
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

    // 추가: PlayerUIManager 참조
    [Header("UI Manager")]
    [SerializeField, Tooltip("PlayerUIManager")]
    private PlayerUIManager playerUIManager;

    // 패턴 실행 코루틴을 제어하기 위한 변수
    private Coroutine executePatternsCoroutine;

    // 소환된 탄환들을 관리하기 위한 리스트
    private List<GameObject> spawnedCircleBullets = new List<GameObject>();

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
            // 필요한 경우 피격 시 효과 추가
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

        // 추가적으로 필요한 사망 처리 로직이 있다면 여기에 추가합니다.
        Destroy(gameObject); // 예시로 보스 오브젝트를 삭제합니다.
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
            else
            {
                Debug.LogWarning("알 수 없는 패턴 인덱스입니다.");
            }

            yield return new WaitForSeconds(1f); // 패턴 간 대기 시간
        }
    }

    // 1번 패턴: 원형 탄환 패턴 수정
    private IEnumerator CircleBulletPattern()
    {
        Debug.Log("원형 탄환 패턴 시작");

        int repeatCount = lustPatternData.circlePatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            foreach (Transform spawnPoint in circleBulletSpawnPoints)
            {
                // 탄환 소환
                SpawnCircleBullets(spawnPoint);

                // 0.5초 대기
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

            ActivateHeartBullets();

            yield return new WaitForSeconds(0.5f); // 다음 반복 전 대기 시간
        }
    }

    private List<GameObject> spawnedHeartBullets = new List<GameObject>();

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
        Debug.Log("각도 탄환 패턴 시작");

        int repeatCount = lustPatternData.anglePatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
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
        float angleOffset = 10f; // 각 탄환 간 각도 차이

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
        Debug.Log("스폰 후 폭발 패턴 시작");

        int repeatCount = lustPatternData.spawnExplosionPatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            foreach (Transform spawnPoint in spawnExplosionSpawnPoints)
            {
                SpawnExplosionBullets(spawnPoint);
            }

            yield return new WaitForSeconds(1f); // 모든 탄환이 소환된 후 대기 시간

            ActivateExplosionBullets();

            yield return new WaitForSeconds(0.5f); // 다음 반복 전 대기 시간
        }
    }

    private List<GameObject> spawnedExplosionBullets = new List<GameObject>();

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
}
