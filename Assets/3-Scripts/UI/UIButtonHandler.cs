using UnityEngine;
using UnityEngine.UI;

public class UIButtonHandler : MonoBehaviour
{
    public Button changeSceneButton;
    public Button quitGameButton;
    public Button settingsButton;
    public int sceneIndexToLoad;

    void Start()
    {
        if (changeSceneButton != null)
        {
            changeSceneButton.onClick.AddListener(() => UIManager.Instance.ChangeScene(sceneIndexToLoad));
        }

        if (quitGameButton != null)
        {
            quitGameButton.onClick.AddListener(() => UIManager.Instance.QuitGame());
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(() => UIManager.Instance.OpenSettings());
        }
    }
}