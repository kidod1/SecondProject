using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseExperience")]
public class IncreaseExperience : Ability
{
    [Tooltip("�� �������� ����ġ ȹ�淮 ���� ��� (��: 0.1 = 10% ����)")]
    public float[] experienceMultipliers;

    public override void Apply(Player player)
    {
        if (currentLevel < experienceMultipliers.Length && currentLevel == 0)
        {
            player.stat.experienceMultiplier += experienceMultipliers[currentLevel];
        }
        if (player == null) return;
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {

            Player player = FindObjectOfType<Player>();
            if (player != null && currentLevel < experienceMultipliers.Length)
            {
                player.stat.experienceMultiplier += experienceMultipliers[currentLevel - 1];
            }
        }
        else
        {
            Debug.LogWarning("IncreaseExperience: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }


    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel && currentLevel < experienceMultipliers.Length)
        {
            return Mathf.RoundToInt(experienceMultipliers[currentLevel] * 100);
        }
        return 0;
    }

    public override string GetDescription()
    {
        float totalIncrease = 0f;
        for (int i = 0; i < currentLevel; i++)
        {
            totalIncrease += experienceMultipliers[i];
        }
        int totalPercentIncrease = Mathf.RoundToInt(totalIncrease * 100);

        if (currentLevel < maxLevel && currentLevel < experienceMultipliers.Length)
        {
            int percentIncrease = Mathf.RoundToInt(experienceMultipliers[currentLevel] * 100);
            return $"{baseDescription}\nLv {currentLevel + 1}: ����ġ ȹ�� +{percentIncrease}%\n������� �� {totalPercentIncrease}% ���";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����\n�� {totalPercentIncrease}% ���";
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
    }
}
