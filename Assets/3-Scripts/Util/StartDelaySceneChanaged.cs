using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AK.Wwise; // WWISE ���ӽ����̽� �߰�

public class StartDelaySceneChanged : MonoBehaviour
{
    [SerializeField]
    private int nextSceneNumber; // ���� ���� �̸��� �Ҵ�

    [SerializeField]
    private Image fadeOutImage; // ���̵� �ƿ��� ����� �̹���

    [SerializeField]
    private float fadeDuration = 1f; // ���̵� �ƿ��� �ɸ��� �ð�

    [Header("WWISE Events")]
    [SerializeField]
    private AK.Wwise.Event onFadeInCompleteEvent; // ���̵� �� �Ϸ� �� ������ WWISE �̺�Ʈ �߰�

    private void Start()
    {
        if (fadeOutImage == null)
        {
            Debug.LogError("FadeOutImage�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // ó���� �̹����� ���ĸ� 0���� ���� (���� ����)
        fadeOutImage.gameObject.SetActive(true);
        Color initialColor = fadeOutImage.color;
        initialColor.a = 0f;
        fadeOutImage.color = initialColor;

        // �ڷ�ƾ ����
        StartCoroutine(DelayedSceneChange());
    }

    private IEnumerator DelayedSceneChange()
    {
        // 3�� ���
        yield return new WaitForSeconds(3f);
        // ���̵� �� �Ϸ� �� WWISE �̺�Ʈ ����
        if (onFadeInCompleteEvent != null)
        {
            onFadeInCompleteEvent.Post(gameObject);
            Debug.Log("���̵� �� �Ϸ� �� WWISE �̺�Ʈ�� ����Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("onFadeInCompleteEvent�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // ���̵� �ƿ� ����
        yield return StartCoroutine(FadeOut());

        // ���� �� �ε�
        PlayManager.I.ChangeScene(nextSceneNumber);
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color color = fadeOutImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            color.a = alpha;
            fadeOutImage.color = color;
            yield return null;
        }

        // ���������� ���ĸ� 1�� �����Ͽ� ������ �������ϰ� ����
        color.a = 1f;
        fadeOutImage.color = color;
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeOutImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            color.a = alpha;
            fadeOutImage.color = color;
            yield return null;
        }

        // ���������� ���ĸ� 0���� �����Ͽ� ������ �����ϰ� ����
        color.a = 0f;
        fadeOutImage.color = color;
    }
}
