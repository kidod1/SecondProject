using UnityEngine;

[CreateAssetMenu(fileName = "LustBossPatternData", menuName = "ScriptableObjects/LustBossPatternData", order = 1)]
public class LustBossPatternData : ScriptableObject
{
    [Header("Circle Bullet Pattern Settings")]
    public int circlePatternRepeatCount = 4;
    public GameObject circleBulletPrefab;
    public int circleBulletCount = 20;
    public float circleBulletSpeed = 5f;
    public float circlePatternProbability = 0.25f; // ���� źȯ ���� Ȯ�� (25%)

    [Header("Heart Bullet Pattern Settings")]
    public int heartPatternRepeatCount = 3;
    public GameObject heartBulletPrefab;
    public float heartBulletSpeed = 3f;
    public float heartPatternProbability = 0.25f; // ��Ʈ źȯ ���� Ȯ�� (25%)

    [Header("Angle Bullet Pattern Settings")]
    public int anglePatternRepeatCount = 3;
    public GameObject angleBulletPrefab;
    public int angleBulletCount = 5;
    public float angleBulletSpeed = 6f;
    public float anglePatternProbability = 0.25f; // ���� źȯ ���� Ȯ�� (25%)

    [Header("Additional Pattern Settings")]
    public int spawnExplosionPatternRepeatCount = 3;
    public GameObject spawnExplosionPrefab;
    public int spawnExplosionBulletCount = 10;
    public float spawnExplosionBulletSpeed = 7f;
    public float spawnExplosionPatternProbability = 0.25f; // ���� �� ���� ���� Ȯ�� (25%)

    [Header("Specified Direction Pattern Settings")]

    [SerializeField, Tooltip("������ ���� ������ źȯ ������")]
    public GameObject specifiedPatternBulletPrefab;

    [SerializeField, Tooltip("������ ���� ������ źȯ �ӵ�")]
    public float specifiedPatternBulletSpeed = 5f;

    [SerializeField, Tooltip("������ ���� ������ �߻� ����")]
    public float specifiedPatternFireInterval = 0.35f;

    [SerializeField, Tooltip("������ ���� ������ ���� �ð�")]
    public float specifiedPatternDuration = 5f;


    public float specifiedPatternProbability = 0.25f; // ������ ���� ���� Ȯ�� (��: 25%)
}
