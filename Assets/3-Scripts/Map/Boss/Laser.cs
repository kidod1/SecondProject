using UnityEngine;

public class Laser : MonoBehaviour
{
    public int damage = 20;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // �÷��̾�� ���� ������ ����
            collision.GetComponent<Player>().TakeDamage(damage);
        }
    }
}
