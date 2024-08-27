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

        // 회전 경로를 원으로 그리기
        Vector3 pivotPoint = transform.position; // 부모 오브젝트의 위치를 회전 축으로 사용
        float radius = Vector3.Distance(pivotPoint, transform.GetChild(0).position); // 자식 오브젝트의 위치를 기준으로 반지름 계산

        Gizmos.DrawWireSphere(pivotPoint, radius);
    }
}
