using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseSpeed")]
public class IncreaseSpeed : Ability
{
    [Tooltip("�� ���������� �̵� �ӵ� ������ (��: 0.5f = 0.5 ���� ����)")]
    public float[] speedIncreases;

    // �÷��̾ �����Ƽ ����
    public override void Apply(Player player)
    {
        if (currentLevel < speedIncreases.Length)
        {
            player.stat.currentPlayerSpeed += speedIncreases[currentLevel - 1];
        }
    }

    // �����Ƽ ���׷��̵�
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
        }
        else
        {
            Debug.LogWarning("IncreaseSpeed: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    // ���� ���� ������ ��ȯ
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel && currentLevel < speedIncreases.Length)
        {
            return Mathf.RoundToInt(speedIncreases[currentLevel] * 100); // �ۼ�Ʈ�� ��ȯ
        }
        return 0;
    }

    // �ɷ��� ������ ��ȯ
    public override string GetDescription()
    {
        if (currentLevel < maxLevel && currentLevel < speedIncreases.Length)
        {
            float speedIncrease = speedIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: �̵� �ӵ� +{speedIncrease} ����";
        }
        else if (currentLevel == maxLevel && currentLevel < speedIncreases.Length)
        {
            float speedIncrease = speedIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: �̵� �ӵ� +{speedIncrease} ����\n�ִ� ���� ����";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����";
        }
    }

    // �����Ƽ ���� �ʱ�ȭ
    public override void ResetLevel()
    {
        base.ResetLevel();
        // �ʿ� �� �÷��̾��� �ӵ� �ʱ�ȭ ���� �߰�
        // ��: player.stat.currentPlayerSpeed -= speedIncreases[currentLevel - 1];
    }
}
