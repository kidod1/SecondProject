using System;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName;
    public string baseDescription;
    public string category;
    public int maxLevel = 5;
    public int currentLevel;
    public Sprite abilityIcon;

    // ���� ������ ���� ������ ��ȯ�ϴ� �޼���
    public virtual string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            return $"{baseDescription}{Environment.NewLine}(Level {currentLevel + 1}: + {GetNextLevelIncrease()} )";
        }
        else
        {
            return $"{baseDescription}{Environment.NewLine}(Max Level)";
        }
    }

    // ���� �������� �����ϴ� ��ġ�� ��ȯ�ϴ� �߻� �޼���
    protected abstract int GetNextLevelIncrease();

    public abstract void Apply(Player player);
    public abstract void Upgrade();

    // �ɷ��� �ʱ�ȭ�ϴ� �޼���
    public virtual void ResetLevel()
    {
        currentLevel = 0;
    }
}
