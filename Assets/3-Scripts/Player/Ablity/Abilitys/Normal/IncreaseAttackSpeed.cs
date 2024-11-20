using UnityEngine;
using TMPro;

[CreateAssetMenu(menuName = "Abilities/IncreaseAttackSpeed")]
public class IncreaseAttackSpeed : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("������ ���� �ӵ� ������ (�ʴ� ���� Ƚ�� ����)")]
    public float[] attackSpeedIncrements;

    private Player playerInstance; // �÷��̾� �ν��Ͻ� ����
    private float previousTotalIncrease = 0f; // ������ ����� �� ���� �ӵ� ������

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        Debug.Log("IncreaseAttackSpeed Apply");
        if (player == null)
        {
            Debug.LogError("IncreaseAttackSpeed Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;

        // ���� ���������� �� ���� �ӵ� ������ ���
        float totalIncrease = GetTotalAttackSpeedIncrease();

        // ������ ����� ���� �ӵ� �������� ����
        player.stat.defaultAttackSpeed -= previousTotalIncrease;

        // ���ο� �� ���� �ӵ� �������� ����
        player.stat.defaultAttackSpeed += totalIncrease;
        player.stat.currentAttackSpeed = player.stat.defaultAttackSpeed;

        // ���� �� ���� �ӵ� ������ ������Ʈ
        previousTotalIncrease = totalIncrease;
        Debug.Log($"IncreaseAttackSpeed: Applied total attack speed increase of {totalIncrease} at level {currentLevel}");
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        Debug.Log("IncreaseAttackSpeed Upgrade");

        if (playerInstance == null)
        {
            Debug.LogWarning("IncreaseAttackSpeed Upgrade: playerInstance�� null�Դϴ�.");
            return;
        }

        // ���� ���������� �� ���� �ӵ� ������ ���
        float totalIncrease = GetTotalAttackSpeedIncrease();

        // ������ ����� ���� �ӵ� �������� ����
        playerInstance.stat.defaultAttackSpeed -= previousTotalIncrease;

        // ���ο� �� ���� �ӵ� �������� ����
        playerInstance.stat.defaultAttackSpeed += totalIncrease;
        playerInstance.stat.currentAttackSpeed = playerInstance.stat.defaultAttackSpeed;

        // ���� �� ���� �ӵ� ������ ������Ʈ
        previousTotalIncrease = totalIncrease;
    }

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ��� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        // �� ���� �ӵ� ������ ���
        float totalIncrease = GetTotalAttackSpeedIncrease();

        if (currentLevel < attackSpeedIncrements.Length)
        {
            float nextIncrease = attackSpeedIncrements[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}:\n���� �ӵ� +{nextIncrease} ����/��\n���� ����: ���� �ӵ� +{nextIncrease}";
        }
        else
        {
            float finalIncrease = attackSpeedIncrements.Length > 0 ? attackSpeedIncrements[attackSpeedIncrements.Length - 1] : 0f;
            return $"{baseDescription}\n�ִ� ���� ����\n���� �ӵ� +{finalIncrease} ����/��\n�� ���� �ӵ� ������: +{totalIncrease} ����/��";
        }
    }

    /// <summary>
    /// ���� ���� ������ �ʿ��� ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���� ���� �� �ʿ��� ��</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < attackSpeedIncrements.Length)
        {
            return Mathf.RoundToInt(attackSpeedIncrements[currentLevel]);
        }
        else
        {
            Debug.LogWarning($"IncreaseAttackSpeed: currentLevel ({currentLevel})�� attackSpeedIncrements �迭�� ������ ������ϴ�. �⺻�� 0�� ��ȯ�մϴ�.");
            return 0;
        }
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        if (playerInstance != null)
        {
            playerInstance.stat.defaultAttackSpeed -= GetTotalAttackSpeedIncrease();
            playerInstance.stat.currentAttackSpeed = playerInstance.stat.defaultAttackSpeed;
        }
        currentLevel = 0;
        previousTotalIncrease = 0f;
    }

    /// <summary>
    /// ���� ���������� �� ���� �ӵ� �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�� ���� �ӵ� ������</returns>
    private float GetTotalAttackSpeedIncrease()
    {
        float totalIncrease = 0f;
        for (int i = 0; i < currentLevel; i++)
        {
            if (i < attackSpeedIncrements.Length)
                totalIncrease += attackSpeedIncrements[i];
        }
        return totalIncrease;
    }
}
