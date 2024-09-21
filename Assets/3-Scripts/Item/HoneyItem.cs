using UnityEngine;

public class HoneyItem : MonoBehaviour
{
    [SerializeField]
    private HealthItemData itemData;
    public HealthItemData ItemData => itemData;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            player.Heal(itemData.healAmount);
            Destroy(gameObject);
        }
    }
}
