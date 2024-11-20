using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UICutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneFrame
    {
        public Sprite sprite; // ǥ���� ��������Ʈ
        public Image image;   // ��������Ʈ�� �Ҵ�� Image ������Ʈ
    }

    [System.Serializable]
    public class CutscenePage
    {
        public CutsceneFrame[] frames; // �� �������� ���� �ƽ� �����ӵ�
    }

    [SerializeField]
    private CutscenePage[] cutscenePages; // �������� �ƽ� ������ �迭
    [SerializeField]
    private float fadeDuration = 1f;     // ���̵� ��/�ƿ� ���� �ð�
    [SerializeField]
    private float initialDelay = 2f;     // �� ���� �� ù �� ���� �� ���� �ð�
    [SerializeField]
    private string loadSceneName = "";   // �ƽ� ���� �� �ε��� �� �̸�

    [SerializeField]
    private SceneChangeSkeleton sceneChangeSkeleton; // SceneChangeSkeleton ���� (�ν����Ϳ��� �Ҵ�)

    private int currentPageIndex = 0;       // ���� ������ �ε���
    private int currentFrameIndex = 0;      // ���� ������ �ε���
    private bool isTransitioning = false;   // ���� ��ȯ ������ ����

    private void Start()
    {
        // �ʱ� ����: ��� Image�� �����ϰ� �����ϰ� ��Ȱ��ȭ
        foreach (var page in cutscenePages)
        {
            foreach (var frame in page.frames)
            {
                if (frame.image != null)
                {
                    frame.image.color = new Color(frame.image.color.r, frame.image.color.g, frame.image.color.b, 0f);
                    frame.image.gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("CutsceneFrame.image�� �Ҵ���� �ʾҽ��ϴ�.");
                }
            }
        }

        // �ƽ� ������ �迭 Ȯ��
        if (cutscenePages.Length > 0)
        {
            // �ʱ� ���� �� ù �ƽ� ����
            StartCoroutine(InitialDelayAndStart());
        }
        else
        {
            Debug.LogWarning("CutscenePages �迭�� ����ֽ��ϴ�.");
        }
    }

    private IEnumerator InitialDelayAndStart()
    {
        yield return new WaitForSeconds(initialDelay);
        // ù ������ ǥ��
        ShowNextFrame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTransitioning)
        {
            // ���� ������ �Ǵ� ������ ��ȯ�� ���� �����̽��� �Է� ó��
            ShowNextFrame();
        }
    }

    private void ShowNextFrame()
    {
        if (currentPageIndex >= cutscenePages.Length)
        {
            // ��� �������� ������ �� ��ȯ
            if (sceneChangeSkeleton != null && !string.IsNullOrEmpty(loadSceneName))
            {
                sceneChangeSkeleton.PlayCloseAnimation(loadSceneName);
            }
            else
            {
                Debug.LogError("SceneChangeSkeleton�� �Ҵ���� �ʾҰų� loadSceneName�� �������� �ʾҽ��ϴ�.");
            }
            return;
        }

        CutscenePage currentPage = cutscenePages[currentPageIndex];

        if (currentFrameIndex < currentPage.frames.Length)
        {
            // ���� ������ ǥ��
            StartCoroutine(FadeInFrame(currentPage.frames[currentFrameIndex]));
            currentFrameIndex++;
        }
        else
        {
            // ���� �������� ��� �������� ǥ�õ� ���¿��� �����̽��� �Է� �� ������ ��ȯ
            StartCoroutine(TransitionToNextPage());
        }
    }

    private IEnumerator FadeInFrame(CutsceneFrame frame)
    {
        if (frame.image == null)
        {
            yield break;
        }

        frame.image.sprite = frame.sprite;
        frame.image.gameObject.SetActive(true);

        float elapsed = 0f;
        Color color = frame.image.color;
        color.a = 0f;
        frame.image.color = color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            color.a = alpha;
            frame.image.color = color;
            yield return null;
        }

        color.a = 1f;
        frame.image.color = color;
    }

    private IEnumerator TransitionToNextPage()
    {
        isTransitioning = true;

        // ���� �������� ��� ������ ���̵� �ƿ�
        CutscenePage currentPage = cutscenePages[currentPageIndex];
        foreach (var frame in currentPage.frames)
        {
            if (frame.image != null && frame.image.gameObject.activeSelf)
            {
                StartCoroutine(FadeOutFrame(frame));
            }
        }

        // ���̵� �ƿ��� �Ϸ�� ������ ���
        yield return new WaitForSeconds(fadeDuration);

        // ��� ������ ��Ȱ��ȭ
        foreach (var frame in currentPage.frames)
        {
            if (frame.image != null)
            {
                frame.image.gameObject.SetActive(false);
            }
        }

        // �߰� ��� �ð� (2��)
        yield return new WaitForSeconds(2f);

        // ���� �������� �̵�
        currentPageIndex++;
        currentFrameIndex = 0;

        if (currentPageIndex < cutscenePages.Length)
        {
            // ���� �������� ù �������� �غ� (����ڰ� �����̽��ٸ� ���� ������ ���)
            isTransitioning = false;
        }
        else
        {
            // ��� �������� ������ �� ��ȯ
            if (sceneChangeSkeleton != null && !string.IsNullOrEmpty(loadSceneName))
            {
                sceneChangeSkeleton.PlayCloseAnimation(loadSceneName);
            }
            else
            {
                Debug.LogError("SceneChangeSkeleton�� �Ҵ���� �ʾҰų� loadSceneName�� �������� �ʾҽ��ϴ�.");
            }
        }

        isTransitioning = false;
    }

    private IEnumerator FadeOutFrame(CutsceneFrame frame)
    {
        float elapsed = 0f;
        Color color = frame.image.color;
        color.a = 1f;
        frame.image.color = color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            color.a = alpha;
            frame.image.color = color;
            yield return null;
        }

        color.a = 0f;
        frame.image.color = color;
    }
}
