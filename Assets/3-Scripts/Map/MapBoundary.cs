using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MapBoundary : MonoBehaviour
{
    private Collider2D boundaryCollider;

    private void Awake()
    {
        boundaryCollider = GetComponent<Collider2D>();
        if (boundaryCollider == null)
        {
            Debug.LogError("Collider2D 컴포넌트가 필요합니다.");
        }
    }

    private void OnDrawGizmos()
    {
        if (boundaryCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = boundaryCollider.transform.localToWorldMatrix;

            if (boundaryCollider is BoxCollider2D boxCollider)
            {
                Gizmos.DrawWireCube(boxCollider.offset, boxCollider.size);
            }
            else if (boundaryCollider is CircleCollider2D circleCollider)
            {
                Gizmos.DrawWireSphere(circleCollider.offset, circleCollider.radius);
            }
            else if (boundaryCollider is PolygonCollider2D polygonCollider)
            {
                DrawPolygonGizmo(polygonCollider);
            }
        }
    }

    private void DrawPolygonGizmo(PolygonCollider2D polygonCollider)
    {
        Vector2[] points = polygonCollider.points;
        Vector3 previousPoint = polygonCollider.transform.TransformPoint(polygonCollider.offset + points[points.Length - 1]);
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 currentPoint = polygonCollider.transform.TransformPoint(polygonCollider.offset + points[i]);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }



    public Vector2 ClampPosition(Vector2 position)
    {
        if (boundaryCollider == null)
        {
            return position;
        }

        if (boundaryCollider is BoxCollider2D)
        {
            return ClampPositionToBox(position, (BoxCollider2D)boundaryCollider);
        }
        else if (boundaryCollider is CircleCollider2D)
        {
            return ClampPositionToCircle(position, (CircleCollider2D)boundaryCollider);
        }
        else if (boundaryCollider is PolygonCollider2D)
        {
            return ClampPositionToPolygon(position, (PolygonCollider2D)boundaryCollider);
        }

        return position;
    }

    private Vector2 ClampPositionToBox(Vector2 position, BoxCollider2D box)
    {
        Vector2 localPosition = boundaryCollider.transform.InverseTransformPoint(position);
        localPosition.x = Mathf.Clamp(localPosition.x, box.offset.x - box.size.x / 2, box.offset.x + box.size.x / 2);
        localPosition.y = Mathf.Clamp(localPosition.y, box.offset.y - box.size.y / 2, box.offset.y + box.size.y / 2);
        return boundaryCollider.transform.TransformPoint(localPosition);
    }

    private Vector2 ClampPositionToCircle(Vector2 position, CircleCollider2D circle)
    {
        Vector2 localPosition = boundaryCollider.transform.InverseTransformPoint(position);
        Vector2 direction = localPosition - circle.offset;
        if (direction.magnitude > circle.radius)
        {
            direction.Normalize();
            localPosition = circle.offset + direction * circle.radius;
        }
        return boundaryCollider.transform.TransformPoint(localPosition);
    }

    private Vector2 ClampPositionToPolygon(Vector2 position, PolygonCollider2D polygon)
    {
        Vector2 localPosition = boundaryCollider.transform.InverseTransformPoint(position);
        if (!polygon.OverlapPoint(localPosition))
        {
            localPosition = GetClosestPointOnPolygon(polygon, localPosition);
        }
        return boundaryCollider.transform.TransformPoint(localPosition);
    }

    private Vector2 GetClosestPointOnPolygon(PolygonCollider2D polygon, Vector2 point)
    {
        Vector2 closestPoint = polygon.points[0];
        float closestDistance = Vector2.Distance(point, closestPoint);

        foreach (Vector2 vertex in polygon.points)
        {
            float distance = Vector2.Distance(point, vertex);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = vertex;
            }
        }

        return closestPoint;
    }
    private void DefineHexagonPoints(PolygonCollider2D polygonCollider, float radius)
    {
        Vector2[] points = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = i * Mathf.PI / 3;
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
        polygonCollider.points = points;
    }
}
