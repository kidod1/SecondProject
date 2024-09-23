using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/ParasiticNest")]
public class ParasiticNest : Ability
{
    [Range(0f, 1f)]
    public float infectionChance = 0.2f;
    private Player playerInstance;

    public override void Apply(Player player)
    {
        playerInstance = player;
    }

    public void OnProjectileHit(Collider2D enemy)
    {
        if (Random.value <= infectionChance)
        {
            Monster monster = enemy.GetComponent<Monster>();
            if (monster != null && !monster.isInfected)
            {
                monster.isInfected = true;
                monster.StartCoroutine(ApplyInfectionEffect(monster));
            }
        }
    }

    private IEnumerator ApplyInfectionEffect(Monster monster)
    {
        SpriteRenderer renderer = monster.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.color;
            renderer.color = Color.green;

            yield return new WaitForSecondsRealtime(0.5f);
            renderer.color = originalColor;
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
    }

    protected override int GetNextLevelIncrease()
    {
        return 1;
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            // ���� �� �� ���� Ȯ�� ����
            infectionChance += 0.05f; // ��: ���� �� 5% ����
        }
    }
}
