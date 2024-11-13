using System;
using UnityEngine;

[Serializable]
public class PlayerAbility
{
    public Ability ability; // Ability ���ø�
    public int currentLevel;

    public PlayerAbility(Ability ability, int level = 0)
    {
        this.ability = UnityEngine.Object.Instantiate(ability); // Ability�� ���纻 ����
        this.currentLevel = level;
    }

    public string GetDescription()
    {
        return ability.GetDescription();
    }

    public void Apply(Player player)
    {
        ability.currentLevel = currentLevel;
        ability.Apply(player);
    }

    public void Upgrade(Player player)
    {
        if (currentLevel < ability.maxLevel - 1)
        {
            currentLevel++;
            ability.currentLevel = currentLevel;
            ability.Upgrade();
            ability.Apply(player);
        }
        else
        {
            Debug.LogWarning($"{ability.abilityName}: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    public void ResetLevel()
    {
        currentLevel = 0;
        ability.ResetLevel();
    }
}
