using UnityEngine;

public class HealthItem : MonoBehaviour
{
    public HealthItemData itemData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.Heal(itemData.healAmount);
                Destroy(gameObject);
            }
        }
    }
}
