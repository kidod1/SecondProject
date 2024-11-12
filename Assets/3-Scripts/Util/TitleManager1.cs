using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ÿ��Ʋ �Ŵ����� ������ Ÿ��Ʋ �������� �����մϴ�.
/// </summary>
public class TitleManager1 : MonoBehaviour
{
    [SerializeField]
    private GameObject whitePanel;     // ��� ��� �г�
    [SerializeField]
    private Image teamLogoImage;       // �� �ΰ� �̹���
    [SerializeField]
    private float fadeDuration = 1f;   // ���̵� ��/�ƿ� �ð�
    [SerializeField]
    private float logoDisplayTime = 1f;// �ΰ� ������ ���·� ǥ�õǴ� �ð�
    [SerializeField]
    private TitleSoundManager titleSoundManager;

    void Start()
    {
        StartCoroutine(PlayTitleSequence());
    }

    // Ÿ��Ʋ ������ ����
    private IEnumerator PlayTitleSequence()
    {
        // ��� ���� �ΰ� �̹����� ������
        whitePanel.SetActive(true);
        teamLogoImage.color = new Color(teamLogoImage.color.r, teamLogoImage.color.g, teamLogoImage.color.b, 0f); // ���� �� �ΰ� �����ϰ�

        // ���̵��� ����
        yield return StartCoroutine(FadeIn(teamLogoImage));
        // �� �ΰ� ������ ���������� �� �ΰ� ���� ���
        titleSoundManager.PlayLogoSound();

        // ���尡 ����� �� ���� �ð� ���
        yield return new WaitForSeconds(logoDisplayTime);

        // ���̵�ƿ� ����
        yield return StartCoroutine(FadeOut(teamLogoImage));

        // ���̵�ƿ� �� �г� ��Ȱ��ȭ
        whitePanel.SetActive(false);

        // Ÿ��Ʋ ���� ���
        titleSoundManager.PlayTitleSound();
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
}
