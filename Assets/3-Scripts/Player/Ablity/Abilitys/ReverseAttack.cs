using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ReverseAttack")]
public class ReverseAttack : Ability
{
    [Tooltip("레벨별 반전 공격 데미지 퍼센트 (0.0f ~ 1.0f)")]
    [Range(0f, 1f)]
    public float[] damagePercentages; // 레벨별 반전 공격 데미지 퍼센트 배열

    private Player playerInstance;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        // 중복 등록 방지
        if (playerInstance != null)
        {
            playerInstance.OnShoot.RemoveListener(OnShootHandler);
        }

        playerInstance = player;
        playerInstance.OnShoot.AddListener(OnShootHandler); // 시그니처 일치

        // 현재 레벨에 따른 데미지 퍼센트 적용
        ApplyDamagePercentage();

        Debug.Log($"ReverseAttack applied. Current Level: {currentLevel}");
    }

    /// <summary>
    /// 현재 레벨에 따른 데미지 퍼센트를 플레이어에게 적용합니다.
    /// </summary>
    private void ApplyDamagePercentage()
    {
        Debug.Log($"Applying damage percentage. Current Level: {currentLevel}, damagePercentages.Length: {damagePercentages.Length}");

        if (currentLevel <= 0)
        {
            Debug.LogWarning("ReverseAttack: currentLevel is less than or equal to 0.");
            return;
        }

        if (currentLevel - 1 < damagePercentages.Length)
        {
            // 플레이어의 반전 공격 데미지 퍼센트를 설정
            playerInstance.stat.reverseAttackDamagePercentage = damagePercentages[currentLevel - 1];
            Debug.Log($"ReverseAttack: Level {currentLevel} damage percentage set to {damagePercentages[currentLevel - 1] * 100}%");
        }
        else
        {
            Debug.LogWarning($"ReverseAttack: currentLevel ({currentLevel}) exceeds damagePercentages array bounds ({damagePercentages.Length}). Setting to max defined level.");
            // 최대 정의된 레벨로 설정
            playerInstance.stat.reverseAttackDamagePercentage = damagePercentages[damagePercentages.Length - 1];
            currentLevel = damagePercentages.Length; // currentLevel을 최대 레벨로 설정
        }
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 데미지 퍼센트가 변경됩니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            ApplyDamagePercentage();
            Debug.Log($"ReverseAttack upgraded. Current Level: {currentLevel}");
        }
        else
        {
            Debug.LogWarning("ReverseAttack: Already at max level.");
        }
    }

    /// <summary>
    /// 다음 레벨의 데미지 퍼센트 증가값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨에서의 데미지 퍼센트 (퍼센트)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < damagePercentages.Length)
        {
            return Mathf.RoundToInt(damagePercentages[currentLevel] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    /// <summary>
    /// 능력이 적용된 후 플레이어가 발사할 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="direction">발사 방향</param>
    /// <param name="prefabIndex">프리팹 인덱스</param>
    /// <param name="projectile">생성된 프로젝트트</param>
    private void OnShootHandler(Vector2 direction, int prefabIndex, GameObject projectile)
    {
        if (playerInstance != null && projectile != null)
        {
            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                // 원래 데미지를 가져와 퍼센트만큼 줄임
                int originalDamage = projScript.projectileCurrentDamage; // 올바른 필드명 사용
                float reverseDamagePercentage = playerInstance.stat.reverseAttackDamagePercentage;
                int adjustedDamage = Mathf.RoundToInt(originalDamage * reverseDamagePercentage);
                projScript.projectileCurrentDamage = adjustedDamage;

                // 방향은 원래 방향 그대로 유지
                projScript.SetDirection(direction);
                Debug.Log($"ReverseAttack: Original damage adjusted to {adjustedDamage} based on current level percentage.");
            }
            else
            {
                Debug.LogError("ReverseAttack: Projectile script not found.");
            }
        }
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        if (playerInstance != null)
        {
            playerInstance.OnShoot.RemoveListener(OnShootHandler);
            playerInstance.stat.reverseAttackDamagePercentage = 1.0f; // 초기화 (100%)
        }
        playerInstance = null;
        Debug.Log("ReverseAttack level has been reset.");
    }

    /// <summary>
    /// 능력의 현재 상태와 효과를 설명하는 문자열을 반환합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {
        Debug.Log($"GetDescription called. Current Level: {currentLevel}, damagePercentages.Length: {damagePercentages.Length}, maxLevel: {maxLevel}");

        if (currentLevel < maxLevel && currentLevel <= damagePercentages.Length)
        {
            float percentChance = damagePercentages[currentLevel] * 100f;
            return $"{baseDescription}\nLv {currentLevel+1}: 발사 시 공격의 {percentChance}% 데미지로 반전 공격 발동";
        }
        else if (currentLevel == maxLevel && currentLevel <= damagePercentages.Length)
        {
            float percentChance = damagePercentages[currentLevel] * 100f;
            return $"{baseDescription}\n최대 레벨 도달: 발사 시 공격의 {percentChance}% 데미지로 반전 공격 발동";
        }
        else
        {
            return $"{baseDescription}\nMaximum level reached.";
        }
    }
}
