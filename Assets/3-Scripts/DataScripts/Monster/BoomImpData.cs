using UnityEngine;

[CreateAssetMenu(menuName = "MonsterData/BoomImpData")]
public class BoomImpData : MonsterData
{
    [Tooltip("Æø¹ß ½Ã ÇÇÇØ·®")]
    public int explosionDamage = 50;

    [Tooltip("Æø¹ß ¹Ý°æ")]
    public float explosionRadius = 3f;
}
