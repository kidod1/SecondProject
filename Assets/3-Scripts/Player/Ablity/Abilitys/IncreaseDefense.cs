using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseDefense")]
public class IncreaseDefense : Ability
{
    public int[] defenseIncreases;

    public override void Apply(Player player)
    {
        if (currentLevel > 0)
        {
            player.stat.currentDefense += defenseIncreases[currentLevel];
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
            return defenseIncreases[currentLevel];
        }
        return 0;
    }
}
