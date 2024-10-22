using UnityEngine;

public class BoundaryController2D : MonoBehaviour
{
    [Header("�÷��̾� ����")]
    public Transform player; // �÷��̾��� Transform�� Inspector���� �Ҵ�

    [Header("����� ����")]
    public float boundaryRadius = 50f; // ������� ������

    private Vector2 center;

    void Start()
    {
        center = transform.position; // �� ��ũ��Ʈ�� ������ ������Ʈ�� ��ġ�� �߽����� ����
    }

    void LateUpdate()
    {
        Vector2 playerPosition = player.position;
        Vector2 direction = playerPosition - center;

        if (direction.magnitude > boundaryRadius)
        {
            Vector2 clampedPosition = center + direction.normalized * boundaryRadius;
            player.position = clampedPosition;

            // �÷��̾ ��踦 ������� ���� ���� ó�� (���� ����)
            // ��: �Ҹ� ���, UI ��� ǥ�� ��
        }
    }

    /// <summary>
    /// Gizmos�� �̿��Ͽ� ����� ��踦 �ð������� ǥ���մϴ�.
    /// </summary>
    void OnDrawGizmos()
    {
        // �߽��� ���� (���� ���� ���� �ƴ� ���� ��� ó��)
        Vector3 gizmoCenter = transform.position;

        // Gizmos ���� ����
        Gizmos.color = Color.green;

        // ����� ��� �׸��� (2D ����, ��������)
        Gizmos.DrawWireSphere(gizmoCenter, boundaryRadius);
    }
}
