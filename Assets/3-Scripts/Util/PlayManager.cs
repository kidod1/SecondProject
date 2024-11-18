using UnityEngine;
using UnityEngine.SceneManagement;
using AK.Wwise; // WWISE 네임스페이스 추가

public class PlayManager : MonoBehaviour
{
    public static PlayManager I { get; private set; }

    private Player player;
    public bool isPlayerDied = false;

    private void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 현재 씬에서 Player 찾기
        player = FindObjectOfType<Player>();

        // 씬 로드 이벤트 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 씬 로드 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새로운 씬이 로드될 때마다 Player를 찾습니다.
        player = FindObjectOfType<Player>();

        if (player == null)
        {
            Debug.Log("Player not found in the scene. This scene might not require a Player.");
        }
        else
        {
            Debug.Log("Player found in the scene.");
        }
    }

    public Vector2 GetPlayerPosition()
    {
        if (player != null)
        {
            return player.PlayerPosition;
        }
        else
        {
            Debug.LogWarning("Player is not assigned!");
            return Vector2.zero;
        }
    }

    public Player GetPlayer()
    {
        if (player != null)
        {
            return player;
        }
        else
        {
            Debug.LogWarning("Player is not assigned!");
            return null;
        }
    }

    public void isPlayerDie()
    {
        isPlayerDied = true;
    }

    /// <summary>
    /// 모든 WWISE 사운드를 중지합니다.
    /// </summary>
    public void StopAllSounds()
    {
        // WWISE의 모든 사운드를 중지합니다.
        AkSoundEngine.StopAll();
        Debug.Log("모든 WWISE 사운드가 중지되었습니다.");
    }
}
