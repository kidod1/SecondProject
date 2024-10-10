using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseExperience")]
public class IncreaseExperience : Ability
{
    [Tooltip("�� �������� ����ġ ȹ�淮 ���� ��� (��: 0.1 = 10% ����)")]
    public float[] experienceMultipliers;

    public override void Apply(Player player)
    {
        if (currentLevel > 0 && currentLevel - 1 < experienceMultipliers.Length)
        {
            player.stat.experienceMultiplier += experienceMultipliers[currentLevel - 1];
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            Debug.Log($"IncreaseExperience ���׷��̵�: ���� ���� {currentLevel}");
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel && currentLevel < experienceMultipliers.Length)
        {
            return Mathf.RoundToInt(experienceMultipliers[currentLevel] * 100); // �ۼ�Ʈ�� ��ȯ
        }
        return 0;
    }

    public override string GetDescription()
    {
        if (currentLevel < maxLevel && currentLevel < experienceMultipliers.Length)
        {
            int percentIncrease = Mathf.RoundToInt(experienceMultipliers[currentLevel] * 100);
            return $"{baseDescription}\n���� {currentLevel + 1}: ����ġ ȹ�� +{percentIncrease}%";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����";
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        // �ʿ� �� �÷��̾��� ����ġ ��� �ʱ�ȭ ���� �߰�
    }
}
