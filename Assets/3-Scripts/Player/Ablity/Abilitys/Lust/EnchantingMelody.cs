using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/EnchantingMelody")]
public class EnchantingMelody : Ability
{
    [Tooltip("버프가 바뀌는 시간 간격 (초)")]
    public float buffChangeInterval = 5f;

    [Header("레벨별 공격력 버프 수치")]
    public int[] attackDamageBuffs = { 10, 15, 20, 25, 30 };

    [Header("레벨별 공격 속도 버프 수치 (예: 0.1 = 10% 감소 쿨타임)")]
    public float[] attackSpeedBuffs = { 0.1f, 0.15f, 0.2f, 0.25f, 0.3f };

    [Header("레벨별 이동 속도 버프 수치 (예: 1.0 = 1 단위 증가)")]
    public float[] movementSpeedBuffs = { 1f, 1.5f, 2f, 2.5f, 3f };

    [Header("Buff Effects")]
    [SerializeField]
    private GameObject attackDamageEffectPrefab; // 공격력 버프 이펙트 프리팹

    [SerializeField]
    private GameObject attackSpeedEffectPrefab; // 공격 속도 버프 이펙트 프리팹

    [SerializeField]
    private GameObject movementSpeedEffectPrefab; // 이동 속도 버프 이펙트 프리팹

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
                InstantiateBuffEffect(attackDamageEffectPrefab);
                break;
            case BuffType.AttackSpeed:
                lastAppliedAttackSpeedBuff = GetAttackSpeedBuff();
                playerInstance.stat.currentShootCooldown -= lastAppliedAttackSpeedBuff;
                if (playerInstance.stat.currentShootCooldown < 0.1f)
                {
                    playerInstance.stat.currentShootCooldown = 0.1f; // 최소 쿨다운 제한
                }
                InstantiateBuffEffect(attackSpeedEffectPrefab);
                break;
            case BuffType.MovementSpeed:
                lastAppliedMovementSpeedBuff = GetMovementSpeedBuff();
                playerInstance.stat.currentPlayerSpeed += lastAppliedMovementSpeedBuff;
                InstantiateBuffEffect(movementSpeedEffectPrefab);
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

    private void InstantiateBuffEffect(GameObject effectPrefab)
    {
        if (playerInstance != null && effectPrefab != null)
        {
            // 이펙트를 플레이어의 자식으로 인스턴스화
            GameObject effect = Instantiate(effectPrefab, playerInstance.transform);

            // 이펙트의 위치와 회전을 조정 (필요시)
            effect.transform.localPosition = Vector3.zero; // 중앙에 배치
            effect.transform.localRotation = Quaternion.identity; // 기본 회전

            // 이펙트가 일정 시간 후 자동으로 파괴되도록 설정 (옵션)
            Destroy(effect, 5f); // 예: 5초 후 파괴
        }
        else
        {
            Debug.LogWarning("Buff effect prefab이 할당되지 않았거나 playerInstance가 null입니다.");
        }
    }

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
