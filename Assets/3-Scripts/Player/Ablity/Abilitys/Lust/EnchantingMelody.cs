using System.Collections;
using UnityEngine;
using AK.Wwise;

[CreateAssetMenu(menuName = "Abilities/EnchantingMelody")]
public class EnchantingMelody : Ability
{
    [Tooltip("������ �ٲ�� �ð� ���� (��)")]
    public float buffChangeInterval = 5f;

    [Header("������ ���ݷ� ���� ��ġ")]
    public int[] attackDamageBuffs = { 10, 15, 20, 25, 30 };

    [Header("������ ���� �ӵ� ���� ��ġ (�ʴ� ���� Ƚ�� ������)")]
    public float[] attackSpeedBuffs = { 0.5f, 0.75f, 1.0f, 1.25f, 1.5f };

    [Header("������ �̵� �ӵ� ���� ��ġ (��: 1.0 = 1 ���� ����)")]
    public float[] movementSpeedBuffs = { 1f, 1.5f, 2f, 2.5f, 3f };

    [Header("Buff Effects")]
    [SerializeField]
    private GameObject attackDamageEffectPrefab; // ���ݷ� ���� ����Ʈ ������

    [SerializeField]
    private GameObject attackSpeedEffectPrefab; // ���� �ӵ� ���� ����Ʈ ������

    [SerializeField]
    private GameObject movementSpeedEffectPrefab; // �̵� �ӵ� ���� ����Ʈ ������

    [Tooltip("���� ���� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event buffAppliedSound;

    private PlayerData playerData;
    private Player playerInstance;

    private BuffType currentBuffType = BuffType.AttackDamage;
    private Coroutine buffCoroutine;

    // �⺻ ���� ����
    private int baseAttackDamage;
    private float baseAttackSpeed;
    private float baseMovementSpeed;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("EnchantingMelody Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;
        playerData = player.stat; // PlayerData ����

        // �⺻ ���� ���� (���� Apply ��)
        if (buffCoroutine == null)
        {
            baseAttackDamage = playerData.currentPlayerDamage;
            baseAttackSpeed = playerData.currentAttackSpeed;
            baseMovementSpeed = playerData.currentPlayerSpeed;

            buffCoroutine = player.StartCoroutine(BuffRotationCoroutine());
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++; // ���� ����
            RemoveCurrentBuff();
            ApplyCurrentBuff();
        }
        else
        {
            Debug.LogWarning("EnchantingMelody: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    public override void ResetLevel()
    {
        if (buffCoroutine != null)
        {
            if (playerInstance != null)
            {
                playerInstance.StopCoroutine(buffCoroutine);
            }
            buffCoroutine = null;
        }
        currentLevel = 0;
        RemoveAllBuffs();
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
        if (playerData == null || playerInstance == null)
        {
            Debug.LogWarning("EnchantingMelody ApplyCurrentBuff: playerData �Ǵ� playerInstance�� null�Դϴ�.");
            return;
        }

        float buffValue = 0f;
        GameObject effectPrefab = null;

        switch (currentBuffType)
        {
            case BuffType.AttackDamage:
                buffValue = GetAttackDamageBuff();
                effectPrefab = attackDamageEffectPrefab;
                playerData.currentPlayerDamage = baseAttackDamage + Mathf.RoundToInt(buffValue);
                break;
            case BuffType.AttackSpeed:
                buffValue = GetAttackSpeedBuff();
                effectPrefab = attackSpeedEffectPrefab;
                playerData.currentAttackSpeed = baseAttackSpeed + buffValue;
                break;
            case BuffType.MovementSpeed:
                buffValue = GetMovementSpeedBuff();
                effectPrefab = movementSpeedEffectPrefab;
                playerData.currentPlayerSpeed = baseMovementSpeed + buffValue;
                break;
        }

        // ���� ����
        playerData.AddBuff(this.name, currentBuffType, buffValue);

        // ����Ʈ ���� �� 3�� �� �ı�
        InstantiateBuffEffect(effectPrefab);

        // ���� ���� �� ���� ���
        if (buffAppliedSound != null)
        {
            buffAppliedSound.Post(playerInstance.gameObject);
        }

        Debug.Log($"EnchantingMelody: Applied {currentBuffType} Buff of {buffValue}");
    }

    private void RemoveCurrentBuff()
    {
        if (playerData == null)
        {
            Debug.LogWarning("EnchantingMelody RemoveCurrentBuff: playerData�� null�Դϴ�.");
            return;
        }

        switch (currentBuffType)
        {
            case BuffType.AttackDamage:
                playerData.currentPlayerDamage = baseAttackDamage;
                break;
            case BuffType.AttackSpeed:
                playerData.currentAttackSpeed = baseAttackSpeed;
                break;
            case BuffType.MovementSpeed:
                playerData.currentPlayerSpeed = baseMovementSpeed;
                break;
        }

        // ���� ����
        playerData.RemoveBuff(this.name, currentBuffType);

        Debug.Log($"EnchantingMelody: Removed {currentBuffType} Buff");
    }

    private void RemoveAllBuffs()
    {
        if (playerData == null)
        {
            Debug.LogWarning("EnchantingMelody RemoveAllBuffs: playerData�� null�Դϴ�.");
            return;
        }

        // ��� ���� Ÿ�Կ� ���� �⺻ �������� ����
        playerData.currentPlayerDamage = baseAttackDamage;
        playerData.currentAttackSpeed = baseAttackSpeed;
        playerData.currentPlayerSpeed = baseMovementSpeed;

        // ��� ���� ����
        playerData.RemoveBuff(this.name, currentBuffType);

        Debug.Log("EnchantingMelody: Removed all buffs and reset to base stats");
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

    private void InstantiateBuffEffect(GameObject effectPrefab)
    {
        if (playerInstance == null || effectPrefab == null)
        {
            Debug.LogWarning("EnchantingMelody InstantiateBuffEffect: playerInstance�� null�̰ų� effectPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // ����Ʈ�� �÷��̾��� �ڽ����� �ν��Ͻ�ȭ
        GameObject effectInstance = Instantiate(effectPrefab, playerInstance.transform);
        effectInstance.transform.localPosition = Vector3.zero; // �߾ӿ� ��ġ
        effectInstance.transform.localRotation = Quaternion.identity; // �⺻ ȸ��

        // 3�� �� ����Ʈ �ı�
        Destroy(effectInstance, 3f);
    }

    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";

        description += $"������ {buffChangeInterval}�ʸ��� ��ȯ�մϴ�.\n";
        description += $"Lv {currentLevel + 1}:\n";
        description += $"���ݷ� ����: +{GetAttackDamageBuff()}\n";
        description += $"���� �ӵ� ����: +{GetAttackSpeedBuff()} �ʴ� ���� Ƚ�� ����\n";
        description += $"�̵� �ӵ� ����: +{GetMovementSpeedBuff()}\n";

        return description;
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel && (currentLevel + 1) < attackDamageBuffs.Length)
        {
            // ���� �������� �����ϴ� ���ݷ� ���� ���� ��ȯ (���÷� ���ݷ� ���� ������ ���)
            int nextAttackDamageBuff = attackDamageBuffs[currentLevel + 1] - attackDamageBuffs[currentLevel];
            return nextAttackDamageBuff;
        }
        return 0;
    }
}
