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
    /// 플레이어가 죽었는지 여부를 처리하는 메서드
    /// </summary>
    /// <param name="isDie">플레이어 사망 여부</param>
    /// <returns>isDie 값 반환</returns>
    public void isPlayerDie()
    {
        isPlayerDied = true;
    }
}
