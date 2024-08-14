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
}
