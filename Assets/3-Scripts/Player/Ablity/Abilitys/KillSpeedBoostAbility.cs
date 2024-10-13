using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/KillSpeedBoost")]
public class KillSpeedBoostAbility : Ability
{
    [Tooltip("������ ų ���ǵ� ������")]
    public float[] speedBoostAmounts; // ������ �̵� �ӵ� ������ �迭

    public int killThreshold = 10; // ų �Ӱ�ġ
    public float boostDuration = 5f; // �ν�Ʈ ���� �ð�

    private int killCount = 0; // ���� ų ��
    private bool isBoostActive = false; // �ν�Ʈ Ȱ��ȭ ����

    private Player playerInstance; // �ɷ��� ����� �÷��̾� �ν��Ͻ�

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("KillSpeedBoostAbility Apply: �÷��̾� �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;
        Debug.Log($"KillSpeedBoostAbility��(��) �÷��̾�� ����Ǿ����ϴ�. ���� ����: {currentLevel + 1}");
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        playerInstance = null;
        killCount = 0;
        isBoostActive = false;
        Debug.Log("KillSpeedBoostAbility ������ �ʱ�ȭ�Ǿ����ϴ�.");
    }

    /// <summary>
    /// ���͸� óġ���� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public void OnMonsterKilled()
    {
        killCount++;

        if (killCount >= killThreshold && !isBoostActive)
        {
            ActivateSpeedBoost();
        }
    }

    /// <summary>
    /// �̵� �ӵ� �ν�Ʈ�� Ȱ��ȭ�մϴ�.
    /// </summary>
    private void ActivateSpeedBoost()
    {
        if (currentLevel >= speedBoostAmounts.Length)
        {
            Debug.LogWarning("KillSpeedBoostAbility: ���� ������ speedBoostAmounts �迭�� ������ ������ϴ�.");
            return;
        }

        isBoostActive = true;
        float boostAmount = speedBoostAmounts[currentLevel];
        playerInstance.stat.currentPlayerSpeed += boostAmount;

        playerInstance.StartCoroutine(SpeedBoostCoroutine(boostAmount));
    }

    /// <summary>
    /// �̵� �ӵ� �ν�Ʈ�� ���� �ð��� �����ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <param name="boostAmount">�ν�Ʈ�� �̵� �ӵ� ��</param>
    /// <returns></returns>
    private IEnumerator SpeedBoostCoroutine(float boostAmount)
    {
        yield return new WaitForSeconds(boostDuration);

        playerInstance.stat.currentPlayerSpeed -= boostAmount;
        isBoostActive = false;
        killCount = 0;
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ �̵� �ӵ� �������� ������ �迭�� ���� ����˴ϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel�� 5��� currentLevel�� 0~4
        {
            currentLevel++;
        }
        else
        {
            Debug.LogWarning("KillSpeedBoostAbility: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// ���� ������ �̵� �ӵ� �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���������� �̵� �ӵ� ������ (�ۼ�Ʈ)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < speedBoostAmounts.Length)
        {
            return Mathf.RoundToInt(speedBoostAmounts[currentLevel + 1] * 100); // �ۼ�Ʈ�� ��ȯ
        }
        return 0;
    }

    /// <summary>
    /// �ɷ��� ���� ���¿� ȿ���� �����ϴ� ���ڿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel && currentLevel < speedBoostAmounts.Length)
        {
            float boostAmount = speedBoostAmounts[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: �̵� �ӵ� +{boostAmount} ���� (ų {killThreshold}ȸ �޼� ��)";
        }
        else if (currentLevel == maxLevel && currentLevel < speedBoostAmounts.Length)
        {
            float boostAmount = speedBoostAmounts[currentLevel];
            return $"{baseDescription}\n�ִ� ���� ����: �̵� �ӵ� +{boostAmount} ���� (ų {killThreshold}ȸ �޼� ��)";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����";
        }
    }
}
