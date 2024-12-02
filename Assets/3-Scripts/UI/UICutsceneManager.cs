using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

// WWISE ���ӽ����̽� �߰� (�ʿ� ��)
using AK.Wwise;

public class UICutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneFrame
    {
        public Sprite sprite; // ǥ���� ��������Ʈ
        public Image image;   // ��������Ʈ�� �Ҵ�� Image ������Ʈ

        [Header("WWISE Events")]
        public AK.Wwise.Event frameSound; // ������ ǥ�� �� ����� WWISE �̺�Ʈ
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
    private int loadSceneNumber;   // �ƽ� ���� �� �ε��� �� �̸�

    [SerializeField]
    private SceneChangeSkeleton sceneChangeSkeleton; // SceneChangeSkeleton ���� (�ν����Ϳ��� �Ҵ�)

    [Header("Transition Options")]
    [SerializeField]
    private bool usePlayCloseAnimation = true; // true: PlayCloseAnimation ���, false: SceneManager.LoadScene ���

    // WWISE �̺�Ʈ�� �ν����Ϳ��� �Ҵ��� �� �ֵ��� ����
    [Header("WWISE Events")]
    [SerializeField]
    private AK.Wwise.Event pageTransitionSound; // ������ ��ȯ �� ����� WWISE �̺�Ʈ

    [Header("Scene Transition Events")]
    [Tooltip("�� ��ȯ ���� ȣ��Ǵ� �̺�Ʈ")]
    [SerializeField]
    private UnityEvent OnBeforeSceneTransition; // �� ��ȯ ���� ȣ��� UnityEvent �߰�

    [Header("Scene Transition Events")]
    [Tooltip("�� ��ȯ �� ȣ��Ǵ� �̺�Ʈ")]
    [SerializeField]
    private UnityEvent OnAfterSceneTransition; // �� ��ȯ �� ȣ��� UnityEvent �߰�

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
            TransitionToScene();
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

        // ������ ǥ�� �� WWISE ȿ���� ���
        if (frame.frameSound != null)
        {
            frame.frameSound.Post(gameObject);
        }
        else
        {
            Debug.LogWarning("�����ӿ� �Ҵ�� WWISE ȿ������ �����ϴ�.");
        }

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

        // WWISE ȿ���� ��� (������ ��ȯ ��)
        if (pageTransitionSound != null)
        {
            pageTransitionSound.Post(gameObject);
        }
        else
        {
            Debug.LogWarning("pageTransitionSound��(��) �Ҵ���� �ʾҽ��ϴ�.");
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
            TransitionToScene();
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

    private void TransitionToScene()
    {
        if (sceneChangeSkeleton != null)
        {
            // �� ��ȯ ���� �̺�Ʈ ȣ��
            InvokeBeforeSceneTransitionEvent();

            if (usePlayCloseAnimation)
            {
                sceneChangeSkeleton.PlayCloseAnimation(loadSceneNumber);
            }
            else
            {
                SceneManager.LoadScene(loadSceneNumber);
            }

            // �� ��ȯ �� �̺�Ʈ ȣ�� (�� ��ȯ�� �񵿱������� �Ͼ�� ���, �� �ε� �Ϸ� �� ȣ���ϵ��� ������ ���� �ֽ��ϴ�.)
            InvokeAfterSceneTransitionEvent();
        }
        else
        {
            Debug.LogError("SceneChangeSkeleton�� �Ҵ���� �ʾҰų� loadSceneName�� �������� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// �� ��ȯ ���� ȣ��Ǵ� UnityEvent�� ȣ���մϴ�.
    /// </summary>
    private void InvokeBeforeSceneTransitionEvent()
    {
        if (OnBeforeSceneTransition != null)
        {
            OnBeforeSceneTransition.Invoke();
            Debug.Log("UICutsceneManager: OnBeforeSceneTransition �̺�Ʈ�� ȣ��Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("UICutsceneManager: OnBeforeSceneTransition �̺�Ʈ�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// �� ��ȯ �� ȣ��Ǵ� UnityEvent�� ȣ���մϴ�.
    /// </summary>
    private void InvokeAfterSceneTransitionEvent()
    {
        if (OnAfterSceneTransition != null)
        {
            OnAfterSceneTransition.Invoke();
            Debug.Log("UICutsceneManager: OnAfterSceneTransition �̺�Ʈ�� ȣ��Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("UICutsceneManager: OnAfterSceneTransition �̺�Ʈ�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// ���� �ε�� �� ȣ��Ǵ� �� �ε� �Ϸ� �̺�Ʈ�� ���� ����� �� �ִ� �޼���
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���� �ε�� �� AfterTransition �̺�Ʈ ȣ��
        InvokeAfterSceneTransitionEvent();
    }

    /// <summary>
    /// �� ��ȯ�� �Ϸ�Ǿ����� Ȯ���ϱ� ���� �� �ε� �Ϸ� �̺�Ʈ ���
    /// </summary>
    // ������ InvokeAfterSceneTransitionEvent�� ȣ���ϴ� �κ��� OnSceneLoaded�� �̵���ŵ�ϴ�.
    // �̸� ���� ���� ������ �ε�� �� �̺�Ʈ�� ȣ��˴ϴ�.
    private void TransitionToScene_correct()
    {
        if (sceneChangeSkeleton != null)
        {
            // �� ��ȯ ���� �̺�Ʈ ȣ��
            InvokeBeforeSceneTransitionEvent();

            if (usePlayCloseAnimation)
            {
                sceneChangeSkeleton.PlayCloseAnimation(loadSceneNumber);
            }
            else
            {
                PlayManager.I.ChangeScene(loadSceneNumber);
            }

            // ������ AfterSceneTransition �̺�Ʈ ȣ���� �����ϰ�, OnSceneLoaded���� ȣ���ϵ��� �մϴ�.
        }
        else
        {
            Debug.LogError("SceneChangeSkeleton�� �Ҵ���� �ʾҰų� loadSceneName�� �������� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// ���� �ε�� �� ȣ��Ǵ� �� �ε� �Ϸ� �̺�Ʈ�� ���� ����� �� �ִ� �޼���
    /// </summary>
    private void TransitionToScene_final()
    {
        if (sceneChangeSkeleton != null)
        {
            // �� ��ȯ ���� �̺�Ʈ ȣ��
            InvokeBeforeSceneTransitionEvent();

            if (usePlayCloseAnimation)
            {
                sceneChangeSkeleton.PlayCloseAnimation(loadSceneNumber);
            }
            else
            {
                PlayManager.I.ChangeScene(loadSceneNumber);
            }

            // OnSceneLoaded���� AfterTransition �̺�Ʈ ȣ��
        }
        else
        {
            Debug.LogError("SceneChangeSkeleton�� �Ҵ���� �ʾҰų� loadSceneName�� �������� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// �� ��ȯ �Ŀ� �߰����� ������ �����ϰ� �ʹٸ� OnSceneLoaded���� ó���� �� �ֽ��ϴ�.
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded_final(Scene scene, LoadSceneMode mode)
    {
        // ���� �ε�� �� AfterTransition �̺�Ʈ ȣ��
        InvokeAfterSceneTransitionEvent();
    }


    /// <summary>
    /// ��� �������� �Ϸ�� �� �� ��ȯ�� ó���մϴ�.
    /// </summary>
    private void TransitionToScene_correct_final()
    {
        if (sceneChangeSkeleton != null)
        {
            // �� ��ȯ ���� �̺�Ʈ ȣ��
            InvokeBeforeSceneTransitionEvent();

            if (usePlayCloseAnimation)
            {
                sceneChangeSkeleton.PlayCloseAnimation(loadSceneNumber);
            }
            else
            {
                PlayManager.I.ChangeScene(loadSceneNumber);
            }

            // AfterTransition �̺�Ʈ�� OnSceneLoaded���� ȣ��˴ϴ�.
        }
        else
        {
            Debug.LogError("SceneChangeSkeleton�� �Ҵ���� �ʾҰų� loadSceneName�� �������� �ʾҽ��ϴ�.");
        }
    }
}
