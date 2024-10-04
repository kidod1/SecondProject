using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TitleManager1 : MonoBehaviour
{
    [SerializeField]
    private GameObject whitePanel;     // ��� ��� �г�
    [SerializeField]
    private Image logoImage;           // �ΰ� �̹���
    [SerializeField]
    private Image teamLogoImage;       // �� �ΰ� �̹���
    [SerializeField]
    private AudioSource audioSource;   // ���带 ����� AudioSource
    [SerializeField]
    private AudioClip teamLogoClip;    // �� �ΰ� ���� �� ����� ����� Ŭ��
    [SerializeField]
    private float fadeDuration;   // ���̵� �ƿ� �ð�
    [SerializeField]
    private float logoDisplayTime = 3f;// �ΰ� ǥ�õǴ� �ð�

    void Start()
    {
        StartCoroutine(PlayTitleSequence());
    }

    // Ÿ��Ʋ ������ ����
    private IEnumerator PlayTitleSequence()
    {
        // ��� ���� �ΰ� �̹����� ������
        whitePanel.SetActive(true);
        logoImage.gameObject.SetActive(true);
        teamLogoImage.gameObject.SetActive(false);

        // 3�� ���� �ΰ� ǥ��
        yield return new WaitForSeconds(logoDisplayTime);

        // �ΰ� ���̵� �ƿ�
        yield return StartCoroutine(FadeOut(logoImage));
        // �� �ΰ� �����ֱ�
        logoImage.gameObject.SetActive(false);
        teamLogoImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(fadeDuration);
        // �� �ΰ� ���� ���
        PlaySound(teamLogoClip);

        // ���� ��� �� 3�� ���
        yield return new WaitForSeconds(teamLogoClip.length + 3f);

        // �г� ��Ȱ��ȭ
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
