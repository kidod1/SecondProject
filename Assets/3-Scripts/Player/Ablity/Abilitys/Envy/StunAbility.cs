using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/StunAbility")]
public class StunAbility : Ability
{
    [Tooltip("레벨별 스턴 확률 (0.0f ~ 1.0f)")]
    [Range(0f, 1f)]
    public float[] stunChances;   // 레벨별 스턴 확률 배열

    public float stunDuration = 2f;   // 기절 지속 시간

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
    /// 몬스터에게 스턴을 시도합니다.
    /// </summary>
    /// <param name="monster">스턴을 시도할 몬스터</param>
    public void TryStun(Monster monster)
    {
        float currentStunChance = GetCurrentStunChance();
        float randomValue = Random.value;
        if (randomValue < currentStunChance) // 스턴 확률 체크
        {
            monster.Stun(stunDuration); // 몬스터 기절시키기
        }
        else
        {
            Debug.Log($"{monster.name} resisted the stun.");
        }
    }

    /// <summary>
    /// 현재 레벨에 따른 스턴 확률을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 스턴 확률 (0.0f ~ 1.0f)</returns>
    private float GetCurrentStunChance()
    {
        if (currentLevel < stunChances.Length)
        {
            return stunChances[currentLevel];
        }
        else
        {
            Debug.LogWarning($"StunAbility: currentLevel ({currentLevel}) exceeds stunChances 배열 범위. 마지막 레벨의 스턴 확률을 사용합니다.");
            return stunChances[stunChances.Length - 1];
        }
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 스턴 확률이 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel이 5라면 currentLevel은 0~4
        {
            currentLevel++;
            Debug.Log($"StunAbility upgraded to Level {currentLevel + 1}. 스턴 확률: {stunChances[currentLevel] * 100}%");
        }
        else
        {
            Debug.LogWarning("StunAbility: Already at max level.");
        }
    }

    /// <summary>
    /// 다음 레벨의 스턴 확률 증가값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨에서의 스턴 확률 증가량 (퍼센트)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < stunChances.Length)
        {
            return Mathf.RoundToInt(stunChances[currentLevel + 1] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        currentLevel = 0;
    }

    /// <summary>
    /// 능력의 현재 상태와 효과를 설명하는 문자열을 반환합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {

        if (currentLevel < stunChances.Length && currentLevel >= 0)
        {
            float stunChancePercent = stunChances[currentLevel] * 100f;
            return $"{baseDescription}\nLv {currentLevel + 1}: 적을 공격할 때 {stunChancePercent}% 확률로 {stunDuration}초 동안 기절";
        }
        else if (currentLevel >= stunChances.Length)
        {
            float maxStunChancePercent = stunChances[stunChances.Length - 1] * 100f;
            return $"{baseDescription}\n최대 레벨 도달: 적을 공격할 때 {maxStunChancePercent}% 확률로 {stunDuration}초 동안 기절";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달.";
        }
    }
}
