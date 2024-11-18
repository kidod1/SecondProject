using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIManager>();
                if (_instance == null)
                {
                    GameObject uiManager = new GameObject("UIManager");
                    _instance = uiManager.AddComponent<UIManager>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // Singleton ���� ����
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // �ʿ�� �ּ� ����
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // �ߺ��� �ν��Ͻ��� ����
        }
    }
    /// <summary>
    /// ��� ���� �����ϴ� �޼���
    /// </summary>
    /// <param name="sceneIndex">������ ���� �ε���</param>
    public void ChangeScene(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndex);
            Time.timeScale = 1;
        }
        else
        {
            Debug.LogError("Invalid scene index");
        }
    }

    /// <summary>
    /// ������ �ð� �Ŀ� ���� �����ϴ� �޼���
    /// </summary>
    /// <param name="sceneIndex">������ ���� �ε���</param>
    /// <param name="delaySeconds">���� �ð�(��)</param>
    public void DelayedChangeScene(int sceneIndex, float delaySeconds)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            StartCoroutine(ChangeSceneAfterDelay(sceneIndex, delaySeconds));
        }
        else
        {
            Debug.LogError("Invalid scene index");
        }
    }

    /// <summary>
    /// �ڷ�ƾ: ������ �ð� �Ŀ� ���� ����
    /// </summary>
    /// <param name="sceneIndex">������ ���� �ε���</param>
    /// <param name="delaySeconds">���� �ð�(��)</param>
    /// <returns></returns>
    private IEnumerator ChangeSceneAfterDelay(int sceneIndex, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        ChangeScene(sceneIndex);
    }

    /// <summary>
    /// �� �̸��� ���� ��� ���� �����ϴ� �޼���
    /// </summary>
    /// <param name="sceneName">������ ���� �̸�</param>
    public void ChangeScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            Time.timeScale = 1;
        }
        else
        {
            Debug.LogError("Invalid scene name");
        }
    }

    /// <summary>
    /// ������ �ð� �Ŀ� ���� �����ϴ� �޼��� (�� �̸� ���)
    /// </summary>
    /// <param name="sceneName">������ ���� �̸�</param>
    /// <param name="delaySeconds">���� �ð�(��)</param>
    public void DelayedChangeScene(string sceneName, float delaySeconds)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            StartCoroutine(ChangeSceneAfterDelay(sceneName, delaySeconds));
        }
        else
        {
            Debug.LogError("Invalid scene name");
        }
    }

    private IEnumerator ChangeSceneAfterDelay(string sceneName, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        ChangeScene(sceneName);
    }

    /// <summary>
    /// ���� ��� �����ϴ� ���� �޼��� (�Ű����� ����)
    /// </summary>
    public void ChangeSceneWrapper()
    {
        int sceneIndex = 1; // ������ ���� �ε����� ���⼭ ����
        ChangeScene(sceneIndex);
    }

    /// <summary>
    /// ������ �ð� �Ŀ� ���� �����ϴ� ���� �޼��� (�Ű����� ����)
    /// </summary>
    public void DelayedChangeSceneWrapper()
    {
        int sceneIndex = 1; // ������ ���� �ε����� ���⼭ ����
        float delaySeconds = 0.5f; // ���� �ð��� ���⼭ ����
        DelayedChangeScene(sceneIndex, delaySeconds);
    }

    /// <summary>
    /// ������ �����ϴ� �޼���
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// ���� ���� ���� �޼���
    /// </summary>
    public void OpenSettings()
    {
        ChangeScene(2); // ���� ���� �ε����� 2�� ����
    }
}
