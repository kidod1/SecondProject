using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    [SerializeField]
    private string moveStartEventName; // 이동 시작 시 재생할 WWISE 이벤트 이름

    [SerializeField]
    private string moveStopEventName; // 이동 종료 시 재생할 WWISE 이벤트 이름

    public void PlayMoveSound()
    {
        AkSoundEngine.PostEvent(moveStartEventName, gameObject);
    }

    public void StopMoveSound()
    {
        AkSoundEngine.PostEvent(moveStopEventName, gameObject);
    }
}
