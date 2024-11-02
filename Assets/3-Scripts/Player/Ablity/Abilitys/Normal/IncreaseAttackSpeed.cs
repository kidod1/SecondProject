using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseAttackSpeed")]
public class IncreaseAttackSpeed : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("레벨별 공격 속도 증가량 (초당 공격 횟수 증가)")]
    public float[] attackSpeedIncrements;

    private Player playerInstance;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("IncreaseAttackSpeed Apply: player 인스턴스가 null입니다.");
            return;
        }
        playerInstance = player;

        ApplyAttackSpeedIncrease();
        // currentLevel 증가 부분 제거
    }

    public override void Upgrade()
    {
        if (currentLevel <= maxLevel)
        {
            ApplyAttackSpeedIncrease();
            // currentLevel 증가 부분 제거
        }
    }

    private void ApplyAttackSpeedIncrease()
    {
        int levelIndex = Mathf.Clamp(currentLevel - 1, 0, attackSpeedIncrements.Length - 1);

        float increment = attackSpeedIncrements[levelIndex];
        playerInstance.stat.defaultAttackSpeed += increment;
        playerInstance.stat.currentAttackSpeed += increment;
    }

    public override string GetDescription()
    {
        // 총 공격 속도 증가량 계산
        float totalIncrement = 0f;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < attackSpeedIncrements.Length)
                totalIncrement += attackSpeedIncrements[i];
        }

        if (currentLevel < attackSpeedIncrements.Length)
        {
            float currentIncrement = attackSpeedIncrements[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 공격 속도 {currentIncrement} 증가\n지금까지 총 {totalIncrement} 증가";
        }
        else
        {
            float finalIncrement = attackSpeedIncrements.Length > 0 ? attackSpeedIncrements[attackSpeedIncrements.Length - 1] : 0f;
            return $"{baseDescription}\nMax Level: 공격 속도 {finalIncrement} 증가\n지금까지 총 {totalIncrement} 증가";
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < attackSpeedIncrements.Length)
        {
            return Mathf.RoundToInt(attackSpeedIncrements[currentLevel]);
        }
        Debug.LogWarning($"IncreaseAttackSpeed: currentLevel ({currentLevel})이 attackSpeedIncrements 배열의 범위를 벗어났습니다. 기본값 0을 반환합니다.");
        return 0;
    }
}
