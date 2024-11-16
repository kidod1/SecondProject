using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("Shield Settings")]
    [Tooltip("Shield�� �ı��� �� ����� ����Ʈ")]
    public GameObject shieldBreakEffectPrefab;

    [Header("Sound Settings")]
    [Tooltip("Shield �ı� �� ����� �Ҹ�")]
    public AK.Wwise.Event shieldBreakSound;

    /// <summary>
    /// Shield�� �ı��մϴ�. �ʿ��� �ð��� �� ���� ȿ���� ������ �� ������Ʈ�� �����մϴ�.
    /// </summary>
    public void BreakShield()
    {
        // Shield �ı� ����Ʈ ����
        if (shieldBreakEffectPrefab != null)
        {
            Instantiate(shieldBreakEffectPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("ShieldBreakEffectPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // Shield �ı� �Ҹ� ���
        if (shieldBreakSound != null)
        {
            shieldBreakSound.Post(gameObject);
        }

        // Shield ������Ʈ ����
        Destroy(gameObject);
        Debug.Log("Shield�� �ı��Ǿ����ϴ�.");
    }
}
