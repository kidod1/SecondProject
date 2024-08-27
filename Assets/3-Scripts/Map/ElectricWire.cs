using UnityEngine;

public class ElectricWire : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 10f;
    [SerializeField]
    private float increaseSpeedUp = 2f;
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
        rotationSpeed *= increaseSpeedUp;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // ȸ�� ��θ� ������ �׸���
        Vector3 pivotPoint = transform.position; // �θ� ������Ʈ�� ��ġ�� ȸ�� ������ ���
        float radius = Vector3.Distance(pivotPoint, transform.GetChild(0).position); // �ڽ� ������Ʈ�� ��ġ�� �������� ������ ���

        Gizmos.DrawWireSphere(pivotPoint, radius);
    }
}
