using UnityEngine;

[CreateAssetMenu(menuName = "MonsterData/BackDancerMonsterData")]
public class BackDancerMonsterData : MonsterData
{
    [Tooltip("공격 쿨다운 시간")]
    public float danceCooldown = 4f;

    [Tooltip("공격 속도")]
    public float danceSpeed = 3f;
}
