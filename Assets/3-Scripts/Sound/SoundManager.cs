using UnityEngine;
using AK.Wwise;

/// <summary>
/// ���� �Ŵ����� ���� �� ��� ���带 �߾ӿ��� �����մϴ�.
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

    [Header("WWISE Events")]
    [Tooltip("�ΰ� ���� �̺�Ʈ")]
    public AK.Wwise.Event logoSoundEvent;   // �ΰ� ���� �̺�Ʈ

    [Tooltip("Ÿ��Ʋ ���� �̺�Ʈ")]
    public AK.Wwise.Event titleSoundEvent;  // Ÿ��Ʋ ���� �̺�Ʈ

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

        // WWISE �̺�Ʈ �Ҵ� Ȯ��
        if (logoSoundEvent == null)
        {
            Debug.LogWarning("SoundManager: logoSoundEvent�� �������� �ʾҽ��ϴ�.");
        }
        if (titleSoundEvent == null)
        {
            Debug.LogWarning("SoundManager: titleSoundEvent�� �������� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// �ΰ� ���带 ����մϴ�.
    /// </summary>
    public void PlayLogoSound()
    {
        if (logoSoundEvent != null)
        {
            logoSoundEvent.Post(gameObject);
        }
        else
        {
            Debug.LogWarning("SoundManager: logoSoundEvent�� �������� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// Ÿ��Ʋ ���带 ����մϴ�.
    /// </summary>
    public void PlayTitleSound()
    {
        if (titleSoundEvent != null)
        {
            titleSoundEvent.Post(gameObject);
        }
        else
        {
            Debug.LogWarning("SoundManager: titleSoundEvent�� �������� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// �Ϲ����� WWISE �̺�Ʈ�� ����մϴ�.
    /// </summary>
    /// <param name="wwiseEvent">����� WWISE �̺�Ʈ</param>
    public void PlayCustomEvent(AK.Wwise.Event wwiseEvent)
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
