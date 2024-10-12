using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/EnchantingMelody")]
public class EnchantingMelody : Ability
{
    [Tooltip("버프가 바뀌는 시간 간격 (초)")]
    public float buffChangeInterval = 5f;

    [Header("레벨별 공격력 버프 수치")]
    public int[] attackDamageBuffs = { 10, 15, 20, 25, 30 };

    [Header("레벨별 공격 속도 버프 수치 (예: 0.1 = 10% 증가)")]
    public float[] attackSpeedBuffs = { 0.1f, 0.15f, 0.2f, 0.25f, 0.3f };

    [Header("레벨별 이동 속도 버프 수치 (예: 1.0 = 1 단위 증가)")]
    public float[] movementSpeedBuffs = { 1f, 1.5f, 2f, 2.5f, 3f };

    private Player playerInstance;

    private enum BuffType
    {
        AttackDamage,   // 공격력 버프
        AttackSpeed,    // 공격 속도 버프
        MovementSpeed   // 이동 속도 버프
    }

    private BuffType currentBuffType = BuffType.AttackDamage;
    private Coroutine buffCoroutine;

    // 현재 적용된 버프 값을 저장하는 변수
    private int lastAppliedAttackDamageBuff = 0;
    private float lastAppliedAttackSpeedBuff = 0f;
    private float lastAppliedMovementSpeedBuff = 0f;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("EnchantingMelody Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;

        // 버프 순환 코루틴 시작
        if (buffCoroutine == null)
        {
            buffCoroutine = player.StartCoroutine(BuffRotationCoroutine());
            Debug.Log("EnchantingMelody: BuffRotationCoroutine 시작.");
        }
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 버프 수치가 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;

            // 현재 적용된 버프를 새로운 레벨에 맞게 재적용
            RemoveCurrentBuff();
            ApplyCurrentBuff();

            Debug.Log($"EnchantingMelody: 업그레이드 완료. 현재 레벨: {currentLevel}");
        }
        else
        {
            Debug.LogWarning("EnchantingMelody: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력을 초기화하고 버프를 제거합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        if (buffCoroutine != null)
        {
            if (playerInstance != null)
            {
                playerInstance.StopCoroutine(buffCoroutine);
                Debug.Log("EnchantingMelody: buffCoroutine 정지.");
            }
            else
            {
                Debug.LogError("EnchantingMelody: playerInstance가 null입니다. 코루틴을 정지할 수 없습니다.");
            }
            buffCoroutine = null;
        }

        RemoveCurrentBuff();
        currentBuffType = BuffType.AttackDamage;
        Debug.Log("EnchantingMelody: ResetLevel 완료. currentBuffType을 AttackDamage로 변경.");
    }

    /// <summary>
    /// 버프를 순환시키는 코루틴입니다.
    /// </summary>
    private IEnumerator BuffRotationCoroutine()
    {
        while (true)
        {
            ApplyCurrentBuff();
            Debug.Log($"EnchantingMelody: {currentBuffType} 버프 적용.");
            yield return new WaitForSeconds(buffChangeInterval);
            RemoveCurrentBuff();
            Debug.Log($"EnchantingMelody: {currentBuffType} 버프 제거.");
            AdvanceToNextBuff();
            Debug.Log($"EnchantingMelody: 다음 버프로 변경: {currentBuffType}");
        }
    }

    /// <summary>
    /// 현재 버프를 플레이어에게 적용합니다.
    /// </summary>
    private void ApplyCurrentBuff()
    {
        switch (currentBuffType)
        {
            case BuffType.AttackDamage:
                lastAppliedAttackDamageBuff = GetAttackDamageBuff();
                playerInstance.stat.currentPlayerDamage += lastAppliedAttackDamageBuff;
                Debug.Log($"EnchantingMelody: AttackDamage 버프 적용: +{lastAppliedAttackDamageBuff}");
                break;
            case BuffType.AttackSpeed:
                lastAppliedAttackSpeedBuff = GetAttackSpeedBuff();
                playerInstance.stat.currentShootCooldown -= lastAppliedAttackSpeedBuff;
                if (playerInstance.stat.currentShootCooldown < 0.1f)
                {
                    playerInstance.stat.currentShootCooldown = 0.1f; // 최소 쿨다운 제한
                }
                Debug.Log($"EnchantingMelody: AttackSpeed 버프 적용: -{lastAppliedAttackSpeedBuff} 쿨타임");
                break;
            case BuffType.MovementSpeed:
                lastAppliedMovementSpeedBuff = GetMovementSpeedBuff();
                playerInstance.stat.currentPlayerSpeed += lastAppliedMovementSpeedBuff;
                Debug.Log($"EnchantingMelody: MovementSpeed 버프 적용: +{lastAppliedMovementSpeedBuff}");
                break;
        }
    }

    /// <summary>
    /// 현재 버프를 제거합니다.
    /// </summary>
    private void RemoveCurrentBuff()
    {
        if (playerInstance == null)
        {
            Debug.LogError("RemoveCurrentBuff: playerInstance가 null입니다.");
            return;
        }

        if (playerInstance.stat == null)
        {
            Debug.LogError("RemoveCurrentBuff: playerInstance.stat이 null입니다.");
            return;
        }

        switch (currentBuffType)
        {
            case BuffType.AttackDamage:
                playerInstance.stat.currentPlayerDamage -= lastAppliedAttackDamageBuff;
                Debug.Log($"EnchantingMelody: AttackDamage 버프 제거: -{lastAppliedAttackDamageBuff}");
                lastAppliedAttackDamageBuff = 0;
                break;
            case BuffType.AttackSpeed:
                playerInstance.stat.currentShootCooldown += lastAppliedAttackSpeedBuff;
                Debug.Log($"EnchantingMelody: AttackSpeed 버프 제거: +{lastAppliedAttackSpeedBuff} 쿨타임");
                lastAppliedAttackSpeedBuff = 0f;
                break;
            case BuffType.MovementSpeed:
                playerInstance.stat.currentPlayerSpeed -= lastAppliedMovementSpeedBuff;
                Debug.Log($"EnchantingMelody: MovementSpeed 버프 제거: -{lastAppliedMovementSpeedBuff}");
                lastAppliedMovementSpeedBuff = 0f;
                break;
        }
    }

    /// <summary>
    /// 다음 버프로 이동합니다.
    /// </summary>
    private void AdvanceToNextBuff()
    {
        currentBuffType = (BuffType)(((int)currentBuffType + 1) % System.Enum.GetValues(typeof(BuffType)).Length);
    }

    /// <summary>
    /// 현재 레벨에 맞는 공격력 버프 수치를 반환합니다.
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
    /// 현재 레벨에 맞는 공격 속도 버프 수치를 반환합니다.
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
    /// 현재 레벨에 맞는 이동 속도 버프 수치를 반환합니다.
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
    /// 능력의 설명을 반환합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";

        description += $"버프는 {buffChangeInterval}초마다 순환합니다.\n";
        description += $"현재 레벨: {currentLevel + 1}\n";
        description += $"공격력 버프: +{GetAttackDamageBuff()}\n";
        description += $"공격 속도 버프: +{GetAttackSpeedBuff() * 100}%\n";
        description += $"이동 속도 버프: +{GetMovementSpeedBuff()}\n";

        return description;
    }

    /// <summary>
    /// 다음 레벨에서의 버프 증가량을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨에서의 버프 증가량</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel)
        {
            int nextAttackDamageBuff = attackDamageBuffs[currentLevel + 1] - attackDamageBuffs[currentLevel];
            // float nextAttackSpeedBuff = attackSpeedBuffs[currentLevel + 1] - attackSpeedBuffs[currentLevel];
            // float nextMovementSpeedBuff = movementSpeedBuffs[currentLevel + 1] - movementSpeedBuffs[currentLevel];

            // 여기서는 공격력 버프 증가량만 반환하지만, 필요에 따라 수정 가능
            return nextAttackDamageBuff;
        }
        return 0;
    }
}
