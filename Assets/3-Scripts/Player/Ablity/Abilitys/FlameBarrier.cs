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
            Debug.LogError("ȭ�� �帷 �������� �������� �ʾҽ��ϴ�.");
            return;
        }

        if (activeFlameBarrier != null)
        {
            Object.Destroy(activeFlameBarrier);
        }

        // �÷��̾��� �ڽ����� ����
        activeFlameBarrier = Object.Instantiate(flameBarrierPrefab, playerInstance.transform);

        // �ڽ� ������Ʈ ��ġ�� ���� ��ǥ�迡�� (0,0)���� ����
        activeFlameBarrier.transform.localPosition = Vector3.zero;

        FlameBarrierEffect effect = activeFlameBarrier.GetComponent<FlameBarrierEffect>();

        if (effect != null)
        {
            effect.Initialize(damagePerTick, barrierRadius, damageInterval, playerInstance);
        }
        else
        {
            Debug.LogError("FlameBarrierEffect ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    protected override int GetNextLevelIncrease()
    {
        return Mathf.RoundToInt(damagePerTick + 5f);
    }
}
