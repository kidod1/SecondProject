using UnityEngine;

[CreateAssetMenu(fileName = "LustBossPatternData", menuName = "ScriptableObjects/LustBossPatternData", order = 1)]
public class LustBossPatternData : ScriptableObject
{
    [Header("Circle Bullet Pattern Settings")]
    public int circlePatternRepeatCount = 4;
    public GameObject circleBulletPrefab;
    public int circleBulletCount = 20;
    public float circleBulletSpeed = 5f;
    public float circlePatternProbability = 0.25f; // ¿øÇü ÅºÈ¯ ÆÐÅÏ È®·ü (25%)

    [Header("Heart Bullet Pattern Settings")]
    public int heartPatternRepeatCount = 3;
    public GameObject heartBulletPrefab;
    public float heartBulletSpeed = 3f;
    public float heartPatternProbability = 0.25f; // ÇÏÆ® ÅºÈ¯ ÆÐÅÏ È®·ü (25%)

    [Header("Angle Bullet Pattern Settings")]
    public int anglePatternRepeatCount = 3;
    public GameObject angleBulletPrefab;
    public int angleBulletCount = 5;
    public float angleBulletSpeed = 6f;
    public float anglePatternProbability = 0.25f; // °¢µµ ÅºÈ¯ ÆÐÅÏ È®·ü (25%)

    [Header("Additional Pattern Settings")]
    public int spawnExplosionPatternRepeatCount = 3;
    public GameObject spawnExplosionPrefab;
    public int spawnExplosionBulletCount = 10;
    public float spawnExplosionBulletSpeed = 7f;
    public float spawnExplosionPatternProbability = 0.25f; // ½ºÆù ÈÄ Æø¹ß ÆÐÅÏ È®·ü (25%)
}
