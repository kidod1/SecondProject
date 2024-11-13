using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseAttack")]
public class IncreaseAttack : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("레벨별 공격력 증가량 (레벨 1부터 시작)")]
    public int[] attackIncreases;

    private Player playerInstance; // 플레이어 인스턴스 참조
    private int previousTotalIncrease = 0; // 이전에 적용된 총 공격력 증가량

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        Debug.Log("IncreaseAttack Apply");
        if (player == null)
        {
            Debug.LogError("IncreaseAttack Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;

        // 현재 레벨까지의 총 공격력 증가량 계산
        int totalIncrease = GetTotalAttackIncrease();

        // 이전에 적용된 공격력 증가량을 제거
        player.stat.defaultPlayerDamage -= previousTotalIncrease;

        // 새로운 총 공격력 증가량을 적용
        player.stat.defaultPlayerDamage += totalIncrease;
        player.stat.currentPlayerDamage = player.stat.defaultPlayerDamage;

        // 이전 총 공격력 증가량 업데이트
        previousTotalIncrease = totalIncrease;
        Debug.Log($"IncreaseAttack: Applied total attack increase of {totalIncrease} at level {currentLevel}");
    }

    /// <summary>
    /// 능력을 업그레이드합니다.
    /// </summary>
    public override void Upgrade()
    {
        Debug.Log("IncreaseAttack Upgrade");

        if (playerInstance == null)
        {
            Debug.LogWarning("IncreaseAttack Upgrade: playerInstance가 null입니다.");
            return;
        }

        // 현재 레벨까지의 총 공격력 증가량 계산
        int totalIncrease = GetTotalAttackIncrease();

        // 이전에 적용된 공격력 증가량을 제거
        playerInstance.stat.defaultPlayerDamage -= previousTotalIncrease;

        // 새로운 총 공격력 증가량을 적용
        playerInstance.stat.defaultPlayerDamage += totalIncrease;
        playerInstance.stat.currentPlayerDamage = playerInstance.stat.defaultPlayerDamage;

        // 이전 총 공격력 증가량 업데이트
        previousTotalIncrease = totalIncrease;
    }

    /// <summary>
    /// 능력의 설명을 반환합니다.
    /// </summary>
    /// <returns>능력의 설명 문자열</returns>
    public override string GetDescription()
    {
        // 총 공격력 증가량 계산
        int totalIncrease = GetTotalAttackIncrease();

        if (currentLevel < attackIncreases.Length)
        {
            int nextIncrease = attackIncreases[currentLevel + 1];
            return $"{baseDescription}\nLv {currentLevel + 1}:\n총 공격력 +{totalIncrease}\n다음 레벨: 공격력 +{nextIncrease}";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달\n총 공격력 +{totalIncrease}";
        }
    }

    /// <summary>
    /// 다음 레벨 증가에 필요한 값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨 증가 시 필요한 값</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < attackIncreases.Length)
        {
            return attackIncreases[currentLevel];
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        if (playerInstance != null)
        {
            playerInstance.stat.defaultPlayerDamage -= GetTotalAttackIncrease();
            playerInstance.stat.currentPlayerDamage = playerInstance.stat.defaultPlayerDamage;
        }
        currentLevel = 0;
        previousTotalIncrease = 0;
    }

    /// <summary>
    /// 현재 레벨까지의 총 공격력 증가량을 반환합니다.
    /// </summary>
    /// <returns>총 공격력 증가량</returns>
    private int GetTotalAttackIncrease()
    {
        int totalIncrease = 0;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < attackIncreases.Length)
                totalIncrease += attackIncreases[i];
        }
        return totalIncrease;
    }
}
