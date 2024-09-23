using UnityEngine;

public class ElectricWire : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 10f;
    [SerializeField]
    private float increaseSpeedUp = 2f;
    private bool isReversed = false;

    private Vector3 pivotPoint;

    private void Start()
    {
        pivotPoint = transform.parent != null ? transform.parent.position : Vector3.zero;
    }

    private void Update()
    {
        float direction = isReversed ? -1 : 1;
        transform.RotateAround(pivotPoint, Vector3.forward, rotationSpeed * direction * Time.deltaTime);
    }

    public void ReverseRotation()
    {
        isReversed = !isReversed;
    }

    public void IncreaseSpeed()
    {
        rotationSpeed *= increaseSpeedUp;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 pivotPointGizmo = transform.parent != null ? transform.parent.position : Vector3.zero;
        float radius = Vector3.Distance(pivotPointGizmo, transform.position);

        Gizmos.DrawWireSphere(pivotPointGizmo, radius);
    }
}
