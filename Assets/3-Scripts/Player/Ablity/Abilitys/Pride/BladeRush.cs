using UnityEngine;
using System.Collections;
using AK.Wwise; // WWISE 네임스페이스 추가

[CreateAssetMenu(menuName = "Abilities/BladeRush")]
public class BladeRush : Ability
{
    [Header("BladeRush Settings")]
    [Tooltip("레벨별 능력 발동 쿨다운 (초)")]
    public float[] cooldownPerLevel = { 5f, 4.5f, 4f, 3.5f, 3f }; // 레벨 1~5

    [Tooltip("레벨별 칼날의 피해량")]
    public int[] damagePerLevel = { 20, 25, 30, 35, 40 }; // 레벨 1~5

    [Tooltip("레벨별 칼날의 사거리")]
    public float[] rangePerLevel = { 10f, 12f, 14f, 16f, 18f }; // 레벨 1~5

    [Tooltip("칼날의 이동 속도")]
    public float bladeSpeed = 15f;

    [Tooltip("칼날의 프리팹")]
    public GameObject bladePrefab;

    // WWISE 이벤트 변수 추가
    [Header("WWISE Sound Events")]
    [Tooltip("BladeRush 능력 발동 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event activateSound;

    [Tooltip("BladeRush 업그레이드 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event upgradeSound;

    [Tooltip("BladeRush 제거 시 재생될 WWISE 이벤트")]
    public AK.Wwise.Event deactivateSound;

    private Player playerInstance;
    private Coroutine abilityCoroutine;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
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

        // BladeRush 능력 발동 시 WWISE 사운드 재생
        if (activateSound != null)
        {
            activateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 데미지와 사거리를 증가시킵니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            Debug.Log($"BladeRush 업그레이드: 현재 레벨 {currentLevel}");

            // 현재 레벨에 맞는 데미지와 사거리를 적용합니다.
            UpdateBladeRushParameters();

            // 업그레이드 시 WWISE 사운드 재생
            if (upgradeSound != null)
            {
                upgradeSound.Post(playerInstance.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("BladeRush: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

        if (abilityCoroutine != null)
        {
            playerInstance.StopCoroutine(abilityCoroutine);
            abilityCoroutine = null;
        }

        currentLevel = 0;

        // BladeRush 제거 시 WWISE 사운드 재생
        if (deactivateSound != null && playerInstance != null)
        {
            deactivateSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// BladeRush의 파라미터를 현재 레벨에 맞게 업데이트합니다.
    /// </summary>
    private void UpdateBladeRushParameters()
    {
        // 현재 레벨에 맞는 데미지, 쿨타임, 사거리를 적용합니다.
        // 예: 필요 시 다른 파라미터도 업데이트할 수 있습니다.
    }

    /// <summary>
    /// BladeRush 발동 코루틴입니다. 쿨타임마다 칼날을 발사합니다.
    /// </summary>
    private IEnumerator AbilityCoroutine()
    {
        while (true)
        {
            float currentCooldown = GetCurrentCooldown();
            yield return new WaitForSeconds(currentCooldown);
            FireBlade();
        }
    }

    /// <summary>
    /// 칼날을 발사합니다.
    /// </summary>
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

        if (facingDirection == Vector2.zero)
        {
            Debug.LogWarning("BladeRush: 플레이어의 방향이 설정되지 않았습니다.");
            return;
        }

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

    /// <summary>
    /// 능력의 설명을 반환합니다. 설명은 현재 레벨보다 1레벨 더 높은 레벨의 정보를 포함합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            // 다음 레벨의 인덱스는 currentLevel (currentLevel은 0부터 시작)
            int nextLevelIndex = currentLevel;
            float nextLevelCooldown = (nextLevelIndex < cooldownPerLevel.Length) ? cooldownPerLevel[nextLevelIndex] : cooldownPerLevel[cooldownPerLevel.Length - 1];
            int nextLevelDamage = (nextLevelIndex < damagePerLevel.Length) ? damagePerLevel[nextLevelIndex] : damagePerLevel[damagePerLevel.Length - 1];
            float nextLevelRange = (nextLevelIndex < rangePerLevel.Length) ? rangePerLevel[nextLevelIndex] : rangePerLevel[rangePerLevel.Length - 1];

            return $"{baseDescription}\n" +
                   $"Lv {currentLevel + 1}:\n" +
                   $"- 쿨타임: {nextLevelCooldown}초\n" +
                   $"- 피해량: {nextLevelDamage}\n" +
                   $"- 사거리: {nextLevelRange}m\n";
        }
        else
        {
            // 최대 레벨 설명
            int maxLevelIndex = currentLevel - 1;
            float finalCooldown = (maxLevelIndex < cooldownPerLevel.Length) ? cooldownPerLevel[maxLevelIndex] : cooldownPerLevel[cooldownPerLevel.Length - 1];
            int finalDamage = (maxLevelIndex < damagePerLevel.Length) ? damagePerLevel[maxLevelIndex] : damagePerLevel[damagePerLevel.Length - 1];
            float finalRange = (maxLevelIndex < rangePerLevel.Length) ? rangePerLevel[maxLevelIndex] : rangePerLevel[rangePerLevel.Length - 1];

            return $"{baseDescription}\n" +
                   $"Max Level: {currentLevel}\n" +
                   $"- 쿨타임: {finalCooldown}초\n" +
                   $"- 피해량: {finalDamage}\n" +
                   $"- 사거리: {finalRange}m\n";
        }
    }

    /// <summary>
    /// 현재 레벨에 맞는 쿨타임을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 쿨타임</returns>
    private float GetCurrentCooldown()
    {
        if (currentLevel == 0)
        {
            return cooldownPerLevel[0];
        }
        else if (currentLevel - 1 < cooldownPerLevel.Length)
        {
            return cooldownPerLevel[currentLevel - 1];
        }

        Debug.LogWarning($"BladeRush: currentLevel ({currentLevel})이 cooldownPerLevel 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return cooldownPerLevel[cooldownPerLevel.Length - 1];
    }

    /// <summary>
    /// 현재 레벨에 맞는 피해량을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 피해량</returns>
    private int GetCurrentDamage()
    {
        if (currentLevel == 0)
        {
            return damagePerLevel[currentLevel];
        }
        if (currentLevel < damagePerLevel.Length)
        {
            return damagePerLevel[currentLevel - 1];
        }
        Debug.LogWarning($"BladeRush: currentLevel ({currentLevel})이 damagePerLevel 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return damagePerLevel[damagePerLevel.Length - 1];
    }

    /// <summary>
    /// 현재 레벨에 맞는 사거리를 반환합니다.
    /// </summary>
    /// <returns>현재 레벨의 사거리</returns>
    private float GetCurrentRange()
    {
        if (currentLevel < rangePerLevel.Length)
        {
            return rangePerLevel[currentLevel];
        }
        Debug.LogWarning($"BladeRush: currentLevel ({currentLevel})이 rangePerLevel 배열의 범위를 벗어났습니다. 마지막 값을 반환합니다.");
        return rangePerLevel[rangePerLevel.Length - 1];
    }

    /// <summary>
    /// 다음 레벨 증가에 필요한 값을 반환합니다.
    /// (이 메서드는 더 이상 사용되지 않으므로 제거할 수 있습니다.)
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        // 필요에 따라 구현하거나 제거
        return 0;
    }

    /// <summary>
    /// Gizmos를 사용하여 BladeRush 발사 방향 시각화
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (playerInstance != null)
        {
            Vector2 facingDirection = playerInstance.GetFacingDirection();
            Vector2 backwardDirection = -facingDirection;

            Vector3 origin = playerInstance.transform.position;
            Vector3 direction = backwardDirection * 5f; // 예시: 5 단위 길이

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + (Vector3)direction);
            Gizmos.DrawSphere(origin + (Vector3)direction, 0.2f);
        }
    }

    /// <summary>
    /// OnValidate 메서드를 통해 배열의 길이를 maxLevel과 일치시킵니다.
    /// </summary>
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
