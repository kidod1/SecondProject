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

    public override void Apply(Player player)
    {
        if (player == null)
            return;

        playerInstance = player;

        if (buffCoroutine == null)
        {
            buffCoroutine = player.StartCoroutine(BuffRotationCoroutine());
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
            RemoveCurrentBuff();
            ApplyCurrentBuff();
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        if (buffCoroutine != null)
        {
            if (playerInstance != null)
            {
                playerInstance.StopCoroutine(buffCoroutine);
            }
            buffCoroutine = null;
        }

        RemoveCurrentBuff();
        currentBuffType = BuffType.AttackDamage;
    }

    private IEnumerator BuffRotationCoroutine()
    {
        while (true)
        {
            ApplyCurrentBuff();
            yield return new WaitForSeconds(buffChangeInterval);
            RemoveCurrentBuff();
            AdvanceToNextBuff();
        }
    }

    private void ApplyCurrentBuff()
    {
        switch (currentBuffType)
        {
            case BuffType.AttackDamage:
                lastAppliedAttackDamageBuff = GetAttackDamageBuff();
                playerInstance.stat.currentPlayerDamage += lastAppliedAttackDamageBuff;
                break;
            case BuffType.AttackSpeed:
                lastAppliedAttackSpeedBuff = GetAttackSpeedBuff();
                playerInstance.stat.currentShootCooldown -= lastAppliedAttackSpeedBuff;
                if (playerInstance.stat.currentShootCooldown < 0.1f)
                {
                    playerInstance.stat.currentShootCooldown = 0.1f; // �ּ� ��ٿ� ����
                }
                break;
            case BuffType.MovementSpeed:
                lastAppliedMovementSpeedBuff = GetMovementSpeedBuff();
                playerInstance.stat.currentPlayerSpeed += lastAppliedMovementSpeedBuff;
                break;
        }
    }

    private void RemoveCurrentBuff()
    {
        if (playerInstance == null || playerInstance.stat == null)
            return;

        switch (currentBuffType)
        {
            case BuffType.AttackDamage:
                playerInstance.stat.currentPlayerDamage -= lastAppliedAttackDamageBuff;
                lastAppliedAttackDamageBuff = 0;
                break;
            case BuffType.AttackSpeed:
                playerInstance.stat.currentShootCooldown += lastAppliedAttackSpeedBuff;
                lastAppliedAttackSpeedBuff = 0f;
                break;
            case BuffType.MovementSpeed:
                playerInstance.stat.currentPlayerSpeed -= lastAppliedMovementSpeedBuff;
                lastAppliedMovementSpeedBuff = 0f;
                break;
        }
    }

    private void AdvanceToNextBuff()
    {
        currentBuffType = (BuffType)(((int)currentBuffType + 1) % System.Enum.GetValues(typeof(BuffType)).Length);
    }

    private int GetAttackDamageBuff()
    {
        if (currentLevel < attackDamageBuffs.Length)
        {
            return attackDamageBuffs[currentLevel];
        }
        return attackDamageBuffs[attackDamageBuffs.Length - 1];
    }

    private float GetAttackSpeedBuff()
    {
        if (currentLevel < attackSpeedBuffs.Length)
        {
            return attackSpeedBuffs[currentLevel];
        }
        return attackSpeedBuffs[attackSpeedBuffs.Length - 1];
    }

    private float GetMovementSpeedBuff()
    {
        if (currentLevel < movementSpeedBuffs.Length)
        {
            return movementSpeedBuffs[currentLevel];
        }
        return movementSpeedBuffs[movementSpeedBuffs.Length - 1];
    }

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

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel)
        {
            int nextAttackDamageBuff = attackDamageBuffs[currentLevel + 1] - attackDamageBuffs[currentLevel];
            return nextAttackDamageBuff;
        }
        return 0;
    }
}
