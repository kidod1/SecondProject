using System.Collections;
using UnityEngine;
using AK.Wwise; // WWISE ���ӽ����̽� �߰�

[CreateAssetMenu(menuName = "Abilities/TooMuchWork")]
public class TooMuchWork : Ability
{
    [Tooltip("������ �ִ� ���� �ӵ� ����")]
    public float[] maxAttackSpeedMultipliers = { 2.0f, 2.5f, 3.0f, 3.5f, 4.0f };

    [Tooltip("������ �ִ� ���� �ӵ��� �����ϴ� �ð� (��)")]
    public float[] baseTimeToMaxSpeedLevels = { 5.0f, 4.5f, 4.0f, 3.5f, 3.0f };

    [Tooltip("������ ���� ���� �ð� (��)")]
    public float[] overheatDurationsLevels = { 5.0f, 4.5f, 4.0f, 3.5f, 3.0f };

    // WWISE �̺�Ʈ ������
    [Tooltip("���� �ӵ� ���� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event attackSpeedIncreaseSound;

    private Coroutine overheatCoroutine;
    private Coroutine attackSpeedCoroutine;
    private Player playerInstance;
    private bool isOverheated = false;
    private bool isListenerRegistered = false;

    // ���� ������ ���� ������
    private float maxAttackSpeedMultiplier;
    private float baseTimeToMaxSpeed;
    private float overheatDuration;

    public override void Apply(Player player)
    {
        Debug.Log(currentLevel);
        playerInstance = player;

        if (playerInstance == null)
        {
            return;
        }

        player.OnShoot.AddListener(HandleShooting);
        player.OnShootCanceled.AddListener(HandleShootCanceled);
        isListenerRegistered = true;
        ApplyLevelEffects();
    }

    private void ApplyLevelEffects()
    {
        int levelIndex = Mathf.Clamp(currentLevel, 0, maxAttackSpeedMultipliers.Length - 1);

        maxAttackSpeedMultiplier = maxAttackSpeedMultipliers[levelIndex];
        baseTimeToMaxSpeed = baseTimeToMaxSpeedLevels[levelIndex];
        overheatDuration = overheatDurationsLevels[levelIndex];
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            ApplyLevelEffects();

            // ���׷��̵� �� ���� �ӵ� ���� ���� ���
            if (attackSpeedIncreaseSound != null)
            {
                attackSpeedIncreaseSound.Post(playerInstance.gameObject);
            }
        }
    }

    protected override int GetNextLevelIncrease()
    {
        int levelIndex = currentLevel; // ���� ������ �̸� ���� ���� currentLevel ���

        if (levelIndex < maxAttackSpeedMultipliers.Length)
        {
            return Mathf.RoundToInt(maxAttackSpeedMultipliers[levelIndex] * 100); // �ۼ�Ʈ�� ��ȯ
        }
        return 0;
    }

    private void HandleShooting(Vector2 direction, int prefabIndex, GameObject projectile)
    {
        Debug.Log("HandleShooting called.");

        if (isOverheated || playerInstance == null) return;

        if (attackSpeedCoroutine == null)
        {
            attackSpeedCoroutine = playerInstance.StartCoroutine(IncreaseAttackSpeed());
        }
    }

    private void HandleShootCanceled()
    {
        if (attackSpeedCoroutine != null && playerInstance != null)
        {
            playerInstance.StopCoroutine(attackSpeedCoroutine);
            attackSpeedCoroutine = null;
        }

        if (playerInstance != null)
        {
            playerInstance.stat.currentAttackSpeed = playerInstance.stat.defaultAttackSpeed;
        }
    }

    private IEnumerator IncreaseAttackSpeed()
    {
        if (playerInstance == null)
        {
            Debug.Log("IncreaseAttackSpeed: playerInstance is null, exiting coroutine.");
            yield break;
        }

        Debug.Log("IncreaseAttackSpeed: Coroutine started.");

        float elapsedTime = 0f;
        float originalAttackSpeed = playerInstance.stat.defaultAttackSpeed;
        float targetAttackSpeed = originalAttackSpeed * maxAttackSpeedMultiplier;

        while (elapsedTime < baseTimeToMaxSpeed)
        {
            if (!playerInstance.isShooting)
            {
                Debug.Log("IncreaseAttackSpeed: isShooting is false, resetting attack speed and exiting coroutine.");
                playerInstance.stat.currentAttackSpeed = originalAttackSpeed;
                attackSpeedCoroutine = null;
                yield break;
            }

            if (playerInstance.IsOverheated)
            {
                Debug.Log("IncreaseAttackSpeed: isOverheated is true, exiting coroutine.");
                attackSpeedCoroutine = null;
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / baseTimeToMaxSpeed;
            float newAttackSpeed = Mathf.Lerp(originalAttackSpeed, targetAttackSpeed, progress);

            playerInstance.stat.currentAttackSpeed = newAttackSpeed;

            yield return null;
        }

        // �ִ� ���� �ӵ� ����
        playerInstance.stat.currentAttackSpeed = targetAttackSpeed;
        // ���� ���� Ʈ���� �� ���� ���
        TriggerOverheat();

        // �ڷ�ƾ ���� �ʱ�ȭ
        attackSpeedCoroutine = null;
    }

    private void TriggerOverheat()
    {
        if (playerInstance == null)
        {
            return;
        }

        if (overheatCoroutine != null)
        {
            playerInstance.StopCoroutine(overheatCoroutine);
        }
        overheatCoroutine = playerInstance.StartCoroutine(Overheat());
    }

    private IEnumerator Overheat()
    {
        if (playerInstance == null)
        {
            yield break;
        }

        playerInstance.IsOverheated = true; // ���� ���� ����

        yield return new WaitForSeconds(overheatDuration);

        if (playerInstance != null)
        {
            playerInstance.IsOverheated = false; // ���� ���� ����
            playerInstance.stat.currentAttackSpeed = playerInstance.stat.defaultAttackSpeed;

            // ������ �簳�� �� �ֵ��� nextShootTime �缳��
            playerInstance.ResetNextShootTime();
        }
    }

    public override void ResetLevel()
    {
        if (playerInstance != null)
        {
            if (overheatCoroutine != null)
            {
                playerInstance.StopCoroutine(overheatCoroutine);
                overheatCoroutine = null;
            }
            if (attackSpeedCoroutine != null)
            {
                playerInstance.StopCoroutine(attackSpeedCoroutine);
                attackSpeedCoroutine = null;
            }

            playerInstance.stat.currentAttackSpeed = playerInstance.stat.defaultAttackSpeed;

            // �̺�Ʈ ������ ����
            if (isListenerRegistered)
            {
                playerInstance.OnShoot.RemoveListener(HandleShooting);
                playerInstance.OnShootCanceled.RemoveListener(HandleShootCanceled);
                isListenerRegistered = false;
            }
        }
        currentLevel = 0;
        isOverheated = false;
    }

    public override string GetDescription()
    {
        int levelIndex = Mathf.Clamp(currentLevel, 0, maxAttackSpeedMultipliers.Length - 1);

        if (currentLevel < maxAttackSpeedMultipliers.Length && currentLevel >= 0)
        {
            float maxAttackSpeedMultiplierPercent = maxAttackSpeedMultipliers[levelIndex] * 100f;
            float baseTimeToMaxSpeedValue = baseTimeToMaxSpeedLevels[levelIndex];
            float overheatDurationValue = overheatDurationsLevels[levelIndex];

            return $"{baseDescription}\nLv {currentLevel + 1}: ���� �� �ִ� ���� �ӵ� ������ {maxAttackSpeedMultiplierPercent}%���� ������Ű��, �ִ� �ӵ��� �����ϴ� �ð��� {baseTimeToMaxSpeedValue}���Դϴ�. �ִ� �ӵ� ���� �� �����Ǿ� {overheatDurationValue}�� ���� ������ �� �����ϴ�.";
        }
        else if (currentLevel > maxAttackSpeedMultipliers.Length)
        {
            float maxAttackSpeedMultiplierPercent = maxAttackSpeedMultipliers[maxAttackSpeedMultipliers.Length - 1] * 100f;
            float baseTimeToMaxSpeedValue = baseTimeToMaxSpeedLevels[baseTimeToMaxSpeedLevels.Length - 1];
            float overheatDurationValue = overheatDurationsLevels[overheatDurationsLevels.Length - 1];

            return $"{baseDescription}\n�ִ� ���� ����: ���� �� �ִ� ���� �ӵ� ������ {maxAttackSpeedMultiplierPercent}%���� ������Ű��, �ִ� �ӵ��� �����ϴ� �ð��� {baseTimeToMaxSpeedValue}���Դϴ�. �ִ� �ӵ� ���� �� �����Ǿ� {overheatDurationValue}�� ���� ������ �� �����ϴ�.";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����.";
        }
    }
}
