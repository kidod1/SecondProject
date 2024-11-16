using UnityEngine;

public class HealthItem : CollectibleItem
{
    [SerializeField]
    private HealthItemData itemData;

    protected override void Collect()
    {
        if (player != null)
        {
            player.Heal(itemData.healAmount);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("HealthItem: 플레이어가 할당되지 않았습니다.");
        }
    }
}
