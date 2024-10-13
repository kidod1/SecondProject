using UnityEngine;

public class Laser : MonoBehaviour
{
    public int damage = 20;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 플레이어에게 지속 데미지 적용
            collision.GetComponent<Player>().TakeDamage(damage);
        }
    }
}
