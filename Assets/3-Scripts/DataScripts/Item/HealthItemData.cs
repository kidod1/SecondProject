using UnityEngine;

[CreateAssetMenu(menuName = "ItemData/HealthItem")]
public class HealthItemData : ScriptableObject
{
    [Tooltip("아이템이 회복시키는 체력량")]
    public int healAmount;
}
