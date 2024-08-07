using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/IncreaseSpeed")]
public class IncreaseSpeed : Ability
{
    public float[] speedIncreases;

    // 플레이어에 어빌리티 적용
    public override void Apply(Player player)
    {
        if (currentLevel > 0)
        {
            player.stat.playerSpeed += speedIncreases[currentLevel - 1];
        }
    }

    // 어빌리티 업그레이드
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
        }
    }

    // 다음 레벨 증가값 반환
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel)
        {
            return (int)speedIncreases[currentLevel];
        }
        return 0;
    }
}
