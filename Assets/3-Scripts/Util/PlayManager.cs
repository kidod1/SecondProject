using System;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public static PlayManager I { get; private set; }

    private Player player;

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 플레이어 객체 참조 가져오기
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
}
