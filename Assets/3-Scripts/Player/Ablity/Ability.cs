using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName;
    public string baseDescription;
    public string category;
    public int maxLevel = 5;
    public int currentLevel;

    // 다음 레벨에 따른 설명을 반환하는 메서드
    public virtual string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            return $"{baseDescription} (Level {currentLevel + 1}: + {GetNextLevelIncrease()} )";
        }
        else
        {
            return $"{baseDescription} (Max Level)";
        }
    }

    // 다음 레벨에서 증가하는 수치를 반환하는 추상 메서드
    protected abstract int GetNextLevelIncrease();

    public abstract void Apply(Player player);
    public abstract void Upgrade();

    // 능력을 초기화하는 메서드
    public virtual void ResetLevel()
    {
        currentLevel = 0;
    }
}
