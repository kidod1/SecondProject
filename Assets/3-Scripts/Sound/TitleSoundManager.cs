using UnityEngine;
using AK.Wwise;

/// <summary>
/// TitleSoundManager�� Ÿ��Ʋ �� �ΰ� ���� ����� ������ ����մϴ�.
/// </summary>
public class TitleSoundManager : MonoBehaviour
{
    [Header("WWISE Events")]
    [Tooltip("�ΰ� ���� �̺�Ʈ")]
    public AK.Wwise.Event logoSoundEvent;   // �ΰ� ���� �̺�Ʈ

    [Tooltip("Ÿ��Ʋ ���� �̺�Ʈ")]
    public AK.Wwise.Event titleSoundEvent;  // Ÿ��Ʋ ���� �̺�Ʈ

    private void Awake()
    {
        // WWISE �̺�Ʈ �Ҵ� Ȯ��
        if (logoSoundEvent == null)
        {
            Debug.LogWarning("TitleSoundManager: logoSoundEvent�� �������� �ʾҽ��ϴ�.");
        }
        if (titleSoundEvent == null)
        {
            Debug.LogWarning("TitleSoundManager: titleSoundEvent�� �������� �ʾҽ��ϴ�.");
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
            Debug.LogWarning("TitleSoundManager: logoSoundEvent�� �������� �ʾҽ��ϴ�.");
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
            Debug.LogWarning("TitleSoundManager: titleSoundEvent�� �������� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// �ΰ� ���带 �����մϴ�.
    /// </summary>
    public void StopLogoSound()
    {
        if (logoSoundEvent != null)
        {
            logoSoundEvent.Stop(gameObject);
        }
        else
        {
            Debug.LogWarning("TitleSoundManager: logoSoundEvent�� �������� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// Ÿ��Ʋ ���带 �����մϴ�.
    /// </summary>
    public void StopTitleSound()
    {
        if (titleSoundEvent != null)
        {
            titleSoundEvent.Stop(gameObject);
        }
        else
        {
            Debug.LogWarning("TitleSoundManager: titleSoundEvent�� �������� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// Ư�� WWISE �̺�Ʈ�� �����մϴ�.
    /// </summary>
    /// <param name="wwiseEvent">������ WWISE �̺�Ʈ</param>
    public void StopCustomEvent(AK.Wwise.Event wwiseEvent)
    {
        if (wwiseEvent != null)
        {
            wwiseEvent.Stop(gameObject);
        }
        else
        {
            Debug.LogWarning("TitleSoundManager: wwiseEvent�� �������� �ʾҽ��ϴ�.");
        }
    }
}
