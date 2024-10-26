using System;
using Unity.VisualScripting;
using UnityEngine;

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
        }
        player = FindObjectOfType<Player>();

        if (player == null)
        {
            Debug.LogError("Player not found in the scene!");
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
            Debug.LogError("Player is not assigned!");
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
            Debug.LogError("Player is not assigned!");
            return null;
        }
    }

    /// <summary>
    /// �÷��̾ �׾����� ���θ� ó���ϴ� �޼���
    /// </summary>
    /// <param name="isDie">�÷��̾� ��� ����</param>
    /// <returns>isDie �� ��ȯ</returns>
    public void isPlayerDie()
    {
        isPlayerDied = true;
    }
}
