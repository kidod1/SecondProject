using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/EnchantingMelody")]
public class EnchantingMelody : Ability
{
    [Tooltip("������ �ٲ�� �ð� ���� (��)")]
    public float buffChangeInterval = 5f;

    [Header("������ ���ݷ� ���� ��ġ")]
    public int[] attackDamageBuffs = { 10, 15, 20, 25, 30 };

    [Header("������ ���� �ӵ� ���� ��ġ (��: 0.1 = 10% ����)")]
    public float[] attackSpeedBuffs = { 0.1f, 0.15f, 0.2f, 0.25f, 0.3f };

    [Header("������ �̵� �ӵ� ���� ��ġ (��: 1.0 = 1 ���� ����)")]
    public float[] movementSpeedBuffs = { 1f, 1.5f, 2f, 2.5f, 3f };

    private Player playerInstance;

    private enum BuffType
    {
        AttackDamage,   // ���ݷ� ����
        AttackSpeed,    // ���� �ӵ� ����
        MovementSpeed   // �̵� �ӵ� ����
    }

    private BuffType currentBuffType = BuffType.AttackDamage;
    private Coroutine buffCoroutine;

    // ���� ����� ���� ���� �����ϴ� ����
    private int lastAppliedAttackDamageBuff = 0;
    private float lastAppliedAttackSpeedBuff = 0f;
    private float lastAppliedMovementSpeedBuff = 0f;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("EnchantingMelody Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;

        // ���� ��ȯ �ڷ�ƾ ����
        if (buffCoroutine == null)
        {
            buffCoroutine = player.StartCoroutine(BuffRotationCoroutine());
            Debug.Log("EnchantingMelody: BuffRotationCoroutine ����.");
        }
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ���� ��ġ�� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;

            // ���� ����� ������ ���ο� ������ �°� ������
            RemoveCurrentBuff();
            ApplyCurrentBuff();

            Debug.Log($"EnchantingMelody: ���׷��̵� �Ϸ�. ���� ����: {currentLevel}");
        }
        else
        {
            Debug.LogWarning("EnchantingMelody: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ��� �ʱ�ȭ�ϰ� ������ �����մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        if (buffCoroutine != null)
        {
            if (playerInstance != null)
            {
                playerInstance.StopCoroutine(buffCoroutine);
                Debug.Log("EnchantingMelody: buffCoroutine ����.");
            }
            else
            {
                Debug.LogError("EnchantingMelody: playerInstance�� null�Դϴ�. �ڷ�ƾ�� ������ �� �����ϴ�.");
            }
            buffCoroutine = null;
        }

        RemoveCurrentBuff();
        currentBuffType = BuffType.AttackDamage;
        Debug.Log("EnchantingMelody: ResetLevel �Ϸ�. currentBuffType�� AttackDamage�� ����.");
    }

    /// <summary>
    /// ������ ��ȯ��Ű�� �ڷ�ƾ�Դϴ�.
    /// </summary>
    private IEnumerator BuffRotationCoroutine()
    {
        while (true)
        {
            ApplyCurrentBuff();
            Debug.Log($"EnchantingMelody: {currentBuffType} ���� ����.");
            yield return new WaitForSeconds(buffChangeInterval);
            RemoveCurrentBuff();
            Debug.Log($"EnchantingMelody: {currentBuffType} ���� ����.");
            AdvanceToNextBuff();
            Debug.Log($"EnchantingMelody: ���� ������ ����: {currentBuffType}");
        }
    }

    /// <summary>
    /// ���� ������ �÷��̾�� �����մϴ�.
    /// </summary>
    private void ApplyCurrentBuff()
    {
        switch (currentBuffType)
        {
            case BuffType.AttackDamage:
                lastAppliedAttackDamageBuff = GetAttackDamageBuff();
                playerInstance.stat.currentPlayerDamage += lastAppliedAttackDamageBuff;
                Debug.Log($"EnchantingMelody: AttackDamage ���� ����: +{lastAppliedAttackDamageBuff}");
                break;
            case BuffType.AttackSpeed:
                lastAppliedAttackSpeedBuff = GetAttackSpeedBuff();
                playerInstance.stat.currentShootCooldown -= lastAppliedAttackSpeedBuff;
                if (playerInstance.stat.currentShootCooldown < 0.1f)
                {
                    playerInstance.stat.currentShootCooldown = 0.1f; // �ּ� ��ٿ� ����
                }
                Debug.Log($"EnchantingMelody: AttackSpeed ���� ����: -{lastAppliedAttackSpeedBuff} ��Ÿ��");
                break;
            case BuffType.MovementSpeed:
                lastAppliedMovementSpeedBuff = GetMovementSpeedBuff();
                playerInstance.stat.currentPlayerSpeed += lastAppliedMovementSpeedBuff;
                Debug.Log($"EnchantingMelody: MovementSpeed ���� ����: +{lastAppliedMovementSpeedBuff}");
                break;
        }
    }

    /// <summary>
    /// ���� ������ �����մϴ�.
    /// </summary>
    private void RemoveCurrentBuff()
    {
        if (playerInstance == null)
        {
            Debug.LogError("RemoveCurrentBuff: playerInstance�� null�Դϴ�.");
            return;
        }

        if (playerInstance.stat == null)
        {
            Debug.LogError("RemoveCurrentBuff: playerInstance.stat�� null�Դϴ�.");
            return;
        }

        switch (currentBuffType)
        {
            case BuffType.AttackDamage:
                playerInstance.stat.currentPlayerDamage -= lastAppliedAttackDamageBuff;
                Debug.Log($"EnchantingMelody: AttackDamage ���� ����: -{lastAppliedAttackDamageBuff}");
                lastAppliedAttackDamageBuff = 0;
                break;
            case BuffType.AttackSpeed:
                playerInstance.stat.currentShootCooldown += lastAppliedAttackSpeedBuff;
                Debug.Log($"EnchantingMelody: AttackSpeed ���� ����: +{lastAppliedAttackSpeedBuff} ��Ÿ��");
                lastAppliedAttackSpeedBuff = 0f;
                break;
            case BuffType.MovementSpeed:
                playerInstance.stat.currentPlayerSpeed -= lastAppliedMovementSpeedBuff;
                Debug.Log($"EnchantingMelody: MovementSpeed ���� ����: -{lastAppliedMovementSpeedBuff}");
                lastAppliedMovementSpeedBuff = 0f;
                break;
        }
    }

    /// <summary>
    /// ���� ������ �̵��մϴ�.
    /// </summary>
    private void AdvanceToNextBuff()
    {
        currentBuffType = (BuffType)(((int)currentBuffType + 1) % System.Enum.GetValues(typeof(BuffType)).Length);
    }

    /// <summary>
    /// ���� ������ �´� ���ݷ� ���� ��ġ�� ��ȯ�մϴ�.
    /// </summary>
    private int GetAttackDamageBuff()
    {
        if (currentLevel < attackDamageBuffs.Length)
        {
            return attackDamageBuffs[currentLevel];
        }
        return attackDamageBuffs[attackDamageBuffs.Length - 1];
    }

    /// <summary>
    /// ���� ������ �´� ���� �ӵ� ���� ��ġ�� ��ȯ�մϴ�.
    /// </summary>
    private float GetAttackSpeedBuff()
    {
        if (currentLevel < attackSpeedBuffs.Length)
        {
            return attackSpeedBuffs[currentLevel];
        }
        return attackSpeedBuffs[attackSpeedBuffs.Length - 1];
    }

    /// <summary>
    /// ���� ������ �´� �̵� �ӵ� ���� ��ġ�� ��ȯ�մϴ�.
    /// </summary>
    private float GetMovementSpeedBuff()
    {
        if (currentLevel < movementSpeedBuffs.Length)
        {
            return movementSpeedBuffs[currentLevel];
        }
        return movementSpeedBuffs[movementSpeedBuffs.Length - 1];
    }

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";

        description += $"������ {buffChangeInterval}�ʸ��� ��ȯ�մϴ�.\n";
        description += $"���� ����: {currentLevel + 1}\n";
        description += $"���ݷ� ����: +{GetAttackDamageBuff()}\n";
        description += $"���� �ӵ� ����: +{GetAttackSpeedBuff() * 100}%\n";
        description += $"�̵� �ӵ� ����: +{GetMovementSpeedBuff()}\n";

        return description;
    }

    /// <summary>
    /// ���� ���������� ���� �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���������� ���� ������</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel)
        {
            int nextAttackDamageBuff = attackDamageBuffs[currentLevel + 1] - attackDamageBuffs[currentLevel];
            // float nextAttackSpeedBuff = attackSpeedBuffs[currentLevel + 1] - attackSpeedBuffs[currentLevel];
            // float nextMovementSpeedBuff = movementSpeedBuffs[currentLevel + 1] - movementSpeedBuffs[currentLevel];

            // ���⼭�� ���ݷ� ���� �������� ��ȯ������, �ʿ信 ���� ���� ����
            return nextAttackDamageBuff;
        }
        return 0;
    }
}
