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
        if (playerShooting == null)
        {
            playerShooting = PlayManager.I.GetPlayer();
        }
        if (player == null)
        {
            player = playerShooting.transform;
        }
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

    private void FollowAttackHandler(Vector2 direction, int prefabIndex, GameObject originalProjectile)
    {
        if (originalProjectile == null)
        {
            Debug.LogError("RotatingObject: ���޵� ���� ������ƮŸ���� null�Դϴ�.");
            return;
        }

        // ���ο� ������ƮŸ�� ����
        GameObject cloneProjectile = Instantiate(originalProjectile, transform.position, Quaternion.identity);
        Projectile projScript = cloneProjectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.Initialize(playerShooting.stat, playerShooting, false, damageMultiplier, playerShooting.stat.buffedPlayerDamage);
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
