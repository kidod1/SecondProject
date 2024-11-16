using System;
using System.Collections;
using UnityEngine;
using AK.Wwise; // Step 1: WWISE ���ӽ����̽� �߰�

[CreateAssetMenu(menuName = "Abilities/CardStrike")]
public class CardStrike : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("��Ʈ �Ӱ谪. �� ���� �����ϸ� ī�带 �߻��մϴ�.")]
    public int hitThreshold = 5;

    [Tooltip("������ ī���� ������ ��")]
    public int[] damageLevels = { 50, 60, 70, 80, 90 }; // ���� 1~5

    [Tooltip("ī���� �߻� ��Ÿ�")]
    public float range = 10f;

    [Tooltip("�߻��� ī�� ������")]
    public GameObject cardPrefab;

    [Tooltip("ī���� �⺻ �ӵ� ����")]
    public float baseSpeedMultiplier = 1.0f;

    [Tooltip("���� �� �ӵ� ���� ����")]
    public float speedIncreaseMultiplier = 2.0f;

    [Header("WWISE Sound Events")]
    [Tooltip("CardStrike �ɷ� �ߵ� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event activateSound; // Step 2: WWISE �̺�Ʈ �ʵ� �߰�

    [Tooltip("ī�� �߻� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event fireCardSound; // �߰�: ī�� �߻� ���� �ʵ�

    private Player playerInstance;
    private int hitCount = 0;

    private GameObject currentCardInstance; // ���� ������ ī�� ������Ʈ

    /// <summary>
    /// ���� ������ �ش��ϴ� ������ ���� ��ȯ�մϴ�.
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < damageLevels.Length)
        {
            return Mathf.RoundToInt(damageLevels[currentLevel]);
        }
        return 0;
    }

    /// <summary>
    /// CardStrike �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    public override void Apply(Player player)
    {
        if (player == null)
            return;

        playerInstance = player;

        // �ɷ��� ����� �� Ȱ��ȭ ���� ���
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// �÷��̾ ���� ���߽����� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="enemy">���� ���� Collider2D</param>
    public void OnProjectileHit(Collider2D enemy)
    {
        if (enemy == null)
            return;

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
        if (cardPrefab == null || playerInstance == null || playerInstance.shootPoint == null)
            return;

        Vector3 spawnPosition = playerInstance.shootPoint.position;
        GameObject card = Instantiate(cardPrefab, spawnPosition, Quaternion.identity);
        Projectile cardScript = card.GetComponent<Projectile>();

        if (cardScript != null)
        {
            int currentDamage = GetCurrentDamage();
            Vector2 randomDirection = GetRandomDirection();
            cardScript.Initialize(playerInstance.stat, playerInstance, true, baseSpeedMultiplier, currentDamage);
            cardScript.SetDirection(randomDirection);

            currentCardInstance = card;

            playerInstance.StartCoroutine(DelayedTargetSearch(card, cardScript));
        }

        // ī�� �߻� �� ���� ���
        if (fireCardSound != null && playerInstance != null)
        {
            fireCardSound.Post(playerInstance.gameObject);
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

        if (card == null || cardScript == null)
            yield break;

        Collider2D closestEnemy = FindClosestEnemy(card.transform.position);

        if (closestEnemy != null && closestEnemy.gameObject != null)
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
            if (enemy == null)
                continue;

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
    /// CardStrike �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ �������� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
        }
    }

    /// <summary>
    /// ���� ������ �´� �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ������ ��</returns>
    private int GetCurrentDamage()
    {
        if (currentLevel < damageLevels.Length)
        {
            return damageLevels[currentLevel];
        }
        return damageLevels[damageLevels.Length - 1];
    }

    /// <summary>
    /// �ɷ� ������ �������̵��Ͽ� ���� �� �� ������ �������� ���Խ�ŵ�ϴ�.
    /// </summary>
    /// <returns>�ɷ��� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < damageLevels.Length && currentLevel >= 0)
        {
            int damageIncrease = damageLevels[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: ���� {hitThreshold}ȸ ���� ������ ī�带 �߻��մϴ�. ������ +{damageIncrease}";
        }
        else if (currentLevel >= damageLevels.Length)
        {
            int maxDamageIncrease = damageLevels[damageLevels.Length - 1];
            return $"{baseDescription}\n�ִ� ���� ����: ���� {hitThreshold}ȸ ���� ������ ī�带 �߻��մϴ�. ������ +{maxDamageIncrease}";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����.";
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

        if (currentCardInstance != null)
        {
            Destroy(currentCardInstance);
            currentCardInstance = null;
        }
    }

    /// <summary>
    /// Editor �󿡼� ��ȿ�� �˻�
    /// </summary>
    private void OnValidate()
    {
        if (damageLevels.Length != maxLevel)
        {
            Array.Resize(ref damageLevels, maxLevel);
        }
    }
}
