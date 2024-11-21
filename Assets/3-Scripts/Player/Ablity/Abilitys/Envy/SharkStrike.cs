using System.Collections;
using UnityEngine;
using AK.Wwise; // WWISE 네임스페이스 추가

[CreateAssetMenu(menuName = "Abilities/SharkStrike")]
public class SharkStrike : Ability
{
    [Tooltip("레벨별 상어 데미지 증가량")]
    public int[] damageIncreases = { 10, 20, 30, 40, 50 };  // 레벨 1~5

    public GameObject sharkPrefab;  // 상어 프리팹
    public int hitThreshold = 5;  // 적중 임계값
    public float sharkSpeed = 5f;  // 상어 속도
    public float chaseDelay = 0.5f;  // 상어 추격 시작 전 대기 시간
    public float maxSearchTime = 3f; // 상어가 몬스터를 찾는 최대 시간

    private Player playerInstance;
    private int hitCount = 0;

    public override void Apply(Player player)
    {
        playerInstance = player;
    }

    public void OnProjectileHit(Collider2D enemy)
    {
        hitCount++;

        if (hitCount >= hitThreshold)
        {
            SpawnShark();
            hitCount = 0;
        }
    }

    private void SpawnShark()
    {
        if (sharkPrefab != null && playerInstance != null)
        {
            GameObject sharkObject = Instantiate(sharkPrefab, playerInstance.transform.position, Quaternion.identity);
            Shark sharkInstance = sharkObject.GetComponent<Shark>();

            if (sharkInstance != null)
            {
                int damageIncrease = GetSharkDamageIncrease();
                sharkInstance.Initialize(sharkSpeed, chaseDelay, maxSearchTime, damageIncrease);

            }
            else
            {
                Debug.LogError("Shark 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("SharkPrefab 또는 PlayerInstance가 설정되지 않았습니다.");
        }
    }

    private int GetSharkDamageIncrease()
    {
        if (currentLevel < damageIncreases.Length)
        {
            return damageIncreases[currentLevel - 1];
        }
        else
        {
            return damageIncreases[damageIncreases.Length - 1];
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            // 업그레이드 로직 추가 필요 시 구현
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < damageIncreases.Length)
        {
            return damageIncreases[currentLevel + 1];
        }
        return 0;
    }

    public override void ResetLevel()
    {
        hitCount = 0;
        currentLevel = 0;
    }

    public override string GetDescription()
    {
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
