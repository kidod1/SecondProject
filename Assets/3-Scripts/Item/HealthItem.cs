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
            Debug.LogError("HealthItem: �÷��̾ �Ҵ���� �ʾҽ��ϴ�.");
        }
    }
}
