using UnityEngine;
using AK.Wwise;

/// <summary>
/// 사운드 매니저는 게임 내 모든 사운드를 중앙에서 관리합니다.
/// WWISE의 AK.Wwise.Event를 사용하여 사운드를 재생합니다.
/// </summary>
public class SoundManager : MonoBehaviour
{
    // Singleton 인스턴스
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                // 씬 내에 SoundManager가 없으면 새로 생성
                GameObject obj = new GameObject("SoundManager");
                instance = obj.AddComponent<SoundManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    [Header("WWISE Events")]
    [Tooltip("로고 사운드 이벤트")]
    public AK.Wwise.Event logoSoundEvent;   // 로고 사운드 이벤트

    [Tooltip("타이틀 사운드 이벤트")]
    public AK.Wwise.Event titleSoundEvent;  // 타이틀 사운드 이벤트

    private void Awake()
    {
        // Singleton 패턴 구현
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

        // WWISE 이벤트 할당 확인
        if (logoSoundEvent == null)
        {
            Debug.LogWarning("SoundManager: logoSoundEvent가 설정되지 않았습니다.");
        }
        if (titleSoundEvent == null)
        {
            Debug.LogWarning("SoundManager: titleSoundEvent가 설정되지 않았습니다.");
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
            Debug.LogWarning("SoundManager: logoSoundEvent가 설정되지 않았습니다.");
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
            Debug.LogWarning("SoundManager: titleSoundEvent가 설정되지 않았습니다.");
        }
    }

    /// <summary>
    /// 일반적인 WWISE 이벤트를 재생합니다.
    /// </summary>
    /// <param name="wwiseEvent">재생할 WWISE 이벤트</param>
    public void PlayCustomEvent(AK.Wwise.Event wwiseEvent)
    {
        if (wwiseEvent != null)
        {
            wwiseEvent.Post(gameObject);
        }
        else
        {
            Debug.LogWarning("SoundManager: wwiseEvent가 설정되지 않았습니다.");
        }
    }
}
