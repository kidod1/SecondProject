using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/FlameBarrier")]
public class FlameBarrier : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("������ ������ (���� 1���� ����)")]
    public float[] damagePerTickLevels = { 10f, 15f, 20f }; // ��: ���� 1~

    [Tooltip("������ �帷 �ݰ� (���� 1���� ����)")]
    public float[] barrierRadiusLevels = { 5f, 5.5f, 6f }; // ��: ���� 1~3

    [Tooltip("������ ���� (��)")]
    public float damageInterval = 1f;

    [Tooltip("ȭ�� �帷 ������")]
    public GameObject flameBarrierPrefab;

    private Player playerInstance;
    private GameObject activeFlameBarrier;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("FlameBarrier Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;
        CreateFlameBarrier();
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ �������� �帷 �ݰ��� ������ŵ�ϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;

            // ���� �� �� �������� �帷 �ݰ� ������Ʈ
            UpdateFlameBarrierParameters();
        }
        else
        {
            Debug.LogWarning("FlameBarrier: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
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
    /// ȭ�� �帷�� �����մϴ�.
    /// </summary>
    private void CreateFlameBarrier()
    {
        if (flameBarrierPrefab == null)
        {
            Debug.LogError("FlameBarrier: ȭ�� �帷 �������� �������� �ʾҽ��ϴ�.");
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
            // ���� ������ �������� �ݰ��� ����
            float currentDamage = GetCurrentDamagePerTick();
            float currentRadius = GetCurrentBarrierRadius();
            effect.Initialize(currentDamage, currentRadius, damageInterval, playerInstance);
        }
        else
        {
            Debug.LogError("FlameBarrier: FlameBarrierEffect ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    /// <summary>
    /// ���� ������ �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ������</returns>
    public float GetCurrentDamagePerTick()
    {
        if (currentLevel < damagePerTickLevels.Length)
        {
            return damagePerTickLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"FlameBarrier: currentLevel ({currentLevel})�� damagePerTickLevels �迭�� ������ ������ϴ�. �⺻�� {damagePerTickLevels[damagePerTickLevels.Length - 1]}�� ��ȯ�մϴ�.");
            return damagePerTickLevels[damagePerTickLevels.Length - 1];
        }
    }

    /// <summary>
    /// ���� ������ �帷 �ݰ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ �帷 �ݰ�</returns>
    public float GetCurrentBarrierRadius()
    {
        if (currentLevel < barrierRadiusLevels.Length)
        {
            return barrierRadiusLevels[currentLevel];
        }
        else
        {
            Debug.LogWarning($"FlameBarrier: currentLevel ({currentLevel})�� barrierRadiusLevels �迭�� ������ ������ϴ�. �⺻�� {barrierRadiusLevels[barrierRadiusLevels.Length - 1]}�� ��ȯ�մϴ�.");
            return barrierRadiusLevels[barrierRadiusLevels.Length - 1];
        }
    }

    /// <summary>
    /// ȭ�� �帷�� �Ķ���͸� ���� ������ �°� ������Ʈ�մϴ�.
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
                Debug.LogError("FlameBarrier: FlameBarrierEffect ��ũ��Ʈ�� ã�� �� �����ϴ�.");
            }
        }
        else
        {
            // ȭ�� �帷�� ���� �������� �ʾҴٸ�, �����մϴ�.
            CreateFlameBarrier();
        }
    }

    /// <summary>
    /// ���� ���� ������ �ʿ��� ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���� ���� �� �ʿ��� ��</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < damagePerTickLevels.Length)
        {
            return Mathf.RoundToInt(damagePerTickLevels[currentLevel] + 5f);
        }
        Debug.LogWarning($"FlameBarrier: currentLevel ({currentLevel})�� damagePerTickLevels �迭�� ������ ������ϴ�. �⺻�� 1�� ��ȯ�մϴ�.");
        return 1;
    }
}
