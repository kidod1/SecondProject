using UnityEngine;

[CreateAssetMenu(fileName = "BossPatternData", menuName = "ScriptableObjects/BossPatternData", order = 1)]
public class BossPatternData : ScriptableObject
{
    [Header("���� Ȯ�� ����")]
    [InspectorName("ź�� ���� Ȯ��")]
    [Tooltip("ź�� ���� Ȯ��")]
    [Range(0, 100)]
    public float bulletPatternProbability = 25f;

    [InspectorName("��� �� ���� ���� Ȯ��")]
    [Tooltip("��� �� ���� ���� Ȯ��")]
    [Range(0, 100)]
    public float warningAttackPatternProbability = 25f;

    [InspectorName("��� ������ ���� Ȯ��")]
    [Tooltip("��� ������ ���� Ȯ��")]
    [Range(0, 100)]
    public float warningLaserPatternProbability = 25f;

    [InspectorName("�ٴ� ��� ���� Ȯ��")]
    [Tooltip("�ٴ� ��� ���� Ȯ��")]
    [Range(0, 100)]
    public float groundSmashPatternProbability = 25f;

    [Header("ź�� ���� ����")]
    [InspectorName("ź�� �ݺ� Ƚ��")]
    public int bulletPatternRepeatCount = 5;

    [InspectorName("źȯ ������")]
    public GameObject bulletPrefab;

    [InspectorName("ź�� ���� ����Ʈ �迭")]
    [Tooltip("źȯ�� �߻�� ���� ��ġ�� Transform �迭")]
    public Transform[] bulletSpawnPoints;

    [InspectorName("ź�� �߻� ����")]
    public float bulletFireInterval = 0.5f;

    [InspectorName("źȯ ����")]
    public float bulletLifeTime = 5f;

    [Header("��� �� ���� ���� ����")]
    [InspectorName("��� �� ���� �ݺ� Ƚ��")]
    public int warningAttackRepeatCount = 5;

    [InspectorName("��� ����Ʈ ������")]
    public GameObject warningEffectPrefab;

    [InspectorName("���� ����Ʈ ������")]
    public GameObject attackEffectPrefab;

    [InspectorName("��� ���� �ð�")]
    public float warningDuration = 2f;

    [InspectorName("���� ����Ʈ ����")]
    public float attackEffectDuration = 2f;

    [InspectorName("���� ��� ���� ������")]
    [Tooltip("���� ��� ���۵Ǳ������ ������ (�� ���� warningDuration���� ������ ��� ��Ĩ�ϴ�)")]
    public float warningStartInterval = 0.5f;


    [Header("���� ���� ����")]
    [InspectorName("��� ������ ��� ������")]
    public GameObject warningLaserWarningPrefab;

    [InspectorName("��� ������ ���� ������")]
    public GameObject warningLaserAttackPrefab;

    [InspectorName("��� ������ ��� ���� �ð�")]
    public float warningLaserWarningDuration = 2f;

    [InspectorName("��� ������ ���� ���� �ð�")]
    public float warningLaserAttackDuration = 2f;

    [InspectorName("��� ������ ���� ������")]
    public int warningLaserAttackDamage = 20;

    [Header("�ٴ� ��� ���� ����")]
    [InspectorName("�ٴ� ��� ������Ʈ ������")]
    public GameObject groundSmashMeteorPrefab;

    [InspectorName("�ٴ� ��� ��� ������")]
    public GameObject groundSmashWarningPrefab;

    [InspectorName("�ٴ� ��� ��� ���� �ð�")]
    public float groundSmashWarningDuration = 2f;

    [InspectorName("�ٴ� ��� ������Ʈ ���ط�")]
    public float groundSmashMeteorDamage = 50f;

    [InspectorName("�ٴ� ��� ������Ʈ �浹 �ݰ�")]
    public float groundSmashMeteorRadius = 2f;

    [InspectorName("�ٴ� ��� ������Ʈ ���� �ӵ�")]
    public float groundSmashMeteorFallSpeed = 10f;

    [InspectorName("�ٴ� ��� ������Ʈ ����")]
    public int groundSmashMeteorCount = 3;

    [InspectorName("�ٴ� ��� ���� �ݰ�")]
    public float groundSmashSpawnRadius = 0f;

    [InspectorName("�ٴ� ��� ��ٿ� �ð�")]
    public float groundSmashCooldown = 1f;

    [InspectorName("�ٴ� ��� źȯ ����")]
    public int groundSmashBulletCount = 12;

    [InspectorName("�ٴ� ��� źȯ �ӵ�")]
    public float groundSmashBulletSpeed = 5f;

    [InspectorName("�ٴ� ��� źȯ ������")]
    public GameObject groundSmashBulletPrefab;

    [InspectorName("�ٴ� ��� źȯ ���ط�")]
    public int groundSmashBulletDamage = 10;

    [Header("Attack Damage Values")]
    [InspectorName("źȯ ������")]
    public int bulletDamage = 10;

    [InspectorName("��� ���� ������")]
    public int warningAttackDamage = 20;
    [InspectorName("�ٴ� ��� ������")]
    public int groundSmashDamage = 15;

    [Header("Ground Smash Parameters")]
    [InspectorName("�ٴ� ��� ���ƿ��� ����")]
    public float groundSmashAngle = 270f;

    [InspectorName("�ٴ� ��� ������Ʈ ȸ�� ����")]
    public float groundSmashObjectRotation = 0f;

    [InspectorName("�ٴ� ��� ���ƿ��� �ӵ�")]
    public float groundSmashSpeed = 10f;
}
