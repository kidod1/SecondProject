using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    // ���� ���� �ڷ�ƾ�� �����ϱ� ���� ����
    private Coroutine executePatternsCoroutine;

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

        // ���� ������ �����մϴ�.
        SetAttackable(true);
    }

    protected override void Die()
    {
        base.Die();

        // �߰������� �ʿ��� ��� ó�� ������ �ִٸ� ���⿡ �߰��մϴ�.
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

            // ������ �����ϰ� �����մϴ�. ���� 1~4�� ������ �����Ǿ� ����
            int patternIndex = Random.Range(1, 5); // 1, 2, 3, 4 �� ���� ����

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
                    Debug.LogWarning("�� �� ���� ���� �ε����Դϴ�.");
                    break;
            }

            yield return new WaitForSeconds(1f); // ���� �� ��� �ð�
        }
    }

    // 1�� ����: ���� źȯ ����
    private IEnumerator CircleBulletPattern()
    {
        Debug.Log("���� źȯ ���� ����");

        int repeatCount = lustPatternData.circlePatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            foreach (Transform spawnPoint in circleBulletSpawnPoints)
            {
                SpawnCircleBullets(spawnPoint);
            }

            yield return new WaitForSeconds(1f); // ��� źȯ�� ��ȯ�� �� ��� �ð�

            ActivateCircleBullets();

            yield return new WaitForSeconds(0.5f); // ���� �ݺ� �� ��� �ð�
        }
    }

    private List<GameObject> spawnedCircleBullets = new List<GameObject>();

    private void SpawnCircleBullets(Transform spawnPoint)
    {
        int bulletCount = lustPatternData.circleBulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            GameObject bullet = Instantiate(lustPatternData.circleBulletPrefab, spawnPoint.position, Quaternion.identity, patternParent);
            bullet.SetActive(false); // �ϴ� ��Ȱ��ȭ�Ͽ� ���� ���·� �Ӵϴ�.

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
                    // źȯ�� ���� ��ġ���� ��������� �߻�ǵ��� �ӵ��� �����մϴ�.
                    Vector2 direction = (bullet.transform.position - transform.position).normalized;
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
            foreach (Transform spawnPoint in heartBulletSpawnPoints)
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
