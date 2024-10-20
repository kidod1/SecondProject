using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/FlameBarrier")]
public class FlameBarrier : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("레벨별 데미지 (레벨 1부터 시작)")]
    public float[] damagePerTickLevels = { 10f, 15f, 20f }; // 예: 레벨 1~

    [Tooltip("레벨별 장막 반경 (레벨 1부터 시작)")]
    public float[] barrierRadiusLevels = { 5f, 5.5f, 6f }; // 예: 레벨 1~3

    [Tooltip("데미지 간격 (초)")]
    public float damageInterval = 1f;

    [Tooltip("화염 장막 프리팹")]
    public GameObject flameBarrierPrefab;

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
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 데미지와 장막 반경을 증가시킵니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
            UpdateFlameBarrierParameters();
        }
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

        if (activeFlameBarrier != null)
        {
            Object.Destroy(activeFlameBarrier);
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
            Object.Destroy(activeFlameBarrier);
        }

        // 플레이어의 자식으로 설정
        activeFlameBarrier = Object.Instantiate(flameBarrierPrefab, playerInstance.transform);

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
    }

    /// <summary>
    /// 현재 레벨의 데미지를 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 데미지</returns>
    public float GetCurrentDamagePerTick()
    {
        if (currentLevel < damagePerTickLevels.Length)
        {
            return damagePerTickLevels[currentLevel];
        }
        return damagePerTickLevels[damagePerTickLevels.Length - 1];
    }

    /// <summary>
    /// 현재 레벨의 장막 반경을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 장막 반경</returns>
    public float GetCurrentBarrierRadius()
    {
        if (currentLevel < barrierRadiusLevels.Length)
        {
            return barrierRadiusLevels[currentLevel];
        }
        return barrierRadiusLevels[barrierRadiusLevels.Length - 1];
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
    /// 다음 레벨 증가에 필요한 값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨 증가 시 필요한 값</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < damagePerTickLevels.Length)
        {
            return Mathf.RoundToInt(damagePerTickLevels[currentLevel] + 5f);
        }
        return 1;
    }

    /// <summary>
    /// 능력의 설명을 반환합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";

        description += $"현재 레벨: {currentLevel + 1}\n";
        description += $"데미지: {GetCurrentDamagePerTick()} per {damageInterval}초\n";
        description += $"장막 반경: {GetCurrentBarrierRadius()}m\n";

        return description;
    }
}
