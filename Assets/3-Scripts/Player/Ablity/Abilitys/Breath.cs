using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(menuName = "Abilities/Breath")]
public class Breath : Ability
{
    [Header("Ability Parameters")]
    public float breathDamage = 20f;        // 브레스의 기본 피해량
    public float cooldownTime = 10f;        // 브레스의 기본 쿨타임
    public float breathDuration = 2f;       // 브레스의 지속 시간
    public float breathRange = 10f;         // 브레스의 사거리
    public float breathAngle = 45f;         // 브레스의 각도 (양옆으로 22.5도씩)

    [Tooltip("브레스 능력 업그레이드 시 레벨별 데미지 증가량")]
    public int[] damageIncrements = { 10, 15, 20, 25, 30 }; // 레벨 1~5

    public GameObject breathPrefab;         // 브레스 프리팹

    private Player playerInstance;
    private Coroutine breathCoroutine;

    /// <summary>
    /// 현재 레벨에 맞는 데미지 증가량을 반환합니다.
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < damageIncrements.Length)
        {
            return damageIncrements[currentLevel];
        }
        Debug.LogWarning($"Breath: currentLevel ({currentLevel})이 damageIncrements 배열의 범위를 벗어났습니다. 기본값 0을 반환합니다.");
        return 0;
    }

    /// <summary>
    /// 브레스 능력을 플레이어에게 적용합니다. 초기 레벨에서는 브레스 발사를 시작합니다.
    /// </summary>
    public override void Apply(Player player)
    {
        playerInstance = player;

        // 브레스 발사 코루틴 시작
        if (breathCoroutine == null)
        {
            breathCoroutine = playerInstance.StartCoroutine(BreathRoutine());
        }
    }

    /// <summary>
    /// 브레스 능력을 업그레이드합니다. 레벨이 증가할 때마다 데미지가 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            Debug.Log($"Breath 업그레이드: 현재 레벨 {currentLevel}");

            // 업그레이드 후 데미지 증가 적용
            breathDamage += GetNextLevelIncrease();
        }
        else
        {
            Debug.LogWarning("Breath: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력 설명을 오버라이드하여 레벨 업 시 데미지 증가량을 포함시킵니다.
    /// </summary>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            int damageIncrease = GetNextLevelIncrease();
            return $"{baseDescription}{System.Environment.NewLine}(Level {currentLevel + 1}: +{damageIncrease} 데미지)";
        }
        else
        {
            int finalDamageIncrease = GetNextLevelIncrease();
            return $"{baseDescription}{System.Environment.NewLine}(Max Level: +{finalDamageIncrease} 데미지)";
        }
    }

    /// <summary>
    /// 브레스 발사 코루틴입니다. 쿨타임마다 브레스를 발사합니다.
    /// </summary>
    private IEnumerator BreathRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(cooldownTime);
            FireBreath();
        }
    }

    /// <summary>
    /// 플레이어의 현재 방향을 반환합니다.
    /// </summary>
    private Vector3 GetPlayerDirection()
    {
        return playerInstance.GetFacingDirection();
    }

    /// <summary>
    /// 브레스를 발사합니다.
    /// </summary>
    private void FireBreath()
    {
        if (breathPrefab == null)
        {
            Debug.LogError("Breath 프리팹이 설정되지 않았습니다.");
            return;
        }

        Vector3 spawnPosition = playerInstance.transform.position;

        // 플레이어의 실제 방향을 가져옵니다.
        Vector2 direction = GetPlayerDirection();

        // Breath 프리팹 생성
        GameObject breath = Instantiate(breathPrefab, spawnPosition, Quaternion.identity);
        BreathAttack breathAttackScript = breath.GetComponent<BreathAttack>();

        if (breathAttackScript != null)
        {
            breathAttackScript.Initialize(breathDamage, breathRange, breathAngle, breathDuration, playerInstance);

            // Breath의 방향을 설정합니다.
            breathAttackScript.SetDirection(direction);
        }
        else
        {
            Debug.LogError("BreathAttack 스크립트를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 레벨 초기화 시 호출됩니다. 브레스 코루틴을 중지하고 변수들을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

        // 브레스 코루틴 중지 및 변수 초기화
        if (breathCoroutine != null && playerInstance != null)
        {
            playerInstance.StopCoroutine(breathCoroutine);
            breathCoroutine = null;
        }

        currentLevel = 0;
    }
    private void OnValidate()
    {
        if (damageIncrements.Length != maxLevel)
        {
            Debug.LogWarning($"Breath: damageIncrements 배열의 길이가 maxLevel ({maxLevel})과 일치하지 않습니다. 배열 길이를 맞춥니다.");
            Array.Resize(ref damageIncrements, maxLevel);
        }
    }
}