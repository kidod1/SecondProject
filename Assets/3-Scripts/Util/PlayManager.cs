using UnityEngine;
using UnityEngine.SceneManagement;
using AK.Wwise; // WWISE ���ӽ����̽� �߰�

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

        // ���� ������ Player ã��
        player = FindObjectOfType<Player>();

        // �� �ε� �̺�Ʈ ���
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // �� �ε� �̺�Ʈ ����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���ο� ���� �ε�� ������ Player�� ã���ϴ�.
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
    /// ��� WWISE ���带 �����մϴ�.
    /// </summary>
    public void StopAllSounds()
    {
        // WWISE�� ��� ���带 �����մϴ�.
        AkSoundEngine.StopAll();
        Debug.Log("��� WWISE ���尡 �����Ǿ����ϴ�.");
    }
}
