using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartDelaySceneChanged : MonoBehaviour
{
    [SerializeField]
    private string nextSceneName; // ���� ���� �̸��� �Ҵ�
    [SerializeField]
    private Image fadeOutImage; // ���̵� �ƿ��� ����� �̹���
    [SerializeField]
    private float fadeDuration = 1f; // ���̵� �ƿ��� �ɸ��� �ð�

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

        // ���̵� �ƿ� ����
        yield return StartCoroutine(FadeOut());

        // ���� �� �ε�
        SceneManager.LoadScene(nextSceneName);
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
}
