using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseDefense")]
public class IncreaseDefense : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("레벨별 방어력 증가량 (레벨 1부터 시작)")]
    public int[] defenseIncreases; // 레벨 1~5

    private Player playerInstance; // 플레이어 인스턴스 참조
    private int previousTotalIncrease = 0; // 이전에 적용된 총 방어력 증가량

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

        // 현재 레벨까지의 총 방어력 증가량 계산
        int totalIncrease = GetTotalDefenseIncrease();

        // 이전에 적용된 방어력 증가량을 제거
        player.stat.defaultDefense -= previousTotalIncrease;

        // 새로운 총 방어력 증가량을 적용
        player.stat.defaultDefense += totalIncrease;
        player.stat.currentDefense = player.stat.defaultDefense;

        // 이전 총 방어력 증가량 업데이트
        previousTotalIncrease = totalIncrease;
    }

    /// <summary>
    /// 능력을 업그레이드합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (playerInstance == null)
        {
            Debug.LogWarning("IncreaseDefense Upgrade: playerInstance가 null입니다.");
            return;
        }

        // 현재 레벨까지의 총 방어력 증가량 계산
        int totalIncrease = GetTotalDefenseIncrease();

        // 이전에 적용된 방어력 증가량을 제거
        playerInstance.stat.defaultDefense -= previousTotalIncrease;

        // 새로운 총 방어력 증가량을 적용
        playerInstance.stat.defaultDefense += totalIncrease;
        playerInstance.stat.currentDefense = playerInstance.stat.defaultDefense;

        // 이전 총 방어력 증가량 업데이트
        previousTotalIncrease = totalIncrease;
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

    /// <summary>
    /// 능력의 설명을 반환합니다.
    /// </summary>
    /// <returns>능력의 설명 문자열</returns>
    public override string GetDescription()
    {
        // 총 방어력 증가량 계산
        int totalIncrease = GetTotalDefenseIncrease();

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
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        if (playerInstance != null)
        {
            playerInstance.stat.defaultDefense -= previousTotalIncrease;
            playerInstance.stat.currentDefense = playerInstance.stat.defaultDefense;
        }
        currentLevel = 0;
        previousTotalIncrease = 0;
    }

    /// <summary>
    /// 현재 레벨까지의 총 방어력 증가량을 반환합니다.
    /// </summary>
    /// <returns>총 방어력 증가량</returns>
    private int GetTotalDefenseIncrease()
    {
        int totalIncrease = 0;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < defenseIncreases.Length)
                totalIncrease += defenseIncreases[i];
        }
        return totalIncrease;
    }
}
