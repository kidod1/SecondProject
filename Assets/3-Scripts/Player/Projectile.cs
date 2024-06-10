using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    [SerializeField]
    private PlayerData stat;

    private void OnEnable()
    {
        Invoke(nameof(Deactivate), stat.projectileRange);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Update()
    {
        transform.Translate(direction * stat.projectileSpeed * Time.deltaTime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}