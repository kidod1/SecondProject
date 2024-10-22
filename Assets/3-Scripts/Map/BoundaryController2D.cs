using UnityEngine;

public class BoundaryController2D : MonoBehaviour
{
    [Header("플레이어 설정")]
    public Transform player; // 플레이어의 Transform을 Inspector에서 할당

    [Header("경기장 설정")]
    public float boundaryRadius = 50f; // 경기장의 반지름

    private Vector2 center;

    void Start()
    {
        center = transform.position; // 이 스크립트가 부착된 오브젝트의 위치를 중심으로 설정
    }

    void LateUpdate()
    {
        Vector2 playerPosition = player.position;
        Vector2 direction = playerPosition - center;

        if (direction.magnitude > boundaryRadius)
        {
            Vector2 clampedPosition = center + direction.normalized * boundaryRadius;
            player.position = clampedPosition;

            // 플레이어가 경계를 벗어나려고 했을 때의 처리 (선택 사항)
            // 예: 소리 재생, UI 경고 표시 등
        }
    }

    /// <summary>
    /// Gizmos를 이용하여 경기장 경계를 시각적으로 표시합니다.
    /// </summary>
    void OnDrawGizmos()
    {
        // 중심점 설정 (실행 중일 때와 아닐 때를 모두 처리)
        Vector3 gizmoCenter = transform.position;

        // Gizmos 색상 설정
        Gizmos.color = Color.green;

        // 경기장 경계 그리기 (2D 기준, 원형으로)
        Gizmos.DrawWireSphere(gizmoCenter, boundaryRadius);
    }
}
