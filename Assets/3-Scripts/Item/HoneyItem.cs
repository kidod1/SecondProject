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
            Debug.LogError("HoneyItem: �÷��̾ �Ҵ���� �ʾҽ��ϴ�.");
        }
    }
}
