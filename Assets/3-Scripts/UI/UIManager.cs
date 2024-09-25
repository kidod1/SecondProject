using UnityEngine;
using UnityEngine.SceneManagement;

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
        // DontDestroyOnLoad을 제거하고, 중복된 UIManager를 파괴하지 않음
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // 중복된 인스턴스는 삭제
        }
    }

    private void Update()
    {
        // Esc 키가 눌렸을 때 게임 종료
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }

        // L 키가 눌렸을 때 지정된 씬으로 이동
        if (Input.GetKeyDown(KeyCode.L))
        {
            ChangeScene(6); // 씬 인덱스를 원하는 대로 변경
        }
    }

    public void ChangeScene(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError("Invalid scene index");
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenSettings()
    {
        ChangeScene(2);
    }
}
