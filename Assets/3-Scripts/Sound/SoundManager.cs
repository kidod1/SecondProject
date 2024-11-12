using UnityEngine;
using AK.Wwise;

/// <summary>
/// ���� �Ŵ����� ���� �� �������� ���带 �߾ӿ��� �����մϴ�.
/// WWISE�� AK.Wwise.Event�� ����Ͽ� ���带 ����մϴ�.
/// </summary>
public class SoundManager : MonoBehaviour
{
    // Singleton �ν��Ͻ�
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                // �� ���� SoundManager�� ������ ���� ����
                GameObject obj = new GameObject("SoundManager");
                instance = obj.AddComponent<SoundManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private void Awake()
    {
        // Singleton ���� ����
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// �Ϲ����� WWISE �̺�Ʈ�� ����մϴ�.
    /// </summary>
    /// <param name="wwiseEvent">����� WWISE �̺�Ʈ</param>
    public void PlayEvent(AK.Wwise.Event wwiseEvent)
    {
        if (wwiseEvent != null)
        {
            wwiseEvent.Post(gameObject);
        }
        else
        {
            Debug.LogWarning("SoundManager: wwiseEvent�� �������� �ʾҽ��ϴ�.");
        }
    }
}
