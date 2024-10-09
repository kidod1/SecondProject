using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/CardStrike")]
public class CardStrike : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("히트 임계값. 이 값에 도달하면 카드를 발사합니다.")]
    public int hitThreshold = 5;

    [Tooltip("레벨별 카드의 데미지 값")]
    public float[] damageLevels = { 50f, 60f, 70f, 80f, 90f }; // 레벨 1~5

    [Tooltip("카드의 발사 사거리")]
    public float range = 10f;

    [Tooltip("발사할 카드 프리팹")]
    public GameObject cardPrefab;

    [Tooltip("카드의 기본 속도 배율")]
    public float baseSpeedMultiplier = 1.0f;

    [Tooltip("유도 시 속도 증가 배율")]
    public float speedIncreaseMultiplier = 2.0f;

    private Player playerInstance;
    private int hitCount = 0;

    /// <summary>
    /// 현재 레벨에 해당하는 데미지 값을 반환합니다.
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < damageLevels.Length)
        {
            return Mathf.RoundToInt(damageLevels[currentLevel]);
        }
        Debug.LogWarning($"CardStrike: currentLevel ({currentLevel})이 damageLevels 배열의 범위를 벗어났습니다. 기본값 0을 반환합니다.");
        return 0;
    }

    /// <summary>
    /// CardStrike 능력을 플레이어에게 적용합니다.
    /// </summary>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("CardStrike Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;
    }

    /// <summary>
    /// 적이 프로젝트일에 맞았을 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="enemy">맞은 적의 Collider2D</param>
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
    /// 카드를 발사합니다.
    /// </summary>
    private void FireCard()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("CardStrike: 카드 프리팹이 설정되지 않았습니다.");
            return;
        }

        if (playerInstance == null)
        {
            Debug.LogWarning("CardStrike: playerInstance가 설정되지 않았습니다. Apply 메서드를 먼저 호출하세요.");
            return;
        }

        if (playerInstance.shootPoint == null)
        {
            Debug.LogError("CardStrike: playerInstance에 shootPoint가 설정되지 않았습니다.");
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

            // 0.25초 후에 유도 시작 및 속도 증가
            playerInstance.StartCoroutine(DelayedTargetSearch(card, cardScript));
        }
        else
        {
            Debug.LogError("CardStrike: Projectile 스크립트를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 랜덤한 방향을 반환합니다.
    /// </summary>
    /// <returns>정규화된 랜덤 벡터2 방향</returns>
    private Vector2 GetRandomDirection()
    {
        float offsetX = UnityEngine.Random.Range(0, 2) == 0 ? 1f : -1f;
        float offsetY = UnityEngine.Random.Range(0, 2) == 0 ? 1f : -1f;
        return new Vector2(offsetX, offsetY).normalized;
    }

    /// <summary>
    /// 유도 타겟을 찾아 방향을 설정하는 코루틴입니다.
    /// </summary>
    /// <param name="card">발사된 카드 게임 오브젝트</param>
    /// <param name="cardScript">카드의 Projectile 스크립트</param>
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
    /// 지정된 위치에서 가장 가까운 적을 찾습니다.
    /// </summary>
    /// <param name="position">검색을 시작할 위치</param>
    /// <returns>가장 가까운 적의 Collider2D 또는 null</returns>
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
    /// CardStrike 능력을 업그레이드합니다. 레벨이 증가할 때마다 카드의 데미지가 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            Debug.Log($"CardStrike 업그레이드: 현재 레벨 {currentLevel}");

            // 업그레이드 후 데미지 증가 적용
            // 데미지는 damageLevels 배열을 통해 동적으로 가져옵니다.
            // 별도의 로직은 필요 없습니다.
        }
        else
        {
            Debug.LogWarning("CardStrike: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 현재 레벨에 맞는 데미지를 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 데미지 값</returns>
    private float GetCurrentDamage()
    {
        if (currentLevel < damageLevels.Length)
        {
            return damageLevels[currentLevel];
        }
        Debug.LogWarning($"CardStrike: currentLevel ({currentLevel})이 damageLevels 배열의 범위를 벗어났습니다. 기본값 {damageLevels[0]}을 반환합니다.");
        return damageLevels[0];
    }

    /// <summary>
    /// 능력 설명을 오버라이드하여 레벨 업 시 데미지 증가량을 포함시킵니다.
    /// </summary>
    /// <returns>능력의 설명 문자열</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            float damageIncrease = damageLevels[currentLevel];
            return $"{baseDescription}{Environment.NewLine}(Level {currentLevel + 1}: +{damageIncrease} 데미지)";
        }
        else
        {
            float finalDamage = damageLevels[currentLevel];
            return $"{baseDescription}{Environment.NewLine}(Max Level: +{finalDamage} 데미지)";
        }
    }

    /// <summary>
    /// 능력의 레벨을 초기화하고 히트 카운트를 리셋합니다.
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
            Debug.LogWarning($"CardStrike: damageLevels 배열의 길이가 maxLevel ({maxLevel})과 일치하지 않습니다. 배열 길이를 맞춥니다.");
            Array.Resize(ref damageLevels, maxLevel);
        }
    }
}