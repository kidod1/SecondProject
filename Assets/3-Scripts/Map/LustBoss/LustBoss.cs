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
    [SerializeField, Tooltip("���� ������Ʈ���� ���� �θ� Transform")]
    private Transform patternParent;

    [Header("Spawn Points")]

    [Header("Circle Bullet Pattern Spawn Points")]
    [SerializeField, Tooltip("���� źȯ ������ �߻� ������")]
    private Transform[] circleBulletSpawnPoints;

    [Header("Heart Bullet Pattern Spawn Points")]
    [SerializeField, Tooltip("��Ʈ źȯ ������ �߻� ������")]
    private Transform[] heartBulletSpawnPoints;

    [Header("Angle Bullet Pattern Spawn Points")]
    [SerializeField, Tooltip("���� źȯ ������ �߻� ������ �� ��� �ð�")]
    private AngleBulletSpawnData[] angleBulletSpawnData;

    [Header("Spawn Explosion Pattern Spawn Points")]
    [SerializeField, Tooltip("���� �� ���� ������ �߻� ������")]
    private Transform[] spawnExplosionSpawnPoints;

    // �߰�: PlayerUIManager ����
    [Header("UI Manager")]
    [SerializeField, Tooltip("PlayerUIManager")]
    private PlayerUIManager playerUIManager;

    // ���� ���� �ڷ�ƾ�� �����ϱ� ���� ����
    private Coroutine executePatternsCoroutine;

    // ��ȯ�� źȯ���� �����ϱ� ���� ����Ʈ
    private List<GameObject> spawnedCircleBullets = new List<GameObject>();

    // AngleBulletSpawnData Ŭ���� ����
    [System.Serializable]
    public class AngleBulletSpawnData
    {
        [Tooltip("���� źȯ ������ �߻� ����")]
        public Transform spawnPoint;

        [Tooltip("�߻� ���������� ��� �ð� (��)")]
        public float waitTime = 0.5f;
    }

    protected override void Start()
    {
        base.Start();
        // LustBoss ���� ���� �����͸� �����մϴ�.
        if (lustPatternData == null)
        {
            Debug.LogError("LustBoss: LustBossPatternData�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // ���� �θ� �Ҵ���� �ʾҴٸ�, ���� ����
        if (patternParent == null)
        {
            GameObject parentObj = new GameObject("BossPatterns");
            patternParent = parentObj.transform;
        }

        // ������ �ִ� ü�� ���� �� UI �ʱ�ȭ
        if (monsterBaseStat != null)
        {
            monsterBaseStat.maxHP = monsterBaseStat.maxHP > 0 ? monsterBaseStat.maxHP : 1000; // ���÷� 1000 ����
            currentHP = monsterBaseStat.maxHP;
        }
        else
        {
            Debug.LogError("LustBoss: MonsterData(monsterBaseStat)�� �Ҵ���� �ʾҽ��ϴ�.");
            currentHP = 1000; // �⺻ ü�� ����
        }

        // PlayerUIManager �ʱ�ȭ
        if (playerUIManager != null)
        {
            playerUIManager.InitializeBossHealth(currentHP);
        }
        else
        {
            Debug.LogError("LustBoss: PlayerUIManager�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // ���� ������ �����մϴ�.
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
            // �ʿ��� ��� �ǰ� �� ȿ�� �߰�
        }

        Debug.Log($"LustBoss�� �������� �Ծ����ϴ�! ���� ü��: {currentHP}/{monsterBaseStat.maxHP}");

        // ���� ü�� UI ������Ʈ
        if (playerUIManager != null)
        {
            playerUIManager.UpdateBossHealth(currentHP);
        }
        else
        {
            Debug.LogWarning("LustBoss: PlayerUIManager�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        // ���� ü�� UI ����
        if (playerUIManager != null)
        {
            playerUIManager.UpdateBossHealth(0);
            playerUIManager.HideBossHealthUI(); // ���� ü�� UI �г� ��Ȱ��ȭ
        }

        // �߰������� �ʿ��� ��� ó�� ������ �ִٸ� ���⿡ �߰��մϴ�.
        Destroy(gameObject); // ���÷� ���� ������Ʈ�� �����մϴ�.
    }

    /// <summary>
    /// ������ ���¸� �ʱ�ȭ�մϴ�. LustBoss�� ���� �ý����� ������� �����Ƿ� �� ����.
    /// </summary>
    protected override void InitializeStates()
    {
        // LustBoss�� ���� �ý����� ������� �ʽ��ϴ�.
    }

    /// <summary>
    /// LustBoss�� ���� ������ �����մϴ�. ����� ���Ͽ� �����ϹǷ� �� ����.
    /// </summary>
    public override void Attack()
    {
        // LustBoss�� ������ ���� ������ �����մϴ�.
    }

    /// <summary>
    /// ���� ���� �ڷ�ƾ�� �������̵��Ͽ� LustBoss�� ������ �����մϴ�.
    /// </summary>
    /// <returns>�ڷ�ƾ�� IEnumerator</returns>
    private IEnumerator ExecutePatterns()
    {
        while (true)
        {
            if (isDead)
            {
                yield break;
            }

            float randomValue = Random.value; // 0.0���� 1.0 ������ ���� ��
            float cumulativeProbability = 0f;

            // �� ������ Ȯ���� ���Ͽ� ������ �����մϴ�.
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
                Debug.LogWarning("�� �� ���� ���� �ε����Դϴ�.");
            }

            yield return new WaitForSeconds(1f); // ���� �� ��� �ð�
        }
    }

    // 1�� ����: ���� źȯ ���� ����
    private IEnumerator CircleBulletPattern()
    {
        Debug.Log("���� źȯ ���� ����");

        int repeatCount = lustPatternData.circlePatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            foreach (Transform spawnPoint in circleBulletSpawnPoints)
            {
                // źȯ ��ȯ
                SpawnCircleBullets(spawnPoint);

                // 0.5�� ���
                yield return new WaitForSeconds(0.1f);

                // źȯ �߻�
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
                    // źȯ�� �÷��̾ ���� ���ư����� ���� ����
                    Vector2 direction = (playerPosition - bullet.transform.position).normalized;
                    bulletRb.velocity = direction * lustPatternData.circleBulletSpeed;
                }
                else
                {
                    Debug.LogError("Circle Bullet�� Rigidbody2D�� �����ϴ�.");
                }
            }
        }

        // ����Ʈ�� �ʱ�ȭ�մϴ�.
        spawnedCircleBullets.Clear();
    }

    // 2�� ����: ��Ʈ źȯ ����
    private IEnumerator HeartBulletPattern()
    {
        Debug.Log("��Ʈ źȯ ���� ����");

        int repeatCount = lustPatternData.heartPatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            // ��Ʈ źȯ ������ �߻� ���� �� 4���� �����ϰ� ����
            List<Transform> randomSpawnPoints = heartBulletSpawnPoints.OrderBy(x => Random.value).Take(4).ToList();

            foreach (Transform spawnPoint in randomSpawnPoints)
            {
                SpawnHeartBullets(spawnPoint);
            }

            yield return new WaitForSeconds(2f); // ��� źȯ�� ��ȯ�� �� ��� �ð�

            ActivateHeartBullets();

            yield return new WaitForSeconds(0.5f); // ���� �ݺ� �� ��� �ð�
        }
    }

    private List<GameObject> spawnedHeartBullets = new List<GameObject>();

    private void SpawnHeartBullets(Transform spawnPoint)
    {
        GameObject bullet = Instantiate(lustPatternData.heartBulletPrefab, spawnPoint.position, Quaternion.identity, patternParent);
        bullet.SetActive(false); // �ϴ� ��Ȱ��ȭ�Ͽ� ���� ���·� �Ӵϴ�.

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
                    // źȯ�� �Ʒ��� ���������� �ӵ��� �����մϴ�.
                    bulletRb.velocity = Vector2.down * lustPatternData.heartBulletSpeed;
                }
                else
                {
                    Debug.LogError("Heart Bullet�� Rigidbody2D�� �����ϴ�.");
                }
            }
        }

        // ����Ʈ�� �ʱ�ȭ�մϴ�.
        spawnedHeartBullets.Clear();
    }

    // 3�� ����: ���� źȯ ����
    private IEnumerator AngleBulletPattern()
    {
        Debug.Log("���� źȯ ���� ����");

        int repeatCount = lustPatternData.anglePatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            foreach (AngleBulletSpawnData spawnData in angleBulletSpawnData)
            {
                FireAngleBullets(spawnData.spawnPoint);
                yield return new WaitForSeconds(spawnData.waitTime); // �� �߻� �������� ��� �ð�
            }

            yield return new WaitForSeconds(0.5f); // ���� �ݺ� �� ��� �ð�
        }
    }

    private void FireAngleBullets(Transform spawnPoint)
    {
        Vector3 playerPosition = GetPlayerPosition();
        Vector3 directionToPlayer = (playerPosition - spawnPoint.position).normalized;
        float baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        int bulletCount = lustPatternData.angleBulletCount;
        float angleOffset = 10f; // �� źȯ �� ���� ����

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
                Debug.LogError("Angle Bullet�� Rigidbody2D�� �����ϴ�.");
            }

            bullet.SetActive(true);
            // Optional: �� źȯ �߻� �� ��� �ð� �߰�
        }
    }

    // 4�� ����: ���� �� ���� ����
    private IEnumerator SpawnExplosionPattern()
    {
        Debug.Log("���� �� ���� ���� ����");

        int repeatCount = lustPatternData.spawnExplosionPatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            foreach (Transform spawnPoint in spawnExplosionSpawnPoints)
            {
                SpawnExplosionBullets(spawnPoint);
            }

            yield return new WaitForSeconds(1f); // ��� źȯ�� ��ȯ�� �� ��� �ð�

            ActivateExplosionBullets();

            yield return new WaitForSeconds(0.5f); // ���� �ݺ� �� ��� �ð�
        }
    }

    private List<GameObject> spawnedExplosionBullets = new List<GameObject>();

    private void SpawnExplosionBullets(Transform spawnPoint)
    {
        int bulletCount = lustPatternData.spawnExplosionBulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            GameObject bullet = Instantiate(lustPatternData.spawnExplosionPrefab, spawnPoint.position, Quaternion.identity, patternParent);
            bullet.SetActive(false); // �ϴ� ��Ȱ��ȭ�Ͽ� ���� ���·� �Ӵϴ�.

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
                    // źȯ�� ������ �������� ���߽�Ű���� �ӵ��� �����մϴ�.
                    float angle = Random.Range(0f, 360f);
                    Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
                    bulletRb.velocity = direction * lustPatternData.spawnExplosionBulletSpeed;
                }
                else
                {
                    Debug.LogError("Explosion Bullet�� Rigidbody2D�� �����ϴ�.");
                }
            }
        }

        // ����Ʈ�� �ʱ�ȭ�մϴ�.
        spawnedExplosionBullets.Clear();
    }

    /// <summary>
    /// ���� ������ �����ϰų� �����մϴ�.
    /// </summary>
    /// <param name="value">���� ���� ����</param>
    public void SetAttackable(bool value)
    {
        if (value)
        {
            if (executePatternsCoroutine == null)
            {
                executePatternsCoroutine = StartCoroutine(ExecutePatterns());
                Debug.Log("LustBoss�� ������ �����մϴ�.");
            }
        }
        else
        {
            if (executePatternsCoroutine != null)
            {
                StopCoroutine(executePatternsCoroutine);
                executePatternsCoroutine = null;
                Debug.Log("LustBoss�� ������ �����Ǿ����ϴ�.");
            }
        }
    }

    /// <summary>
    /// �÷��̾��� ��ġ�� �����ɴϴ�.
    /// </summary>
    /// <returns>�÷��̾��� ��ġ ����</returns>
    private Vector3 GetPlayerPosition()
    {
        if (player != null)
        {
            return player.transform.position;
        }
        return Vector3.zero;
    }
}
