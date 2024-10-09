using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/FieryBloodToastAbility")]
public class FieryBloodToastAbility : Ability
{
    [Header("Damage Multiplier Parameters")]
    [Tooltip("������ �ִ� ���ݷ� ����")]
    public float[] damageMultipliers = { 1.5f, 1.75f, 2.0f }; // ��: ���� 1~3

    private Player playerInstance;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("FieryBloodToastAbility Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;
        playerInstance.OnTakeDamage.AddListener(UpdateDamage);
        playerInstance.OnHeal.AddListener(UpdateDamage); // ȸ�� �ÿ��� ������Ʈ

        // �ɷ� ���� �� ��� ���ݷ� ������Ʈ
        UpdateDamage();
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ������ ������ �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            Debug.Log($"FieryBloodToastAbility ���׷��̵�: ���� ���� {currentLevel}");

            // ���� �� �� ������ ���� ������Ʈ
            UpdateDamage();
        }
        else
        {
            Debug.LogWarning("FieryBloodToastAbility: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            float damageMultiplier = damageMultipliers[currentLevel];
            return $"{baseDescription}{Environment.NewLine}(Level {currentLevel + 1}: x{damageMultiplier} ���ݷ�)";
        }
        else
        {
            float finalDamageMultiplier = damageMultipliers[currentLevel];
            return $"{baseDescription}{Environment.NewLine}(Max Level: x{finalDamageMultiplier} ���ݷ�)";
        }
    }

    /// <summary>
    /// ���� ���� ������ �ʿ��� ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���� ���� �� �ʿ��� ��</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < damageMultipliers.Length)
        {
            return Mathf.RoundToInt(damageMultipliers[currentLevel]);
        }
        Debug.LogWarning($"FieryBloodToastAbility: currentLevel ({currentLevel})�� damageMultipliers �迭�� ������ ������ϴ�. �⺻�� 1�� ��ȯ�մϴ�.");
        return 1;
    }

    /// <summary>
    /// �÷��̾��� ���ݷ��� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdateDamage()
    {
        if (playerInstance == null || playerInstance.stat == null)
        {
            Debug.LogWarning("FieryBloodToastAbility UpdateDamage: playerInstance �Ǵ� playerInstance.stat�� null�Դϴ�.");
            return;
        }

        float healthPercentage = (float)playerInstance.stat.currentHP / playerInstance.stat.currentMaxHP;
        float damageMultiplier = GetDamageMultiplier();

        // basePlayerDamage�� PlayerData�� ���ǵǾ� �־�� �մϴ�.
        playerInstance.stat.currentPlayerDamage = Mathf.RoundToInt(playerInstance.stat.defaultPlayerDamage * damageMultiplier);

        Debug.Log($"FieryBloodToastAbility: �÷��̾��� ���� ������ ������ x{damageMultiplier}�� ������Ʈ�Ǿ����ϴ�.");
    }

    /// <summary>
    /// �÷��̾��� ���� ü�� ������ ���� ������ ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ����</returns>
    public float GetDamageMultiplier()
    {
        if (playerInstance == null || playerInstance.stat == null)
        {
            return 1f;
        }

        float healthPercentage = (float)playerInstance.stat.currentHP / playerInstance.stat.currentMaxHP;
        // ü���� �������� ���� ������ ����
        float damageMultiplier = Mathf.Lerp(damageMultipliers[0], damageMultipliers[Mathf.Min(currentLevel, damageMultipliers.Length - 1)], 1f - healthPercentage);
        return damageMultiplier;
    }

    /// <summary>
    /// �ɷ��� �����մϴ�.
    /// </summary>
    public void RemoveAbility()
    {
        if (playerInstance != null)
        {
            playerInstance.OnTakeDamage.RemoveListener(UpdateDamage);
            playerInstance.OnHeal.RemoveListener(UpdateDamage);
        }
        currentLevel = 0;
    }
    
}
