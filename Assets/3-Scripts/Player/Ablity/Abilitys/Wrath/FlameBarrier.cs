using UnityEngine;
using System.Collections;
using System;
using AK.Wwise; // WWISE 네임스페이스 추가

[CreateAssetMenu(menuName = "Abilities/FlameBarrier")]
public class FlameBarrier : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("레벨별 데미지 (레벨 1부터 시작)")]
    public float[] damagePerTickLevels = { 10f, 15f, 20f }; // 레벨 1~3

    [Tooltip("레벨별 장막 반경 (레벨 1부터 시작)")]
    public float[] barrierRadiusLevels = { 5f, 5.5f, 6f }; // 레벨 1~3

    [Tooltip("데미지 간격 (초)")]
    public float damageInterval = 1f;

    [Tooltip("화염 장막 프리팹")]
    public GameObject flameBarrierPrefab;

    [Header("WWISE Sound Events")]
    [Tooltip("FlameBarrier 능력 발동 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event activateSound;

    [Tooltip("FlameBarrier 업그레이드 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event upgradeSound;

    [Tooltip("FlameBarrier 제거 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event deactivateSound;

    private Player playerInstance;
    private GameObject activeFlameBarrier;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        if (player == null)
            return;

        playerInstance = player;
        CreateFlameBarrier();

        // FlameBarrier 능력 발동 시 WWISE 사운드 재생
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 데미지와 장막 반경을 증가시킵니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            Debug.Log($"FlameBarrier 업그레이드: 현재 레벨 {currentLevel}");

            // 현재 레벨에 맞는 데미지와 반경을 적용합니다.
            UpdateFlameBarrierParameters();

            // 업그레이드 시 WWISE 사운드 재생
            if (upgradeSound != null)
            {
                upgradeSound.Post(playerInstance.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("FlameBarrier: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        if (activeFlameBarrier != null)
        {
            UnityEngine.Object.Destroy(activeFlameBarrier);

            // FlameBarrier 제거 시 WWISE 사운드 재생
            if (deactivateSound != null && playerInstance != null)
            {
                deactivateSound.Post(playerInstance.gameObject);
            }

            activeFlameBarrier = null;
        }
        currentLevel = 0;
    }

    /// <summary>
    /// 화염 장막을 생성합니다.
    /// </summary>
    private void CreateFlameBarrier()
    {
        if (flameBarrierPrefab == null || playerInstance == null)
            return;

        if (activeFlameBarrier != null)
        {
            UnityEngine.Object.Destroy(activeFlameBarrier);
        }

        // 플레이어의 자식으로 설정
        activeFlameBarrier = UnityEngine.Object.Instantiate(flameBarrierPrefab, playerInstance.transform);

        // 자식 오브젝트 위치를 로컬 좌표계에서 (0,0)으로 고정
        activeFlameBarrier.transform.localPosition = Vector3.zero;

        FlameBarrierEffect effect = activeFlameBarrier.GetComponent<FlameBarrierEffect>();

        if (effect != null)
        {
            // 현재 레벨의 데미지와 반경을 전달
            float currentDamage = GetCurrentDamagePerTick();
            float currentRadius = GetCurrentBarrierRadius();
            effect.Initialize(currentDamage, currentRadius, damageInterval, playerInstance);
        }
        else
        {
            Debug.LogError("FlameBarrierEffect 스크립트를 찾을 수 없습니다.");
        }

        // FlameBarrier 능력 발동 시 WWISE 사운드 재생
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// 현재 레벨의 데미지를 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 데미지</returns>
    public float GetCurrentDamagePerTick()
    {
        Debug.Log(currentLevel);
        // currentLevel은 0부터 시작하므로, 배열 인덱스는 currentLevel - 1
        if (currentLevel == 0)
        {
            return damagePerTickLevels[0];
        }
        else
        {
            return damagePerTickLevels[currentLevel - 1];
        }
    }

    /// <summary>
    /// 현재 레벨의 장막 반경을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 장막 반경</returns>
    public float GetCurrentBarrierRadius()
    {
        if (currentLevel == 0)
        {
            return barrierRadiusLevels[0];
        }
        else if (currentLevel - 1 < barrierRadiusLevels.Length)
        {
            return barrierRadiusLevels[currentLevel - 1];
        }
        else
        {
            // 배열 범위를 초과할 경우 마지막 값을 반환
            return barrierRadiusLevels[barrierRadiusLevels.Length - 1];
        }
    }

    /// <summary>
    /// 화염 장막의 파라미터를 현재 레벨에 맞게 업데이트합니다.
    /// </summary>
    private void UpdateFlameBarrierParameters()
    {
        if (activeFlameBarrier != null)
        {
            FlameBarrierEffect effect = activeFlameBarrier.GetComponent<FlameBarrierEffect>();
            if (effect != null)
            {
                float newDamagePerTick = GetCurrentDamagePerTick();
                float newBarrierRadius = GetCurrentBarrierRadius();
                effect.UpdateParameters(newDamagePerTick, newBarrierRadius, damageInterval);
            }
            else
            {
                // FlameBarrierEffect 스크립트가 없다면 다시 생성
                CreateFlameBarrier();
            }
        }
        else
        {
            // 화염 장막이 아직 생성되지 않았다면, 생성합니다.
            CreateFlameBarrier();
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
            // 다음 레벨의 인덱스는 currentLevel
            int nextLevelIndex = currentLevel;
            float nextLevelDamage = (nextLevelIndex < damagePerTickLevels.Length) ? damagePerTickLevels[nextLevelIndex] : damagePerTickLevels[damagePerTickLevels.Length - 1];
            float nextLevelRadius = (nextLevelIndex < barrierRadiusLevels.Length) ? barrierRadiusLevels[nextLevelIndex] : barrierRadiusLevels[barrierRadiusLevels.Length - 1];

            return $"{baseDescription}\n" +
                   $"Lv {currentLevel + 1}:\n" +
                   $"데미지: {nextLevelDamage} per {damageInterval}초\n" +
                   $"장막 반경: {nextLevelRadius}m\n";
        }
        else
        {
            // 최대 레벨 설명
            int maxLevelIndex = currentLevel - 1;
            float finalDamage = (maxLevelIndex < damagePerTickLevels.Length) ? damagePerTickLevels[maxLevelIndex] : damagePerTickLevels[damagePerTickLevels.Length - 1];
            float finalRadius = (maxLevelIndex < barrierRadiusLevels.Length) ? barrierRadiusLevels[maxLevelIndex] : barrierRadiusLevels[barrierRadiusLevels.Length - 1];

            return $"{baseDescription}\n" +
                   $"Max Level: {currentLevel}\n" +
                   $"데미지: {finalDamage} per {damageInterval}초\n" +
                   $"장막 반경: {finalRadius}m\n";
        }
    }

    /// <summary>
    /// 다음 레벨 증가에 필요한 값을 반환합니다.
    /// (이 메서드는 더 이상 사용되지 않으므로 제거할 수 있습니다.)
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        // 필요에 따라 구현하거나 제거
        return 0;
    }

    /// <summary>
    /// Gizmos를 사용하여 FlameBarrier 발사 방향 시각화
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
