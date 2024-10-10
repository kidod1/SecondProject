using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ParasiticNest")]
public class ParasiticNest : Ability
{
    [Tooltip("레벨별 감염 확률 (0.0f ~ 1.0f)")]
    [Range(0f, 1f)]
    public float[] infectionChances; // 레벨별 감염 확률 배열

    private Player playerInstance;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("ParasiticNest Apply: 플레이어 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;
        Debug.Log($"ParasiticNest이(가) 플레이어에게 적용되었습니다. 현재 레벨: {currentLevel + 1}");
    }

    /// <summary>
    /// 투사체가 몬스터에 맞았을 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="enemy">공격한 몬스터의 Collider2D</param>
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
                Debug.Log($"ParasiticNest: {monster.name}이(가) 감염되었습니다! (확률: {currentChance * 100}%)");
            }
        }
    }

    /// <summary>
    /// 감염 효과를 적용하는 코루틴입니다.
    /// </summary>
    /// <param name="monster">감염 대상 몬스터</param>
    /// <returns></returns>
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

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        playerInstance = null;
        Debug.Log("ParasiticNest 레벨이 초기화되었습니다.");
    }

    /// <summary>
    /// 다음 레벨의 감염 확률 증가값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨에서의 감염 확률 증가 (퍼센트)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < infectionChances.Length)
        {
            return Mathf.RoundToInt(infectionChances[currentLevel + 1] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 감염 확률이 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel이 5라면 currentLevel은 0~4
        {
            currentLevel++;
            Debug.Log($"ParasiticNest 업그레이드: 현재 레벨 {currentLevel + 1}, 감염 확률 {GetCurrentInfectionChance() * 100}%");
        }
        else
        {
            Debug.LogWarning("ParasiticNest: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력의 현재 상태와 효과를 설명하는 문자열을 반환합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
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

    /// <summary>
    /// 현재 레벨의 감염 확률을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 감염 확률</returns>
    private float GetCurrentInfectionChance()
    {
        if (currentLevel < infectionChances.Length)
        {
            return infectionChances[currentLevel];
        }
        else
        {
            Debug.LogWarning($"ParasiticNest: 현재 레벨 {currentLevel + 1}이 infectionChances 배열의 범위를 벗어났습니다. 마지막 값을 사용합니다.");
            return infectionChances[infectionChances.Length - 1];
        }
    }
}
