using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject tilePrefab; // 타일 프리팹
    public int gridSize = 3; // 3x3 그리드
    public float tileSize = 10f; // 각 타일의 크기

    private Transform playerTransform;
    private Vector2Int playerGridPosition;
    private Dictionary<Vector2Int, GameObject> activeTiles = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        InitializeTiles();
    }

    void Update()
    {
        Vector2Int currentGridPos = GetGridPosition(playerTransform.position);
        if (currentGridPos != playerGridPosition)
        {
            playerGridPosition = currentGridPos;
            UpdateTiles();
        }
    }

    Vector2Int GetGridPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize);
        int y = Mathf.RoundToInt(position.z / tileSize); // Assuming Y is up
        return new Vector2Int(x, y);
    }

    void InitializeTiles()
    {
        playerGridPosition = GetGridPosition(playerTransform.position);
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int gridPos = new Vector2Int(playerGridPosition.x + x, playerGridPosition.y + y);
                SpawnTile(gridPos);
            }
        }
    }

    void UpdateTiles()
    {
        List<Vector2Int> requiredTiles = new List<Vector2Int>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int gridPos = new Vector2Int(playerGridPosition.x + x, playerGridPosition.y + y);
                requiredTiles.Add(gridPos);
                if (!activeTiles.ContainsKey(gridPos))
                {
                    SpawnTile(gridPos);
                }
            }
        }

        List<Vector2Int> tilesToRemove = new List<Vector2Int>();
        foreach (var tile in activeTiles.Keys)
        {
            if (!requiredTiles.Contains(tile))
            {
                tilesToRemove.Add(tile);
            }
        }

        foreach (var tilePos in tilesToRemove)
        {
            Destroy(activeTiles[tilePos]);
            activeTiles.Remove(tilePos);
        }
    }

    void SpawnTile(Vector2Int gridPos)
    {
        Vector3 position = new Vector3(gridPos.x * tileSize, 0, gridPos.y * tileSize);
        GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity, this.transform);
        activeTiles.Add(gridPos, newTile);
        Debug.Log($"TileManager: 타일 생성됨 - {gridPos}");
    }
}
