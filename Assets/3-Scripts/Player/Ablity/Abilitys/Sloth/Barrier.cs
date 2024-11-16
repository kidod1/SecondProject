using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Barrier")]
public class Barrier : Ability
{
    [Header("Barrier Visuals")]
    [Tooltip("������ ������")]
    public GameObject shieldPrefab;

    [Tooltip("�ǵ� �극��ũ ����Ʈ ������")] // �߰��� ����
    public GameObject shieldBreakEffectPrefab;

    [Header("Cooldown Settings")]
    [Tooltip("�� �������� ������ ��Ÿ�� �ð� (��)")]
    public int[] cooldownTimes = { 30, 25, 20, 15, 10 }; // ���� 1~5

    private GameObject activeShield;
    private Player playerInstance;
    private Coroutine cooldownCoroutine;
    private bool isShieldActive;


    private void start()
    {
        playerInstance = PlayManager.I.GetPlayer();
    }
    /// <summary>
    /// Ability Ŭ������ GetNextLevelIncrease() �޼��带 �������̵��Ͽ� ���� ������ �ش��ϴ� ��Ÿ�� �ð��� ��ȯ�մϴ�.
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < cooldownTimes.Length)
        {
            return cooldownTimes[currentLevel];
        }
        Debug.LogWarning($"Barrier: currentLevel ({currentLevel})�� cooldownTimes �迭�� ������ ������ϴ�. �⺻�� 0�� ��ȯ�մϴ�.");
        return 0;
    }

    /// <summary>
    /// ���� �ɷ��� �÷��̾�� �����մϴ�. �ʱ� ���������� ���и� Ȱ��ȭ�մϴ�.
    /// </summary>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("Barrier Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;
        if (currentLevel == 1)
        {
            ActivateBarrierVisual();
        }
    }

    /// <summary>
    /// ���� �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ��Ÿ���� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            Debug.Log($"Barrier ���׷��̵�: ���� ���� {currentLevel}");

            // ���׷��̵� �� ��Ÿ�� ���� �� ����
            if (isShieldActive)
            {
                if (cooldownCoroutine != null)
                {
                    playerInstance.StopCoroutine(cooldownCoroutine);
                    cooldownCoroutine = null;
                }
                StartCooldown();
            }
        }
        else
        {
            Debug.LogWarning("Barrier: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// ������ �ð��� ȿ���� Ȱ��ȭ�ϰ� ��Ÿ���� �����մϴ�.
    /// </summary>
    public void ActivateBarrierVisual()
    {
        if (playerInstance == null)
        {
            Debug.LogWarning("Barrier Activate: playerInstance�� �������� �ʾҽ��ϴ�. Apply �޼��带 ���� ȣ���ϼ���.");
            return;
        }

        isShieldActive = true;

        if (shieldPrefab != null)
        {
            if (activeShield != null)
            {
                Destroy(activeShield);
            }

            activeShield = Instantiate(shieldPrefab, playerInstance.transform);
            activeShield.transform.SetParent(playerInstance.transform);
        }
        else
        {
            Debug.LogError("Barrier: shieldPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // ���� Ȱ��ȭ �� ��Ÿ�� ����
        StartCooldown();
    }

    /// <summary>
    /// ������ �ð��� ȿ���� ��Ȱ��ȭ�ϰ� ��Ÿ���� �����մϴ�.
    /// </summary>
    public void DeactivateBarrierVisual()
    {
        if (playerInstance == null)
        {
            Debug.LogWarning("Barrier Deactivate: playerInstance�� �������� �ʾҽ��ϴ�.");
            return;
        }

        isShieldActive = false;

        if (activeShield != null)
        {
            // �ǵ� �극��ũ ����Ʈ ����
            if (shieldBreakEffectPrefab != null)
            {
                Instantiate(shieldBreakEffectPrefab, activeShield.transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Barrier: shieldBreakEffectPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
            }

            // �ǵ� ������Ʈ �ı�
            Destroy(activeShield);
            activeShield = null;
        }

        // ��ٿ� ����
        StartCooldown();
    }

    /// <summary>
    /// ���а� ���� Ȱ��ȭ�Ǿ� �ִ��� Ȯ���մϴ�.
    /// </summary>
    public bool IsShieldActive()
    {
        return isShieldActive;
    }
    /// <summary>
    /// ��Ÿ���� �����մϴ�. �̹� ��Ÿ���� ���� ���� ��� �ߺ����� �������� �ʽ��ϴ�.
    /// </summary>
    public void StartCooldown()
    {
        if (playerInstance == null)
        {
            Debug.LogWarning("Barrier StartCooldown: playerInstance�� �������� �ʾҽ��ϴ�. Apply �޼��带 ���� ȣ���ϼ���.");
            return;
        }

        if (cooldownCoroutine == null)
        {
            float cooldownTime = GetCurrentCooldownTime();
            Debug.Log($"Barrier: ��Ÿ�� ���� ({cooldownTime}��)");

            cooldownCoroutine = playerInstance.StartCoroutine(BarrierCooldown(cooldownTime));
        }
    }

    /// <summary>
    /// ��Ÿ���� �Ϸ�Ǹ� ���и� �ٽ� Ȱ��ȭ�մϴ�.
    /// </summary>
    private IEnumerator BarrierCooldown(float cooldownTime)
    {
        yield return new WaitForSeconds(cooldownTime);

        if (playerInstance != null)
        {
            ActivateBarrierVisual();
        }

        cooldownCoroutine = null;
    }

    /// <summary>
    /// ���� ������ �´� ��Ÿ�� �ð��� ��ȯ�մϴ�.
    /// </summary>
    private float GetCurrentCooldownTime()
    {
        if (currentLevel < cooldownTimes.Length)
        {
            return cooldownTimes[currentLevel];
        }
        Debug.LogWarning($"Barrier: currentLevel ({currentLevel})�� cooldownTimes �迭�� ������ ������ϴ�. �⺻�� 0�� ��ȯ�մϴ�.");
        return 0f;
    }

    /// <summary>
    /// Ability Ŭ������ GetDescription() �޼��带 �������̵��Ͽ� ������ ���� ��Ÿ�� �ð��� ���� ���Խ�ŵ�ϴ�.
    /// </summary>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            int currentCooldown = GetNextLevelIncrease();
            return $"{baseDescription}\nLv {currentLevel + 1}: {currentCooldown}�� ��Ÿ��";
        }
        else
        {
            int finalCooldown = GetNextLevelIncrease();
            return $"{baseDescription}\n(Max Level: {finalCooldown}�� ��Ÿ��)";
        }
    }

    /// <summary>
    /// �ɷ��� ������ �ʱ�ȭ�ϰ� ��Ÿ���� ������ �� ���и� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

        if (cooldownCoroutine != null)
        {
            playerInstance.StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        DeactivateBarrierVisual();
        currentLevel = 0;
    }

    private void OnValidate()
    {
        if (cooldownTimes.Length != maxLevel)
        {
            Debug.LogWarning($"Barrier: cooldownTimes �迭�� ���̰� maxLevel ({maxLevel})�� ��ġ���� �ʽ��ϴ�. �迭 ���̸� ����ϴ�.");
            Array.Resize(ref cooldownTimes, maxLevel);
        }
    }
}
