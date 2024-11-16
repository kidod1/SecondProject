using System;
using System.Collections;
using UnityEngine;
using AK.Wwise; // Step 1: WWISE 네임스페이스 추가

[CreateAssetMenu(menuName = "Abilities/CardStrike")]
public class CardStrike : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("히트 임계값. 이 값에 도달하면 카드를 발사합니다.")]
    public int hitThreshold = 5;

    [Tooltip("레벨별 카드의 데미지 값")]
    public int[] damageLevels = { 50, 60, 70, 80, 90 }; // 레벨 1~5

    [Tooltip("카드의 발사 사거리")]
    public float range = 10f;

    [Tooltip("발사할 카드 프리팹")]
    public GameObject cardPrefab;

    [Tooltip("카드의 기본 속도 배율")]
    public float baseSpeedMultiplier = 1.0f;

    [Tooltip("유도 시 속도 증가 배율")]
    public float speedIncreaseMultiplier = 2.0f;

    [Header("WWISE Sound Events")]
    [Tooltip("CardStrike 능력 발동 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event activateSound; // Step 2: WWISE 이벤트 필드 추가

    [Tooltip("카드 발사 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event fireCardSound; // 추가: 카드 발사 사운드 필드

    private Player playerInstance;
    private int hitCount = 0;

    private GameObject currentCardInstance; // 현재 생성된 카드 오브젝트

    /// <summary>
    /// 현재 레벨에 해당하는 데미지 값을 반환합니다.
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
    /// CardStrike 능력을 플레이어에게 적용합니다.
    /// </summary>
    public override void Apply(Player player)
    {
        if (player == null)
            return;

        playerInstance = player;

        // 능력이 적용될 때 활성화 사운드 재생
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// 플레이어가 적을 적중시켰을 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="enemy">맞은 적의 Collider2D</param>
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
    /// 카드를 발사합니다.
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

        // 카드 발사 시 사운드 재생
        if (fireCardSound != null && playerInstance != null)
        {
            fireCardSound.Post(playerInstance.gameObject);
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
    /// CardStrike 능력을 업그레이드합니다. 레벨이 증가할 때마다 데미지가 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
        }
    }

    /// <summary>
    /// 현재 레벨에 맞는 데미지를 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 데미지 값</returns>
    private int GetCurrentDamage()
    {
        if (currentLevel < damageLevels.Length)
        {
            return damageLevels[currentLevel];
        }
        return damageLevels[damageLevels.Length - 1];
    }

    /// <summary>
    /// 능력 설명을 오버라이드하여 레벨 업 시 데미지 증가량을 포함시킵니다.
    /// </summary>
    /// <returns>능력의 설명 문자열</returns>
    public override string GetDescription()
    {
        if (currentLevel < damageLevels.Length && currentLevel >= 0)
        {
            int damageIncrease = damageLevels[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 적을 {hitThreshold}회 맞출 때마다 카드를 발사합니다. 데미지 +{damageIncrease}";
        }
        else if (currentLevel >= damageLevels.Length)
        {
            int maxDamageIncrease = damageLevels[damageLevels.Length - 1];
            return $"{baseDescription}\n최대 레벨 도달: 적을 {hitThreshold}회 맞출 때마다 카드를 발사합니다. 데미지 +{maxDamageIncrease}";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달.";
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

        if (currentCardInstance != null)
        {
            Destroy(currentCardInstance);
            currentCardInstance = null;
        }
    }

    /// <summary>
    /// Editor 상에서 유효성 검사
    /// </summary>
    private void OnValidate()
    {
        if (damageLevels.Length != maxLevel)
        {
            Array.Resize(ref damageLevels, maxLevel);
        }
    }
}
