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
                playerShooting.OnShoot.AddListener(FollowAttack);
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

    private void FollowAttack(Vector2 direction, int prefabIndex)
    {
        GameObject projectile = playerShooting.objectPool.GetObject(prefabIndex);
        projectile.transform.position = transform.position;

        Projectile projScript = projectile.GetComponent<Projectile>();
        projScript.Initialize(playerShooting.stat, true, damageMultiplier);
        projScript.SetDirection(direction);
    }

    private void OnDestroy()
    {
        if (playerShooting != null)
        {
            playerShooting.OnShoot.RemoveListener(FollowAttack);
        }
    }
}
