using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/TooMuchWork")]
public class TooMuchWork : Ability
{
    public float maxAttackSpeedMultiplier = 3.0f; // 최대 공격 속도 배수
    public float baseTimeToMaxSpeed = 5.0f; // 기본적으로 최대 공격 속도에 도달하는 시간
    public float overheatDuration = 5.0f; // 과열 시 공격 불가능 상태 지속 시간
    private const float minAttackCooldown = 0.15f; // 최소 공격 속도 (쿨다운)

    private Coroutine overheatCoroutine;
    private Coroutine attackSpeedCoroutine;
    private Player playerInstance;
    private bool isOverheated = false;

    public override void Apply(Player player)
    {
        playerInstance = player;

        if (playerInstance == null)
        {
            Debug.LogError("Player instance is null. Cannot apply TooMuchWork ability.");
            return;
        }

        if (currentLevel == 0)
        {
            player.OnShoot.AddListener(HandleShooting);
            player.OnShootCanceled.AddListener(HandleShootCanceled); // 리스너 추가
        }

        // 레벨에 따른 효과 적용
        switch (currentLevel)
        {
            case 1:
                baseTimeToMaxSpeed = 4.0f;
                break;
            case 2:
                overheatDuration = 3.0f;
                break;
            case 3:
                baseTimeToMaxSpeed = 3.0f;
                break;
        }
    }

    protected override int GetNextLevelIncrease()
    {
        return currentLevel < maxLevel ? currentLevel + 1 : 0;
    }

    private void HandleShooting(Vector2 direction, int prefabIndex)
    {
        if (isOverheated || playerInstance == null) return;

        if (attackSpeedCoroutine != null)
        {
            playerInstance.StopCoroutine(attackSpeedCoroutine);
        }

        attackSpeedCoroutine = playerInstance.StartCoroutine(IncreaseAttackSpeed());
    }

    private void HandleShootCanceled()
    {
        if (attackSpeedCoroutine != null && playerInstance != null)
        {
            playerInstance.StopCoroutine(attackSpeedCoroutine);
            attackSpeedCoroutine = null;
        }

        if (playerInstance != null)
        {
            playerInstance.stat.currentShotCooldown = playerInstance.stat.defalutShotCooldown;
        }
    }

    private IEnumerator IncreaseAttackSpeed()
    {
        if (playerInstance == null)
        {
            yield break;
        }

        float elapsedTime = 0f;
        float originalCooldown = playerInstance.stat.currentShotCooldown;

        while (elapsedTime < baseTimeToMaxSpeed)
        {
            if (!playerInstance.isShooting)
            {
                playerInstance.stat.currentShotCooldown = originalCooldown;
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float newCooldown = Mathf.Lerp(originalCooldown, originalCooldown / maxAttackSpeedMultiplier, elapsedTime / baseTimeToMaxSpeed);

            // 공격 속도가 최소값에 도달하면 과열 상태로 전환
            if (newCooldown <= minAttackCooldown)
            {
                playerInstance.stat.currentShotCooldown = minAttackCooldown;
                TriggerOverheat(); // 과열 상태로 전환
                yield break;
            }

            playerInstance.stat.currentShotCooldown = newCooldown;

            yield return null;
        }
    }

    private void TriggerOverheat()
    {
        if (playerInstance == null)
        {
            Debug.LogError("Player instance is null. Cannot trigger overheat.");
            return;
        }

        if (overheatCoroutine != null)
        {
            playerInstance.StopCoroutine(overheatCoroutine);
        }
        overheatCoroutine = playerInstance.StartCoroutine(Overheat());
    }

    private IEnumerator Overheat()
    {
        if (playerInstance == null)
        {
            yield break;
        }

        isOverheated = true;
        Debug.Log("Weapon overheated! Can't attack for " + overheatDuration + " seconds.");
        playerInstance.stat.currentShotCooldown = Mathf.Infinity;

        yield return new WaitForSeconds(overheatDuration);

        if (playerInstance != null)
        {
            isOverheated = false;
            playerInstance.stat.currentShotCooldown = playerInstance.stat.defalutShotCooldown;
            Debug.Log("Weapon cooled down. You can attack again.");
        }
    }

    public override void Upgrade()
    {
        currentLevel++; // 레벨을 증가시킵니다.
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        if (playerInstance != null)
        {
            if (overheatCoroutine != null)
            {
                playerInstance.StopCoroutine(overheatCoroutine);
                overheatCoroutine = null;
            }
            if (attackSpeedCoroutine != null)
            {
                playerInstance.StopCoroutine(attackSpeedCoroutine);
                attackSpeedCoroutine = null;
            }

            playerInstance.stat.currentShotCooldown = playerInstance.stat.defalutShotCooldown;
        }

        isOverheated = false;
    }
}
