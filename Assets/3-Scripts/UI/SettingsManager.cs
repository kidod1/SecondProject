using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    public Slider voiceVolumeSlider;

    [Header("Control Settings")]
    public Slider mouseSensitivitySlider;

    private void Start()
    {
        // �׷��� ���� �ʱ�ȭ
        InitializeGraphicsSettings();

        // ����� ���� �ʱ�ȭ
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        voiceVolumeSlider.onValueChanged.AddListener(SetVoiceVolume);

        // ��Ʈ�� ���� �ʱ�ȭ
        mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
    }

    // �׷��� ���� �ʱ�ȭ
    void InitializeGraphicsSettings()
    {
        // �ػ� ���� �ʱ�ȭ
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (Resolution res in Screen.resolutions)
        {
            options.Add(res.width + "x" + res.height);
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);

        // ǰ�� ���� �ʱ�ȭ
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string> { "Low", "Medium", "High" });
        qualityDropdown.onValueChanged.AddListener(SetQuality);

        // ��ü ȭ�� ���� �ʱ�ȭ
        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    // �ػ� ����
    public void SetResolution(int index)
    {
        Resolution res = Screen.resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    // ǰ�� ����
    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    // ��ü ȭ�� ���
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // ������ ���� ����
    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    // ���� ���� ����
    public void SetMusicVolume(float volume)
    {
        // ���� ���� ���� ���� (��: AudioManager ���)
    }

    // ȿ���� ���� ����
    public void SetSFXVolume(float volume)
    {
        // ȿ���� ���� ���� ���� (��: AudioManager ���)
    }

    // ���� ���� ����
    public void SetVoiceVolume(float volume)
    {
        // ���� ���� ���� ���� (��: AudioManager ���)
    }

    // ���콺 ���� ����
    public void SetMouseSensitivity(float sensitivity)
    {
        // ���콺 ���� ���� ����
    }
}
