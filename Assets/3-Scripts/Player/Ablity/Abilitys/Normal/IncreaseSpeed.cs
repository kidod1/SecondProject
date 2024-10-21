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
            PlayManager.I.GetPlayer().stat.currentPlayerSpeed += speedIncreases[currentLevel];
        }
    }
    // �����Ƽ ���׷��̵�
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel�� 5�� ���, currentLevel�� 0~4
        {
            currentLevel++;

            // ���� �� �� �̵� �ӵ� ���� ����
            Player player = FindObjectOfType<Player>();
            if (player != null && currentLevel < speedIncreases.Length)
            {
                player.stat.currentPlayerSpeed += speedIncreases[currentLevel - 1];
            }
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
        // �� �̵� �ӵ� ������ ���
        float totalIncrease = 0f;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < speedIncreases.Length)
                totalIncrease += speedIncreases[i];
        }
        float totalSpeedIncrease = Mathf.Round(totalIncrease * 100f) / 100f; // �Ҽ��� �� �ڸ����� �ݿø�

        if (currentLevel < maxLevel && currentLevel < speedIncreases.Length)
        {
            float speedIncrease = speedIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: �̵� �ӵ� +{speedIncrease} ����\n���ݱ��� �� {totalSpeedIncrease} ���� ���";
        }
        else if (currentLevel == maxLevel && currentLevel < speedIncreases.Length)
        {
            float speedIncrease = speedIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: �̵� �ӵ� +{speedIncrease} ����\n���ݱ��� �� {totalSpeedIncrease} ���� ���\n�ִ� ���� ����";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����\n�� {totalSpeedIncrease} ���� ���";
        }
    }

    // �����Ƽ ���� �ʱ�ȭ
    public override void ResetLevel()
    {
        base.ResetLevel();
    }
}
