using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/JokerDraw")]
public class JokerDraw : Ability
{
    [Range(0f, 1f)]
    public float instantKillChance = 0.01f;

    private Player playerInstance;

    public override void Apply(Player player)
    {
        playerInstance = player;
    }

    public void OnHitMonster(Collider2D enemy)
    {
        Monster monster = enemy.GetComponent<Monster>();

        if (monster != null && !monster.isElite)
        {
            if (Random.value < instantKillChance)
            {
                monster.TakeDamage(monster.GetCurrentHP());
            }
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
    }

    protected override int GetNextLevelIncrease()
    {
        return 0;
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
        }
    }
}
