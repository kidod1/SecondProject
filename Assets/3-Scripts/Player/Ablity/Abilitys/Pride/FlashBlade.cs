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

    private Player playerInstance;
    private int hitCount = 0;

    /// <summary>
    /// 현재 레벨에 해당하는 피해량을 반환합니다.
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < damageLevels.Length)
        {
            return Mathf.RoundToInt(damageLevels[currentLevel]);
        }
        return 0;
    }

    /// <summary>
    /// FlashBlade 능력을 플레이어에게 적용합니다.
    /// </summary>
    public override void Apply(Player player)
    {
        if (player == null)
            return;

        playerInstance = player;
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

        // Debugging: 방향 확인을 위한 로그 추가
        Debug.Log($"Firing blade at angle: {angle}, Direction: {backwardDirection}, Spawn Position: {spawnPosition}");

        // FlashBlade 칼날 발사 시 WWISE 사운드 재생
        if (fireSound != null)
        {
            fireSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// FlashBlade 능력을 업그레이드합니다. 레벨이 증가할 때마다 피해량이 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
            Debug.Log($"FlashBlade 업그레이드: 현재 레벨 {currentLevel + 1}");
        }
        else
        {
            Debug.LogWarning("FlashBlade: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 현재 레벨에 맞는 피해량을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 피해량</returns>
    private int GetCurrentDamage()
    {
        if (currentLevel < damageLevels.Length)
        {
            return damageLevels[currentLevel];
        }
        return damageLevels[damageLevels.Length - 1];
    }

    /// <summary>
    /// 능력 설명을 오버라이드하여 레벨 업 시 피해량 증가를 포함시킵니다.
    /// </summary>
    /// <returns>능력의 설명 문자열</returns>
    public override string GetDescription()
    {
        if (currentLevel < damageLevels.Length && currentLevel >= 0)
        {
            int damageValue = damageLevels[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 적을 {hitThreshold}회 맞출 때마다 후방으로 칼날 발사. 피해량: {damageValue}";
        }
        else if (currentLevel >= damageLevels.Length)
        {
            int maxDamageValue = damageLevels[damageLevels.Length - 1];
            return $"{baseDescription}\n최대 레벨 도달: 적을 {hitThreshold}회 맞출 때마다 후방으로 칼날 발사. 피해량: {maxDamageValue}";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달.";
        }
    }

    /// <summary>
    /// 능력의 레벨을 초기화하고 히트 카운트를 리셋합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        hitCount = 0;
        currentLevel = 0;
    }

    /// <summary>
    /// Editor 상에서 유효성 검사
    /// </summary>
    private void OnValidate()
    {
        if (damageLevels.Length != maxLevel)
        {
            Array.Resize(ref damageLevels, maxLevel);
        }
    }

    /// <summary>
    /// Gizmos를 사용하여 칼날 발사 방향 시각화
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
