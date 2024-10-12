using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/JokerDraw")]
public class JokerDraw : Ability
{
    [Tooltip("������ �Ϲ� ���� ��� Ȯ�� (0.0f ~ 1.0f)")]
    [Range(0f, 1f)]
    public float[] instantKillChances; // ������ ��� Ȯ�� �迭

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

        // �Ϲ� ���Ϳ� ���ؼ��� ��� Ȯ�� ����
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
            return Mathf.RoundToInt(instantKillChances[currentLevel] * 100); // �ۼ�Ʈ�� ��ȯ
        }
        return 0;
    }

    public override string GetDescription()
    {
        if (currentLevel < maxLevel && currentLevel < instantKillChances.Length)
        {
            float percentChance = instantKillChances[currentLevel] * 100f;
            return $"{baseDescription}\nLv {currentLevel + 1}: �Ϲ� ���� ��� Ȯ�� +{percentChance}%";
        }
        else if (currentLevel == maxLevel && currentLevel < instantKillChances.Length)
        {
            float percentChance = instantKillChances[currentLevel] * 100f;
            return $"{baseDescription}\n�ִ� ���� ����: �Ϲ� ���� ��� Ȯ�� +{percentChance}%";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����";
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
