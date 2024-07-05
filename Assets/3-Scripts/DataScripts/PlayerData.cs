using UnityEngine;

[CreateAssetMenu]
public class PlayerData : ScriptableObject
{
    public float defaultPlayerSpeed = 5;
    public int defaultPlayerDamage = 5;
    public float defaultKnockbackDuration = 0.1f;
    public float defaultKnockbackSpeed = 5.0f;
    public float defaultProjectileSpeed = 10;
    public float defaultProjectileRange = 2;
    public int defaultProjectileType = 0;
    public int defaultMaxHP = 10;
    public int defaultShield = 0;

    public float playerSpeed;
    public int playerDamage;
    public float knockbackDuration;
    public float knockbackSpeed;
    public float projectileSpeed;
    public float projectileRange;
    public int projectileType;
    public int maxHP;
    public int currentShield;
    public void InitializeStats()
    {
        playerSpeed = defaultPlayerSpeed;
        playerDamage = defaultPlayerDamage;
        knockbackDuration = defaultKnockbackDuration;
        knockbackSpeed = defaultKnockbackSpeed;
        projectileSpeed = defaultProjectileSpeed;
        projectileRange = defaultProjectileRange;
        projectileType = defaultProjectileType;
        maxHP = defaultMaxHP;
    }
}