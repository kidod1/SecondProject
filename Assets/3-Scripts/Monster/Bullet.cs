using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int attackDamage;

    public void SetAttackDamage(int damage)
    {
        attackDamage = damage;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
