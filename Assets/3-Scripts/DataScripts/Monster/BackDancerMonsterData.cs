using UnityEngine;

[CreateAssetMenu(menuName = "MonsterData/BackDancerMonsterData")]
public class BackDancerMonsterData : MonsterData
{
    [Tooltip("���� ��ٿ� �ð�")]
    public float danceCooldown = 4f;

    [Tooltip("���� �ӵ�")]
    public float danceSpeed = 3f;
}
