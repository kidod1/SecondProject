using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/Breath")]
public class Breath : Ability
{
    [Header("Ability Parameters")]
    public float breathDamage = 20f;        // 브레스의 피해량
    public float cooldownTime = 10f;        // 브레스의 쿨타임
    public float breathDuration = 2f;       // 브레스의 지속 시간
    public float breathRange = 10f;         // 브레스의 사거리
    public float breathAngle = 45f;         // 브레스의 각도 (양옆으로 22.5도씩)

    public GameObject breathPrefab;         // 브레스 프리팹

    private Player playerInstance;
    private Coroutine breathCoroutine;

    public override void Apply(Player player)
    {
        playerInstance = player;

        // 브레스 발사 코루틴 시작
        if (breathCoroutine == null)
        {
            breathCoroutine = playerInstance.StartCoroutine(BreathRoutine());
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        // 초기화 로직 추가: 브레스 코루틴 중지 및 변수 초기화
        if (breathCoroutine != null)
        {
            playerInstance.StopCoroutine(breathCoroutine);
            breathCoroutine = null;
        }

        // 필요에 따라 추가 초기화 작업 수행
    }

    private IEnumerator BreathRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(cooldownTime);
            FireBreath();
        }
    }

    private Vector3 GetPlayerDirection()
    {
        return playerInstance.GetFacingDirection();
    }

    private void FireBreath()
    {
        if (breathPrefab == null)
        {
            Debug.LogError("브레스 프리팹이 설정되지 않았습니다.");
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

            // 디버그 메시지 추가
            Debug.Log($"[Breath Ability] 브레스 사용됨 - 위치: {spawnPosition}, 방향: {direction}");
        }
        else
        {
            Debug.LogError("BreathAttack 스크립트를 찾을 수 없습니다.");
        }
    }

    protected override int GetNextLevelIncrease()
    {
        return Mathf.RoundToInt(breathDamage + 10f);
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            breathDamage += 10f;
        }
    }
}
