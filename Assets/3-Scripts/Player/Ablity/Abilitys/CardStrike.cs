using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/CardStrike")]
public class CardStrike : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("��Ʈ �Ӱ谪. �� ���� �����ϸ� ī�带 �߻��մϴ�.")]
    public int hitThreshold = 5;

    [Tooltip("������ ī���� ������ ��")]
    public float[] damageLevels = { 50f, 60f, 70f, 80f, 90f }; // ���� 1~5

    [Tooltip("ī���� �߻� ��Ÿ�")]
    public float range = 10f;

    [Tooltip("�߻��� ī�� ������")]
    public GameObject cardPrefab;

    [Tooltip("ī���� �⺻ �ӵ� ����")]
    public float baseSpeedMultiplier = 1.0f;

    [Tooltip("���� �� �ӵ� ���� ����")]
    public float speedIncreaseMultiplier = 2.0f;

    private Player playerInstance;
    private int hitCount = 0;

    /// <summary>
    /// ���� ������ �ش��ϴ� ������ ���� ��ȯ�մϴ�.
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < damageLevels.Length)
        {
            return Mathf.RoundToInt(damageLevels[currentLevel]);
        }
        Debug.LogWarning($"CardStrike: currentLevel ({currentLevel})�� damageLevels �迭�� ������ ������ϴ�. �⺻�� 0�� ��ȯ�մϴ�.");
        return 0;
    }

    /// <summary>
    /// CardStrike �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("CardStrike Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;
    }

    /// <summary>
    /// ���� ������Ʈ�Ͽ� �¾��� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="enemy">���� ���� Collider2D</param>
    public void OnProjectileHit(Collider2D enemy)
    {
        hitCount++;
        if (hitCount >= hitThreshold)
        {
            hitCount = 0;
            FireCard();
        }
    }

    /// <summary>
    /// ī�带 �߻��մϴ�.
    /// </summary>
    private void FireCard()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("CardStrike: ī�� �������� �������� �ʾҽ��ϴ�.");
            return;
        }

        if (playerInstance == null)
        {
            Debug.LogWarning("CardStrike: playerInstance�� �������� �ʾҽ��ϴ�. Apply �޼��带 ���� ȣ���ϼ���.");
            return;
        }

        if (playerInstance.shootPoint == null)
        {
            Debug.LogError("CardStrike: playerInstance�� shootPoint�� �������� �ʾҽ��ϴ�.");
            return;
        }

        Vector3 spawnPosition = playerInstance.shootPoint.position;
        GameObject card = Instantiate(cardPrefab, spawnPosition, Quaternion.identity);
        Projectile cardScript = card.GetComponent<Projectile>();

        if (cardScript != null)
        {
            float currentDamage = GetCurrentDamage();
            Vector2 randomDirection = GetRandomDirection();
            cardScript.Initialize(playerInstance.stat, playerInstance, true, baseSpeedMultiplier, currentDamage);
            cardScript.SetDirection(randomDirection);

            // 0.25�� �Ŀ� ���� ���� �� �ӵ� ����
            playerInstance.StartCoroutine(DelayedTargetSearch(card, cardScript));
        }
        else
        {
            Debug.LogError("CardStrike: Projectile ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    /// <summary>
    /// ������ ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>����ȭ�� ���� ����2 ����</returns>
    private Vector2 GetRandomDirection()
    {
        float offsetX = UnityEngine.Random.Range(0, 2) == 0 ? 1f : -1f;
        float offsetY = UnityEngine.Random.Range(0, 2) == 0 ? 1f : -1f;
        return new Vector2(offsetX, offsetY).normalized;
    }

    /// <summary>
    /// ���� Ÿ���� ã�� ������ �����ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <param name="card">�߻�� ī�� ���� ������Ʈ</param>
    /// <param name="cardScript">ī���� Projectile ��ũ��Ʈ</param>
    private IEnumerator DelayedTargetSearch(GameObject card, Projectile cardScript)
    {
        yield return new WaitForSeconds(0.25f);

        Collider2D closestEnemy = FindClosestEnemy(card.transform.position);

        if (closestEnemy != null)
        {
            Vector2 directionToEnemy = (closestEnemy.transform.position - card.transform.position).normalized;
            cardScript.SetDirection(directionToEnemy, speedIncreaseMultiplier);
        }
        else
        {
            Destroy(card);
        }
    }

    /// <summary>
    /// ������ ��ġ���� ���� ����� ���� ã���ϴ�.
    /// </summary>
    /// <param name="position">�˻��� ������ ��ġ</param>
    /// <returns>���� ����� ���� Collider2D �Ǵ� null</returns>
    private Collider2D FindClosestEnemy(Vector3 position)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(position, range);
        Collider2D closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            if (enemy.GetComponent<Monster>())
            {
                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        return closestEnemy;
    }

    /// <summary>
    /// CardStrike �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ī���� �������� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            Debug.Log($"CardStrike ���׷��̵�: ���� ���� {currentLevel}");

            // ���׷��̵� �� ������ ���� ����
            // �������� damageLevels �迭�� ���� �������� �����ɴϴ�.
            // ������ ������ �ʿ� �����ϴ�.
        }
        else
        {
            Debug.LogWarning("CardStrike: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// ���� ������ �´� �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ������ ��</returns>
    private float GetCurrentDamage()
    {
        if (currentLevel < damageLevels.Length)
        {
            return damageLevels[currentLevel];
        }
        Debug.LogWarning($"CardStrike: currentLevel ({currentLevel})�� damageLevels �迭�� ������ ������ϴ�. �⺻�� {damageLevels[0]}�� ��ȯ�մϴ�.");
        return damageLevels[0];
    }

    /// <summary>
    /// �ɷ� ������ �������̵��Ͽ� ���� �� �� ������ �������� ���Խ�ŵ�ϴ�.
    /// </summary>
    /// <returns>�ɷ��� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            float damageIncrease = damageLevels[currentLevel];
            return $"{baseDescription}{Environment.NewLine}(Level {currentLevel + 1}: +{damageIncrease} ������)";
        }
        else
        {
            float finalDamage = damageLevels[currentLevel];
            return $"{baseDescription}{Environment.NewLine}(Max Level: +{finalDamage} ������)";
        }
    }

    /// <summary>
    /// �ɷ��� ������ �ʱ�ȭ�ϰ� ��Ʈ ī��Ʈ�� �����մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        hitCount = 0;
        currentLevel = 0;
    }
    private void OnValidate()
    {
        if (damageLevels.Length != maxLevel)
        {
            Debug.LogWarning($"CardStrike: damageLevels �迭�� ���̰� maxLevel ({maxLevel})�� ��ġ���� �ʽ��ϴ�. �迭 ���̸� ����ϴ�.");
            Array.Resize(ref damageLevels, maxLevel);
        }
    }
}