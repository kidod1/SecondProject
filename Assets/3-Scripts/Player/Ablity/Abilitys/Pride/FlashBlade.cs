using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise; // WWISE 네임스페이스 추가

[CreateAssetMenu(menuName = "Abilities/FlashBlade")]
public class FlashBlade : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("히트 임계값. 이 값에 도달하면 칼날을 발사합니다.")]
    public int hitThreshold = 5;

    [Tooltip("레벨별 칼날의 피해량")]
    public int[] damageLevels = { 50, 60, 70, 80, 90 }; // 레벨 1~5

    [Tooltip("칼날의 사거리")]
    public float range = 10f;

    [Tooltip("발사할 칼날 프리팹")]
    public GameObject bladePrefab;

    [Tooltip("칼날의 이동 속도")]
    public float bladeSpeed = 15f;

    [Header("WWISE Sound Events")]
    [Tooltip("FlashBlade 칼날 발사 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event fireSound;

    [Tooltip("FlashBlade 업그레이드 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event upgradeSound;

    [Tooltip("FlashBlade 제거 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event deactivateSound;

    private Player playerInstance;
    private int hitCount = 0;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        if (player == null)
            return;

        playerInstance = player;
    }

    /// <summary>
    /// FlashBlade 능력을 업그레이드합니다. 레벨이 증가할 때마다 피해량이 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
        }
        else
        {
            Debug.LogWarning("FlashBlade: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력 레벨을 초기화하고 히트 카운트를 리셋합니다.
    /// </summary>
    public override void ResetLevel()
    {
        hitCount = 0;
        currentLevel = 0;
    }

    /// <summary>
    /// 플레이어가 적을 적중시켰을 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="enemy">맞은 적의 Collider2D</param>
    public void OnProjectileHit(Collider2D enemy)
    {
        if (enemy == null)
            return;

        hitCount++;

        if (hitCount >= hitThreshold)
        {
            hitCount = 0;
            FireBlade();
        }
    }

    /// <summary>
    /// 칼날을 발사합니다.
    /// </summary>
    private void FireBlade()
    {
        if (bladePrefab == null || playerInstance == null)
            return;

        Vector3 spawnPosition = playerInstance.transform.position;

        // 플레이어의 후방 방향을 계산합니다.
        Vector2 facingDirection = playerInstance.GetFacingDirection();
        Vector2 backwardDirection = -facingDirection;

        if (backwardDirection == Vector2.zero)
        {
            Debug.LogWarning("FlashBlade: 플레이어의 방향이 설정되지 않았습니다.");
            return;
        }

        // 칼날의 회전을 후방 방향에 맞게 설정합니다.
        float angle = Mathf.Atan2(backwardDirection.y, backwardDirection.x) * Mathf.Rad2Deg;

        // 프리팹의 기본 방향에 따라 각도 보정 (필요 시)
        float angleOffset = -90f; // 필요에 따라 조정 (예: 90f)
        angle += angleOffset;

        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, angle);

        GameObject blade = Instantiate(bladePrefab, spawnPosition, spawnRotation);
        BladeProjectile bladeScript = blade.GetComponent<BladeProjectile>();

        if (bladeScript != null)
        {
            int currentDamage = GetCurrentDamage();
            bladeScript.Initialize(currentDamage, range, bladeSpeed, playerInstance, backwardDirection);
        }
        else
        {
            Debug.LogError("FlashBlade: bladePrefab에 BladeProjectile 컴포넌트가 없습니다.");
        }

        // Debugging: 방향 확인을 위한 로그 추가
        Debug.Log($"Firing blade at angle: {angle}, Direction: {backwardDirection}, Spawn Position: {spawnPosition}");

        // FlashBlade 칼날 발사 시 WWISE 사운드 재생
        if (fireSound != null)
        {
            fireSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// 능력의 설명을 반환합니다. 설명은 현재 레벨보다 1레벨 더 높은 레벨의 정보를 포함합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            // 다음 레벨의 인덱스은 currentLevel (currentLevel은 0부터 시작)
            int nextLevelIndex = currentLevel;
            int nextLevelDamage = (nextLevelIndex < damageLevels.Length) ? damageLevels[nextLevelIndex] : damageLevels[damageLevels.Length - 1];

            return $"{baseDescription}\n" +
                   $"Lv {currentLevel + 1}:\n" +
                   $"- 히트 임계값: {hitThreshold}회\n" +
                   $"- 피해량: {nextLevelDamage}\n" +
                   $"- 사거리: {range}m\n";
        }
        else
        {
            // 최대 레벨 설명
            int maxLevelIndex = currentLevel - 1;
            int finalDamage = (maxLevelIndex < damageLevels.Length) ? damageLevels[maxLevelIndex] : damageLevels[damageLevels.Length - 1];

            return $"{baseDescription}\n" +
                   $"Max Level: {currentLevel}\n" +
                   $"- 히트 임계값: {hitThreshold}회\n" +
                   $"- 피해량: {finalDamage}\n" +
                   $"- 사거리: {range}m\n";
        }
    }

    /// <summary>
    /// 현재 레벨에 맞는 피해량을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 피해량</returns>
    private int GetCurrentDamage()
    {
        if (currentLevel == 0)
        {
            return damageLevels[0];
        }
        else if (currentLevel - 1 < damageLevels.Length)
        {
            return damageLevels[currentLevel - 1];
        }
        Debug.LogWarning($"FlashBlade: currentLevel ({currentLevel})이 damageLevels 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return damageLevels[damageLevels.Length - 1];
    }

    /// <summary>
    /// 다음 레벨 증가에 필요한 값을 반환합니다.
    /// (이 메서드는 더 이상 사용되지 않으므로 제거할 수 있습니다.)
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        // 더 이상 사용되지 않으므로 0을 반환하거나 메서드를 제거할 수 있습니다.
        return 0;
    }

    /// <summary>
    /// OnValidate 메서드를 통해 배열의 길이를 maxLevel과 일치시킵니다.
    /// </summary>
    private void OnValidate()
    {
        if (damageLevels.Length != maxLevel)
        {
            Debug.LogWarning($"FlashBlade: damageLevels 배열의 길이가 maxLevel ({maxLevel})과 일치하지 않습니다. 배열 길이를 맞춥니다.");
            Array.Resize(ref damageLevels, maxLevel);
        }
    }

    /// <summary>
    /// Gizmos를 사용하여 FlashBlade 발사 방향 시각화
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (playerInstance != null)
        {
            Vector2 facingDirection = playerInstance.GetFacingDirection();
            Vector2 backwardDirection = -facingDirection;

            Vector3 origin = playerInstance.transform.position;
            Vector3 direction = backwardDirection * 5f; // 예시: 5 단위 길이

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + (Vector3)direction);
            Gizmos.DrawSphere(origin + (Vector3)direction, 0.2f);
        }
    }
}
