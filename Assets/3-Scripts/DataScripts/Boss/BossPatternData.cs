using UnityEngine;

[CreateAssetMenu(fileName = "BossPatternData", menuName = "ScriptableObjects/BossPatternData", order = 1)]
public class BossPatternData : ScriptableObject
{
    [Header("���� Ȯ�� ����")]
    [Tooltip("ź�� ���� Ȯ��")]
    [Range(0, 100)]
    public float bulletPatternProbability = 25f;

    [Tooltip("��� �� ���� ���� Ȯ��")]
    [Range(0, 100)]
    public float warningAttackPatternProbability = 25f;

    [Tooltip("������ ���� Ȯ��")]
    [Range(0, 100)]
    public float laserPatternProbability = 25f;

    [Tooltip("�ٴ� ��� ���� Ȯ��")]
    [Range(0, 100)]
    public float groundSmashPatternProbability = 25f;

    [Header("ź�� ���� ����")]
    public int bulletPatternRepeatCount = 5; // X����
    public GameObject bulletPrefab;
    public Transform[] bulletSpawnPoints;
    public float bulletFireInterval = 0.5f; // ź�� �߻� ����
    public float bulletLifeTime = 5f; // źȯ�� ����

    [Header("��� �� ���� ���� ����")]
    public int warningAttackRepeatCount = 5; // X�� �ݺ�
    public GameObject warningEffectPrefab;
    public GameObject attackEffectPrefab;
    public float warningDuration = 2f; // ��� �� ���ݱ����� �ð�
    public float attackInterval = 0.5f; // ���� ����
    public float attackEffectDuration = 2f; // ���� ����Ʈ�� ����
    [Tooltip("���� ��� ���۵Ǳ������ ������ (�� ���� warningDuration���� ������ ��� ��Ĩ�ϴ�)")]
    public float warningStartInterval = 0.5f; // ���� ��� ���۱����� ������

    [Header("������ ���� ����")]
    public GameObject laserPrefab;
    public GameObject laserWarningPrefab;
    public float laserWarningDuration = 2f; // ������ ��� ǥ�� �ð�
    public float laserDuration = 5f; // ������ ���� �ð�

    [Header("�ٴ� ��� ���� ����")]
    public GameObject groundSmashWarningPrefab; // �ٴڿ� ǥ�õ� ���� ��� ������
    public GameObject groundSmashEffectPrefab; // ��� �� ������� ����Ʈ ������
    public float groundSmashWarningDuration = 2f; // ��� ǥ�� �� ������������ �ð�
    public float groundSmashEffectDuration = 2f; // ������� ����Ʈ ���� �ð�
    public float groundSmashCooldown = 1f; // ���� ������ ��� �ð�
    public int groundSmashBulletCount = 12; // �߻�� źȯ �� (�������� ��ġ)
    public float groundSmashBulletSpeed = 5f; // źȯ �ӵ�
}
