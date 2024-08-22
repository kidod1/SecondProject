using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseSpeed")]
public class IncreaseSpeed : Ability
{
    public float[] speedIncreases;

    // �÷��̾ �����Ƽ ����
    public override void Apply(Player player)
    {
        if (currentLevel > 0)
        {
            player.stat.playerSpeed += speedIncreases[currentLevel];
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
            return (int)speedIncreases[currentLevel];
        }
        return 0;
    }
}
