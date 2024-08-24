using UnityEngine;

public class EffectTracking : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public bool warnIfNoTarget = true;

    private void Start()
    {
        // Ÿ���� �������� �ʾ����� �⺻������ Player �±װ� �ִ� ������Ʈ�� ã�Ƽ� Ÿ������ ����
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
