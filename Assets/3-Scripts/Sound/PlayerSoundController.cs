using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    [SerializeField]
    private string moveStartEventName; // �̵� ���� �� ����� WWISE �̺�Ʈ �̸�

    [SerializeField]
    private string moveStopEventName; // �̵� ���� �� ����� WWISE �̺�Ʈ �̸�

    public void PlayMoveSound()
    {
        AkSoundEngine.PostEvent(moveStartEventName, gameObject);
    }

    public void StopMoveSound()
    {
        AkSoundEngine.PostEvent(moveStopEventName, gameObject);
    }
}
