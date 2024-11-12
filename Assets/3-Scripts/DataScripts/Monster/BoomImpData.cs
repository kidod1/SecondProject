using UnityEngine;

[CreateAssetMenu(menuName = "MonsterData/BoomImpData")]
public class BoomImpData : MonsterData
{
    [Tooltip("���� �� ���ط�")]
    public int explosionDamage = 50;

    [Tooltip("���� �ݰ�")]
    public float explosionRadius = 3f;
}
