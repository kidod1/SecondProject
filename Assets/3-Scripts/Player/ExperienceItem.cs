using UnityEngine;

public class ExperienceItem : MonoBehaviour
{
    public int experienceAmount;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.GainExperience(experienceAmount);
                Destroy(gameObject);
            }
        }
    }
}
