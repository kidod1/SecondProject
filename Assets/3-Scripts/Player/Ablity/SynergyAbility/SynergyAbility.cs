using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SynergyAbility")]
public class SynergyAbility : Ability
{
    public override void Apply(Player player)
    {
        // �ó��� �ɷ� ���� ����
    }

    public override void Upgrade()
    {
        // �ó��� �ɷ��� ���׷��̵� ����
    }

    public override string GetDescription()
    {
        return baseDescription; // �ó��� �ɷ��� ������ ���� ���� ���� ����
    }

    protected override int GetNextLevelIncrease()
    {
        return 0; // �ó��� �ɷ¿��� ������� ����
    }
}
