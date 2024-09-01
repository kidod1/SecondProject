using UnityEngine;

[CreateAssetMenu(menuName = "ObjectData/ObjectData")]
public class ObjectData : ScriptableObject
{
    [Tooltip("이 오브젝트가 충돌 시 입히는 데미지")]
    public int damage;
}
