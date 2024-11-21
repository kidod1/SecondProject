using System.Collections;
using UnityEngine;
using AK.Wwise;

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

    [Tooltip("버프 적용 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event buffAppliedSound;

    private PlayerData playerData;
    private Player playerInstance;

    private BuffType currentBuffType = BuffType.AttackDamage;
    private Coroutine buffCoroutine;

    // 기본 스탯 저장
    private int baseAttackDamage;
    private float baseAttackSpeed;
    private float baseMovementSpeed;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("EnchantingMelody Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;
        playerData = player.stat; // PlayerData 참조

        // 기본 스탯 저장 (최초 Apply 시)
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
            currentLevel++; // 레벨 증가
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

        // 버프 적용
        playerData.AddBuff(this.name, currentBuffType, buffValue);

        // 이펙트 생성 및 3초 후 파괴
        InstantiateBuffEffect(effectPrefab);

        // 버프 적용 시 사운드 재생
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
            Debug.LogWarning("EnchantingMelody RemoveCurrentBuff: playerData가 null입니다.");
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

        // 버프 제거
        playerData.RemoveBuff(this.name, currentBuffType);

        Debug.Log($"EnchantingMelody: Removed {currentBuffType} Buff");
    }

    private void RemoveAllBuffs()
    {
        if (playerData == null)
        {
            Debug.LogWarning("EnchantingMelody RemoveAllBuffs: playerData가 null입니다.");
            return;
        }

        // 모든 버프 타입에 대해 기본 스탯으로 복원
        playerData.currentPlayerDamage = baseAttackDamage;
        playerData.currentAttackSpeed = baseAttackSpeed;
        playerData.currentPlayerSpeed = baseMovementSpeed;

        // 모든 버프 제거
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
        description += $"Lv {currentLevel + 1}:\n";
        description += $"공격력 버프: +{GetAttackDamageBuff()}\n";
        description += $"공격 속도 버프: +{GetAttackSpeedBuff()} 초당 공격 횟수 증가\n";
        description += $"이동 속도 버프: +{GetMovementSpeedBuff()}\n";

        return description;
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel && (currentLevel + 1) < attackDamageBuffs.Length)
        {
            // 다음 레벨에서 증가하는 공격력 버프 값을 반환 (예시로 공격력 버프 증가량 사용)
            int nextAttackDamageBuff = attackDamageBuffs[currentLevel + 1] - attackDamageBuffs[currentLevel];
            return nextAttackDamageBuff;
        }
        return 0;
    }
}
