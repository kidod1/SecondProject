using UnityEngine;

public class ExperienceItem : MonoBehaviour
{
    public int experienceAmount;
    private bool isAttracting = false;
    private Transform target; // 플레이어의 Transform
    private float attractSpeed;
    private float attractDuration;
    private float attractTimer = 0f;

    public void StartAttractingToPlayer(Transform playerTransform, float speed, float duration)
    {
        isAttracting = true;
        target = playerTransform;
        attractSpeed = speed;
        attractDuration = duration;
        attractTimer = 0f;
    }

    private void Update()
    {
        if (isAttracting)
        {
            attractTimer += Time.deltaTime;

            if (attractTimer >= attractDuration)
            {
                isAttracting = false;
            }
            else
            {
                Vector3 direction = (target.position - transform.position).normalized;
                transform.position += direction * attractSpeed * Time.deltaTime;
            }
        }
    }
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
