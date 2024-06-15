using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    public Transform player; // 플레이어 오브젝트의 Transform
    public float radius = 2.0f; // 회전 반경
    public float rotationSpeed = 50.0f; // 회전 속도 (도/초)
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
            // 각도를 회전 속도에 맞게 증가
            angle += rotationSpeed * Time.deltaTime;

            // 각도를 라디안 단위로 변환
            float angleRad = angle * Mathf.Deg2Rad;

            // 회전 반경을 이용하여 새로운 위치 계산
            float x = player.position.x + Mathf.Cos(angleRad) * radius;
            float y = player.position.y + Mathf.Sin(angleRad) * radius;

            // 오브젝트 위치 업데이트
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