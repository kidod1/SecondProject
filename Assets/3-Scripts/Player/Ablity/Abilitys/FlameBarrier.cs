using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/FlameBarrier")]
public class FlameBarrier : Ability
{
    [Header("Ability Parameters")]
    public float damagePerTick = 10f;
    public float barrierRadius = 5f;
    public float damageInterval = 1f;
    public GameObject flameBarrierPrefab;

    private Player playerInstance;
    private GameObject activeFlameBarrier;

    public override void Apply(Player player)
    {
        playerInstance = player;
        CreateFlameBarrier();
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            damagePerTick += 5f;
            barrierRadius += 0.5f;

            if (activeFlameBarrier != null)
            {
                FlameBarrierEffect effect = activeFlameBarrier.GetComponent<FlameBarrierEffect>();
                if (effect != null)
                {
                    effect.UpdateParameters(damagePerTick, barrierRadius, damageInterval);
                }
            }
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        if (activeFlameBarrier != null)
        {
            Object.Destroy(activeFlameBarrier);
            activeFlameBarrier = null;
        }
    }

    private void CreateFlameBarrier()
    {
        if (flameBarrierPrefab == null)
        {
            Debug.LogError("화염 장막 프리팹이 설정되지 않았습니다.");
            return;
        }

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
            effect.Initialize(damagePerTick, barrierRadius, damageInterval, playerInstance);
        }
        else
        {
            Debug.LogError("FlameBarrierEffect 스크립트를 찾을 수 없습니다.");
        }
    }

    protected override int GetNextLevelIncrease()
    {
        return Mathf.RoundToInt(damagePerTick + 5f);
    }
}
