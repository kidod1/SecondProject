using System.Collections;
using UnityEngine;

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

    private float currentAttackSpeedBuffValue = 0f;
    private int currentAttackDamageBuffValue = 0;
    private float currentMovementSpeedBuffValue = 0f;

    private PlayerData playerData;
    private Player playerInstance;

    // ���� BuffType ������ ���
    private BuffType currentBuffType = BuffType.AttackDamage;
    private Coroutine buffCoroutine;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("EnchantingMelody Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;
        playerData = player.stat; // PlayerData ����

        if (buffCoroutine == null && currentLevel >= 0)
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
        else
        {
            Debug.LogWarning("EnchantingMelody: �̹� �ִ� ������ �����߽��ϴ�.");
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
                playerData.currentPlayerDamage += Mathf.RoundToInt(buffValue);
                currentAttackDamageBuffValue = Mathf.RoundToInt(buffValue);
                break;
            case BuffType.AttackSpeed:
                buffValue = GetAttackSpeedBuff();
                effectPrefab = attackSpeedEffectPrefab;
                playerData.currentAttackSpeed += buffValue;
                currentAttackSpeedBuffValue = buffValue;
                break;
            case BuffType.MovementSpeed:
                buffValue = GetMovementSpeedBuff();
                effectPrefab = movementSpeedEffectPrefab;
                playerData.currentPlayerSpeed += buffValue;
                currentMovementSpeedBuffValue = buffValue;
                break;
        }

        // ���� ����
        playerData.AddBuff(this.name, currentBuffType, buffValue);

        // ����Ʈ ���� �� 3�� �� �ı�
        InstantiateBuffEffect(effectPrefab);
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
                playerData.currentPlayerDamage -= currentAttackDamageBuffValue;
                currentAttackDamageBuffValue = 0;
                break;
            case BuffType.AttackSpeed:
                playerData.currentAttackSpeed -= currentAttackSpeedBuffValue;
                currentAttackSpeedBuffValue = 0f;
                break;
            case BuffType.MovementSpeed:
                playerData.currentPlayerSpeed -= currentMovementSpeedBuffValue;
                currentMovementSpeedBuffValue = 0f;
                break;
        }

        // ���� ����
        playerData.RemoveBuff(this.name, currentBuffType);
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
        description += $"���� ����: {currentLevel + 1}\n";
        description += $"���ݷ� ����: +{GetAttackDamageBuff()}\n";
        description += $"���� �ӵ� ����: +{GetAttackSpeedBuff()} �ʴ� ���� Ƚ�� ����\n";
        description += $"�̵� �ӵ� ����: +{GetMovementSpeedBuff()}\n";

        return description;
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel)
        {
            // ���� �������� �����ϴ� ���ݷ� ���� ���� ��ȯ (���÷� ���ݷ� ���� ������ ���)
            int nextAttackDamageBuff = attackDamageBuffs[currentLevel + 1] - attackDamageBuffs[currentLevel];
            return nextAttackDamageBuff;
        }
        return 0;
    }
}
