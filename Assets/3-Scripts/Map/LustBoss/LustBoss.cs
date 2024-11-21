using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine.UI; // Import Spine Unity namespace

public class LustBoss : Monster
{
    // �ǰ� �� ���� ������ ���� �ʵ� �߰�
    private MeshRenderer redMeshRenderer;
    private Color bossOriginalColor;

    [Header("Animation Settings")]
    [SerializeField, SpineAnimation]
    private string idleAnimationName;

    [SerializeField, SpineAnimation]
    private string attackAnimationName;

    [SerializeField, Tooltip("Spine SkeletonAnimation component")]
    private SkeletonAnimation skeletonAnimation; // SkeletonAnimation ���� �߰�

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

    [Header("Specified Direction Pattern Settings")]
    [SerializeField, Tooltip("������ ���� ������ �߻� ������")]
    private Transform[] specifiedPatternSpawnPoints;

    [SerializeField, Tooltip("������ ���� ������ ��ǥ ������")]
    private Transform[] specifiedPatternTargetPoints;

    [Header("Specified Direction Pattern Sound Settings")]
    [SerializeField, Tooltip("������ ���� ������ ���� �̺�Ʈ")]
    private AK.Wwise.Event specifiedDirectionPatternSound;

    [Header("Circle Bullet Pattern Sound Settings")]
    [SerializeField, Tooltip("���� źȯ ������ ���� �̺�Ʈ")]
    private AK.Wwise.Event circleBulletPatternSound;

    [Header("Heart Bullet Pattern Sound Settings")]
    [SerializeField, Tooltip("��Ʈ źȯ ������ ���� �̺�Ʈ")]
    private AK.Wwise.Event heartBulletPatternSound;

    [Header("Angle Bullet Pattern Sound Settings")]
    [SerializeField, Tooltip("���� źȯ ������ ���� �̺�Ʈ")]
    private AK.Wwise.Event angleBulletPatternSound;

    [Header("Spawn Explosion Pattern Sound Settings")]
    [SerializeField, Tooltip("���� �� ���� ������ ���� �̺�Ʈ")]
    private AK.Wwise.Event spawnExplosionPatternSound;

    [Header("Death Transition Settings")]
    [SerializeField, Tooltip("���̵� �ο� ����� UI Image")]
    private Image fadeInImage; // ���̵� ���� ���� UI Image

    [SerializeField, Tooltip("���� ��� �� ����� Wwise ���� �̺�Ʈ")]
    private AK.Wwise.Event bossDeathSound; // ���� ��� ���� �̺�Ʈ

    [SerializeField, Tooltip("���� ��� �� ��ȯ�� ���� �̸�")]
    private string deathTransitionSceneName; // ���� ��� �� ��ȯ�� �� �̸�


    // �߰�: PlayerUIManager ����
    [Header("UI Manager")]
    [SerializeField, Tooltip("PlayerUIManager")]
    private PlayerUIManager playerUIManager;

    // ���� ���� �ڷ�ƾ�� �����ϱ� ���� ����
    private Coroutine executePatternsCoroutine;

    // ��ȯ�� źȯ���� �����ϱ� ���� ����Ʈ
    private List<GameObject> spawnedCircleBullets = new List<GameObject>();
    private List<GameObject> spawnedHeartBullets = new List<GameObject>();
    private List<GameObject> spawnedExplosionBullets = new List<GameObject>();

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

