using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/CoinBetting")]
public class CoinBetting : Ability
{
    [Header("Coin Bomb Settings")]
    [Tooltip("각 레벨에서 폭탄의 피해량")]
    public int[] damagePerLevel = { 50, 60, 70, 80, 90 }; // 레벨 1~5

    [Tooltip("각 레벨에서 폭탄을 생성하는 쿨다운 시간 (초)")]
    public float[] cooldownPerLevel = { 5f, 4.5f, 4f, 3.5f, 3f }; // 레벨 1~5

    [Tooltip("각 레벨에서 폭탄의 지속 시간 (초)")]
    public float[] bombDurationPerLevel = { 10f, 12f, 14f, 16f, 18f }; // 레벨 1~5

    [Tooltip("생성할 코인 폭탄 프리팹")]
    public GameObject coinBombPrefab;

    [Tooltip("폭발 시 생성할 이펙트 프리팹")]
    public GameObject explosionEffectPrefab; // 추가된 변수

    private Player playerInstance;
    private Coroutine bombCoroutine;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("CoinBetting Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;

        // 코인 폭탄 생성 코루틴 시작
        if (bombCoroutine == null)
        {
            bombCoroutine = player.StartCoroutine(DropCoinBombs());
        }
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 쿨타임과 피해량, 폭탄 지속 시간이 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
            Debug.Log($"CoinBetting 업그레이드: 현재 레벨 {currentLevel + 1}");

            // 업그레이드 후 데미지, 쿨타임, 폭탄 지속 시간 업데이트
            if (playerInstance != null)
            {
                // 필요 시 추가 로직을 구현할 수 있습니다.
            }
        }
        else
        {
            Debug.LogWarning("CoinBetting: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        // 코루틴 정지
        if (bombCoroutine != null)
        {
            playerInstance.StopCoroutine(bombCoroutine);
            bombCoroutine = null;
        }

        // 현재 적용된 버프 제거
        RemoveCurrentBuff();
        currentLevel = 0;
    }

    /// <summary>
    /// 다음 레벨의 데미지 증가값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨에서의 피해량 증가량</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < damagePerLevel.Length)
        {
            return damagePerLevel[currentLevel + 1] - damagePerLevel[currentLevel];
        }
        return 0;
    }

    /// <summary>
    /// 일정 시간마다 코인 폭탄을 떨어뜨리는 코루틴
    /// </summary>
    private IEnumerator DropCoinBombs()
    {
        while (true)
        {
            DropCoinBomb();
            float currentCooldown = GetCurrentCooldownTime();
            yield return new WaitForSeconds(currentCooldown);
        }
    }

    /// <summary>
    /// 현재 플레이어 위치에 코인 폭탄을 생성합니다.
    /// </summary>
    private void DropCoinBomb()
    {
        if (coinBombPrefab == null)
        {
            Debug.LogError("CoinBetting: coinBombPrefab이 설정되지 않았습니다.");
            return;
        }

        Vector3 spawnPosition = playerInstance.transform.position;
        GameObject coinBombInstance = Instantiate(coinBombPrefab, spawnPosition, Quaternion.identity);

        CoinBomb coinBombScript = coinBombInstance.GetComponent<CoinBomb>();
        if (coinBombScript != null)
        {
            int currentDamage = GetCurrentDamage();
            float currentBombDuration = GetCurrentBombDuration();

            // 폭발 이펙트 프리팹을 함께 초기화
            coinBombScript.Initialize(currentDamage, currentBombDuration, explosionEffectPrefab);
        }
        else
        {
            Debug.LogError("CoinBetting: CoinBomb 스크립트를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 현재 레벨에 맞는 피해량을 반환합니다.
    /// </summary>
    private int GetCurrentDamage()
    {
        if (currentLevel < damagePerLevel.Length)
        {
            return damagePerLevel[currentLevel];
        }
        Debug.LogWarning($"CoinBetting: currentLevel ({currentLevel})이 damagePerLevel 배열의 범위를 벗어났습니다. 기본값 {damagePerLevel[damagePerLevel.Length - 1]}을 반환합니다.");
        return damagePerLevel[damagePerLevel.Length - 1];
    }

    /// <summary>
    /// 현재 레벨에 맞는 폭탄 지속 시간을 반환합니다.
    /// </summary>
    private float GetCurrentBombDuration()
    {
        if (currentLevel < bombDurationPerLevel.Length)
        {
            return bombDurationPerLevel[currentLevel];
        }
        Debug.LogWarning($"CoinBetting: currentLevel ({currentLevel})이 bombDurationPerLevel 배열의 범위를 벗어났습니다. 기본값 {bombDurationPerLevel[bombDurationPerLevel.Length - 1]}을 반환합니다.");
        return bombDurationPerLevel[bombDurationPerLevel.Length - 1];
    }

    /// <summary>
    /// 현재 레벨에 맞는 쿨타임 시간을 반환합니다.
    /// </summary>
    private float GetCurrentCooldownTime()
    {
        if (currentLevel < cooldownPerLevel.Length)
        {
            return cooldownPerLevel[currentLevel];
        }
        Debug.LogWarning($"CoinBetting: currentLevel ({currentLevel})이 cooldownPerLevel 배열의 범위를 벗어났습니다. 기본값 {cooldownPerLevel[cooldownPerLevel.Length - 1]}을 반환합니다.");
        return cooldownPerLevel[cooldownPerLevel.Length - 1];
    }

    /// <summary>
    /// 현재 버프를 제거합니다.
    /// </summary>
    private void RemoveCurrentBuff()
    {
        // 현재 구현된 버프가 없으므로 필요 시 추가 구현
    }

    /// <summary>
    /// 능력의 설명을 반환합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";
        if (currentLevel < damagePerLevel.Length && currentLevel < cooldownPerLevel.Length && currentLevel < bombDurationPerLevel.Length)
        {
            description += $"레벨 {currentLevel + 1}:\n" +
                           $"- 폭탄 피해량: {damagePerLevel[currentLevel]}\n" +
                           $"- 쿨다운: {cooldownPerLevel[currentLevel]}초\n" +
                           $"- 폭탄 지속 시간: {bombDurationPerLevel[currentLevel]}초";
        }
        else
        {
            description += "최대 레벨에 도달했습니다.";
        }
        return description;
    }


    private void OnValidate()
    {
        // 배열의 길이가 maxLevel과 일치하도록 조정
        if (damagePerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"CoinBetting: damagePerLevel 배열의 길이가 maxLevel ({maxLevel})과 일치하지 않습니다. 배열 길이를 맞춥니다.");
            System.Array.Resize(ref damagePerLevel, maxLevel);
        }

        if (cooldownPerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"CoinBetting: cooldownPerLevel 배열의 길이가 maxLevel ({maxLevel})과 일치하지 않습니다. 배열 길이를 맞춥니다.");
            System.Array.Resize(ref cooldownPerLevel, maxLevel);
        }

        if (bombDurationPerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"CoinBetting: bombDurationPerLevel 배열의 길이가 maxLevel ({maxLevel})과 일치하지 않습니다. 배열 길이를 맞춥니다.");
            System.Array.Resize(ref bombDurationPerLevel, maxLevel);
        }
    }
}
