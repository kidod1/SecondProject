using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/IlchwiCheonil")]
public class IlchwiCheonil : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("레벨별 독 피해량")]
    public float[] poisonDamageLevels = { 10f, 15f, 20f, 25f, 30f }; // 레벨 1~5

    [Tooltip("레벨별 독 범위")]
    public float[] poisonRangeLevels = { 5f, 5.5f, 6f, 6.5f, 7f }; // 레벨 1~5

    [Tooltip("레벨별 독 지속 시간")]
    public float[] poisonDurationLevels = { 5f, 6f, 7f, 8f, 9f }; // 레벨 1~5

    [Tooltip("레벨별 독 구름 소환 간격 (초)")]
    public float[] spawnIntervalLevels = { 10f, 9f, 8f, 7f, 6f }; // 레벨 1~5

    [Tooltip("독 구름 프리팹")]
    public GameObject poisonCloudPrefab;

    private Player playerInstance;
    private Coroutine poisonCloudCoroutine;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("IlchwiCheonil Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;

        if (poisonCloudCoroutine == null)
        {
            poisonCloudCoroutine = player.StartCoroutine(SpawnPoisonCloudRoutine());
        }

        Debug.Log($"IlchwiCheonil이 적용되었습니다. 현재 레벨 Lv: {currentLevel + 1}");
    }

    /// <summary>
    /// 능력을 제거합니다.
    /// </summary>
    /// <param name="player">능력을 제거할 플레이어</param>
    public void Remove(Player player)
    {
        if (poisonCloudCoroutine != null)
        {
            player.StopCoroutine(poisonCloudCoroutine);
            poisonCloudCoroutine = null;
        }

        Debug.Log("IlchwiCheonil이 제거되었습니다.");
    }

    /// <summary>
    /// 독 구름을 주기적으로 소환하는 코루틴입니다.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator SpawnPoisonCloudRoutine()
    {
        while (true)
        {
            SpawnPoisonCloud();
            yield return new WaitForSeconds(GetCurrentSpawnInterval());
        }
    }

    /// <summary>
    /// 독 구름을 소환합니다.
    /// </summary>
    private void SpawnPoisonCloud()
    {
        if (poisonCloudPrefab == null)
        {
            Debug.LogError("IlchwiCheonil: 독구름 프리팹이 설정되지 않았습니다.");
            return;
        }

        Vector3 spawnPosition = playerInstance.transform.position;

        GameObject poisonCloud = Instantiate(poisonCloudPrefab, spawnPosition, Quaternion.identity);
        PoisonCloud poisonCloudScript = poisonCloud.GetComponent<PoisonCloud>();

        if (poisonCloudScript != null)
        {
            poisonCloudScript.Initialize(GetCurrentPoisonDamage(), GetCurrentPoisonRange(), GetCurrentPoisonDuration());
            Debug.Log($"IlchwiCheonil: 독구름 소환됨. 피해량: {GetCurrentPoisonDamage()}, 범위: {GetCurrentPoisonRange()}, 지속 시간: {GetCurrentPoisonDuration()}초");
        }
        else
        {
            Debug.LogError("IlchwiCheonil: PoisonCloud 스크립트를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

        if (playerInstance != null)
        {
            if (poisonCloudCoroutine != null)
            {
                playerInstance.StopCoroutine(poisonCloudCoroutine);
                poisonCloudCoroutine = null;
            }

            playerInstance = null;
        }

        Debug.Log("IlchwiCheonil 레벨이 초기화되었습니다.");
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 독 피해량, 범위, 지속 시간, 소환 간격을 증가시킵니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel이 5일 경우, currentLevel은 0~4
        {
            currentLevel++;
            Debug.Log($"IlchwiCheonil 업그레이드: 현재 레벨 {currentLevel + 1}");

            // 레벨 업 시 필요한 추가 로직이 있다면 여기에 추가
        }
        else
        {
            Debug.LogWarning("IlchwiCheonil: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력의 설명을 반환합니다.
    /// </summary>
    /// <returns>능력의 설명 문자열</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            float currentDamage = GetCurrentPoisonDamage();
            float currentRange = GetCurrentPoisonRange();
            float currentDuration = GetCurrentPoisonDuration();
            float currentInterval = GetCurrentSpawnInterval();

            return $"{baseDescription}\n(Lv {currentLevel + 1}: 독 피해량 {currentDamage}, 독 범위 {currentRange}m, 독 지속 시간 {currentDuration}초, 소환 간격 {currentInterval}초)";
        }
        else
        {
            float finalDamage = GetCurrentPoisonDamage();
            float finalRange = GetCurrentPoisonRange();
            float finalDuration = GetCurrentPoisonDuration();
            float finalInterval = GetCurrentSpawnInterval();

            return $"{baseDescription}\nMax Level: 독 피해량 {finalDamage}, 독 범위 {finalRange}m, 독 지속 시간 {finalDuration}초, 소환 간격 {finalInterval}초";
        }
    }

    /// <summary>
    /// 다음 레벨 증가에 필요한 값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨 증가 시 필요한 값</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < poisonDamageLevels.Length)
        {
            // 예시: 다음 레벨의 독 피해량을 반환
            return Mathf.RoundToInt(poisonDamageLevels[currentLevel]);
        }
        Debug.LogWarning($"IlchwiCheonil: currentLevel ({currentLevel})이 poisonDamageLevels 배열의 범위를 벗어났습니다. 기본값 1을 반환합니다.");
        return 1;
    }

    /// <summary>
    /// 현재 레벨의 독 피해량을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 독 피해량</returns>
    private float GetCurrentPoisonDamage()
    {
        if (currentLevel < poisonDamageLevels.Length)
        {
            return poisonDamageLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"IlchwiCheonil: currentLevel ({currentLevel})이 poisonDamageLevels 배열의 범위를 벗어났습니다. 기본값 {poisonDamageLevels[poisonDamageLevels.Length - 1]}을 반환합니다.");
            return poisonDamageLevels[poisonDamageLevels.Length - 1];
        }
    }

    /// <summary>
    /// 현재 레벨의 독 범위를 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 독 범위</returns>
    private float GetCurrentPoisonRange()
    {
        if (currentLevel < poisonRangeLevels.Length)
        {
            return poisonRangeLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"IlchwiCheonil: currentLevel ({currentLevel})이 poisonRangeLevels 배열의 범위를 벗어났습니다. 기본값 {poisonRangeLevels[poisonRangeLevels.Length - 1]}을 반환합니다.");
            return poisonRangeLevels[poisonRangeLevels.Length - 1];
        }
    }

    /// <summary>
    /// 현재 레벨의 독 지속 시간을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 독 지속 시간</returns>
    private float GetCurrentPoisonDuration()
    {
        if (currentLevel < poisonDurationLevels.Length)
        {
            return poisonDurationLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"IlchwiCheonil: currentLevel ({currentLevel})이 poisonDurationLevels 배열의 범위를 벗어났습니다. 기본값 {poisonDurationLevels[poisonDurationLevels.Length - 1]}을 반환합니다.");
            return poisonDurationLevels[poisonDurationLevels.Length - 1];
        }
    }

    /// <summary>
    /// 현재 레벨의 독 구름 소환 간격을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 독 구름 소환 간격</returns>
    private float GetCurrentSpawnInterval()
    {
        if (currentLevel < spawnIntervalLevels.Length)
        {
            return spawnIntervalLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"IlchwiCheonil: currentLevel ({currentLevel})이 spawnIntervalLevels 배열의 범위를 벗어났습니다. 기본값 {spawnIntervalLevels[spawnIntervalLevels.Length - 1]}을 반환합니다.");
            return spawnIntervalLevels[spawnIntervalLevels.Length - 1];
        }
    }
}
