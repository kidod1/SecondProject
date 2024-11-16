using UnityEngine;

public class HoneyItem : CollectibleItem
{
    [SerializeField]
    private HealthItemData itemData;
    public HealthItemData ItemData => itemData;

    protected override void Collect()
    {
        if (player != null)
        {
            player.Heal(itemData.healAmount);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("HoneyItem: 플레이어가 할당되지 않았습니다.");
        }
    }
}
