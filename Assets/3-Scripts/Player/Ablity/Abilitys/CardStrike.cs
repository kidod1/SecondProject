using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/CardStrike")]
public class CardStrike : Ability
{
    public int hitThreshold = 5;
    public float damage = 50f;
    public float range = 10f;
    public GameObject cardPrefab;
    public float baseSpeedMultiplier = 1.0f;
    public float speedIncreaseMultiplier = 2.0f;
    private Player playerInstance;
    private int hitCount = 0;

    public override void Apply(Player player)
    {
        playerInstance = player;
    }

    public void OnProjectileHit(Collider2D enemy)
    {
        hitCount++;
        if (hitCount >= hitThreshold)
        {
            hitCount = 0;
            FireCard();
        }
    }

    private void FireCard()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("카드 프리팹이 없습니다.");
            return;
        }

        Vector3 spawnPosition = playerInstance.shootPoint.position;
        GameObject card = Instantiate(cardPrefab, spawnPosition, Quaternion.identity);
        Projectile cardScript = card.GetComponent<Projectile>();

        if (cardScript != null)
        {
            Vector2 randomDirection = GetRandomDirection();
            cardScript.Initialize(playerInstance.stat, playerInstance, true, baseSpeedMultiplier);
            cardScript.SetDirection(randomDirection);

            // 0.25초 후에 유도 시작 및 속도 증가
            playerInstance.StartCoroutine(DelayedTargetSearch(card, cardScript));
        }
    }

    private Vector2 GetRandomDirection()
    {
        float offsetX = Random.Range(0, 2) == 0 ? 1f : -1f;
        float offsetY = Random.Range(0, 2) == 0 ? 1f : -1f;
        return new Vector2(offsetX, offsetY).normalized;
    }

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

    public override void ResetLevel()
    {
        base.ResetLevel();
        hitCount = 0;
    }

    protected override int GetNextLevelIncrease()
    {
        return 0;
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
        }
    }
}
