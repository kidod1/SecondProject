using UnityEngine;
using AK.Wwise;

[CreateAssetMenu(menuName = "Abilities/HomingAttack")]
public class HomingAttack : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("레벨별 Homing 시작 지연 시간 (초)")]
    public float[] homingStartDelayLevels = { 0.3f, 0.25f, 0.2f, 0.15f, 0.1f }; // 레벨 1~5

    [Tooltip("레벨별 Homing 속도")]
    public float[] homingSpeedLevels = { 5f, 6f, 7f, 8f, 9f }; // 레벨 1~5

    [Tooltip("레벨별 Homing 범위")]
    public float[] homingRangeLevels = { 10f, 12f, 14f, 16f, 18f }; // 레벨 1~5

    [Tooltip("유도 탄환 프리팹")]
    public GameObject homingProjectilePrefab;

    [Tooltip("호밍 탄환 생성 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event homingProjectileSound;

    private Player playerInstance;
    private int attackCounter = 0; // 공격 카운터 추가

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("HomingAttack Apply: player 인스턴스가 null입니다.");
            return;
        }
            // 기존 플레이어 인스턴스에 리스너가 등록되어 있으면 제거
            if (playerInstance != null)
            {
                playerInstance.OnShoot.RemoveListener(OnShootHandler);
            }

            playerInstance = player;

            // 리스너 중복 등록 방지
            playerInstance.OnShoot.RemoveListener(OnShootHandler);
            playerInstance.OnShoot.AddListener(OnShootHandler);
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel이 5일 경우, currentLevel은 0~4
        {
        }
        else
        {
            Debug.LogWarning("HomingAttack: 이미 최대 레벨에 도달했습니다.");
        }
    }

    public override void ResetLevel()
    {
        // 이벤트 리스너 제거
        if (playerInstance != null)
        {
            playerInstance.OnShoot.RemoveListener(OnShootHandler);
            playerInstance = null;
        }

        // 공격 카운터 초기화
        attackCounter = 0;
    }

    public override string GetDescription()
    {
        if (currentLevel < homingSpeedLevels.Length)
        {
            float currentDelay = homingStartDelayLevels[currentLevel];
            float currentSpeed = homingSpeedLevels[currentLevel];
            float currentRange = homingRangeLevels[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 지연 {currentDelay}s, 속도 x{currentSpeed}, 범위 {currentRange}m";
        }
        else
        {
            Debug.LogWarning($"HomingAttack: currentLevel ({currentLevel})이 homingSpeedLevels 배열의 범위를 벗어났습니다. 최대 레벨 설명을 반환합니다.");
            float finalDelay = homingStartDelayLevels[homingStartDelayLevels.Length - 1];
            float finalSpeed = homingSpeedLevels[homingSpeedLevels.Length - 1];
            float finalRange = homingRangeLevels[homingRangeLevels.Length - 1];
            return $"{baseDescription}\nMax Level: 지연 {finalDelay}s, 속도 x{finalSpeed}, 범위 {finalRange}m";
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < homingSpeedLevels.Length)
        {
            // 예시: 다음 레벨의 Homing 속도를 반환
            return Mathf.RoundToInt(homingSpeedLevels[currentLevel]);
        }
        Debug.LogWarning($"HomingAttack: currentLevel ({currentLevel})이 homingSpeedLevels 배열의 범위를 벗어났습니다. 기본값 1을 반환합니다.");
        return 1;
    }

    private void OnShootHandler(Vector2 direction, int prefabIndex, GameObject projectile)
    {
        attackCounter++; // 공격 카운터 증가

        if (attackCounter >= 9) // 3번째 공격 시
        {
            CreateHomingProjectile(direction);
            attackCounter = 0; // 카운터 초기화
        }
    }

    private void CreateHomingProjectile(Vector2 direction)
    {
        if (homingProjectilePrefab == null)
        {
            Debug.LogError("HomingAttack: homingProjectilePrefab이 설정되어 있지 않습니다.");
            return;
        }

        // 플레이어의 위치에서 HomingProjectile 생성
        GameObject homingProjectile = Instantiate(homingProjectilePrefab, playerInstance.transform.position, Quaternion.identity);

        HomingProjectile projScript = homingProjectile.GetComponent<HomingProjectile>();
        if (projScript != null)
        {
            // 현재 레벨에 맞는 파라미터 설정
            float currentDelay = GetCurrentHomingStartDelay();
            float currentSpeed = GetCurrentHomingSpeed();
            float currentRange = GetCurrentHomingRange();

            projScript.Initialize(playerInstance.stat, currentDelay, currentSpeed, currentRange);
            projScript.SetDirection(direction);

            // 호밍 탄환 생성 시 사운드 재생
            if (homingProjectileSound != null)
            {
                homingProjectileSound.Post(homingProjectile.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("HomingAttack: homingProjectilePrefab에 HomingProjectile 스크립트가 없습니다.");
        }
    }

    private float GetCurrentHomingStartDelay()
    {
        if (currentLevel < homingStartDelayLevels.Length)
        {
            return homingStartDelayLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"HomingAttack: currentLevel ({currentLevel})이 homingStartDelayLevels 배열의 범위를 벗어났습니다. 기본값 {homingStartDelayLevels[homingStartDelayLevels.Length - 1]}을 반환합니다.");
            return homingStartDelayLevels[homingStartDelayLevels.Length - 1];
        }
    }

    private float GetCurrentHomingSpeed()
    {
        if (currentLevel < homingSpeedLevels.Length)
        {
            return homingSpeedLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"HomingAttack: currentLevel ({currentLevel})이 homingSpeedLevels 배열의 범위를 벗어났습니다. 기본값 {homingSpeedLevels[homingSpeedLevels.Length - 1]}을 반환합니다.");
            return homingSpeedLevels[homingSpeedLevels.Length - 1];
        }
    }

    private float GetCurrentHomingRange()
    {
        if (currentLevel < homingRangeLevels.Length)
        {
            return homingRangeLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"HomingAttack: currentLevel ({currentLevel})이 homingRangeLevels 배열의 범위를 벗어났습니다. 기본값 {homingRangeLevels[homingRangeLevels.Length - 1]}을 반환합니다.");
            return homingRangeLevels[homingRangeLevels.Length - 1];
        }
    }
}
