using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseAttackSpeed")]
public class IncreaseAttackSpeed : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("레벨별 공격 속도 감소량 (초 단위)")]
    public float[] cooldownReductions; // 레벨 1~5
    private Player playerInstance;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("IncreaseAttackSpeed Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;

        // 현재 레벨에 따른 공격 속도 감소 적용
        if (currentLevel < cooldownReductions.Length)
        {
            player.stat.currentShootCooldown -= cooldownReductions[currentLevel];
            // 최소 쿨다운 제한
            if (player.stat.currentShootCooldown < 0.1f)
            {
                player.stat.currentShootCooldown = 0.1f;
            }
        }
        else
        {
            Debug.LogWarning($"IncreaseAttackSpeed: currentLevel ({currentLevel})이 cooldownReductions 배열의 범위를 벗어났거나 레벨이 1 이하입니다.");
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

            // 레벨 업 시 공격 속도 감소 적용
            Player player = FindObjectOfType<Player>();
            if (player != null && currentLevel < cooldownReductions.Length)
            {
                player.stat.currentShootCooldown -= cooldownReductions[currentLevel - 1];
                // 최소 쿨다운 제한
                if (player.stat.currentShootCooldown < 0.1f)
                {
                    player.stat.currentShootCooldown = 0.1f;
                }
            }
            else
            {
                Debug.LogWarning($"IncreaseAttackSpeed: 플레이어를 찾을 수 없거나 currentLevel ({currentLevel})이 cooldownReductions 배열의 범위를 벗어났습니다.");
            }
        }
        else
        {
            Debug.LogWarning("IncreaseAttackSpeed: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력의 설명을 반환합니다.
    /// </summary>
    /// <returns>능력의 설명 문자열</returns>
    public override string GetDescription()
    {
        // 총 공격 속도 감소량 계산
        float totalReduction = 0f;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < cooldownReductions.Length)
                totalReduction += cooldownReductions[i];
        }

        if (currentLevel < cooldownReductions.Length)
        {
            float currentReduction = cooldownReductions[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 공격 쿨다운 감소 {currentReduction}초\n지금까지 총 {totalReduction}초 감소";
        }
        else
        {
            float finalReduction = cooldownReductions.Length > 0 ? cooldownReductions[cooldownReductions.Length - 1] : 0f;
            return $"{baseDescription}\nMax Level: 공격 쿨다운 감소 {finalReduction}초\n지금까지 총 {totalReduction}초 감소";
        }
    }

    /// <summary>
    /// 다음 레벨 증가에 필요한 값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨 증가 시 필요한 값</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < cooldownReductions.Length)
        {
            // 쿨다운 감소량을 정수로 반환 (소수점 버림)
            return Mathf.RoundToInt(cooldownReductions[currentLevel]);
        }
        Debug.LogWarning($"IncreaseAttackSpeed: currentLevel ({currentLevel})이 cooldownReductions 배열의 범위를 벗어났습니다. 기본값 0을 반환합니다.");
        return 0;
    }
}
