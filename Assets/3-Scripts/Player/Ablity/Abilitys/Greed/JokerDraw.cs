using UnityEngine;
using AK.Wwise;

[CreateAssetMenu(menuName = "Abilities/JokerDraw")]
public class JokerDraw : Ability
{
    [Tooltip("레벨별 일반 몬스터 즉사 확률 (0.0f ~ 1.0f)")]
    [Range(0f, 1f)]
    public float[] instantKillChances; // 레벨별 즉사 확률 배열

    [Header("WWISE Sound Events")]
    [Tooltip("JokerDraw 능력 설치 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event activateSound;

    [Tooltip("몬스터 즉사 시 재생될 WWISE 이벤트")]
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
                Debug.Log("JokerDraw: 즉사 확률 성공! 몬스터를 즉시 죽입니다.");
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
