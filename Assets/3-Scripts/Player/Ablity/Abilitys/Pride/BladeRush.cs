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

    [Tooltip("각 레벨별 피해량 증가치")]
    public int[] damageIncreases;

    [Tooltip("각 레벨별 사거리 증가치")]
    public float[] rangeIncreases;

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

            // 레벨별 증가량이 배열에 정의되어 있는지 확인
            if (damageIncreases != null && damageIncreases.Length > currentLevel - 1)
            {
                damage += damageIncreases[currentLevel - 1];
            }
            else
            {
                Debug.LogWarning($"BladeRush: damageIncreases 배열에 레벨 {currentLevel}의 증가치가 정의되어 있지 않습니다.");
            }

            if (rangeIncreases != null && rangeIncreases.Length > currentLevel - 1)
            {
                range += rangeIncreases[currentLevel - 1];
            }
            else
            {
                Debug.LogWarning($"BladeRush: rangeIncreases 배열에 레벨 {currentLevel}의 증가치가 정의되어 있지 않습니다.");
            }
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
        if (currentLevel + 1 < maxLevel && damageIncreases != null && damageIncreases.Length > currentLevel)
        {
            int nextDamageIncrease = damageIncreases[currentLevel];
            return nextDamageIncrease;
        }

        return 0;
    }
}
