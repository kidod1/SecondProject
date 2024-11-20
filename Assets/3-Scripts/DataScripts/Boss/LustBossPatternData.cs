using UnityEngine;

[CreateAssetMenu(fileName = "LustBossPatternData", menuName = "ScriptableObjects/LustBossPatternData", order = 1)]
public class LustBossPatternData : ScriptableObject
{
    [Header("Circle Bullet Pattern Settings")]
    public int circlePatternRepeatCount = 4;
    public GameObject circleBulletPrefab;
    public int circleBulletCount = 20;
    public float circleBulletSpeed = 5f;
    public float circlePatternProbability = 0.25f; // 원형 탄환 패턴 확률 (25%)

    [Header("Heart Bullet Pattern Settings")]
    public int heartPatternRepeatCount = 3;
    public GameObject heartBulletPrefab;
    public float heartBulletSpeed = 3f;
    public float heartPatternProbability = 0.25f; // 하트 탄환 패턴 확률 (25%)

    [Header("Angle Bullet Pattern Settings")]
    public int anglePatternRepeatCount = 3;
    public GameObject angleBulletPrefab;
    public int angleBulletCount = 5;
    public float angleBulletSpeed = 6f;
    public float anglePatternProbability = 0.25f; // 각도 탄환 패턴 확률 (25%)

    [Header("Additional Pattern Settings")]
    public int spawnExplosionPatternRepeatCount = 3;
    public GameObject spawnExplosionPrefab;
    public int spawnExplosionBulletCount = 10;
    public float spawnExplosionBulletSpeed = 7f;
    public float spawnExplosionPatternProbability = 0.25f; // 스폰 후 폭발 패턴 확률 (25%)

    [Header("Specified Direction Pattern Settings")]

    [SerializeField, Tooltip("지정된 방향 패턴의 탄환 프리팹")]
    public GameObject specifiedPatternBulletPrefab;

    [SerializeField, Tooltip("지정된 방향 패턴의 탄환 속도")]
    public float specifiedPatternBulletSpeed = 5f;

    [SerializeField, Tooltip("지정된 방향 패턴의 발사 간격")]
    public float specifiedPatternFireInterval = 0.35f;

    [SerializeField, Tooltip("지정된 방향 패턴의 실행 시간")]
    public float specifiedPatternDuration = 5f;


    public float specifiedPatternProbability = 0.25f; // 지정된 방향 패턴 확률 (예: 25%)
}
