using UnityEngine;
using AK.Wwise;

/// <summary>
/// 사운드 매니저는 게임 내 공통적인 사운드를 중앙에서 관리합니다.
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
    }

    /// <summary>
    /// 일반적인 WWISE 이벤트를 재생합니다.
    /// </summary>
    /// <param name="wwiseEvent">재생할 WWISE 이벤트</param>
    public void PlayEvent(AK.Wwise.Event wwiseEvent)
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
