using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/TooMuchWork")]
public class TooMuchWork : Ability
{
    [Tooltip("������ �ִ� ���� �ӵ� ����")]
    public float[] maxAttackSpeedMultipliers = { 2.0f, 2.5f, 3.0f, 3.5f, 4.0f }; // ������ �ִ� ���� �ӵ� ���� �迭

    [Tooltip("������ �ִ� ���� �ӵ��� �����ϴ� �ð� (��)")]
    public float[] baseTimeToMaxSpeedLevels = { 5.0f, 4.5f, 4.0f, 3.5f, 3.0f }; // ������ �ִ� �ӵ� ���� �ð� �迭

    [Tooltip("������ ���� ���� �ð� (��)")]
    public float[] overheatDurationsLevels = { 5.0f, 4.5f, 4.0f, 3.5f, 3.0f }; // ������ ���� ���� �ð� �迭

    private const float minAttackCooldown = 0.15f; // �ּ� ���� �ӵ� (��ٿ�)

    private Coroutine overheatCoroutine;
    private Coroutine attackSpeedCoroutine;
    private Player playerInstance;
    private bool isOverheated = false;

    // ���� ������ ���� ������
    private float maxAttackSpeedMultiplier;
    private float baseTimeToMaxSpeed;
    private float overheatDuration;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        playerInstance = player;

        if (playerInstance == null)
        {
            Debug.LogError("Player instance is null. Cannot apply TooMuchWork ability.");
            return;
        }

        if (currentLevel == 0)
        {
            player.OnShoot.AddListener(HandleShooting);
            player.OnShootCanceled.AddListener(HandleShootCanceled); // ������ �߰�
        }

