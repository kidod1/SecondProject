using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TitleManager1 : MonoBehaviour
{
    [SerializeField]
    private GameObject whitePanel;     // 흰색 배경 패널
    [SerializeField]
    private Image logoImage;           // 로고 이미지
    [SerializeField]
    private Image teamLogoImage;       // 팀 로고 이미지
    [SerializeField]
    private AudioSource audioSource;   // 사운드를 재생할 AudioSource
    [SerializeField]
    private AudioClip teamLogoClip;    // 팀 로고 등장 시 재생할 오디오 클립
    [SerializeField]
    private float fadeDuration;   // 페이드 아웃 시간
    [SerializeField]
    private float logoDisplayTime = 3f;// 로고가 표시되는 시간

    void Start()
    {
        StartCoroutine(PlayTitleSequence());
    }

    // 타이틀 시퀀스 시작
    private IEnumerator PlayTitleSequence()
    {
        // 흰색 배경과 로고 이미지를 보여줌
        whitePanel.SetActive(true);
        logoImage.gameObject.SetActive(true);
        teamLogoImage.gameObject.SetActive(false);

        // 3초 동안 로고 표시
        yield return new WaitForSeconds(logoDisplayTime);

        // 로고 페이드 아웃
        yield return StartCoroutine(FadeOut(logoImage));
        // 팀 로고 보여주기
        logoImage.gameObject.SetActive(false);
        teamLogoImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(fadeDuration);
        // 팀 로고 사운드 재생
        PlaySound(teamLogoClip);

        // 사운드 재생 후 3초 대기
        yield return new WaitForSeconds(teamLogoClip.length + 3f);

        // 패널 비활성화
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

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
