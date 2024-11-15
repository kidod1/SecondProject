using UnityEngine;

[CreateAssetMenu(fileName = "LustBossPatternData", menuName = "ScriptableObjects/LustBossPatternData", order = 1)]
public class LustBossPatternData : ScriptableObject
{
    [Header("Circle Bullet Pattern Settings")]
    public int circlePatternRepeatCount = 4;
    public GameObject circleBulletPrefab;
    public int circleBulletCount = 20;
    public float circleBulletSpeed = 5f;

    [Header("Heart Bullet Pattern Settings")]
    public int heartPatternRepeatCount = 3;
    public GameObject heartBulletPrefab;
    public float heartBulletSpeed = 3f;

    [Header("Angle Bullet Pattern Settings")]
    public int anglePatternRepeatCount = 3;
    public GameObject angleBulletPrefab;
    public int angleBulletCount = 5;
    public float angleBulletSpeed = 6f;

    [Header("Additional Pattern Settings")]
    public int spawnExplosionPatternRepeatCount = 3;
    public GameObject spawnExplosionPrefab;
    public int spawnExplosionBulletCount = 10;
    public float spawnExplosionBulletSpeed = 7f;
}
