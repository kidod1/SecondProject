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

    [Tooltip("���͸� óġ���� �� �÷��̾ ��� ��ȭ")]
    public int rewardCurrency;
}
