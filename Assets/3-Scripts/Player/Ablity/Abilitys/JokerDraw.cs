using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/JokerDraw")]
public class JokerDraw : Ability
{
    [Tooltip("레벨별 일반 몬스터 즉사 확률 (0.0f ~ 1.0f)")]
    [Range(0f, 1f)]
    public float[] instantKillChances; // 레벨별 즉사 확률 배열

    private Player playerInstance;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            return;
        }

        playerInstance = player;
    }

    public void OnHitMonster(Collider2D enemy)
    {
        if (playerInstance == null)
        {
            return;
        }

        Monster monster = enemy.GetComponent<Monster>();

        // 일반 몬스터에 대해서만 즉사 확률 적용
        if (monster != null && !monster.isElite)
        {
            float currentChance = GetCurrentInstantKillChance();
            if (Random.value < currentChance)
            {
                monster.TakeDamage(monster.GetCurrentHP(), PlayManager.I.GetPlayerPosition());
            }
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel && currentLevel < instantKillChances.Length)
        {
            return Mathf.RoundToInt(instantKillChances[currentLevel] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    public override string GetDescription()
    {
        if (currentLevel < maxLevel && currentLevel < instantKillChances.Length)
        {
            float percentChance = instantKillChances[currentLevel] * 100f;
            return $"{baseDescription}\nLv {currentLevel + 1}: 일반 몬스터 즉사 확률 +{percentChance}%";
        }
        else if (currentLevel == maxLevel && currentLevel < instantKillChances.Length)
        {
            float percentChance = instantKillChances[currentLevel] * 100f;
            return $"{baseDescription}\n최대 레벨 도달: 일반 몬스터 즉사 확률 +{percentChance}%";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달";
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        playerInstance = null;
    }

    private float GetCurrentInstantKillChance()
    {
        if (currentLevel < instantKillChances.Length)
        {
            return instantKillChances[currentLevel];
        }
        else
        {
            return instantKillChances[instantKillChances.Length - 1];
        }
    }
}
