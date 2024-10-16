using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TitleManager1 : MonoBehaviour
{
    [SerializeField]
    private GameObject whitePanel;     // 흰색 배경 패널
    [SerializeField]
    private Image teamLogoImage;       // 팀 로고 이미지
    [SerializeField]
    private AudioSource audioSource;   // 사운드를 재생할 AudioSource
    [SerializeField]
    private AudioClip teamLogoClip;    // 팀 로고 등장 시 재생할 오디오 클립
    [SerializeField]
    private float fadeDuration = 1f;   // 페이드 인/아웃 시간
    [SerializeField]
    private float logoDisplayTime = 1f;// 로고가 선명한 상태로 표시되는 시간

    void Start()
    {
        StartCoroutine(PlayTitleSequence());
    }

    // 타이틀 시퀀스 시작
    private IEnumerator PlayTitleSequence()
    {
        // 흰색 배경과 로고 이미지를 보여줌
        whitePanel.SetActive(true);
        teamLogoImage.color = new Color(teamLogoImage.color.r, teamLogoImage.color.g, teamLogoImage.color.b, 0f); // 시작 시 로고 투명하게

        // 페이드인 시작
        yield return StartCoroutine(FadeIn(teamLogoImage));

        // 팀 로고가 완전히 선명해졌을 때 사운드 재생
        PlaySound(teamLogoClip);

        // 사운드가 재생된 후 일정 시간 대기
        yield return new WaitForSeconds(logoDisplayTime);

        // 페이드아웃 시작
        yield return StartCoroutine(FadeOut(teamLogoImage));

        // 페이드아웃 후 패널 비활성화
        whitePanel.SetActive(false);
    }

    private IEnumerator FadeOut(Image image)
    {
        Color color = image.color;
        float startAlpha = color.a;
        for (float t = 0.0f; t < fadeDuration; t += Time.deltaTime)
        {
            float blend = 1.0f - Mathf.Clamp01(t / fadeDuration);
            color.a = blend * startAlpha;
            image.color = color;
            yield return null;
        }
        color.a = 0;
        image.color = color;
    }

    private IEnumerator FadeIn(Image image)
    {
        Color color = image.color;
        float startAlpha = color.a;

        for (float t = 0.0f; t < fadeDuration; t += Time.deltaTime)
        {
            float blend = Mathf.Clamp01(t / fadeDuration);
            color.a = blend * 1f;
            image.color = color;
            yield return null;
        }

        color.a = 1f;
        image.color = color;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
