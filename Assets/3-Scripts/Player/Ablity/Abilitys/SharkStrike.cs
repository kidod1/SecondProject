using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/SharkStrike")]
public class SharkStrike : Ability
{
    [Tooltip("레벨별 상어 데미지 증가량")]
    public int[] damageIncreases;  // 레벨별 상어 데미지 증가량 배열

    public GameObject sharkPrefab;  // 상어 프리팹
    public int hitThreshold = 5;  // 적중 임계값
    public float sharkSpeed = 5f;  // 상어 속도
    public float chaseDelay = 0.5f;  // 상어 추격 시작 전 대기 시간
    public float maxSearchTime = 3f; // 상어가 몬스터를 찾는 최대 시간

    private Player playerInstance;
    private int hitCount = 0;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        playerInstance = player;
        Debug.Log($"SharkStrike applied. Current Level: {currentLevel + 1}");
    }

    /// <summary>
    /// 플레이어가 적을 적중시켰을 때 호출되는 메서드
    /// </summary>
    /// <param name="enemy">맞은 적의 콜라이더</param>
    public void OnProjectileHit(Collider2D enemy)
    {
        hitCount++;
        Debug.Log($"SharkStrike HitCount: {hitCount}/{hitThreshold}");

        if (hitCount >= hitThreshold)
        {
            SpawnShark();
            hitCount = 0;
        }
    }

    /// <summary>
    /// 상어를 생성하는 메서드
    /// </summary>
    private void SpawnShark()
    {
        if (sharkPrefab != null)
        {
            GameObject sharkObject = Instantiate(sharkPrefab, playerInstance.transform.position, Quaternion.identity);
            Shark sharkInstance = sharkObject.GetComponent<Shark>();

            if (sharkInstance != null)
            {
                // 레벨에 따른 데미지 증가량을 가져옴
                int damageIncrease = GetSharkDamageIncrease();
                sharkInstance.Initialize(sharkSpeed, chaseDelay, maxSearchTime, damageIncrease);
                Debug.Log($"SharkStrike: 상어가 생성되었습니다. 데미지 증가량: {damageIncrease}");
            }
            else
            {
                Debug.LogError("SharkStrike: Shark 컴포넌트가 프리팹에 없습니다.");
            }
        }
        else
        {
            Debug.LogError("SharkStrike: Shark 프리팹이 없습니다. 상어를 생성할 수 없습니다.");
        }
    }


    /// <summary>
    /// 현재 레벨에 따른 상어 데미지 증가량을 반환합니다.
    /// </summary>
    /// <returns>상어의 데미지 증가량 (정수)</returns>
    private int GetSharkDamageIncrease()
    {
        if (currentLevel < damageIncreases.Length)
        {
            return damageIncreases[currentLevel];
        }
        else
        {
            Debug.LogWarning($"SharkStrike: currentLevel ({currentLevel}) exceeds damageIncreases 배열 범위. 마지막 레벨의 데미지를 사용합니다.");
            return damageIncreases[damageIncreases.Length - 1];
        }
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 데미지가 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel이 5라면 currentLevel은 0~4
        {
            currentLevel++;
            Debug.Log($"SharkStrike upgraded to Level {currentLevel + 1}. 데미지 증가량: {damageIncreases[currentLevel]}");
        }
        else
        {
            Debug.LogWarning("SharkStrike: Already at max level.");
        }
    }

    /// <summary>
    /// 다음 레벨의 데미지 증가값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨에서의 데미지 증가량 (정수)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < damageIncreases.Length)
        {
            return damageIncreases[currentLevel + 1];
        }
        return 0;
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        hitCount = 0;  // 적중 횟수 초기화
        currentLevel = 0;
        Debug.Log("SharkStrike level has been reset.");
    }

    /// <summary>
    /// 능력의 현재 상태와 효과를 설명하는 문자열을 반환합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {
        Debug.Log($"GetDescription called. Current Level: {currentLevel + 1}, damageIncreases.Length: {damageIncreases.Length}, maxLevel: {maxLevel}");

        if (currentLevel < damageIncreases.Length && currentLevel >= 0)
        {
            int damageIncrease = damageIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 적을 {hitThreshold}회 맞출 때마다 적을 따라다니는 상어 소환. 데미지 +{damageIncrease}";
        }
        else if (currentLevel >= damageIncreases.Length)
        {
            int maxDamageIncrease = damageIncreases[damageIncreases.Length - 1];
            return $"{baseDescription}\n최대 레벨 도달: 적을 {hitThreshold}회 맞출 때마다 적을 따라다니는 상어 소환. 데미지 +{maxDamageIncrease}";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달.";
        }
    }
}
