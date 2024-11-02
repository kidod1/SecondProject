using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/EnchantingMelody")]
public class EnchantingMelody : Ability
{
    [Tooltip("버프가 바뀌는 시간 간격 (초)")]
    public float buffChangeInterval = 5f;

    [Header("레벨별 공격력 버프 수치")]
    public int[] attackDamageBuffs = { 10, 15, 20, 25, 30 };

    [Header("레벨별 공격 속도 버프 수치 (초당 공격 횟수 증가량)")]
    public float[] attackSpeedBuffs = { 0.5f, 0.75f, 1.0f, 1.25f, 1.5f };

    [Header("레벨별 이동 속도 버프 수치 (예: 1.0 = 1 단위 증가)")]
    public float[] movementSpeedBuffs = { 1f, 1.5f, 2f, 2.5f, 3f };

    [Header("Buff Effects")]
    [SerializeField]
    private GameObject attackDamageEffectPrefab; // 공격력 버프 이펙트 프리팹

    [SerializeField]
    private GameObject attackSpeedEffectPrefab; // 공격 속도 버프 이펙트 프리팹

    [SerializeField]
    private GameObject movementSpeedEffectPrefab; // 이동 속도 버프 이펙트 프리팹

    private float currentAttackSpeedBuffValue = 0f;
    private int currentAttackDamageBuffValue = 0;
    private float currentMovementSpeedBuffValue = 0f;

    private PlayerData playerData;
    private Player playerInstance;

    // 전역 BuffType 열거형 사용
    private BuffType currentBuffType = BuffType.AttackDamage;
    private Coroutine buffCoroutine;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("EnchantingMelody Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;
        playerData = player.stat; // PlayerData 참조

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
            Debug.LogWarning("EnchantingMelody: 이미 최대 레벨에 도달했습니다.");
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
            Debug.LogWarning("EnchantingMelody ApplyCurrentBuff: playerData 또는 playerInstance가 null입니다.");
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

        // 버프 적용
        playerData.AddBuff(this.name, currentBuffType, buffValue);

        // 이펙트 생성 및 3초 후 파괴
        InstantiateBuffEffect(effectPrefab);
    }

    private void RemoveCurrentBuff()
    {
        if (playerData == null)
        {
            Debug.LogWarning("EnchantingMelody RemoveCurrentBuff: playerData가 null입니다.");
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

        // 버프 제거
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
            Debug.LogWarning("EnchantingMelody InstantiateBuffEffect: playerInstance가 null이거나 effectPrefab이 할당되지 않았습니다.");
            return;
        }

        // 이펙트를 플레이어의 자식으로 인스턴스화
        GameObject effectInstance = Instantiate(effectPrefab, playerInstance.transform);
        effectInstance.transform.localPosition = Vector3.zero; // 중앙에 배치
        effectInstance.transform.localRotation = Quaternion.identity; // 기본 회전

        // 3초 후 이펙트 파괴
        Destroy(effectInstance, 3f);
    }

    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";

        description += $"버프는 {buffChangeInterval}초마다 순환합니다.\n";
        description += $"현재 레벨: {currentLevel + 1}\n";
        description += $"공격력 버프: +{GetAttackDamageBuff()}\n";
        description += $"공격 속도 버프: +{GetAttackSpeedBuff()} 초당 공격 횟수 증가\n";
        description += $"이동 속도 버프: +{GetMovementSpeedBuff()}\n";

        return description;
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel)
        {
            // 다음 레벨에서 증가하는 공격력 버프 값을 반환 (예시로 공격력 버프 증가량 사용)
            int nextAttackDamageBuff = attackDamageBuffs[currentLevel + 1] - attackDamageBuffs[currentLevel];
            return nextAttackDamageBuff;
        }
        return 0;
    }
}
