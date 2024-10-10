using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/JokerDraw")]
public class JokerDraw : Ability
{
    [Tooltip("레벨별 일반 몬스터 즉사 확률 (0.0f ~ 1.0f)")]
    [Range(0f, 1f)]
    public float[] instantKillChances; // 레벨별 즉사 확률 배열

    private Player playerInstance;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("JokerDraw Apply: 플레이어 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;
        Debug.Log($"JokerDraw이(가) 플레이어에게 적용되었습니다. 현재 레벨: {currentLevel + 1}");
    }

    /// <summary>
    /// 몬스터를 공격했을 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="enemy">공격한 몬스터의 Collider2D</param>
    public void OnHitMonster(Collider2D enemy)
    {
        if (playerInstance == null)
        {
            Debug.LogError("JokerDraw OnHitMonster: 플레이어 인스턴스가 null입니다.");
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
                Debug.Log($"JokerDraw: {monster.name}을(를) 즉사시켰습니다! (확률: {currentChance * 100}%)");
            }
        }
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 즉사 확률을 증가시킵니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel이 5라면 currentLevel은 0~4
        {
            currentLevel++;
            Debug.Log($"JokerDraw 업그레이드: 현재 레벨 {currentLevel + 1}, 즉사 확률 {GetCurrentInstantKillChance() * 100}%");
        }
        else
        {
            Debug.LogWarning("JokerDraw: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 다음 레벨 증가값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨에서의 즉사 확률 증가 (퍼센트)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel && currentLevel < instantKillChances.Length)
        {
            return Mathf.RoundToInt(instantKillChances[currentLevel] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    /// <summary>
    /// 능력의 현재 상태와 효과를 설명하는 문자열을 반환합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
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

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        playerInstance = null;
        Debug.Log("JokerDraw 레벨이 초기화되었습니다.");
    }

    /// <summary>
    /// 현재 레벨의 즉사 확률을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 즉사 확률</returns>
    private float GetCurrentInstantKillChance()
    {
        if (currentLevel < instantKillChances.Length)
        {
            return instantKillChances[currentLevel];
        }
        else
        {
            Debug.LogWarning($"JokerDraw: 현재 레벨 {currentLevel + 1}이 instantKillChances 배열의 범위를 벗어났습니다. 마지막 값을 사용합니다.");
            return instantKillChances[instantKillChances.Length - 1];
        }
    }
}
