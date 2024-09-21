using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/StunAbility")]
public class StunAbility : Ability
{
    [Range(0f, 1f)]
    public float stunChance = 0.25f;
    public float stunDuration = 2f;   // 기절 지속 시간

    public override void Apply(Player player)
    {
        // 능력 적용 로직 (플레이어의 능력 목록에 추가하는 등)
        Debug.Log($"Stun ability applied. Current Level: {currentLevel}");
    }

    public void TryStun(Monster monster)
    {
        float randomValue = Random.value;
        if (randomValue < stunChance) // 기절 확률 체크
        {
            monster.Stun(); // 몬스터 기절시키기
            Debug.Log($"{monster.name} is stunned!");
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            Debug.Log($"Stun ability upgraded. Current Level: {currentLevel}");
        }
    }

    protected override int GetNextLevelIncrease()
    {
        return 0;
    }
}
