using UnityEngine;
using UnityEngine.Events;

public class PlayerSoundController : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event customEvent;
    // 커스텀 WWISE 사운드 재생 함수
    public void PlayCustomSound()
    {
        if (customEvent != null)
        {
            customEvent.Post(gameObject);
        }
        else
        {
            Debug.LogWarning("customEvent가 설정되지 않았습니다.");
        }
    }
}
