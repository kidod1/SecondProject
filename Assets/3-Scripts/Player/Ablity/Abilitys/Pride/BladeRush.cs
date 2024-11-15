using UnityEngine;
using System.Collections;
using AK.Wwise; // WWISE 네임스페이스 추가

[CreateAssetMenu(menuName = "Abilities/BladeRush")]
public class BladeRush : Ability
{
    [Header("BladeRush Settings")]
    [Tooltip("각 레벨에서 능력 발동 쿨다운 (초)")]
    public float[] cooldownPerLevel = { 5f, 4.5f, 4f, 3.5f, 3f }; // 레벨 1~5

    [Tooltip("각 레벨에서 칼날의 피해량")]
    public int[] damagePerLevel = { 20, 25, 30, 35, 40 }; // 레벨 1~5

    [Tooltip("각 레벨에서 칼날의 사거리")]
    public float[] rangePerLevel = { 10f, 12f, 14f, 16f, 18f }; // 레벨 1~5

    [Tooltip("칼날의 이동 속도")]
    public float bladeSpeed = 15f;

    [Tooltip("칼날의 프리팹")]
    public GameObject bladePrefab;

    // WWISE 이벤트 변수 추가
    [Header("WWISE Sound Events")]
    [Tooltip("BladeRush 능력 발동 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event activateSound;

    // 필요한 경우 추가적인 WWISE 이벤트를 선언할 수 있습니다.
    // 예: public AK.Wwise.Event upgradeSound;

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
            Debug.Log($"BladeRush 업그레이드: 현재 레벨 {currentLevel + 1}");

            // 업그레이드 시 WWISE 사운드 재생 (선택 사항)
            // activateSound.Post(playerInstance.gameObject);
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
    }

    private IEnumerator AbilityCoroutine()
    {
        while (true)
        {
            float currentCooldown = GetCurrentCooldown();
            yield return new WaitForSeconds(currentCooldown);
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
            int currentDamage = GetCurrentDamage();
            float currentRange = GetCurrentRange();

            bladeProjectile.Initialize(currentDamage, currentRange, bladeSpeed, playerInstance, facingDirection);
        }
        else
        {
            Debug.LogError("BladeRush: bladePrefab에 BladeProjectile 컴포넌트가 없습니다.");
        }

        // BladeRush 능력 발동 시 WWISE 사운드 재생
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";
        description += $"Lv {currentLevel + 1}:\n";
        description += $"- 쿨타임: {GetCurrentCooldown()}초\n";
        description += $"- 피해량: {GetCurrentDamage()}\n";
        description += $"- 사거리: {GetCurrentRange()}m";

        return description;
    }

    // 현재 레벨에 맞는 쿨타임을 반환합니다.
    private float GetCurrentCooldown()
    {
        if (currentLevel < cooldownPerLevel.Length)
        {
            return cooldownPerLevel[currentLevel];
        }
        Debug.LogWarning($"BladeRush: currentLevel ({currentLevel})이 cooldownPerLevel 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return cooldownPerLevel[cooldownPerLevel.Length - 1];
    }

    // 현재 레벨에 맞는 피해량을 반환합니다.
    private int GetCurrentDamage()
    {
        if (currentLevel < damagePerLevel.Length)
        {
            return damagePerLevel[currentLevel];
        }
        Debug.LogWarning($"BladeRush: currentLevel ({currentLevel})이 damagePerLevel 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return damagePerLevel[damagePerLevel.Length - 1];
    }

    // 현재 레벨에 맞는 사거리를 반환합니다.
    private float GetCurrentRange()
    {
        if (currentLevel < rangePerLevel.Length)
        {
            return rangePerLevel[currentLevel];
        }
        Debug.LogWarning($"BladeRush: currentLevel ({currentLevel})이 rangePerLevel 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return rangePerLevel[rangePerLevel.Length - 1];
    }

    // GetNextLevelIncrease 메서드 구현
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel && damagePerLevel.Length > currentLevel + 1)
        {
            int nextDamageIncrease = damagePerLevel[currentLevel + 1] - damagePerLevel[currentLevel];
            return nextDamageIncrease;
        }

        return 0;
    }

    private void OnValidate()
    {
        // 배열의 길이가 maxLevel과 일치하도록 조정
        if (cooldownPerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"BladeRush: cooldownPerLevel 배열의 길이가 maxLevel ({maxLevel})과 일치하지 않습니다. 배열 길이를 맞춥니다.");
            System.Array.Resize(ref cooldownPerLevel, maxLevel);
        }

        if (damagePerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"BladeRush: damagePerLevel 배열의 길이가 maxLevel ({maxLevel})과 일치하지 않습니다. 배열 길이를 맞춥니다.");
            System.Array.Resize(ref damagePerLevel, maxLevel);
        }

        if (rangePerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"BladeRush: rangePerLevel 배열의 길이가 maxLevel ({maxLevel})과 일치하지 않습니다. 배열 길이를 맞춥니다.");
            System.Array.Resize(ref rangePerLevel, maxLevel);
        }
    }
}
