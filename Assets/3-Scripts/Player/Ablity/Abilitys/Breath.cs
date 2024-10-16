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
            return $"{baseDescription}\n" +
                   $"현재 레벨: {currentLevel + 1}\n" +
                   $"브레스 공격 데미지 증가: +{damageIncrease}\n" +
                   $"브레스 지속 시간: {breathDuration}초\n" +
                   $"브레스 쿨타임: {cooldownTime}초";
        }
        else
        {
            int finalDamageIncrease = GetNextLevelIncrease();
            return $"{baseDescription}\n" +
                   $"Max Level: {currentLevel + 1}\n" +
                   $"브레스 공격 데미지 증가: +{finalDamageIncrease}\n" +
                   $"브레스 공격 범위: {breathRange}m\n" +
                   $"브레스 공격 각도: {breathAngle}도\n" +
                   $"브레스 지속 시간: {breathDuration}초\n" +
                   $"브레스 쿨타임: {cooldownTime}초";
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

        if (direction == Vector2.zero)
        {
            Debug.LogWarning("Breath: 플레이어의 방향이 설정되지 않았습니다.");
            return;
        }

        // 방향 벡터에서 각도 계산 (오른쪽을 기준으로 시계 반대 방향)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // ParticleSystem의 초기 방향이 위쪽(90도)이므로, 이를 보정하기 위해 -90도 추가
        float rotationOffset = -90f;

        // 최종 각도 계산
        float totalAngleDegrees = angle + rotationOffset;
        float totalAngleRadians = totalAngleDegrees * Mathf.Deg2Rad;

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

        // 자식 ParticleSystem의 Start Rotation 조정
        ParticleSystem ps = breath.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startRotation = totalAngleRadians; // ParticleSystem은 라디안 단위로 회전
        }
        else
        {
            Debug.LogWarning("Breath 프리팹의 자식에 ParticleSystem이 없습니다.");
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
