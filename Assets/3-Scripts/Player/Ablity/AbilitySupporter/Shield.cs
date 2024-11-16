using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("Shield Settings")]
    [Tooltip("Shield가 파괴될 때 재생될 이펙트")]
    public GameObject shieldBreakEffectPrefab;

    [Header("Sound Settings")]
    [Tooltip("Shield 파괴 시 재생될 소리")]
    public AK.Wwise.Event shieldBreakSound;

    /// <summary>
    /// Shield를 파괴합니다. 필요한 시각적 및 음향 효과를 실행한 후 오브젝트를 삭제합니다.
    /// </summary>
    public void BreakShield()
    {
        // Shield 파괴 이펙트 생성
        if (shieldBreakEffectPrefab != null)
        {
            Instantiate(shieldBreakEffectPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("ShieldBreakEffectPrefab이 할당되지 않았습니다.");
        }

        // Shield 파괴 소리 재생
        if (shieldBreakSound != null)
        {
            shieldBreakSound.Post(gameObject);
        }

        // Shield 오브젝트 삭제
        Destroy(gameObject);
        Debug.Log("Shield가 파괴되었습니다.");
    }
}