        // ������ ���� ȿ�� ����
        ApplyLevelEffects();
    }

    /// <summary>
    /// ���� ������ ���� ���� �ӵ� ����, �ִ� �ӵ� ���� �ð�, ���� ���� �ð��� �����մϴ�.
    /// </summary>
    private void ApplyLevelEffects()
    {
        if (currentLevel < maxAttackSpeedMultipliers.Length)
        {
            // ���� ������ ���� ���� ����
            maxAttackSpeedMultiplier = maxAttackSpeedMultipliers[currentLevel];
            baseTimeToMaxSpeed = baseTimeToMaxSpeedLevels[currentLevel];
            overheatDuration = overheatDurationsLevels[currentLevel];
            Debug.Log($"TooMuchWork: Level {currentLevel + 1} ���� - MaxAttackSpeedMultiplier: {maxAttackSpeedMultiplier}, BaseTimeToMaxSpeed: {baseTimeToMaxSpeed}, OverheatDuration: {overheatDuration}");
        }
        else
        {
            // �迭 ������ �ʰ��� ��� ������ ������ ���� ���
            maxAttackSpeedMultiplier = maxAttackSpeedMultipliers[maxAttackSpeedMultipliers.Length - 1];
            baseTimeToMaxSpeed = baseTimeToMaxSpeedLevels[baseTimeToMaxSpeedLevels.Length - 1];
            overheatDuration = overheatDurationsLevels[overheatDurationsLevels.Length - 1];
            Debug.LogWarning($"TooMuchWork: currentLevel ({currentLevel + 1}) exceeds �迭 ����. ������ ������ ���� ����մϴ�.");
        }
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ���� �ӵ� ������ �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel�� 5��� currentLevel�� 0~4
        {
            currentLevel++;
            Debug.Log($"TooMuchWork ���׷��̵�: ���� ���� {currentLevel + 1}, MaxAttackSpeedMultiplier {maxAttackSpeedMultipliers[currentLevel]}");
            ApplyLevelEffects(); // ���׷��̵� �� ������ ���� ȿ�� ������
        }
        else
        {
            Debug.LogWarning("TooMuchWork: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// ���� ������ ���� �ӵ� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ���� �ӵ� ���� (�ۼ�Ʈ)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxAttackSpeedMultipliers.Length)
        {
            return Mathf.RoundToInt(maxAttackSpeedMultipliers[currentLevel + 1] * 100); // �ۼ�Ʈ�� ��ȯ
        }
        return 0;
    }

    /// <summary>
    /// �÷��̾ ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="direction">���� ����</param>
    /// <param name="prefabIndex">���� ������ �ε���</param>
    /// <param name="projectile">�߻�� ����ü</param>
    private void HandleShooting(Vector2 direction, int prefabIndex, GameObject projectile)
    {
        if (isOverheated || playerInstance == null) return;

        if (attackSpeedCoroutine != null)
        {
            playerInstance.StopCoroutine(attackSpeedCoroutine);
        }

        attackSpeedCoroutine = playerInstance.StartCoroutine(IncreaseAttackSpeed());
    }

    /// <summary>
    /// ���� ��� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    private void HandleShootCanceled()
    {
        if (attackSpeedCoroutine != null && playerInstance != null)
        {
            playerInstance.StopCoroutine(attackSpeedCoroutine);
            attackSpeedCoroutine = null;
        }

        if (playerInstance != null)
        {
            playerInstance.stat.currentShootCooldown = playerInstance.stat.defaultShootCooldown;
        }
    }

    /// <summary>
    /// ���� �ӵ��� ������Ű�� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator IncreaseAttackSpeed()
    {
        if (playerInstance == null)
        {
            yield break;
        }

        float elapsedTime = 0f;
        float originalCooldown = playerInstance.stat.currentShootCooldown;

        while (elapsedTime < baseTimeToMaxSpeed)
        {
            if (!playerInstance.isShooting)
            {
                playerInstance.stat.currentShootCooldown = originalCooldown;
                yield break;
            }

            elapsedTime += Time.unscaledDeltaTime;
            float newCooldown = Mathf.Lerp(originalCooldown, originalCooldown / maxAttackSpeedMultiplier, elapsedTime / baseTimeToMaxSpeed);

            // ���� �ӵ��� �ּҰ��� �����ϸ� ���� ���·� ��ȯ
            if (newCooldown <= minAttackCooldown)
            {
                playerInstance.stat.currentShootCooldown = minAttackCooldown;
                TriggerOverheat(); // ���� ���·� ��ȯ
                yield break;
            }

            playerInstance.stat.currentShootCooldown = newCooldown;

            yield return null;
        }
    }

    /// <summary>
    /// ���� ���¸� Ʈ�����ϴ� �޼����Դϴ�.
    /// </summary>
    private void TriggerOverheat()
    {
        if (playerInstance == null)
        {
            Debug.LogError("Player instance is null. Cannot trigger overheat.");
            return;
        }

        if (overheatCoroutine != null)
        {
            playerInstance.StopCoroutine(overheatCoroutine);
        }
        overheatCoroutine = playerInstance.StartCoroutine(Overheat());
    }

    /// <summary>
    /// ���� ���¸� ó���ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator Overheat()
    {
        if (playerInstance == null)
        {
            yield break;
        }

        isOverheated = true;
        Debug.Log("Weapon overheated! Can't attack for " + overheatDuration + " seconds.");
        playerInstance.stat.currentShootCooldown = Mathf.Infinity;

        yield return new WaitForSeconds(overheatDuration);

        if (playerInstance != null)
        {
            isOverheated = false;
            playerInstance.stat.currentShootCooldown = playerInstance.stat.defaultShootCooldown;
            Debug.Log("Weapon cooled down. You can attack again.");
        }
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

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

            playerInstance.stat.currentShootCooldown = playerInstance.stat.defaultShootCooldown;
        }

        isOverheated = false;
    }

    /// <summary>
    /// �ɷ��� ���� ���¿� ȿ���� �����ϴ� ���ڿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        Debug.Log($"GetDescription called. Current Level: {currentLevel + 1}, maxAttackSpeedMultipliers.Length: {maxAttackSpeedMultipliers.Length}, maxLevel: {maxLevel}");

        if (currentLevel < maxAttackSpeedMultipliers.Length && currentLevel >= 0)
        {
            float maxAttackSpeedMultiplierPercent = maxAttackSpeedMultipliers[currentLevel] * 100f;
            float baseTimeToMaxSpeedValue = baseTimeToMaxSpeedLevels[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: ���� �� �ִ� ���� �ӵ� ������ {maxAttackSpeedMultiplierPercent}%���� ������Ű��, �ִ� �ӵ��� �����ϴ� �ð��� {baseTimeToMaxSpeedValue}�ʷ� ����˴ϴ�.";
        }
        else if (currentLevel >= maxAttackSpeedMultipliers.Length)
        {
            float maxAttackSpeedMultiplierPercent = maxAttackSpeedMultipliers[maxAttackSpeedMultipliers.Length - 1] * 100f;
            float baseTimeToMaxSpeedValue = baseTimeToMaxSpeedLevels[baseTimeToMaxSpeedLevels.Length - 1];
            return $"{baseDescription}\n�ִ� ���� ����: ���� �� �ִ� ���� �ӵ� ������ {maxAttackSpeedMultiplierPercent}%���� ������Ű��, �ִ� �ӵ��� �����ϴ� �ð��� {baseTimeToMaxSpeedValue}�ʷ� ����˴ϴ�.";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����.";
        }
    }
}
