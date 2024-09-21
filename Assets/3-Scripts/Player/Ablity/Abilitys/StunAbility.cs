using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/StunAbility")]
public class StunAbility : Ability
{
    [Range(0f, 1f)]
    public float stunChance = 0.25f;
    public float stunDuration = 2f;   // ���� ���� �ð�

    public override void Apply(Player player)
    {
        // �ɷ� ���� ���� (�÷��̾��� �ɷ� ��Ͽ� �߰��ϴ� ��)
        Debug.Log($"Stun ability applied. Current Level: {currentLevel}");
    }

    public void TryStun(Monster monster)
    {
        float randomValue = Random.value;
        if (randomValue < stunChance) // ���� Ȯ�� üũ
        {
            monster.Stun(); // ���� ������Ű��
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
