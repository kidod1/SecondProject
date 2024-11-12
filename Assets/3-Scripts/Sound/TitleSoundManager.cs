using UnityEngine;
using AK.Wwise;

/// <summary>
/// TitleSoundManager는 타이틀 및 로고 사운드 재생과 중지를 담당합니다.
/// </summary>
public class TitleSoundManager : MonoBehaviour
{
    [Header("WWISE Events")]
    [Tooltip("로고 사운드 이벤트")]
    public AK.Wwise.Event logoSoundEvent;   // 로고 사운드 이벤트

    [Tooltip("타이틀 사운드 이벤트")]
    public AK.Wwise.Event titleSoundEvent;  // 타이틀 사운드 이벤트

    private void Awake()
    {
        // WWISE 이벤트 할당 확인
        if (logoSoundEvent == null)
        {
            Debug.LogWarning("TitleSoundManager: logoSoundEvent가 설정되지 않았습니다.");
        }
        if (titleSoundEvent == null)
        {
            Debug.LogWarning("TitleSoundManager: titleSoundEvent가 설정되지 않았습니다.");
        }
    }

    /// <summary>
    /// 로고 사운드를 재생합니다.
    /// </summary>
    public void PlayLogoSound()
    {
        if (logoSoundEvent != null)
        {
            logoSoundEvent.Post(gameObject);
        }
        else
        {
            Debug.LogWarning("TitleSoundManager: logoSoundEvent가 설정되지 않았습니다.");
        }
    }

    /// <summary>
    /// 타이틀 사운드를 재생합니다.
    /// </summary>
    public void PlayTitleSound()
    {
        if (titleSoundEvent != null)
        {
            titleSoundEvent.Post(gameObject);
        }
        else
        {
            Debug.LogWarning("TitleSoundManager: titleSoundEvent가 설정되지 않았습니다.");
        }
    }

    /// <summary>
    /// 로고 사운드를 중지합니다.
    /// </summary>
    public void StopLogoSound()
    {
        if (logoSoundEvent != null)
        {
            logoSoundEvent.Stop(gameObject);
        }
        else
        {
            Debug.LogWarning("TitleSoundManager: logoSoundEvent가 설정되지 않았습니다.");
        }
    }

    /// <summary>
    /// 타이틀 사운드를 중지합니다.
    /// </summary>
    public void StopTitleSound()
    {
        if (titleSoundEvent != null)
        {
            titleSoundEvent.Stop(gameObject);
        }
        else
        {
            Debug.LogWarning("TitleSoundManager: titleSoundEvent가 설정되지 않았습니다.");
        }
    }

    /// <summary>
    /// 특정 WWISE 이벤트를 중지합니다.
    /// </summary>
    /// <param name="wwiseEvent">중지할 WWISE 이벤트</param>
    public void StopCustomEvent(AK.Wwise.Event wwiseEvent)
    {
        if (wwiseEvent != null)
        {
            wwiseEvent.Stop(gameObject);
        }
        else
        {
            Debug.LogWarning("TitleSoundManager: wwiseEvent가 설정되지 않았습니다.");
        }
    }
}
