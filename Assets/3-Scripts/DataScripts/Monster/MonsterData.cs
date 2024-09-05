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

    [Header("Experience Drop Settings")]
    public int highExperiencePoints;
    [Tooltip("���� ���� ����ġ ȹ�� Ȯ��, 0~1 ���̷� ���� ����. 1�� 100��")]
    public float highExperienceDropChance;
}
