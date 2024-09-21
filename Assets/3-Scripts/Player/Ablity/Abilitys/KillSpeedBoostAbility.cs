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
        isBoostActive = true;
        playerInstance.stat.currentPlayerSpeed += speedBoostAmount;
        playerInstance.StartCoroutine(SpeedBoostCoroutine());
    }

    private IEnumerator SpeedBoostCoroutine()
    {
        yield return new WaitForSeconds(boostDuration);

        playerInstance.stat.currentPlayerSpeed -= speedBoostAmount;
        isBoostActive = false;
        killCount = 0;
    }

    protected override int GetNextLevelIncrease()
    {
        return 1;
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            speedBoostAmount += 0.5f;
        }
    }
}
