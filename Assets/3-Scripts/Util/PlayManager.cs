using System;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public static PlayManager I { get; private set; }

    private Player player;

    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �� �ı����� �ʵ��� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // �÷��̾� ��ü ���� ��������
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
