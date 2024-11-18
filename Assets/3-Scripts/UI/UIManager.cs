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
        // Singleton 패턴 설정
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // 필요시 주석 해제
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // 중복된 인스턴스는 삭제
        }
    }
    /// <summary>
    /// 즉시 씬을 변경하는 메서드
    /// </summary>
    /// <param name="sceneIndex">변경할 씬의 인덱스</param>
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
    /// 지정된 시간 후에 씬을 변경하는 메서드
    /// </summary>
    /// <param name="sceneIndex">변경할 씬의 인덱스</param>
    /// <param name="delaySeconds">지연 시간(초)</param>
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
    /// 코루틴: 지정된 시간 후에 씬을 변경
    /// </summary>
    /// <param name="sceneIndex">변경할 씬의 인덱스</param>
    /// <param name="delaySeconds">지연 시간(초)</param>
    /// <returns></returns>
    private IEnumerator ChangeSceneAfterDelay(int sceneIndex, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        ChangeScene(sceneIndex);
    }

    /// <summary>
    /// 씬 이름을 통해 즉시 씬을 변경하는 메서드
    /// </summary>
    /// <param name="sceneName">변경할 씬의 이름</param>
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
    /// 지정된 시간 후에 씬을 변경하는 메서드 (씬 이름 사용)
    /// </summary>
    /// <param name="sceneName">변경할 씬의 이름</param>
    /// <param name="delaySeconds">지연 시간(초)</param>
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
    /// 씬을 즉시 변경하는 래퍼 메서드 (매개변수 없음)
    /// </summary>
    public void ChangeSceneWrapper()
    {
        int sceneIndex = 1; // 변경할 씬의 인덱스를 여기서 설정
        ChangeScene(sceneIndex);
    }

    /// <summary>
    /// 지정된 시간 후에 씬을 변경하는 래퍼 메서드 (매개변수 없음)
    /// </summary>
    public void DelayedChangeSceneWrapper()
    {
        int sceneIndex = 1; // 변경할 씬의 인덱스를 여기서 설정
        float delaySeconds = 0.5f; // 지연 시간을 여기서 설정
        DelayedChangeScene(sceneIndex, delaySeconds);
    }

    /// <summary>
    /// 게임을 종료하는 메서드
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
    /// 설정 씬을 여는 메서드
    /// </summary>
    public void OpenSettings()
    {
        ChangeScene(2); // 설정 씬의 인덱스를 2로 가정
    }
}
