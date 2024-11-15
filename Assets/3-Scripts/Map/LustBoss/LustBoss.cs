using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    // 패턴 실행 코루틴을 제어하기 위한 변수
    private Coroutine executePatternsCoroutine;

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

        // 패턴 실행을 시작합니다.
        SetAttackable(true);
    }

    protected override void Die()
    {
        base.Die();

        // 추가적으로 필요한 사망 처리 로직이 있다면 여기에 추가합니다.
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

            // 패턴을 랜덤하게 선택합니다. 현재 1~4번 패턴이 구현되어 있음
            int patternIndex = Random.Range(1, 5); // 1, 2, 3, 4 중 랜덤 선택

            switch (patternIndex)
            {
                case 1:
                    yield return StartCoroutine(CircleBulletPattern());
                    break;
                case 2:
                    yield return StartCoroutine(HeartBulletPattern());
                    break;
                case 3:
                    yield return StartCoroutine(AngleBulletPattern());
                    break;
                case 4:
                    yield return StartCoroutine(SpawnExplosionPattern());
                    break;
                default:
                    Debug.LogWarning("알 수 없는 패턴 인덱스입니다.");
                    break;
            }

            yield return new WaitForSeconds(1f); // 패턴 간 대기 시간
        }
    }

    // 1번 패턴: 원형 탄환 패턴
    private IEnumerator CircleBulletPattern()
    {
        Debug.Log("원형 탄환 패턴 시작");

        int repeatCount = lustPatternData.circlePatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            foreach (Transform spawnPoint in circleBulletSpawnPoints)
            {
                SpawnCircleBullets(spawnPoint);
            }

            yield return new WaitForSeconds(1f); // 모든 탄환이 소환된 후 대기 시간

            ActivateCircleBullets();

            yield return new WaitForSeconds(0.5f); // 다음 반복 전 대기 시간
        }
    }

    private List<GameObject> spawnedCircleBullets = new List<GameObject>();

    private void SpawnCircleBullets(Transform spawnPoint)
    {
        int bulletCount = lustPatternData.circleBulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            GameObject bullet = Instantiate(lustPatternData.circleBulletPrefab, spawnPoint.position, Quaternion.identity, patternParent);
            bullet.SetActive(false); // 일단 비활성화하여 정지 상태로 둡니다.

            spawnedCircleBullets.Add(bullet);
        }
    }

    private void ActivateCircleBullets()
    {
        foreach (GameObject bullet in spawnedCircleBullets)
        {
            if (bullet != null)
            {
                bullet.SetActive(true);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    // 탄환이 원래 위치에서 방사형으로 발사되도록 속도를 설정합니다.
                    Vector2 direction = (bullet.transform.position - transform.position).normalized;
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
            foreach (Transform spawnPoint in heartBulletSpawnPoints)
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
