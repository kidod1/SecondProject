using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� �̺�Ʈ�� �����ϴ� Ŭ�����Դϴ�. Ư�� ��� Ƚ���� ���� ������ ���带 ����մϴ�.
/// </summary>
[System.Serializable]
public class SoundEvent
{
    [Tooltip("�� ���带 ����� ȣ�� Ƚ��")]
    public int playCount;

    [Tooltip("����� ���� Ŭ��")]
    public AudioClip clip;
}

/// <summary>
/// ���� �Ŵ����� ���� �� ��� ���� ����� �߾ӿ��� �����մϴ�.
/// </summary>
public class SoundManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static SoundManager Instance { get; private set; }

    [Header("Sound Events")]
    [Tooltip("����� ���� �̺�Ʈ ����Ʈ")]
    public List<SoundEvent> soundEvents = new List<SoundEvent>();

    private AudioSource audioSource;
    private int playCount = 0;

    private void Awake()
    {
        // �̱��� ���� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �� �ı����� �ʵ��� ����
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource ������Ʈ �߰� �Ǵ� ��������
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // �⺻ ����
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// ���带 ����մϴ�. Ư�� Ƚ���� �����ϸ� ������ ���带 ����մϴ�.
    /// �� �޼���� Unity Events���� ȣ���� �� �ֽ��ϴ�.
    /// </summary>
    public void PlaySound()
    {
        playCount++;

        foreach (var soundEvent in soundEvents)
        {
            if (playCount == soundEvent.playCount)
            {
                PlayClip(soundEvent.clip);
                break; // �ϳ��� ���常 ����ϵ��� ����. ���� ���带 ���� ��� �� ���� �����ϼ���.
            }
        }

        // �Ϲ� ���� ��� (���� ����)
        // PlayClip(soundClips[Random.Range(0, soundClips.Length)]);
    }

    /// <summary>
    /// Ư�� �ε����� ���带 ����մϴ�.
    /// �� �޼���� Unity Events���� ȣ���� �� �ֽ��ϴ�.
    /// </summary>
    /// <param name="index">����� ������ �ε���</param>
    public void PlaySoundAtIndex(int index)
    {
        if (index >= 0 && index < soundEvents.Count)
        {
            PlayClip(soundEvents[index].clip);
        }
        else
        {
            Debug.LogWarning("SoundManager: ��ȿ���� ���� ���� �ε����Դϴ�.");
        }
    }

    /// <summary>
    /// Ư�� AudioClip�� ����մϴ�.
    /// �� �޼���� Unity Events���� ȣ���� �� �ֽ��ϴ�.
    /// </summary>
    /// <param name="clip">����� ���� Ŭ��</param>
    public void PlaySpecificSound(AudioClip clip)
    {
        PlayClip(clip);
    }

    /// <summary>
    /// ���� ��� Ƚ���� �����մϴ�.
    /// �� �޼���� Unity Events���� ȣ���� �� �ֽ��ϴ�.
    /// </summary>
    public void ResetPlayCount()
    {
        playCount = 0;
    }

    /// <summary>
    /// ���� ��� Ƚ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ��� Ƚ��</returns>
    public int GetPlayCount()
    {
        return playCount;
    }

    /// <summary>
    /// ������ ���� Ŭ���� ����մϴ�.
    /// </summary>
    /// <param name="clip">����� ���� Ŭ��</param>
    private void PlayClip(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("SoundManager: ����Ϸ��� AudioClip�� �����ϴ�.");
            return;
        }

        audioSource.PlayOneShot(clip);
    }
}
