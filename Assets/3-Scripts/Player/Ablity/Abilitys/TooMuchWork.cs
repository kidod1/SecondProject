using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/TooMuchWork")]
public class TooMuchWork : Ability
{
    public float maxAttackSpeedMultiplier = 3.0f; // �ִ� ���� �ӵ� ���
    public float baseTimeToMaxSpeed = 5.0f; // �⺻������ �ִ� ���� �ӵ��� �����ϴ� �ð�
    public float overheatDuration = 5.0f; // ���� �� ���� �Ұ��� ���� ���� �ð�
    private const float minAttackCooldown = 0.15f; // �ּ� ���� �ӵ� (��ٿ�)

    private Coroutine overheatCoroutine;
    private Coroutine attackSpeedCoroutine;
    private Player playerInstance;
    private bool isOverheated = false;

    public override void Apply(Player player)
    {
        playerInstance = player;

        if (currentLevel == 0)
        {
            player.OnShoot.AddListener(HandleShooting);
            player.OnShootCanceled.AddListener(HandleShootCanceled); // ������ �߰�
        }

        // ������ ���� ȿ�� ����
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

        currentLevel++;
    }

    protected override int GetNextLevelIncrease()
    {
        return currentLevel + 1;
    }

    private void HandleShooting(Vector2 direction, int prefabIndex)
    {
        if (isOverheated) return;

        if (attackSpeedCoroutine != null)
        {
            playerInstance.StopCoroutine(attackSpeedCoroutine);
        }

        attackSpeedCoroutine = playerInstance.StartCoroutine(IncreaseAttackSpeed());
    }

    private void HandleShootCanceled()
    {
        if (attackSpeedCoroutine != null)
        {
            playerInstance.StopCoroutine(attackSpeedCoroutine);
            attackSpeedCoroutine = null;
        }

        playerInstance.stat.ShotCooldown = playerInstance.stat.defalutShotCooldown;
    }

    private IEnumerator IncreaseAttackSpeed()
    {
        float elapsedTime = 0f;
        float originalCooldown = playerInstance.stat.ShotCooldown;

        while (elapsedTime < baseTimeToMaxSpeed)
        {
            if (!playerInstance.isShooting)
            {
                playerInstance.stat.ShotCooldown = originalCooldown;
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float newCooldown = Mathf.Lerp(originalCooldown, originalCooldown / maxAttackSpeedMultiplier, elapsedTime / baseTimeToMaxSpeed);

            // ���� �ӵ��� �ּҰ��� �����ϸ� ���� ���·� ��ȯ
            if (newCooldown <= minAttackCooldown)
            {
                playerInstance.stat.ShotCooldown = minAttackCooldown;
                TriggerOverheat(); // ���� ���·� ��ȯ
                yield break;
            }

            playerInstance.stat.ShotCooldown = newCooldown;

            yield return null;
        }
    }

    private void TriggerOverheat()
    {
        if (overheatCoroutine != null)
        {
            playerInstance.StopCoroutine(overheatCoroutine);
        }
        overheatCoroutine = playerInstance.StartCoroutine(Overheat());
    }

    private IEnumerator Overheat()
    {
        isOverheated = true;
        Debug.Log("Weapon overheated! Can't attack for " + overheatDuration + " seconds.");
        playerInstance.stat.ShotCooldown = Mathf.Infinity;

        yield return new WaitForSeconds(overheatDuration);

        isOverheated = false;
        playerInstance.stat.ShotCooldown = playerInstance.stat.defalutShotCooldown;
        Debug.Log("Weapon cooled down. You can attack again.");
    }

    public override void Upgrade()
    {
        Apply(playerInstance);
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

            playerInstance.stat.ShotCooldown = playerInstance.stat.defalutShotCooldown;
        }

        isOverheated = false;
    }
}
