using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/KillSpeedBoost")]
public class KillSpeedBoostAbility : Ability
{
    [Tooltip("레벨별 킬 스피드 증가량")]
    public float[] speedBoostAmounts; // 레벨별 이동 속도 증가량 배열

    public int killThreshold = 10; // 킬 임계치
    public float boostDuration = 5f; // 부스트 지속 시간

    private int killCount = 0; // 현재 킬 수
    private bool isBoostActive = false; // 부스트 활성화 여부

    private Player playerInstance; // 능력이 적용된 플레이어 인스턴스

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("KillSpeedBoostAbility Apply: 플레이어 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;
        Debug.Log($"KillSpeedBoostAbility이(가) 플레이어에게 적용되었습니다. 현재 레벨: {currentLevel + 1}");
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        playerInstance = null;
        killCount = 0;
        isBoostActive = false;
        Debug.Log("KillSpeedBoostAbility 레벨이 초기화되었습니다.");
    }

    /// <summary>
    /// 몬스터를 처치했을 때 호출되는 메서드입니다.
    /// </summary>
    public void OnMonsterKilled()
    {
        killCount++;

        if (killCount >= killThreshold && !isBoostActive)
        {
            ActivateSpeedBoost();
        }
    }

    /// <summary>
    /// 이동 속도 부스트를 활성화합니다.
    /// </summary>
    private void ActivateSpeedBoost()
    {
        if (currentLevel >= speedBoostAmounts.Length)
        {
            Debug.LogWarning("KillSpeedBoostAbility: 현재 레벨이 speedBoostAmounts 배열의 범위를 벗어났습니다.");
            return;
        }

        isBoostActive = true;
        float boostAmount = speedBoostAmounts[currentLevel];
        playerInstance.stat.currentPlayerSpeed += boostAmount;

        playerInstance.StartCoroutine(SpeedBoostCoroutine(boostAmount));
    }

    /// <summary>
    /// 이동 속도 부스트의 지속 시간을 관리하는 코루틴입니다.
    /// </summary>
    /// <param name="boostAmount">부스트된 이동 속도 값</param>
    /// <returns></returns>
    private IEnumerator SpeedBoostCoroutine(float boostAmount)
    {
        yield return new WaitForSeconds(boostDuration);

        playerInstance.stat.currentPlayerSpeed -= boostAmount;
        isBoostActive = false;
        killCount = 0;
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 이동 속도 증가량이 설정된 배열에 따라 변경됩니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel이 5라면 currentLevel은 0~4
        {
            currentLevel++;
        }
        else
        {
            Debug.LogWarning("KillSpeedBoostAbility: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 다음 레벨의 이동 속도 증가값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨에서의 이동 속도 증가량 (퍼센트)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < speedBoostAmounts.Length)
        {
            return Mathf.RoundToInt(speedBoostAmounts[currentLevel + 1] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    /// <summary>
    /// 능력의 현재 상태와 효과를 설명하는 문자열을 반환합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel && currentLevel < speedBoostAmounts.Length)
        {
            float boostAmount = speedBoostAmounts[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: 이동 속도 +{boostAmount} 단위 (킬 {killThreshold}회 달성 시)";
        }
        else if (currentLevel == maxLevel && currentLevel < speedBoostAmounts.Length)
        {
            float boostAmount = speedBoostAmounts[currentLevel];
            return $"{baseDescription}\n최대 레벨 도달: 이동 속도 +{boostAmount} 단위 (킬 {killThreshold}회 달성 시)";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달";
        }
    }
}
