using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private float restartHoldTime = 3f;
    private float restartTimer = 0f;
    private bool isRestarting = false;

    private void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            restartTimer += Time.deltaTime;
            if (restartTimer >= restartHoldTime && !isRestarting)
            {
                isRestarting = true;
                RestartScene();
            }
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            restartTimer = 0f;
            isRestarting = false;
        }
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
