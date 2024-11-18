using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using AK.Wwise; // WWISE ���ӽ����̽� �߰�

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

    // WWISE RTPC �̸� ��� ����
    private const string MasterVolumeRTPC = "Master_Volume";
    private const string MusicVolumeRTPC = "Music_Volume";
    private const string SFXVolumeRTPC = "SFX_Volume";

    private void Start()
    {
        // �׷��� ���� �ʱ�ȭ
        InitializeGraphicsSettings();

        // ����� ���� �ʱ�ȭ
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            masterVolumeSlider.value = 1.0f;
        }
        else
        {
            Debug.LogWarning("MasterVolumeSlider�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            musicVolumeSlider.value = 1.0f;
        }
        else
        {
            Debug.LogWarning("MusicVolumeSlider�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            sfxVolumeSlider.value = 1.0f;
        }
        else
        {
            Debug.LogWarning("SFXVolumeSlider�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    // �׷��� ���� �ʱ�ȭ
    void InitializeGraphicsSettings()
    {
        // �ػ� ���� �ʱ�ȭ
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
            Debug.LogWarning("ResolutionDropdown�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // ǰ�� ���� �ʱ�ȭ
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string> { "Low", "Medium", "High" });
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }
        else
        {
            Debug.LogWarning("QualityDropdown�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // ��ü ȭ�� ���� �ʱ�ȭ
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }
        else
        {
            Debug.LogWarning("FullscreenToggle�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    // �ػ� ����
    public void SetResolution(int index)
    {
        if (Screen.resolutions.Length > index)
        {
            Resolution res = Screen.resolutions[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        }
        else
        {
            Debug.LogWarning("�ػ� �ε����� ��ȿ���� �ʽ��ϴ�.");
        }
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
        AkSoundEngine.SetRTPCValue(MasterVolumeRTPC, volume * 100);
    }

    // ���� ���� ����
    public void SetMusicVolume(float volume)
    {
        AkSoundEngine.SetRTPCValue(MusicVolumeRTPC, volume * 100);
    }

    // ȿ���� ���� ����
    public void SetSFXVolume(float volume)
    {
        AkSoundEngine.SetRTPCValue(SFXVolumeRTPC, volume * 100);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        // ���콺 ���� ���� ������ �����ϼ���.
    }
}
