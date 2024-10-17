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

    [InspectorName("������ ���� Ȯ��")]
    [Tooltip("������ ���� Ȯ��")]
    [Range(0, 100)]
    public float laserPatternProbability = 25f;

    [InspectorName("�ٴ� ��� ���� Ȯ��")]
    [Tooltip("�ٴ� ��� ���� Ȯ��")]
    [Range(0, 100)]
    public float groundSmashPatternProbability = 25f;

    [Header("ź�� ���� ����")]
    [InspectorName("ź�� �ݺ� Ƚ��")]
    public int bulletPatternRepeatCount = 5;

    [InspectorName("źȯ ������")]
    public GameObject bulletPrefab;

    [InspectorName("źȯ ���� ����Ʈ")]
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

    [Header("������ ���� ����")]
    [InspectorName("������ ������")]
    public GameObject laserPrefab;

    [InspectorName("������ ��� ������")]
    public GameObject laserWarningPrefab;

    [InspectorName("������ ��� ���� �ð�")]
    public float laserWarningDuration = 2f;

    [InspectorName("������ ���� �ð�")]
    public float laserDuration = 5f;

    [Header("�ٴ� ��� ���� ����")]
    [InspectorName("�ٴ� ��� ������Ʈ ������")]
    public GameObject groundSmashMeteorPrefab; // �ٴ� ��� ������Ʈ ������

    [InspectorName("�ٴ� ��� ��� ������")]
    public GameObject groundSmashWarningPrefab; // �ٴ� ��� ��� ������

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
    public float groundSmashSpawnRadius = 0f; // ������ ��ġ���� ����

    [InspectorName("�ٴ� ��� ��ٿ� �ð�")]
    public float groundSmashCooldown = 1f;

    [InspectorName("�ٴ� ��� źȯ ����")]
    public int groundSmashBulletCount = 12; // ��: 12�� źȯ

    [InspectorName("�ٴ� ��� źȯ �ӵ�")]
    public float groundSmashBulletSpeed = 5f;

    [InspectorName("�ٴ� ��� źȯ ������")]
    public GameObject groundSmashBulletPrefab; // źȯ ������

    [InspectorName("�ٴ� ��� źȯ ���ط�")]
    public int groundSmashBulletDamage = 10;

    [Header("Attack Damage Values")]
    [InspectorName("źȯ ������")]
    public int bulletDamage = 10;

    [InspectorName("��� ���� ������")]
    public int warningAttackDamage = 20;

    [InspectorName("������ �ʴ� ������")]
    public int laserDamagePerSecond = 5;

    [InspectorName("�ٴ� ��� ������")]
    public int groundSmashDamage = 15;

    [Header("Ground Smash Parameters")]
    [InspectorName("�ٴ� ��� ���ƿ��� ����")]
    public float groundSmashAngle = 270f; // ���ƿ��� ���� (��: 270���� �Ʒ���)

    [InspectorName("�ٴ� ��� ������Ʈ ȸ�� ����")]
    public float groundSmashObjectRotation = 0f; // �ν��Ͻ�ȭ �� ȸ�� ����

    [InspectorName("�ٴ� ��� ���ƿ��� �ӵ�")]
    public float groundSmashSpeed = 10f;

}
