using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/BladeRush")]
public class BladeRush : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("능력 발동 쿨타임 (초)")]
    public float cooldown = 5f;

    [Tooltip("칼날의 피해량")]
    public int damage = 20;

    [Tooltip("칼날의 사거리")]
    public float range = 10f;

    [Tooltip("칼날의 이동 속도")]
    public float bladeSpeed = 15f;

    [Tooltip("칼날의 프리팹")]
    public GameObject bladePrefab;

    private Player playerInstance;
    private Coroutine abilityCoroutine;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("BladeRush Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;

        if (abilityCoroutine == null)
        {
            abilityCoroutine = player.StartCoroutine(AbilityCoroutine());
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
            damage += 10;
            range += 2f;
        }
        else
        {
            Debug.LogWarning("BladeRush: 이미 최대 레벨에 도달했습니다.");
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        if (abilityCoroutine != null)
        {
            playerInstance.StopCoroutine(abilityCoroutine);
            abilityCoroutine = null;
        }

        currentLevel = 0;
        damage = 20;
        range = 10f;
    }

    private IEnumerator AbilityCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(cooldown);
            FireBlade();
        }
    }

    private void FireBlade()
    {
        if (bladePrefab == null)
        {
            Debug.LogError("BladeRush: bladePrefab이 할당되지 않았습니다.");
            return;
        }

        Vector3 spawnPosition = playerInstance.transform.position;

        // 플레이어의 바라보는 방향을 가져옵니다.
        Vector2 facingDirection = playerInstance.GetFacingDirection();

        // 칼날의 회전을 방향에 맞게 설정합니다.
        float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, angle - 90);

        GameObject bladeInstance = Instantiate(bladePrefab, spawnPosition, spawnRotation);
        BladeProjectile bladeProjectile = bladeInstance.GetComponent<BladeProjectile>();

        if (bladeProjectile != null)
        {
            bladeProjectile.Initialize(damage, range, bladeSpeed, playerInstance, facingDirection);
        }
        else
        {
            Debug.LogError("BladeRush: bladePrefab에 BladeProjectile 컴포넌트가 없습니다.");
        }
    }

    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";
        description += $"쿨타임: {cooldown}초\n";
        description += $"피해량: {damage}\n";
        description += $"사거리: {range}";

        return description;
    }

    // GetNextLevelIncrease 메서드 구현
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel)
        {
            int nextDamageIncrease = 10;  // 다음 레벨에서 추가될 피해량
            return nextDamageIncrease;
        }

        return 0;
    }
}