        // MeshRenderer�� ���� ���� ����
        redMeshRenderer = GetComponent<MeshRenderer>();
        if (redMeshRenderer != null)
        {
            // ���׸��� �ν��Ͻ�ȭ
            redMeshRenderer.material = new Material(redMeshRenderer.material);
            bossOriginalColor = redMeshRenderer.material.color;
        }
        else
        {
            Debug.LogWarning("MeshRenderer�� ã�� �� �����ϴ�.");
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
            tempColor.a = 0f; // ������ �����ϰ� ����
            fadeInImage.color = tempColor;
            fadeInImage.gameObject.SetActive(false); // ���� �� ��Ȱ��ȭ
        }
        else
        {
            Debug.LogWarning("LustBoss: FadeInImage�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // Play Idle animation initially
        PlayIdleAnimation();

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
            StartCoroutine(FlashRedCoroutine());
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

        // ���� ��� �� ���̵� �� �� �� ��ȯ �ڷ�ƾ ����
        if (fadeInImage != null && bossDeathSound != null && !string.IsNullOrEmpty(deathTransitionSceneName))
        {
            StartCoroutine(FadeInAndTransition());
        }
        else
        {
            Debug.LogWarning("LustBoss: ���̵� ���̳� �� ��ȯ�� �ʿ��� ������ �����Ǿ����ϴ�.");
            // �ʿ��� ������ ���� ��� ��� �� ��ȯ
            LoadDeathScene();
        }
    }
    /// <summary>
    /// ���̵� �� ȿ���� �����ϰ� ���带 ����� �� ���� ��ȯ�ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <returns>�ڷ�ƾ�� IEnumerator</returns>
    private IEnumerator FadeInAndTransition()
    {
        // ���̵� �� �̹��� Ȱ��ȭ
        fadeInImage.gameObject.SetActive(true);

        // ���̵� �� ���� ���
        bossDeathSound?.Post(gameObject);

        // ���̵� �� �ð� ���� (��: 2��)
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

        // ������ ���̵� �ε� ����
        color.a = 1f;
        fadeInImage.color = color;

        // 3�� ���
        yield return new WaitForSeconds(3f);

        // �� ��ȯ
        LoadDeathScene();
    }

    /// <summary>
    /// ��� �� ������ ���� �ε��ϴ� �޼����Դϴ�.
    /// </summary>
    private void LoadDeathScene()
    {
        if (string.IsNullOrEmpty(deathTransitionSceneName))
        {
            Debug.LogError("LustBoss: DeathTransitionSceneName�� �������� �ʾҽ��ϴ�.");
            return;
        }

        // �� �ε�
        UnityEngine.SceneManagement.SceneManager.LoadScene(deathTransitionSceneName);
    }
    /// <summary>
    /// ���Ͱ� �������� ���� �� �Ӱ� �����̴� ȿ���� ó���ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <returns>�ڷ�ƾ�� IEnumerator</returns>
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

            // Play Attack animation
            PlayAttackAnimation();

            // Wait for the Attack animation to complete
            // This relies on the OnAttackAnimationComplete callback to switch back to Idle

            // Execute the selected pattern
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

    // 1�� ����: ���� źȯ ����
    private IEnumerator CircleBulletPattern()
    {
        Debug.Log("���� źȯ ���� ����");

        int repeatCount = lustPatternData.circlePatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            circleBulletPatternSound?.Post(gameObject);
            foreach (Transform spawnPoint in circleBulletSpawnPoints)
            {
                // źȯ ��ȯ
                SpawnCircleBullets(spawnPoint);

                // 0.1�� ���
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
            heartBulletPatternSound?.Post(gameObject);
            ActivateHeartBullets();

            yield return new WaitForSeconds(0.5f); // ���� �ݺ� �� ��� �ð�
        }
    }

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
        int repeatCount = lustPatternData.anglePatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            angleBulletPatternSound?.Post(gameObject);
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
        float angleOffset = 15f; // �� źȯ �� ���� ����

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
        int repeatCount = lustPatternData.spawnExplosionPatternRepeatCount;

        for (int i = 0; i < repeatCount; i++)
        {
            foreach (Transform spawnPoint in spawnExplosionSpawnPoints)
            {
                SpawnExplosionBullets(spawnPoint);
            }

            yield return new WaitForSeconds(1f); // ��� źȯ�� ��ȯ�� �� ��� �ð�
            spawnExplosionPatternSound?.Post(gameObject);
            ActivateExplosionBullets();

            yield return new WaitForSeconds(0.5f); // ���� �ݺ� �� ��� �ð�
        }
    }

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

    // 5�� ����: ������ ���� ����
    private IEnumerator SpecifiedDirectionPattern()
    {
        Debug.Log("������ ���� ���� ����");

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

        Debug.Log("������ ���� ���� ����");
    }

    private void FireSpecifiedBullets()
    {
        if (specifiedPatternSpawnPoints == null || specifiedPatternTargetPoints == null)
        {
            Debug.LogWarning("�߻� ���� �Ǵ� ��ǥ �������� �������� �ʾҽ��ϴ�.");
            return;
        }

        if (specifiedPatternSpawnPoints.Length != specifiedPatternTargetPoints.Length)
        {
            Debug.LogWarning("�߻� ������ ��ǥ ������ ������ ��ġ���� �ʽ��ϴ�.");
            return;
        }

        for (int i = 0; i < specifiedPatternSpawnPoints.Length; i++)
        {
            Transform spawnPoint = specifiedPatternSpawnPoints[i];
            Transform targetPoint = specifiedPatternTargetPoints[i];

            if (spawnPoint == null || targetPoint == null)
            {
                Debug.LogWarning($"�ε��� {i}�� �߻� ���� �Ǵ� ��ǥ ������ null�Դϴ�.");
                continue;
            }

            // źȯ ����
            GameObject bullet = Instantiate(lustPatternData.specifiedPatternBulletPrefab, spawnPoint.position, Quaternion.identity, patternParent);

            // ���� ����
            Vector2 direction = (targetPoint.position - spawnPoint.position).normalized;

            // źȯ�� �ӵ� ����
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = direction * lustPatternData.specifiedPatternBulletSpeed;
            }
            else
            {
                Debug.LogError("������ ���� źȯ�� Rigidbody2D�� �����ϴ�.");
            }

            bullet.SetActive(true);
        }
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
