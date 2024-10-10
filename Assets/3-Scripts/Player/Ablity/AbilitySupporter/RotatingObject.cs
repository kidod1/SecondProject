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
                playerShooting.OnShoot.RemoveListener(FollowAttackHandler); // 중복 방지를 위해 제거
                playerShooting.OnShoot.AddListener(FollowAttackHandler); // 수정된 리스너 추가
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
            Debug.LogError("RotatingObject: 전달된 프로젝트트가 null입니다.");
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
            Debug.LogError("RotatingObject: Projectile 스크립트를 찾을 수 없습니다.");
        }
    }

    private void OnDestroy()
    {
        if (playerShooting != null)
        {
            playerShooting.OnShoot.RemoveListener(FollowAttackHandler); // 수정된 리스너 제거
        }
    }
}
