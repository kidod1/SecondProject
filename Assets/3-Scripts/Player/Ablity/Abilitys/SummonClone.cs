using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SummonClone")]
public class SummonClone : Ability
{
    [Tooltip("레벨별 클론 데미지 배율 (예: 0.3f = 30%)")]
    [Range(0f, 2f)]
    public float[] damageMultipliers = { 0.3f, 0.5f, 0.7f, 1.0f, 1.2f };

    public GameObject clonePrefab;
    private GameObject cloneInstance;
    private RotatingObject rotatingObject;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        if (currentLevel < damageMultipliers.Length)
        {
            if (cloneInstance == null)
            {
                cloneInstance = Instantiate(clonePrefab, player.transform.position, Quaternion.identity, player.transform);
                rotatingObject = cloneInstance.GetComponent<RotatingObject>();
                if (rotatingObject != null)
                {
                    rotatingObject.player = player.transform;
                    rotatingObject.playerShooting = player;
                    rotatingObject.damageMultiplier = damageMultipliers[currentLevel];

                    player.OnShoot.AddListener(CloneShoot);

                    Debug.Log($"SummonClone applied at Level {currentLevel + 1} with Damage Multiplier: {damageMultipliers[currentLevel] * 100}%");
                }
                else
                {
                    Debug.LogError("SummonClone: RotatingObject 컴포넌트가 클론 프리팹에 없습니다.");
                }
            }
            else
            {
                if (rotatingObject != null)
                {
                    rotatingObject.damageMultiplier = damageMultipliers[currentLevel];
                }
                else
                {
                    Debug.LogError("SummonClone: RotatingObject가 초기화되지 않았습니다.");
                }
            }
        }
    }

    /// <summary>
    /// 클론이 플레이어의 공격을 따라하게 하는 메서드.
    /// </summary>
    /// <param name="direction">발사 방향</param>
    /// <param name="prefabIndex">프리팹 인덱스</param>
    /// <param name="projectile">원본 투사체</param>
    private void CloneShoot(Vector2 direction, int prefabIndex, GameObject originalProjectile)
    {
        if (rotatingObject == null || cloneInstance == null)
        {
            Debug.LogWarning("SummonClone: 클론 인스턴스가 존재하지 않거나 RotatingObject가 설정되지 않았습니다.");
            return;
        }

        // 클론이 발사할 투사체 생성 및 초기화
        GameObject cloneProjectile = Instantiate(originalProjectile, rotatingObject.transform.position, Quaternion.identity);
        Projectile projScript = cloneProjectile.GetComponent<Projectile>();

        if (projScript != null)
        {
            // 클론의 데미지 배율 적용
            float damageMultiplier = damageMultipliers[currentLevel];
            int adjustedDamage = Mathf.RoundToInt(projScript.projectileCurrentDamage * damageMultiplier);
            projScript.Initialize(rotatingObject.playerShooting.stat, rotatingObject.playerShooting, false, 1.0f, adjustedDamage);
            projScript.SetDirection(direction);
        }
        else
        {
            Debug.LogError("SummonClone: 클론의 Projectile 스크립트를 찾을 수 없습니다.");
        }
    }
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < damageMultipliers.Length)
        {
            return Mathf.RoundToInt(damageMultipliers[currentLevel + 1] * 100); // 퍼센트로 변환
        }
        return 0;
    }

    /// <summary>
    /// 능력을 업그레이드합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
            Apply(PlayManager.I.GetPlayer());
        }
    }

    /// <summary>
    /// 능력 레벨을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        if (cloneInstance != null)
        {
            Destroy(cloneInstance);
            cloneInstance = null;
            rotatingObject = null;

            // 플레이어의 공격 이벤트에서 클론의 이벤트 해제
            Player player = PlayManager.I.GetPlayer();
            if (player != null)
            {
                player.OnShoot.RemoveListener(CloneShoot);
            }
        }
    }

    /// <summary>
    /// 능력의 현재 상태와 효과를 설명하는 문자열을 반환합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {
        if (currentLevel < damageMultipliers.Length && currentLevel >= 0)
        {
            float damageMultiplierPercent = damageMultipliers[currentLevel] * 100f;
            return $"{baseDescription}\nLv {currentLevel + 1}: 클론의 데미지 {damageMultiplierPercent}% 증가";
        }
        else if (currentLevel >= damageMultipliers.Length)
        {
            float maxDamageMultiplierPercent = damageMultipliers[damageMultipliers.Length - 1] * 100f;
            return $"{baseDescription}\n최대 레벨 도달: 클론의 데미지 {maxDamageMultiplierPercent}% 증가";
        }
        else
        {
            return $"{baseDescription}\n최대 레벨 도달.";
        }
    }
}