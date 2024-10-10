using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/HoneyDrop")]
public class HoneyDrop : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("레벨별 회복량 (레벨 1~5)")]
    public int[] healthRecoveryAmountLevels = { 20, 30, 40, 50, 60 }; // 레벨 1~5

    [Tooltip("레벨별 벌꿀 드랍 확률")]
    [Range(0f, 1f)]
    public float[] honeyDropChanceLevels = { 0.3f, 0.35f, 0.4f, 0.45f, 0.5f }; // 레벨 1~5

    [Tooltip("벌꿀 아이템 프리팹 경로")]
    public string honeyItemPrefabPath = "HoneyItemPrefab";

    private Player playerInstance;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        playerInstance = player;
    }

    /// <summary>
    /// 몬스터가 죽었을 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="monster">죽은 몬스터</param>
    public void OnMonsterDeath(Monster monster)
    {
        if (Random.value <= GetCurrentHoneyDropChance())
        {
            SpawnHoney(monster.transform.position);
        }
    }

    /// <summary>
    /// 벌꿀 아이템을 스폰합니다.
    /// </summary>
    /// <param name="position">아이템 스폰 위치</param>
    private void SpawnHoney(Vector3 position)
    {
        GameObject honeyPrefab = Resources.Load<GameObject>(honeyItemPrefabPath);

        if (honeyPrefab != null)
        {
            GameObject honeyItem = Instantiate(honeyPrefab, position, Quaternion.identity);
            HoneyItem honeyScript = honeyItem.GetComponent<HoneyItem>();
            if (honeyScript != null)
            {
                honeyScript.ItemData.healAmount = GetCurrentHealthRecoveryAmount();
                Debug.Log($"HoneyDrop: 벌꿀 아이템 스폰됨. 회복량: {honeyScript.ItemData.healAmount}");
            }
            else
            {
                Debug.LogWarning("HoneyDrop: HoneyItem 스크립트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"HoneyDrop: 벌꿀 아이템 프리팹을 찾을 수 없습니다: {honeyItemPrefabPath}");
        }
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 회복량과 드랍 확률을 증가시킵니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel이 5일 경우, currentLevel은 0~4
        {
            currentLevel++;
            Debug.Log($"HoneyDrop 업그레이드: 현재 레벨 {currentLevel + 1}");
            // 레벨 업 시 필요한 추가 로직이 있다면 여기에 추가
        }
        else
        {
            Debug.LogWarning("HoneyDrop: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
    }

    /// <summary>
    /// 능력의 설명을 반환합니다.
    /// </summary>
    /// <returns>능력의 설명 문자열</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            int currentHeal = GetCurrentHealthRecoveryAmount();
            float currentChance = GetCurrentHoneyDropChance();
            return $"{baseDescription}{System.Environment.NewLine}(Lv {currentLevel + 1}: 회복량 +{currentHeal}, 드랍 확률 {currentChance * 100}%)";
        }
        else
        {
            int finalHeal = GetCurrentHealthRecoveryAmount();
            float finalChance = GetCurrentHoneyDropChance();
            return $"{baseDescription}{System.Environment.NewLine}(Max Level: 회복량 +{finalHeal}, 드랍 확률 {finalChance * 100}%)";
        }
    }

    /// <summary>
    /// 다음 레벨 증가에 필요한 값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨 증가 시 필요한 값</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < healthRecoveryAmountLevels.Length)
        {
            // 다음 레벨의 회복량을 반환
            return healthRecoveryAmountLevels[currentLevel];
        }
        Debug.LogWarning($"HoneyDrop: currentLevel ({currentLevel})이 healthRecoveryAmountLevels 배열의 범위를 벗어났습니다. 기본값 1을 반환합니다.");
        return 1;
    }

    /// <summary>
    /// 현재 레벨의 회복량을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 회복량</returns>
    private int GetCurrentHealthRecoveryAmount()
    {
        if (currentLevel < healthRecoveryAmountLevels.Length)
        {
            return healthRecoveryAmountLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"HoneyDrop: currentLevel ({currentLevel})이 healthRecoveryAmountLevels 배열의 범위를 벗어났습니다. 기본값 {healthRecoveryAmountLevels[healthRecoveryAmountLevels.Length - 1]}을 반환합니다.");
            return healthRecoveryAmountLevels[healthRecoveryAmountLevels.Length - 1];
        }
    }

    /// <summary>
    /// 현재 레벨의 벌꿀 드랍 확률을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 벌꿀 드랍 확률</returns>
    private float GetCurrentHoneyDropChance()
    {
        if (currentLevel < honeyDropChanceLevels.Length)
        {
            return honeyDropChanceLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"HoneyDrop: currentLevel ({currentLevel})이 honeyDropChanceLevels 배열의 범위를 벗어났습니다. 기본값 {honeyDropChanceLevels[honeyDropChanceLevels.Length - 1]}을 반환합니다.");
            return honeyDropChanceLevels[honeyDropChanceLevels.Length - 1];
        }
    }
}
