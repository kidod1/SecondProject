using UnityEngine;

[CreateAssetMenu(fileName = "BossPatternData", menuName = "ScriptableObjects/BossPatternData", order = 1)]
public class BossPatternData : ScriptableObject
{
    [Header("패턴 확률 설정")]
    [InspectorName("탄막 패턴 확률")]
    [Tooltip("탄막 패턴 확률")]
    [Range(0, 100)]
    public float bulletPatternProbability = 25f;

    [InspectorName("경고 후 공격 패턴 확률")]
    [Tooltip("경고 후 공격 패턴 확률")]
    [Range(0, 100)]
    public float warningAttackPatternProbability = 25f;

    [InspectorName("경고 레이저 패턴 확률")]
    [Tooltip("경고 레이저 패턴 확률")]
    [Range(0, 100)]
    public float warningLaserPatternProbability = 25f;

    [InspectorName("바닥 찍기 패턴 확률")]
    [Tooltip("바닥 찍기 패턴 확률")]
    [Range(0, 100)]
    public float groundSmashPatternProbability = 25f;

    [Header("탄막 패턴 설정")]
    [InspectorName("탄막 반복 횟수")]
    public int bulletPatternRepeatCount = 5;

    [InspectorName("탄환 프리팹")]
    public GameObject bulletPrefab;

    [InspectorName("탄막 스폰 포인트 배열")]
    [Tooltip("탄환이 발사될 여러 위치의 Transform 배열")]
    public Transform[] bulletSpawnPoints;

    [InspectorName("탄막 발사 간격")]
    public float bulletFireInterval = 0.5f;

    [InspectorName("탄환 수명")]
    public float bulletLifeTime = 5f;

    [Header("경고 후 공격 패턴 설정")]
    [InspectorName("경고 후 공격 반복 횟수")]
    public int warningAttackRepeatCount = 5;

    [InspectorName("경고 이펙트 프리팹")]
    public GameObject warningEffectPrefab;

    [InspectorName("공격 이펙트 프리팹")]
    public GameObject attackEffectPrefab;

    [InspectorName("경고 지속 시간")]
    public float warningDuration = 2f;

    [InspectorName("공격 이펙트 수명")]
    public float attackEffectDuration = 2f;

    [InspectorName("다음 경고 시작 딜레이")]
    [Tooltip("다음 경고가 시작되기까지의 딜레이 (이 값이 warningDuration보다 작으면 경고가 겹칩니다)")]
    public float warningStartInterval = 0.5f;


    [Header("기억력 패턴 설정")]
    [InspectorName("경고 레이저 경고 프리팹")]
    public GameObject warningLaserWarningPrefab;

    [InspectorName("경고 레이저 공격 프리팹")]
    public GameObject warningLaserAttackPrefab;

    [InspectorName("경고 레이저 경고 지속 시간")]
    public float warningLaserWarningDuration = 2f;

    [InspectorName("경고 레이저 공격 지속 시간")]
    public float warningLaserAttackDuration = 2f;

    [InspectorName("경고 레이저 공격 데미지")]
    public int warningLaserAttackDamage = 20;

    [Header("바닥 찍기 패턴 설정")]
    [InspectorName("바닥 찍기 오브젝트 프리팹")]
    public GameObject groundSmashMeteorPrefab;

    [InspectorName("바닥 찍기 경고 프리팹")]
    public GameObject groundSmashWarningPrefab;

    [InspectorName("바닥 찍기 경고 지속 시간")]
    public float groundSmashWarningDuration = 2f;

    [InspectorName("바닥 찍기 오브젝트 피해량")]
    public float groundSmashMeteorDamage = 50f;

    [InspectorName("바닥 찍기 오브젝트 충돌 반경")]
    public float groundSmashMeteorRadius = 2f;

    [InspectorName("바닥 찍기 오브젝트 낙하 속도")]
    public float groundSmashMeteorFallSpeed = 10f;

    [InspectorName("바닥 찍기 오브젝트 개수")]
    public int groundSmashMeteorCount = 3;

    [InspectorName("바닥 찍기 스폰 반경")]
    public float groundSmashSpawnRadius = 0f;

    [InspectorName("바닥 찍기 쿨다운 시간")]
    public float groundSmashCooldown = 1f;

    [InspectorName("바닥 찍기 탄환 개수")]
    public int groundSmashBulletCount = 12;

    [InspectorName("바닥 찍기 탄환 속도")]
    public float groundSmashBulletSpeed = 5f;

    [InspectorName("바닥 찍기 탄환 프리팹")]
    public GameObject groundSmashBulletPrefab;

    [InspectorName("바닥 찍기 탄환 피해량")]
    public int groundSmashBulletDamage = 10;

    [Header("Attack Damage Values")]
    [InspectorName("탄환 데미지")]
    public int bulletDamage = 10;

    [InspectorName("경고 공격 데미지")]
    public int warningAttackDamage = 20;
    [InspectorName("바닥 찍기 데미지")]
    public int groundSmashDamage = 15;

    [Header("Ground Smash Parameters")]
    [InspectorName("바닥 찍기 날아오는 각도")]
    public float groundSmashAngle = 270f;

    [InspectorName("바닥 찍기 오브젝트 회전 각도")]
    public float groundSmashObjectRotation = 0f;

    [InspectorName("바닥 찍기 날아오는 속도")]
    public float groundSmashSpeed = 10f;
}
