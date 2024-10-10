using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SummonClone")]
public class SummonClone : Ability
{
    [Tooltip("레벨별 클론 데미지 배율 (예: 0.3f = 30%)")]
    [Range(0f, 2f)] // 데미지 배율 범위 설정 (0% ~ 200%)
    public float[] damageMultipliers = { 0.3f, 0.5f, 0.7f, 1.0f, 1.2f }; // 레벨별 데미지 배율 배열

    public GameObject clonePrefab;  // 상어 프리팹
    private GameObject cloneInstance;
    private RotatingObject rotatingObject;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {

        if (currentLevel < damageMultipliers.Length)
        {
            if (cloneInstance == null)
            {
                cloneInstance = Instantiate(clonePrefab, player.transform);
                rotatingObject = cloneInstance.GetComponent<RotatingObject>();
                if (rotatingObject != null)
                {
                    rotatingObject.player = player.transform;
                    rotatingObject.playerShooting = player;
                    rotatingObject.damageMultiplier = damageMultipliers[currentLevel];
                    Debug.Log($"SummonClone applied at Level {currentLevel + 1} with Damage Multiplier: {damageMultipliers[currentLevel] * 100}%");
                }
                else
                {
                    Debug.LogError("SummonClone: RotatingObject 컴포넌트가 클론 프리팹에 없습니다.");
                }
            }
            else
            {
                if (rotatingObject != null)
                {
                    rotatingObject.damageMultiplier = damageMultipliers[currentLevel];
                    Debug.Log($"SummonClone: 기존 클론의 Damage Multiplier이 {damageMultipliers[currentLevel] * 100}%로 업데이트되었습니다.");
                }
                else
                {
                    Debug.LogError("SummonClone: RotatingObject가 초기화되지 않았습니다.");
                }
            }
        }
        else
        {
            Debug.LogWarning($"SummonClone: currentLevel ({currentLevel + 1})이 damageMultipliers 배열 범위를 초과했습니다. 마지막 레벨의 데미지 배율을 사용합니다.");
            if (rotatingObject != null)
            {
                rotatingObject.damageMultiplier = damageMultipliers[damageMultipliers.Length - 1];
                Debug.Log($"SummonClone: 클론의 Damage Multiplier이 {damageMultipliers[damageMultipliers.Length - 1] * 100}%로 설정되었습니다.");
            }
        }
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 데미지 배율이 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel이 5라면 currentLevel은 0~4
        {
            currentLevel++;
            Debug.Log($"SummonClone 업그레이드: 현재 레벨 {currentLevel + 1}, 데미지 배율 {damageMultipliers[currentLevel] * 100}%");
            Apply(PlayManager.I.GetPlayer()); // 업그레이드 후 클론의 데미지 배율을 업데이트
        }
        else
        {
            Debug.LogWarning("SummonClone: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 다음 레벨의 데미지 증가값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨에서의 데미지 증가량 (퍼센트)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < damageMultipliers.Length)
        {
            return Mathf.RoundToInt(damageMultipliers[currentLevel + 1] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        if (cloneInstance != null)
        {
            Destroy(cloneInstance);
            cloneInstance = null;
            rotatingObject = null;
            Debug.Log("SummonClone 레벨이 초기화되었습니다. 클론이 파괴되었습니다.");
        }
    }

    /// <summary>
    /// 능력의 현재 상태와 효과를 설명하는 문자열을 반환합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {
        Debug.Log($"GetDescription called. Current Level: {currentLevel + 1}, damageMultipliers.Length: {damageMultipliers.Length}, maxLevel: {maxLevel}");

        if (currentLevel < damageMultipliers.Length && currentLevel >= 0)
        {
            float damageMultiplierPercent = damageMultipliers[currentLevel] * 100f;
            return $"{baseDescription}\nLv {currentLevel + 1}: 클론의 데미지 {damageMultiplierPercent}% 증가";
        }
        else if (currentLevel >= damageMultipliers.Length)
        {
            float maxDamageMultiplierPercent = damageMultipliers[damageMultipliers.Length - 1] * 100f;
            return $"{baseDescription}\n최대 레벨 도달: 클론의 데미지 {maxDamageMultiplierPercent}% 증가";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달.";
        }
    }
}
