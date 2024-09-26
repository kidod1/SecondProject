using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private Image loadingBarFill;  // �ε� ���� �̹��� (Fill Ÿ��)
    [SerializeField]
    private float loadingDuration = 5f;  // �ε� �ٰ� �� ���� �ð� (��)
    [SerializeField]
    private int nextSceneIndex;  // ��ȯ�� ���� ���� �ε��� ��ȣ
    [SerializeField]
    private GameObject pressSpaceText;  // �ε� �Ϸ� �� ��Ÿ�� �ؽ�Ʈ UI
    [SerializeField]
    private GameObject loadingImage;  // �ε� �߿� ǥ�õ� �̹���

    private bool isLoadingComplete = false;

    private void Start()
    {
        if (loadingBarFill != null)
        {
            loadingBarFill.fillAmount = 0f;  // �ε� �� �ʱ�ȭ
            StartCoroutine(FillLoadingBar());  // �ε� �� ä��� ����
        }
        else
        {
            Debug.LogError("LoadingScreen: �ε� �� �̹����� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (pressSpaceText != null)
        {
            pressSpaceText.SetActive(false);  // �ؽ�Ʈ UI ��Ȱ��ȭ
        }
        else
        {
            Debug.LogWarning("LoadingScreen: pressSpaceText�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (loadingImage != null)
        {
            loadingImage.SetActive(true);  // �ε� �� ǥ�õ� �̹��� Ȱ��ȭ
        }
        else
        {
            Debug.LogWarning("LoadingScreen: �ε� �� �̹����� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    private void Update()
    {
        if (isLoadingComplete && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(nextSceneIndex);  // ���� ������ ��ȯ
        }
    }

    private IEnumerator FillLoadingBar()
    {
        float elapsedTime = 0f;

        while (elapsedTime < loadingDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float fillAmount = Mathf.Clamp01(elapsedTime / loadingDuration);
            loadingBarFill.fillAmount = fillAmount;

            yield return null;
        }

        isLoadingComplete = true;

        // �ε��� ������ �ε� �̹����� ��Ȱ��ȭ
        if (loadingImage != null)
        {
            loadingImage.SetActive(false);  // �ε� �� �̹��� ��Ȱ��ȭ
        }
        else
        {
            Debug.LogWarning("LoadingScreen: �ε� �� �̹����� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (pressSpaceText != null)
        {
            pressSpaceText.SetActive(true);  // �ε� �Ϸ� �� �ؽ�Ʈ UI Ȱ��ȭ
        }
        else
        {
            Debug.LogWarning("LoadingScreen: pressSpaceText�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }
}
