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
            return;

        playerInstance = player;
        CreateFlameBarrier();
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ �������� �帷 �ݰ��� ������ŵ�ϴ�.
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
        if (flameBarrierPrefab == null || playerInstance == null)
            return;

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
        return damagePerTickLevels[damagePerTickLevels.Length - 1];
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
        return barrierRadiusLevels[barrierRadiusLevels.Length - 1];
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
                CreateFlameBarrier();
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
        return 1;
    }

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";

        description += $"Lv  {currentLevel + 1}:\n";
        description += $"������: {GetCurrentDamagePerTick()} per {damageInterval}��\n";
        description += $"�帷 �ݰ�: {GetCurrentBarrierRadius()}m\n";

        return description;
    }
}