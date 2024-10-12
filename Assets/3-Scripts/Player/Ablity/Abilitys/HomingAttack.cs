using UnityEngine;

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

    private Player playerInstance;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
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

        // 이벤트 리스너 추가 전에 중복 제거
        playerInstance.OnShoot.RemoveListener(OnShootHandler);
        playerInstance.OnShoot.AddListener(OnShootHandler);
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 Homing 파라미터가 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel이 5일 경우, currentLevel은 0~4
        {
            currentLevel++;

            // 레벨 업 시 필요한 로직 추가 (필요 시)
            // 현재 이 능력은 레벨별 파라미터 배열을 통해 자동으로 조정되므로 별도의 조정은 필요 없습니다.
        }
        else
        {
            Debug.LogWarning("HomingAttack: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

        // 이벤트 리스너 제거
        if (playerInstance != null)
        {
            playerInstance.OnShoot.RemoveListener(OnShootHandler);
            playerInstance = null;
        }

        // 프로젝트 타입을 기본값으로 되돌림 (필요 시)
        // 이 부분은 플레이어 인스턴스가 null이기 때문에 실행되지 않습니다.
        // 필요하다면, 플레이어 인스턴스를 먼저 저장해두고 처리해야 합니다.

        Debug.Log("HomingAttack 레벨이 초기화되었습니다.");
    }

    /// <summary>
    /// 능력의 설명을 반환합니다.
    /// </summary>
    /// <returns>능력의 설명 문자열</returns>
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

    /// <summary>
    /// 다음 레벨 증가에 필요한 값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨 증가 시 필요한 값</returns>
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

    /// <summary>
    /// 능력이 적용된 후 플레이어가 발사할 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="direction">발사 방향</param>
    /// <param name="prefabIndex">프리팹 인덱스</param>
    /// <param name="projectile">생성된 프로젝트트</param>
    private void OnShootHandler(Vector2 direction, int prefabIndex, GameObject projectile)
    {
        if (projectile == null)
        {
            Debug.LogError("HomingAttack: 전달된 프로젝트트가 null입니다.");
            return;
        }

        HomingProjectile projScript = projectile.GetComponent<HomingProjectile>();
        if (projScript != null)
        {
            // 현재 레벨에 맞는 파라미터 설정
            float currentDelay = GetCurrentHomingStartDelay();
            float currentSpeed = GetCurrentHomingSpeed();
            float currentRange = GetCurrentHomingRange();

            projScript.Initialize(playerInstance.stat, currentDelay, currentSpeed, currentRange);
            projScript.SetDirection(direction);
        }
        else
        {
            Debug.LogWarning("HomingAttack: 전달된 프로젝트트에 HomingProjectile 스크립트가 없습니다.");
        }
    }


    /// <summary>
    /// 현재 레벨의 Homing 시작 지연 시간을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 Homing 시작 지연 시간</returns>
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

    /// <summary>
    /// 현재 레벨의 Homing 속도를 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 Homing 속도</returns>
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

    /// <summary>
    /// 현재 레벨의 Homing 범위를 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 Homing 범위</returns>
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
