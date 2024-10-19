using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사운드 이벤트를 정의하는 클래스입니다. 특정 재생 횟수에 따라 지정된 사운드를 재생합니다.
/// </summary>
[System.Serializable]
public class SoundEvent
{
    [Tooltip("이 사운드를 재생할 호출 횟수")]
    public int playCount;

    [Tooltip("재생할 사운드 클립")]
    public AudioClip clip;
}

/// <summary>
/// 사운드 매니저는 게임 내 모든 사운드 재생을 중앙에서 관리합니다.
/// </summary>
public class SoundManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static SoundManager Instance { get; private set; }

    [Header("Sound Events")]
    [Tooltip("재생할 사운드 이벤트 리스트")]
    public List<SoundEvent> soundEvents = new List<SoundEvent>();

    private AudioSource audioSource;
    private int playCount = 0;

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource 컴포넌트 추가 또는 가져오기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 기본 설정
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// 사운드를 재생합니다. 특정 횟수에 도달하면 지정된 사운드를 재생합니다.
    /// 이 메서드는 Unity Events에서 호출할 수 있습니다.
    /// </summary>
    public void PlaySound()
    {
        playCount++;

        foreach (var soundEvent in soundEvents)
        {
            if (playCount == soundEvent.playCount)
            {
                PlayClip(soundEvent.clip);
                break; // 하나의 사운드만 재생하도록 설정. 여러 사운드를 원할 경우 이 줄을 제거하세요.
            }
        }

        // 일반 사운드 재생 (선택 사항)
        // PlayClip(soundClips[Random.Range(0, soundClips.Length)]);
    }

    /// <summary>
    /// 특정 인덱스의 사운드를 재생합니다.
    /// 이 메서드는 Unity Events에서 호출할 수 있습니다.
    /// </summary>
    /// <param name="index">재생할 사운드의 인덱스</param>
    public void PlaySoundAtIndex(int index)
    {
        if (index >= 0 && index < soundEvents.Count)
        {
            PlayClip(soundEvents[index].clip);
        }
        else
        {
            Debug.LogWarning("SoundManager: 유효하지 않은 사운드 인덱스입니다.");
        }
    }

    /// <summary>
    /// 특정 AudioClip을 재생합니다.
    /// 이 메서드는 Unity Events에서 호출할 수 있습니다.
    /// </summary>
    /// <param name="clip">재생할 사운드 클립</param>
    public void PlaySpecificSound(AudioClip clip)
    {
        PlayClip(clip);
    }

    /// <summary>
    /// 사운드 재생 횟수를 리셋합니다.
    /// 이 메서드는 Unity Events에서 호출할 수 있습니다.
    /// </summary>
    public void ResetPlayCount()
    {
        playCount = 0;
    }

    /// <summary>
    /// 현재 재생 횟수를 반환합니다.
    /// </summary>
    /// <returns>현재 재생 횟수</returns>
    public int GetPlayCount()
    {
        return playCount;
    }

    /// <summary>
    /// 지정된 사운드 클립을 재생합니다.
    /// </summary>
    /// <param name="clip">재생할 사운드 클립</param>
    private void PlayClip(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("SoundManager: 재생하려는 AudioClip이 없습니다.");
            return;
        }

        audioSource.PlayOneShot(clip);
    }
}
