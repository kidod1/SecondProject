using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Abilities/FieryBloodToastAbility")]
public class FieryBloodToastAbility : Ability
{
    [Header("Damage Multiplier Parameters")]
    [Tooltip("������ �ִ� ���ݷ� ����")]
    public float[] damageMultipliers = { 1.5f, 1.75f, 2.0f }; // ��: ���� 1~3

    private PlayerData playerData;
    private string buffIdentifier;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("FieryBloodToastAbility Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        if (currentLevel == 0)
        {
            playerData = player.stat; // PlayerData ����
            buffIdentifier = this.name;

            player.OnTakeDamage.AddListener(UpdateDamage);
            player.OnHeal.AddListener(UpdateDamage); // ȸ�� �ÿ��� ������Ʈ

            // �ɷ� ���� �� ��� ���ݷ� ������Ʈ
            UpdateDamage();
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            UpdateDamage();
        }
        else
        {
            Debug.LogWarning("FieryBloodToastAbility: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";

        description += $"Lv {currentLevel + 1}:\n";
        description += $"ü���� �������� ���ݷ� ���� (����: x{damageMultipliers[currentLevel]:F2})\n";

        return description;
    }

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
    /// ���� ü�¿� ���� ������ ������ ��ȯ�մϴ�.
    /// </summary>
    public float GetDamageMultiplier()
    {
        if (playerData == null)
            return 1f;

        float healthPercentage = (float)playerData.currentHP / playerData.currentMaxHP;
        // ü���� �������� ���� ������ ����
        float maxMultiplier = damageMultipliers[Mathf.Min(currentLevel, damageMultipliers.Length - 1)];
        float damageMultiplier = Mathf.Lerp(1f, maxMultiplier, 1f - healthPercentage);
        return damageMultiplier;
    }

    private void UpdateDamage()
    {
        if (playerData == null)
            return;

        float damageMultiplier = GetDamageMultiplier();
        float additionalDamage = playerData.defaultPlayerDamage * (damageMultiplier - 1f);

        // ���� ���� ����
        playerData.RemoveBuff(buffIdentifier, BuffType.AttackDamage);

        // ���ο� ���� ����
        playerData.AddBuff(buffIdentifier, BuffType.AttackDamage, additionalDamage);
    }

    public void RemoveAbility()
    {
        if (playerData != null)
        {
            playerData.RemoveBuff(buffIdentifier, BuffType.AttackDamage);
        }
        currentLevel = 0;
    }
}
