using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using AK.Wwise; // WWISE 네임스페이스 추가

public class SettingsManager : MonoBehaviour
{
    [Header("Graphics Settings")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;
    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider uiVolumeSlider;

    // WWISE RTPC 이름 상수 정의
    private const string MasterVolumeRTPC = "Master_Volume";
    private const string MusicVolumeRTPC = "Music_Volume";
    private const string SFXVolumeRTPC = "SFX_Volume";
    private const string UIVolumeRTPC = "UI_Volume"; // 수정된 부분: UIVolumeRTPC로 이름 수정

    private void Start()
    {
        // 그래픽 설정 초기화
        InitializeGraphicsSettings();

        // UI 볼륨 슬라이더 초기화 및 이벤트 연결
        if (uiVolumeSlider != null)
        {
            uiVolumeSlider.onValueChanged.AddListener(SetUIVolume); // 수정된 부분: SetUIVolume 메서드로 변경
            uiVolumeSlider.value = 1.0f;
        }
        else
        {
            Debug.LogWarning("UIVolumeSlider가 할당되지 않았습니다.");
        }

        // 오디오 설정 초기화
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            masterVolumeSlider.value = 1.0f;
        }
        else
        {
            Debug.LogWarning("MasterVolumeSlider가 할당되지 않았습니다.");
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            musicVolumeSlider.value = 1.0f;
        }
        else
        {
            Debug.LogWarning("MusicVolumeSlider가 할당되지 않았습니다.");
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            sfxVolumeSlider.value = 1.0f;
        }
        else
        {
            Debug.LogWarning("SFXVolumeSlider가 할당되지 않았습니다.");
        }
    }

    // 그래픽 설정 초기화
    void InitializeGraphicsSettings()
    {
        // 해상도 설정 초기화
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            foreach (Resolution res in Screen.resolutions)
            {
                options.Add(res.width + "x" + res.height);
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }
        else
        {
            Debug.LogWarning("ResolutionDropdown이 할당되지 않았습니다.");
        }

        // 품질 설정 초기화
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string> { "Low", "Medium", "High" });
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }
        else
        {
            Debug.LogWarning("QualityDropdown이 할당되지 않았습니다.");
        }

        // 전체 화면 설정 초기화
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }
        else
        {
            Debug.LogWarning("FullscreenToggle이 할당되지 않았습니다.");
        }
    }

    public void SetWindowedMode(int width, int height)
    {
        Screen.SetResolution(width, height, false);
    }
    // 해상도 변경
    public void SetResolution(int index)
    {
        if (Screen.resolutions.Length > index)
        {
            Resolution res = Screen.resolutions[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        }
        else
        {
            Debug.LogWarning("해상도 인덱스가 유효하지 않습니다.");
        }
    }

    // 품질 변경
    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    // 전체 화면 토글
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // 마스터 볼륨 설정
    public void SetMasterVolume(float volume)
    {
        AkSoundEngine.SetRTPCValue(MasterVolumeRTPC, volume * 100);
    }

    // 음악 볼륨 설정
    public void SetMusicVolume(float volume)
    {
        AkSoundEngine.SetRTPCValue(MusicVolumeRTPC, volume * 100);
    }

    // 효과음 볼륨 설정
    public void SetSFXVolume(float volume)
    {
        AkSoundEngine.SetRTPCValue(SFXVolumeRTPC, volume * 100);
    }

    // UI 볼륨 설정 - 추가된 부분
    public void SetUIVolume(float volume)
    {
        AkSoundEngine.SetRTPCValue(UIVolumeRTPC, volume * 100);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        // 마우스 감도 설정 로직을 구현하세요.
    }
}
