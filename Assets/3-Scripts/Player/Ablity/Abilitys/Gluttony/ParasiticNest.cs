using System.Collections;
using UnityEngine;
using AK.Wwise; // Step 1: Import the WWISE namespace

[CreateAssetMenu(menuName = "Abilities/ParasiticNest")]
public class ParasiticNest : Ability
{
    [Tooltip("레벨별 감염 확률 (0.0f ~ 1.0f)")]
    [Range(0f, 1f)]
    public float[] infectionChances; // 레벨별 감염 확률 배열

    [Header("WWISE Sound Events")]
    [Tooltip("ParasiticNest 능력 발동 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event activateSound; // Step 2: Add WWISE event field

    private Player playerInstance;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            return;
        }

        playerInstance = player;

        // Play the activate sound when the ability is applied
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    public void OnProjectileHit(Collider2D enemy)
    {
        float currentChance = GetCurrentInfectionChance();
        if (Random.value <= currentChance)
        {
            Monster monster = enemy.GetComponent<Monster>();
            if (monster != null && !monster.isInfected)
            {
                monster.isInfected = true;
                monster.StartCoroutine(ApplyInfectionEffect(monster));

                // Play the infection sound when an enemy is infected
                if (activateSound != null)
                {
                    activateSound.Post(monster.gameObject);
                }
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

            yield return new WaitForSeconds(0.5f);
            renderer.color = originalColor;
        }
    }

    public override void ResetLevel()
    {
        currentLevel = 0;
        playerInstance = null;
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < infectionChances.Length)
        {
            return Mathf.RoundToInt(infectionChances[currentLevel + 1] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel && currentLevel < infectionChances.Length - 1)
        {
        }
    }

    public override string GetDescription()
    {
        if (currentLevel < maxLevel && currentLevel < infectionChances.Length)
        {
            float percentChance = infectionChances[currentLevel] * 100f;
            return $"{baseDescription}\nLv {currentLevel + 1}: 투사체 히트 시 감염 확률 +{percentChance}%";
        }
        else if (currentLevel == maxLevel && currentLevel < infectionChances.Length)
        {
            float percentChance = infectionChances[currentLevel] * 100f;
            return $"{baseDescription}\n최대 레벨 도달: 투사체 히트 시 감염 확률 +{percentChance}%";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달";
        }
    }

    private float GetCurrentInfectionChance()
    {
        if (currentLevel < infectionChances.Length)
        {
            return infectionChances[currentLevel];
        }
        else
        {
            return infectionChances[infectionChances.Length - 1];
        }
    }
}
