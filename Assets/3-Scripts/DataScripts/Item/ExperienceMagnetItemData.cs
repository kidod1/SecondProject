using UnityEngine;

[CreateAssetMenu(menuName = "ItemData/ExperienceMagnetItem")]
public class ExperienceMagnetItemData : ScriptableObject
{
    [Tooltip("경험치 아이템이 플레이어에게 끌려오는 속도")]
    public float speed = 5f;

    [Tooltip("경험치 아이템이 플레이어에게 끌려오는 지속 시간")]
    public float duration = 2f;
}
