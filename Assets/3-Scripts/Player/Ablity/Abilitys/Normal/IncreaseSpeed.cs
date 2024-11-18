using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseSpeed")]
public class IncreaseSpeed : Ability
{
    [Tooltip("�� ���������� �̵� �ӵ� ������ (��: 0.5f = 0.5 ���� ����)")]
    public float[] speedIncreases;

    private Player playerInstance; // �÷��̾� �ν��Ͻ� ����
    private float previousTotalIncrease = 0f; // ������ ����� �� �ӵ� ������

    // �÷��̾ �����Ƽ ����
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("IncreaseSpeed Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;

        // ���� ���������� �� �̵� �ӵ� ������ ���
        float totalIncrease = GetTotalSpeedIncrease();

        // ������ ����� �̵� �ӵ� �������� ����
        player.stat.defaultPlayerSpeed -= previousTotalIncrease;

        // ���ο� �� �̵� �ӵ� �������� ����
        player.stat.defaultPlayerSpeed += totalIncrease;
        player.stat.currentPlayerSpeed = player.stat.defaultPlayerSpeed;

        // ���� �� �̵� �ӵ� ������ ������Ʈ
        previousTotalIncrease = totalIncrease;
    }

    // �����Ƽ ���׷��̵�
    public override void Upgrade()
    {
        if (playerInstance == null)
        {
            Debug.LogWarning("IncreaseSpeed Upgrade: playerInstance�� null�Դϴ�.");
            return;
        }

        // ���� ���������� �� �̵� �ӵ� ������ ���
        float totalIncrease = GetTotalSpeedIncrease();

        // ������ ����� �̵� �ӵ� �������� ����
        playerInstance.stat.defaultPlayerSpeed -= previousTotalIncrease;

        // ���ο� �� �̵� �ӵ� �������� ����
        playerInstance.stat.defaultPlayerSpeed += totalIncrease;
        playerInstance.stat.currentPlayerSpeed = playerInstance.stat.defaultPlayerSpeed;

        // ���� �� �̵� �ӵ� ������ ������Ʈ
        previousTotalIncrease = totalIncrease;
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
        float totalIncrease = GetTotalSpeedIncrease();
        float totalSpeedIncrease = Mathf.Round(totalIncrease * 100f) / 100f; // �Ҽ��� �� �ڸ����� �ݿø�

        if (currentLevel < maxLevel && currentLevel < speedIncreases.Length)
        {
            float speedIncrease = speedIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: �̵� �ӵ� +{speedIncrease}\n���ݱ��� �� {totalSpeedIncrease} ���";
        }
        else if (currentLevel == maxLevel && currentLevel < speedIncreases.Length)
        {
            float speedIncrease = speedIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: �̵� �ӵ� +{speedIncrease}\n���ݱ��� �� {totalSpeedIncrease} ���\n�ִ� ���� ����";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����\n�� {totalSpeedIncrease} ���� ���";
        }
    }

    // �����Ƽ ���� �ʱ�ȭ
    public override void ResetLevel()
    {
        if (playerInstance != null)
        {
            playerInstance.stat.defaultPlayerSpeed -= previousTotalIncrease;
            playerInstance.stat.currentPlayerSpeed = playerInstance.stat.defaultPlayerSpeed;
        }
        currentLevel = 0;
        previousTotalIncrease = 0f;
    }

    /// <summary>
    /// ���� ���������� �� �̵� �ӵ� �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�� �̵� �ӵ� ������</returns>
    private float GetTotalSpeedIncrease()
    {
        float totalIncrease = 0f;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < speedIncreases.Length)
                totalIncrease += speedIncreases[i];
        }
        return totalIncrease;
    }
}
