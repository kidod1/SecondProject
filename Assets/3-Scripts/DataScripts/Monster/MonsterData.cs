using UnityEngine;

[CreateAssetMenu(menuName = "MonsterData/MonsterData")]
public class MonsterData : ScriptableObject
{
    public int monsterSpeed;
    public int maxHP;
    public int contectDamage;
    public int attackDamage;
    public int experiencePoints;
    public float detectionRange;
    public float attackRange;
    public float attackDelay;

    [Tooltip("몬스터를 처치했을 때 플레이어가 얻는 재화")]
    public int rewardCurrency;

    [Header("Experience Drop Settings")]
    public int highExperiencePoints;
    [Tooltip("몬스터 높은 경험치 획득 확률, 0~1 사이로 숫자 설정. 1이 100퍼")]
    public float highExperienceDropChance;
}
