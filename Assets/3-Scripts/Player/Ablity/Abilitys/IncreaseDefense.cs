using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseDefense")]
public class IncreaseDefense : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("레벨별 방어력 증가량 (레벨 1부터 시작)")]
    public int[] defenseIncreases; // 레벨 1~5
    private Player playerInstance;
    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("IncreaseDefense Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;

        // 현재 레벨에 따른 방어력 증가 적용
        if (currentLevel < defenseIncreases.Length && currentLevel > 0)
        {
            player.stat.currentDefense += defenseIncreases[currentLevel];
            Debug.Log($"IncreaseDefense이 적용되었습니다. 현재 레벨 Lv: {currentLevel + 1}, 방어력 증가: {defenseIncreases[currentLevel]}");
        }
        else
        {
            Debug.LogWarning($"IncreaseDefense: currentLevel ({currentLevel})이 defenseIncreases 배열의 범위를 벗어났거나 레벨이 1 이하입니다.");
        }
    }

    /// <summary>
    /// 능력을 업그레이드합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel이 5일 경우, currentLevel은 0~4
        {
            currentLevel++;
            Debug.Log($"IncreaseDefense 업그레이드: 현재 레벨 {currentLevel + 1}");

            // 레벨 업 시 방어력 증가 적용
            Player player = FindObjectOfType<Player>();
            if (player != null && currentLevel < defenseIncreases.Length)
            {
                player.stat.currentDefense += defenseIncreases[currentLevel];
                Debug.Log($"IncreaseDefense 레벨 {currentLevel + 1}에서 방어력 증가: {defenseIncreases[currentLevel]}");
            }
            else
            {
                Debug.LogWarning($"IncreaseDefense: 플레이어를 찾을 수 없거나 currentLevel ({currentLevel})이 defenseIncreases 배열의 범위를 벗어났습니다.");
            }
        }
        else
        {
            Debug.LogWarning("IncreaseDefense: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력의 설명을 반환합니다.
    /// </summary>
    /// <returns>능력의 설명 문자열</returns>
    public override string GetDescription()
    {
        if (currentLevel < defenseIncreases.Length)
        {
            int currentIncrease = defenseIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 방어력 +{currentIncrease}";
        }
        else
        {
            Debug.LogWarning($"IncreaseDefense: currentLevel ({currentLevel})이 defenseIncreases 배열의 범위를 벗어났습니다. 최대 레벨 설명을 반환합니다.");
            int finalIncrease = defenseIncreases[defenseIncreases.Length - 1];
            return $"{baseDescription}\nMax Level: 방어력 +{finalIncrease}";
        }
    }

    /// <summary>
    /// 다음 레벨 증가에 필요한 값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨 증가 시 필요한 값</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < defenseIncreases.Length)
        {
            return defenseIncreases[currentLevel];
        }
        Debug.LogWarning($"IncreaseDefense: currentLevel ({currentLevel})이 defenseIncreases 배열의 범위를 벗어났습니다. 기본값 0을 반환합니다.");
        return 0;
    }
}
