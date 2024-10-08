using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/FieryBloodToastAbility")]
public class FieryBloodToastAbility : Ability
{
    public float maxDamageMultiplier = 2.0f; // �ִ� ���ݷ� ����

    private Player playerInstance;

    public override void Apply(Player player)
    {
        playerInstance = player;
        playerInstance.OnTakeDamage.AddListener(UpdateDamage);
        playerInstance.OnHeal.AddListener(UpdateDamage); // ȸ�� �ÿ��� ������Ʈ

        // �ɷ� ���� �� ��� ���ݷ� ������Ʈ
        UpdateDamage();
    }

    public override void Upgrade()
    {
        // ���׷��̵� ���� �߰� ����
    }

    public override string GetDescription()
    {
        return "������� �������� �ִ� ���ط��� �����Ѵ�.";
    }

    protected override int GetNextLevelIncrease()
    {
        // �ʿ信 ���� ����
        return 0;
    }

    private void UpdateDamage()
    {
        // ���ο��� �÷��̾��� ���ݷ��� ������Ʈ�մϴ�.
        // �ʿ信 ���� ����
    }

    public float GetDamageMultiplier()
    {
        // �÷��̾��� ���� ü�� ������ ���� ������ ������ ��ȯ�մϴ�.
        if (playerInstance == null || playerInstance.stat == null)
        {
            return 1f;
        }

        float healthPercentage = (float)playerInstance.stat.currentHP / playerInstance.stat.currentMaxHP;
        float damageMultiplier = Mathf.Lerp(maxDamageMultiplier, 1f, healthPercentage);
        return damageMultiplier;
    }

    public void RemoveAbility()
    {
        if (playerInstance != null)
        {
            playerInstance.OnTakeDamage.RemoveListener(UpdateDamage);
            playerInstance.OnHeal.RemoveListener(UpdateDamage);
        }
    }
}
