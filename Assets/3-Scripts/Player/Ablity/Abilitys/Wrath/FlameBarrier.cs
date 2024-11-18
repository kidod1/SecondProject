using UnityEngine;
using System.Collections;
using System;
using AK.Wwise; // WWISE ���ӽ����̽� �߰�

[CreateAssetMenu(menuName = "Abilities/FlameBarrier")]
public class FlameBarrier : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("������ ������ (���� 1���� ����)")]
    public float[] damagePerTickLevels = { 10f, 15f, 20f }; // ���� 1~3

    [Tooltip("������ �帷 �ݰ� (���� 1���� ����)")]
    public float[] barrierRadiusLevels = { 5f, 5.5f, 6f }; // ���� 1~3

    [Tooltip("������ ���� (��)")]
    public float damageInterval = 1f;

    [Tooltip("ȭ�� �帷 ������")]
    public GameObject flameBarrierPrefab;

    [Header("WWISE Sound Events")]
    [Tooltip("FlameBarrier �ɷ� �ߵ� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event activateSound;

    [Tooltip("FlameBarrier ���׷��̵� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event upgradeSound;

    [Tooltip("FlameBarrier ���� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event deactivateSound;

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

        // FlameBarrier �ɷ� �ߵ� �� WWISE ���� ���
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ �������� �帷 �ݰ��� ������ŵ�ϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            Debug.Log($"FlameBarrier ���׷��̵�: ���� ���� {currentLevel}");

            // ���� ������ �´� �������� �ݰ��� �����մϴ�.
            UpdateFlameBarrierParameters();

            // ���׷��̵� �� WWISE ���� ���
            if (upgradeSound != null)
            {
                upgradeSound.Post(playerInstance.gameObject);
            }
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
        if (activeFlameBarrier != null)
        {
            UnityEngine.Object.Destroy(activeFlameBarrier);

            // FlameBarrier ���� �� WWISE ���� ���
            if (deactivateSound != null && playerInstance != null)
            {
                deactivateSound.Post(playerInstance.gameObject);
            }

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
            UnityEngine.Object.Destroy(activeFlameBarrier);
        }

        // �÷��̾��� �ڽ����� ����
        activeFlameBarrier = UnityEngine.Object.Instantiate(flameBarrierPrefab, playerInstance.transform);

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
            Debug.LogError("FlameBarrierEffect ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }

        // FlameBarrier �ɷ� �ߵ� �� WWISE ���� ���
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// ���� ������ �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ������</returns>
    public float GetCurrentDamagePerTick()
    {
        Debug.Log(currentLevel);
        // currentLevel�� 0���� �����ϹǷ�, �迭 �ε����� currentLevel - 1
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
    /// ���� ������ �帷 �ݰ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ �帷 �ݰ�</returns>
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
            // �迭 ������ �ʰ��� ��� ������ ���� ��ȯ
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
                // FlameBarrierEffect ��ũ��Ʈ�� ���ٸ� �ٽ� ����
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
    /// �ɷ��� ������ ��ȯ�մϴ�. ������ ���� �������� 1���� �� ���� ������ ������ �����մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            // ���� ������ �ε����� currentLevel
            int nextLevelIndex = currentLevel;
            float nextLevelDamage = (nextLevelIndex < damagePerTickLevels.Length) ? damagePerTickLevels[nextLevelIndex] : damagePerTickLevels[damagePerTickLevels.Length - 1];
            float nextLevelRadius = (nextLevelIndex < barrierRadiusLevels.Length) ? barrierRadiusLevels[nextLevelIndex] : barrierRadiusLevels[barrierRadiusLevels.Length - 1];

            return $"{baseDescription}\n" +
                   $"Lv {currentLevel + 1}:\n" +
                   $"������: {nextLevelDamage} per {damageInterval}��\n" +
                   $"�帷 �ݰ�: {nextLevelRadius}m\n";
        }
        else
        {
            // �ִ� ���� ����
            int maxLevelIndex = currentLevel - 1;
            float finalDamage = (maxLevelIndex < damagePerTickLevels.Length) ? damagePerTickLevels[maxLevelIndex] : damagePerTickLevels[damagePerTickLevels.Length - 1];
            float finalRadius = (maxLevelIndex < barrierRadiusLevels.Length) ? barrierRadiusLevels[maxLevelIndex] : barrierRadiusLevels[barrierRadiusLevels.Length - 1];

            return $"{baseDescription}\n" +
                   $"Max Level: {currentLevel}\n" +
                   $"������: {finalDamage} per {damageInterval}��\n" +
                   $"�帷 �ݰ�: {finalRadius}m\n";
        }
    }

    /// <summary>
    /// ���� ���� ������ �ʿ��� ���� ��ȯ�մϴ�.
    /// (�� �޼���� �� �̻� ������ �����Ƿ� ������ �� �ֽ��ϴ�.)
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        // �ʿ信 ���� �����ϰų� ����
        return 0;
    }

    /// <summary>
    /// Gizmos�� ����Ͽ� FlameBarrier �߻� ���� �ð�ȭ
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (playerInstance != null)
        {
            Vector2 facingDirection = playerInstance.GetFacingDirection();
            Vector2 backwardDirection = -facingDirection;

            Vector3 origin = playerInstance.transform.position;
            Vector3 direction = backwardDirection * 5f; // ����: 5 ���� ����

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + (Vector3)direction);
            Gizmos.DrawSphere(origin + (Vector3)direction, 0.2f);
        }
    }
}
