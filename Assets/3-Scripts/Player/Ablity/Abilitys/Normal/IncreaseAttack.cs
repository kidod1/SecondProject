using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseAttack")]
public class IncreaseAttack : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("레벨별 공격력 증가량 (레벨 1부터 시작)")]
    public int[] attackIncreases;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("IncreaseAttack Apply: player 인스턴스가 null입니다.");
            return;
        }
        // 현재 레벨에 따른 공격력 증가 적용
        if (currentLevel < attackIncreases.Length && currentLevel == 0)
        {
            player.stat.defaultPlayerDamage += attackIncreases[currentLevel];
            player.stat.currentPlayerDamage = player.stat.defaultPlayerDamage;
        }
        else
        {
            Debug.LogWarning($"IncreaseAttack: currentLevel ({currentLevel})이 attackIncreases 배열의 범위를 벗어났습니다.");
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

            // 레벨 업 시 공격력 증가 적용
            Player player = FindObjectOfType<Player>();
            if (player != null && currentLevel < attackIncreases.Length)
            {
                player.stat.defaultPlayerDamage += attackIncreases[currentLevel - 1];
                player.stat.currentPlayerDamage = player.stat.defaultPlayerDamage;
            }
        }
        else
        {
            Debug.LogWarning("IncreaseAttack: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력의 설명을 반환합니다.
    /// </summary>
    /// <returns>능력의 설명 문자열</returns>
    public override string GetDescription()
    {
        // 총 공격력 증가량 계산
        int totalIncrease = 0;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < attackIncreases.Length)
                totalIncrease += attackIncreases[i];
        }

        if (currentLevel < attackIncreases.Length)
        {
            int currentIncrease = attackIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 공격력 +{currentIncrease}\n지금까지 총 {totalIncrease}만큼 상승";
        }
        else
        {
            int finalIncrease = attackIncreases.Length > 0 ? attackIncreases[attackIncreases.Length - 1] : 0;
            return $"{baseDescription}\nMax Level: 공격력 +{finalIncrease}\n지금까지 총 {totalIncrease}만큼 상승";
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
        Debug.LogWarning($"IncreaseAttack: currentLevel ({currentLevel})이 attackIncreases 배열의 범위를 벗어났습니다. 기본값 0을 반환합니다.");
        return 0;
    }
}
