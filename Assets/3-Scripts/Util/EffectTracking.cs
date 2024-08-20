using UnityEngine;

public class EffectTracking : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public bool warnIfNoTarget = true;

    private void Start()
    {
        // 타겟이 지정되지 않았으면 기본적으로 Player 태그가 있는 오브젝트를 찾아서 타겟으로 설정
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else if (warnIfNoTarget)
            {
                Debug.LogWarning("EffectTracking: No target assigned and no Player found.");
            }
        }
    }

    private void Update()
    {
        if (target != null)
        {
            FollowTarget();
        }
    }

    private void FollowTarget()
    {
        transform.position = target.position + offset;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void ClearTarget()
    {
        target = null;
    }
}
