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

    // WWISE RTPC 이름 상수 정의
    private const string MasterVolumeRTPC = "Master_Volume";
    private const string MusicVolumeRTPC = "Music_Volume";
    private const string SFXVolumeRTPC = "SFX_Volume";

    private void Start()
    {
        // 그래픽 설정 초기화
        InitializeGraphicsSettings();

        // 오디오 설정 초기화
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        // 초기 볼륨 값 설정 (필요에 따라 조정)
        masterVolumeSlider.value = 1.0f;
        musicVolumeSlider.value = 1.0f;
        sfxVolumeSlider.value = 1.0f;
    }

    // 그래픽 설정 초기화
    void InitializeGraphicsSettings()
    {
        // 해상도 설정 초기화
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (Resolution res in Screen.resolutions)
        {
            options.Add(res.width + "x" + res.height);
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);

        // 품질 설정 초기화
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string> { "Low", "Medium", "High" });
        qualityDropdown.onValueChanged.AddListener(SetQuality);

        // 전체 화면 설정 초기화
        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    // 해상도 변경
    public void SetResolution(int index)
    {
        Resolution res = Screen.resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
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

    public void SetMouseSensitivity(float sensitivity)
    {
        // 마우스 감도 설정 로직을 구현하세요.
    }
}
