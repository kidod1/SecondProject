using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    public Transform player;
    public float radius = 2.0f;
    public float rotationSpeed = 50.0f;
    private float angle = 0.0f;

    public Player playerShooting;
    public float damageMultiplier = 0.3f;

    private void Start()
    {
        if (player != null)
        {
            playerShooting = player.GetComponent<Player>();
            if (playerShooting != null)
            {
                playerShooting.OnShoot.RemoveListener(FollowAttackHandler); // �ߺ� ������ ���� ����
                playerShooting.OnShoot.AddListener(FollowAttackHandler); // ������ ������ �߰�
            }
        }
    }

    private void Update()
    {
        if (player != null)
        {
            angle += rotationSpeed * Time.deltaTime;

            float angleRad = angle * Mathf.Deg2Rad;

            float x = player.position.x + Mathf.Cos(angleRad) * radius;
            float y = player.position.y + Mathf.Sin(angleRad) * radius;

            transform.position = new Vector3(x, y, transform.position.z);
        }
    }

    private void FollowAttackHandler(Vector2 direction, int prefabIndex, GameObject projectile)
    {
        if (projectile == null)
        {
            Debug.LogError("RotatingObject: ���޵� ������ƮƮ�� null�Դϴ�.");
            return;
        }

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.Initialize(playerShooting.stat, playerShooting, false, damageMultiplier);
            projScript.SetDirection(direction);
        }
        else
        {
            Debug.LogError("RotatingObject: Projectile ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    private void OnDestroy()
    {
        if (playerShooting != null)
        {
            playerShooting.OnShoot.RemoveListener(FollowAttackHandler); // ������ ������ ����
        }
    }
}
