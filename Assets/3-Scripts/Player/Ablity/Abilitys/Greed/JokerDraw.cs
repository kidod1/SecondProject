using UnityEngine;
using AK.Wwise;

[CreateAssetMenu(menuName = "Abilities/JokerDraw")]
public class JokerDraw : Ability
{
    [Tooltip("������ �Ϲ� ���� ��� Ȯ�� (0.0f ~ 1.0f)")]
    [Range(0f, 1f)]
    public float[] instantKillChances; // ������ ��� Ȯ�� �迭

    [Header("WWISE Sound Events")]
    [Tooltip("JokerDraw �ɷ� ��ġ �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event activateSound;

    [Tooltip("���� ��� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event instantKillSound;

    private Player playerInstance;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            return;
        }

        playerInstance = player;

        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    public void OnHitMonster(Collider2D enemy)
    {
        if (playerInstance == null)
        {
            return;
        }

        Monster monster = enemy.GetComponent<Monster>();

        if (monster != null && !monster.isElite)
        {
            float currentChance = GetCurrentInstantKillChance();

            if (Random.value < currentChance)
            {
                Debug.Log("JokerDraw: ��� Ȯ�� ����! ���͸� ��� ���Դϴ�.");
                monster.TakeDamage(monster.GetCurrentHP(), PlayManager.I.GetPlayerPosition());
                monster.isInstantKilled = true;

                if (instantKillSound != null)
                {
                    instantKillSound.Post(monster.gameObject);
                }
            }
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel && currentLevel < instantKillChances.Length)
        {
            return Mathf.RoundToInt(instantKillChances[currentLevel] * 100);
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
        playerInstance = null;
    }

    private float GetCurrentInstantKillChance()
    {
        if (currentLevel < instantKillChances.Length)
        {
            return instantKillChances[currentLevel - 1];
        }
        else
        {
            return instantKillChances[instantKillChances.Length - 1];
        }
    }
}
