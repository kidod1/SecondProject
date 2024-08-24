using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseAttackSpeed")]
public class IncreaseAttackSpeed : Ability
{
    public float[] cooldownReductions;

    public override void Apply(Player player)
    {
        if (currentLevel > 0)
        {
            player.stat.currentShotCooldown -= cooldownReductions[currentLevel];
            if (player.stat.currentShotCooldown < 0.1f)
            {
                player.stat.currentShotCooldown = 0.1f;
            }
        }
    }

    // �����Ƽ ���׷��̵�
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
        }
    }

    // ���� ���� ������ ��ȯ
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel)
        {
            return (int)cooldownReductions[currentLevel];
        }
        return 0;
    }
}
