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

    public override void Apply(Player player)
    {
        if (player == null) return;

        playerInstance = player;

        if (poisonCloudCoroutine == null)
        {
            poisonCloudCoroutine = player.StartCoroutine(SpawnPoisonCloudRoutine());
        }
    }

    public void Remove(Player player)
    {
        if (poisonCloudCoroutine != null)
        {
            player.StopCoroutine(poisonCloudCoroutine);
            poisonCloudCoroutine = null;
        }
    }

    private IEnumerator SpawnPoisonCloudRoutine()
    {
        while (true)
        {
            SpawnPoisonCloud();
            yield return new WaitForSeconds(GetCurrentSpawnInterval());
        }
    }

    private void SpawnPoisonCloud()
    {
        if (poisonCloudPrefab == null) return;

        Vector3 spawnPosition = playerInstance.transform.position;

        GameObject poisonCloud = Instantiate(poisonCloudPrefab, spawnPosition, Quaternion.identity);
        PoisonCloud poisonCloudScript = poisonCloud.GetComponent<PoisonCloud>();

        if (poisonCloudScript != null)
        {
            poisonCloudScript.Initialize(GetCurrentPoisonDamage(), GetCurrentPoisonRange(), GetCurrentPoisonDuration());
        }
    }

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
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
        }
    }

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

            return $"{baseDescription}\nMax Level: 독 피해량 {finalDamage}, 독 범위 {finalRange}m, 독 지속 시간 {finalDuration}초, 소환 간격 {finalInterval}초)";
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < poisonDamageLevels.Length)
        {
            return Mathf.RoundToInt(poisonDamageLevels[currentLevel]);
        }
        return 1;
    }

    private float GetCurrentPoisonDamage()
    {
        if (currentLevel < poisonDamageLevels.Length)
        {
            return poisonDamageLevels[currentLevel];
        }
        return poisonDamageLevels[poisonDamageLevels.Length - 1];
    }

    private float GetCurrentPoisonRange()
    {
        if (currentLevel < poisonRangeLevels.Length)
        {
            return poisonRangeLevels[currentLevel];
        }
        return poisonRangeLevels[poisonRangeLevels.Length - 1];
    }

    private float GetCurrentPoisonDuration()
    {
        if (currentLevel < poisonDurationLevels.Length)
        {
            return poisonDurationLevels[currentLevel];
        }
        return poisonDurationLevels[poisonDurationLevels.Length - 1];
    }

    private float GetCurrentSpawnInterval()
    {
        if (currentLevel < spawnIntervalLevels.Length)
        {
            return spawnIntervalLevels[currentLevel];
        }
        return spawnIntervalLevels[spawnIntervalLevels.Length - 1];
    }
}
