using UnityEngine;

public class ElectricWire : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 10f;
    [SerializeField]
    private int Damage = 1;
    private bool isReversed = false;

    private void Update()
    {
        float direction = isReversed ? -1 : 1;
        transform.Rotate(Vector3.forward, rotationSpeed * direction * Time.deltaTime);
    }

    public void ReverseRotation()
    {
        isReversed = !isReversed;
    }

    public void IncreaseSpeed()
    {
        rotationSpeed *= 2;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(Damage);
            }
        }
    }
}
