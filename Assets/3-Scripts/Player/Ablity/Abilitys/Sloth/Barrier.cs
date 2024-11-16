using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise; // WWISE ���ӽ����̽� �߰�

[CreateAssetMenu(menuName = "Abilities/Barrier")]
public class Barrier : Ability
{
    [Header("Barrier Prefab")]
    [Tooltip("������ ������")]
    public GameObject barrierPrefab;

    [Header("Shield Settings")]
    [Tooltip("���а� Ȱ��ȭ�� ��ġ�� ������")]
    public Vector3 spawnOffset = Vector3.zero;

    [Header("Cooldown Settings")]
    [Tooltip("�� �������� ������ ��Ÿ�� �ð� (��)")]
    public int[] cooldownTimes = { 30, 25, 20, 15, 10 }; // ���� 1~5

    [Header("WWISE Sound Events")]
    [Tooltip("Barrier ���� Ȱ��ȭ �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event activateSound;

    [Tooltip("Barrier ���� ���׷��̵� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event upgradeSound;

    [Tooltip("Barrier ���� ��Ȱ��ȭ �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event deactivateSound;

    // Barrier Ȱ��ȭ �ν��Ͻ� ����
    private GameObject activeBarrierInstance;

    /// <summary>
    /// Barrier �ɷ��� �÷��̾�� �����մϴ�. ���и� Ȱ��ȭ�մϴ�.
    /// </summary>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("Barrier Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        if (activeBarrierInstance != null)
        {
            Debug.LogWarning("Barrier Apply: �̹� Ȱ��ȭ�� ���а� �����մϴ�.");
            return;
        }

        // ���� �������� �ν��Ͻ�ȭ
        activeBarrierInstance = Instantiate(barrierPrefab, player.transform.position + spawnOffset, Quaternion.identity, player.transform);

        // ���� ������Ʈ�� "Barrier" �±� �Ҵ�
        activeBarrierInstance.tag = "Barrier";

        // ���� ������Ʈ�� Shield ������Ʈ�� �ִ��� Ȯ��
        Shield shieldComponent = activeBarrierInstance.GetComponent<Shield>();
        if (shieldComponent == null)
        {
            Debug.LogError("Barrier Apply: barrierPrefab�� Shield ������Ʈ�� �����ϴ�.");
            return;
        }

        // WWISE ���� ���
        if (activateSound != null)
        {
            activateSound.Post(activeBarrierInstance);
        }

        Debug.Log("Barrier�� Ȱ��ȭ�Ǿ����ϴ�.");
    }

    /// <summary>
    /// Barrier �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ��Ÿ���� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            // ���� ����
            currentLevel++;
            Debug.Log($"Barrier ���׷��̵�: ���� ���� {currentLevel}");

            // WWISE ���׷��̵� ���� ���
            if (upgradeSound != null && activeBarrierInstance != null)
            {
                upgradeSound.Post(activeBarrierInstance);
            }

            // �߰����� ���׷��̵� ���� (��: ��Ÿ�� ����) �ʿ� �� ����
        }
        else
        {
            Debug.LogWarning("Barrier: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// Barrier �ɷ��� ������ �ʱ�ȭ�ϰ� ���и� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

        if (activeBarrierInstance != null)
        {
            // Barrier ��Ȱ��ȭ
            Shield shield = activeBarrierInstance.GetComponent<Shield>();
            if (shield != null)
            {
                shield.BreakShield();
            }
            else
            {
                Debug.LogWarning("Barrier ������Ʈ�� Shield ������Ʈ�� �����ϴ�.");
                Destroy(activeBarrierInstance);
            }

            activeBarrierInstance = null;
        }

        currentLevel = 0;
    }

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�.
    /// </summary>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            int nextLevelIndex = currentLevel;
            int nextLevelCooldown = (nextLevelIndex < cooldownTimes.Length) ? cooldownTimes[nextLevelIndex] : cooldownTimes[cooldownTimes.Length - 1];

            return $"{baseDescription}\n" +
                   $"Lv {currentLevel + 1}:\n" +
                   $"- ��Ÿ��: {nextLevelCooldown}��\n";
        }
        else
        {
            int maxLevelIndex = currentLevel - 1;
            int finalCooldown = (maxLevelIndex < cooldownTimes.Length) ? cooldownTimes[maxLevelIndex] : cooldownTimes[cooldownTimes.Length - 1];

            return $"{baseDescription}\n" +
                   $"Max Level: {currentLevel}\n" +
                   $"- ��Ÿ��: {finalCooldown}��\n";
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < cooldownTimes.Length)
        {
            return cooldownTimes[currentLevel];
        }
        Debug.LogWarning($"Barrier: currentLevel ({currentLevel})�� cooldownTimes �迭�� ������ ������ϴ�. �⺻�� 0�� ��ȯ�մϴ�.");
        return 0;
    }
}
