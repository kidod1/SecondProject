using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    public Transform player; // �÷��̾� ������Ʈ�� Transform
    public float radius = 2.0f; // ȸ�� �ݰ�
    public float rotationSpeed = 50.0f; // ȸ�� �ӵ� (��/��)
    private float angle = 0.0f;

    private Player playerShooting;

    private void Start()
    {
        if (player != null)
        {
            playerShooting = player.GetComponent<Player>();
            if (playerShooting != null)
            {
                playerShooting.OnShoot.AddListener(FollowAttack);
            }
        }
    }

    private void Update()
    {
        if (player != null)
        {
            // ������ ȸ�� �ӵ��� �°� ����
            angle += rotationSpeed * Time.deltaTime;

            // ������ ���� ������ ��ȯ
            float angleRad = angle * Mathf.Deg2Rad;

            // ȸ�� �ݰ��� �̿��Ͽ� ���ο� ��ġ ���
            float x = player.position.x + Mathf.Cos(angleRad) * radius;
            float y = player.position.y + Mathf.Sin(angleRad) * radius;

            // ������Ʈ ��ġ ������Ʈ
            transform.position = new Vector3(x, y, transform.position.z);
        }
    }

    private void FollowAttack(Vector2 direction, int prefabIndex)
    {
        GameObject projectile = playerShooting.objectPool.GetObject(prefabIndex);
        projectile.transform.position = transform.position;
        projectile.GetComponent<Projectile>().SetDirection(direction);
    }

    private void OnDestroy()
    {
        if (playerShooting != null)
        {
            playerShooting.OnShoot.RemoveListener(FollowAttack);
        }
    }
}