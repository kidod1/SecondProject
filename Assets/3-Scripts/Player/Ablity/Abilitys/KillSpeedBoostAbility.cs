using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/KillSpeedBoost")]
public class KillSpeedBoostAbility : Ability
{
    public int killThreshold = 10;
    public float speedBoostAmount = 2f; 
    public float boostDuration = 5f;

    private int killCount = 0;
    private bool isBoostActive = false;

    private Player playerInstance;

    public override void Apply(Player player)
    {
        playerInstance = player;
        player.OnMonsterKilled.AddListener(OnMonsterKilled);
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        if (playerInstance != null)
        {
            playerInstance.OnMonsterKilled.RemoveListener(OnMonsterKilled);
        }
        playerInstance = null;
        killCount = 0;
        isBoostActive = false;
    }

    private void OnMonsterKilled()
    {
        killCount++;
        if (killCount >= killThreshold && !isBoostActive)
        {
            ActivateSpeedBoost();
        }
    }

    private void ActivateSpeedBoost()
    {
        // 속도 증가
        isBoostActive = true;
        playerInstance.stat.currentPlayerSpeed += speedBoostAmount;

        // 5초 후에 속도를 원래대로 복구
        playerInstance.StartCoroutine(SpeedBoostCoroutine());
    }

    private IEnumerator SpeedBoostCoroutine()
    {
        yield return new WaitForSeconds(boostDuration);

        // 이동 속도를 원래대로 복구
        playerInstance.stat.currentPlayerSpeed -= speedBoostAmount;
        isBoostActive = false;
        killCount = 0;  // 처치 수 초기화
    }

    // GetNextLevelIncrease() 구현
    protected override int GetNextLevelIncrease()
    {
        return 1; // 레벨이 오를 때 증가할 양을 반환 (필요에 따라 수정 가능)
    }

    // Upgrade() 구현
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            speedBoostAmount += 0.5f;
        }
    }
}
