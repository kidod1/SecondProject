using UnityEngine;
using UnityEngine.Events;

public class PlayerSoundController : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event customEvent;
    // Ŀ���� WWISE ���� ��� �Լ�
    public void PlayCustomSound()
    {
        if (customEvent != null)
        {
            customEvent.Post(gameObject);
        }
        else
        {
            Debug.LogWarning("customEvent�� �������� �ʾҽ��ϴ�.");
        }
    }
}
