using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseDefense")]
public class IncreaseDefense : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("레벨별 방어력 증가량 (레벨 1부터 시작)")]
    public int[] defenseIncreases; // 레벨 1~5

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        if (player == null) return;

        // 현재 레벨에 따른 방어력 증가 적용
        if (currentLevel < defenseIncreases.Length)
        {
            player.stat.currentDefense += defenseIncreases[currentLevel];
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

            // 레벨 업 시 방어력 증가 적용
            Player player = FindObjectOfType<Player>();
            if (player != null && currentLevel < defenseIncreases.Length)
            {
                player.stat.currentDefense += defenseIncreases[currentLevel - 1];
            }
        }
    }

    /// <summary>
    /// 능력의 설명을 반환합니다.
    /// </summary>
    /// <returns>능력의 설명 문자열</returns>
    public override string GetDescription()
    {
        // 총 방어력 증가량 계산
        int totalIncrease = 0;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < defenseIncreases.Length)
                totalIncrease += defenseIncreases[i];
        }

        if (currentLevel < defenseIncreases.Length)
        {
            int currentIncrease = defenseIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 방어력 +{currentIncrease}\n지금까지 총 {totalIncrease}만큼 상승";
        }
        else
        {
            int finalIncrease = defenseIncreases.Length > 0 ? defenseIncreases[defenseIncreases.Length - 1] : 0;
            return $"{baseDescription}\nMax Level: 방어력 +{finalIncrease}\n지금까지 총 {totalIncrease}만큼 상승";
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
        return 0;
    }
}
