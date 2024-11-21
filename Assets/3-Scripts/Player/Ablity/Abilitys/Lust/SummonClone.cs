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

                    // 리스너 중복 등록 방지
                    player.OnShoot.RemoveListener(CloneShoot);
                    player.OnShoot.AddListener(CloneShoot);
                }
            }
            else
            {
                if (rotatingObject != null)
                {
                    rotatingObject.damageMultiplier = damageMultipliers[currentLevel];
                }
            }
        }
    }



    private void CloneShoot(Vector2 direction, int prefabIndex, GameObject originalProjectile)
    {
        if (rotatingObject == null || cloneInstance == null)
        {
            return;
        }

        GameObject cloneProjectile = Instantiate(originalProjectile, rotatingObject.transform.position, Quaternion.identity);
        Projectile projScript = cloneProjectile.GetComponent<Projectile>();
        PlayerData data = PlayManager.I.GetPlayer().stat;

        if (projScript != null)
        {
            float damageMultiplier = damageMultipliers[currentLevel];
            int adjustedDamage = Mathf.RoundToInt(data.buffedPlayerDamage * damageMultiplier);
            projScript.Initialize(rotatingObject.playerShooting.stat, rotatingObject.playerShooting, false, 1.0f, adjustedDamage);
            projScript.SetDirection(direction);
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < damageMultipliers.Length)
        {
            return Mathf.RoundToInt(damageMultipliers[currentLevel + 1] * 100);
        }
        return 0;
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            Apply(PlayManager.I.GetPlayer());
        }
    }

    public override void ResetLevel()
    {
        if (cloneInstance != null)
        {
            Destroy(cloneInstance);
            cloneInstance = null;
            rotatingObject = null;

            Player player = PlayManager.I.GetPlayer();
            if (player != null)
            {
                player.OnShoot.RemoveListener(CloneShoot);
            }
        }
    }


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
