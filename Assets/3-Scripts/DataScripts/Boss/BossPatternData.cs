using UnityEngine;

[CreateAssetMenu(fileName = "BossPatternData", menuName = "ScriptableObjects/BossPatternData", order = 1)]
public class BossPatternData : ScriptableObject
{
    [Header("패턴 확률 설정")]
    [Tooltip("탄막 패턴 확률")]
    [Range(0, 100)]
    public float bulletPatternProbability = 25f;

    [Tooltip("경고 후 공격 패턴 확률")]
    [Range(0, 100)]
    public float warningAttackPatternProbability = 25f;

    [Tooltip("레이저 패턴 확률")]
    [Range(0, 100)]
    public float laserPatternProbability = 25f;

    [Tooltip("바닥 찍기 패턴 확률")]
    [Range(0, 100)]
    public float groundSmashPatternProbability = 25f;

    [Header("탄막 패턴 설정")]
    public int bulletPatternRepeatCount = 5; // X차례
    public GameObject bulletPrefab;
    public Transform[] bulletSpawnPoints;
    public float bulletFireInterval = 0.5f; // 탄막 발사 간격
    public float bulletLifeTime = 5f; // 탄환의 수명

    [Header("경고 후 공격 패턴 설정")]
    public int warningAttackRepeatCount = 5; // X번 반복
    public GameObject warningEffectPrefab;
    public GameObject attackEffectPrefab;
    public float warningDuration = 2f; // 경고 후 공격까지의 시간
    public float attackInterval = 0.5f; // 공격 간격
    public float attackEffectDuration = 2f; // 공격 이펙트의 수명
    [Tooltip("다음 경고가 시작되기까지의 딜레이 (이 값이 warningDuration보다 작으면 경고가 겹칩니다)")]
    public float warningStartInterval = 0.5f; // 다음 경고 시작까지의 딜레이

    [Header("레이저 패턴 설정")]
    public GameObject laserPrefab;
    public GameObject laserWarningPrefab;
    public float laserWarningDuration = 2f; // 레이저 경고 표시 시간
    public float laserDuration = 5f; // 레이저 지속 시간

    [Header("바닥 찍기 패턴 설정")]
    public GameObject groundSmashWarningPrefab; // 바닥에 표시될 위험 경고 프리팹
    public GameObject groundSmashEffectPrefab; // 기계 팔 내려찍기 이펙트 프리팹
    public float groundSmashWarningDuration = 2f; // 경고 표시 후 내려찍기까지의 시간
    public float groundSmashEffectDuration = 2f; // 내려찍기 이펙트 지속 시간
    public float groundSmashCooldown = 1f; // 패턴 사이의 대기 시간
    public int groundSmashBulletCount = 12; // 발사될 탄환 수 (원형으로 배치)
    public float groundSmashBulletSpeed = 5f; // 탄환 속도
}
