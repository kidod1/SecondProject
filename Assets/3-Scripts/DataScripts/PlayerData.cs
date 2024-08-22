using UnityEngine;

[CreateAssetMenu]
public class PlayerData : ScriptableObject
{
    public float defaultPlayerSpeed = 5;
    public int defaultPlayerDamage = 5;
    public float defaultProjectileSpeed = 10;
    public float defaultProjectileRange = 2;
    public int defaultProjectileType = 0;
    public int defaultMaxHP = 100;
    public int defaultShield = 0;
    public float defalutShotCooldown = 0.5f;
    public int defalutDefense = 0;

    public float playerSpeed;
    public int playerDamage;
    public float knockbackDuration;
    public float knockbackSpeed;
    public float projectileSpeed;
    public float projectileRange;
    public int projectileType;
    public int maxHP;
    public int currentShield;
    public float shotCooldown;
    public int defense;

    [Tooltip("경험치 테이블")]
    public int[] experienceThresholds = { 100, 200, 400, 800, 1600 };

    [Tooltip("경험치 획득량 배수 (기본값 1.0 = 100%)")]
    public float experienceMultiplier;

    public float defalutExperienceMultiplier = 1.0f;

    public void InitializeStats()
    {
        playerSpeed = defaultPlayerSpeed;
        playerDamage = defaultPlayerDamage;
        projectileSpeed = defaultProjectileSpeed;
        projectileRange = defaultProjectileRange;
        projectileType = defaultProjectileType;
        shotCooldown = defalutShotCooldown;
        defense = defalutDefense;
        maxHP = defaultMaxHP;
        experienceMultiplier = defalutExperienceMultiplier;
    }
}