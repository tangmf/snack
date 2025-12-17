using UnityEngine;
using System.Collections.Generic;

public class GridMap : MonoBehaviour
{
    public enum TileType { Empty, Wall, KeyCircle, KeySquare, KeyTriangle, Door }

    [System.Serializable]
    public struct Tile
    {
        public TileType type;
        public bool blocksMovement;
        public bool blocksLight;
    }

    public int width = 30;
    public int height = 20;
    public float tileSize = 1f;

    private Tile[,] grid;

    void Awake()
    {
        EnsureGrid();
    }

    Tile MakeTile(TileType t)
    {
        Tile tile = new Tile();
        tile.type = t;
        tile.blocksMovement = (t == TileType.Wall);
        tile.blocksLight = (t == TileType.Wall);
        return tile;
    }

    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        int gx = Mathf.FloorToInt(worldPos.x / tileSize);
        int gy = Mathf.FloorToInt(worldPos.y / tileSize);
        return new Vector2Int(gx, gy);
    }

    public Vector2 GridToWorldCenter(Vector2Int cell)
    {
        return new Vector2(
            cell.x * tileSize + tileSize * 0.5f,
            cell.y * tileSize + tileSize * 0.5f
        );
    }

    public bool InBounds(Vector2Int cell)
        => cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;

    public Tile GetTile(Vector2Int cell)
    {
        if (!InBounds(cell)) return MakeTile(TileType.Wall);
        return grid[cell.x, cell.y];
    }

    public void SetTile(Vector2Int cell, TileType type)
    {
        if (!InBounds(cell)) return;
        grid[cell.x, cell.y] = MakeTile(type);
    }

    public bool BlocksMovement(Vector2Int cell) => GetTile(cell).blocksMovement;

    void OnDrawGizmos()
    {
        EnsureGrid();

        if (grid == null) return;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var cell = new Vector2Int(x, y);
                var t = grid[x, y].type;

                Vector3 center = new Vector3(x * tileSize + tileSize * 0.5f, y * tileSize + tileSize * 0.5f, 0);
                Vector3 size = new Vector3(tileSize, tileSize, 0);

                Gizmos.color = (t == TileType.Wall) ? Color.gray :
                               (t == TileType.Door) ? Color.green :
                               (t == TileType.KeyCircle || t == TileType.KeySquare || t == TileType.KeyTriangle) ? Color.yellow :
                               new Color(1, 1, 1, 0.05f);
                               
                Gizmos.DrawWireCube(center, size);

                if (t == TileType.Wall || t == TileType.Door || t == TileType.KeyCircle || t == TileType.KeySquare || t == TileType.KeyTriangle)
                    Gizmos.DrawCube(center, size * 0.9f);
            }
    }
    
    void EnsureGrid()
    {
        if (grid != null) return;

        grid = new Tile[width, height];

        // init empty
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            grid[x, y] = MakeTile(TileType.Empty);

        // border walls
        for (int x = 0; x < width; x++)
        {
            SetTile(new Vector2Int(x, 0), TileType.Wall);
            SetTile(new Vector2Int(x, height - 1), TileType.Wall);
        }
        for (int y = 0; y < height; y++)
        {
            SetTile(new Vector2Int(0, y), TileType.Wall);
            SetTile(new Vector2Int(width - 1, y), TileType.Wall);
        }

        // Door
        Vector2Int doorPos = new Vector2Int(Mathf.RoundToInt(width * 0.75f), height - 1);
        SetTile(doorPos, TileType.Door);

        // sample obstacles (可删)
        for (int x = 5; x < 15; x++)
            SetTile(new Vector2Int(x, 10), TileType.Wall);

        // Keys
        GenerateKeyPos(width, height, Mathf.Min(width, height) * 0.75f, doorPos);
    }

    Vector2Int[] GenerateKeyPos(int width, int height, float minDistance, Vector2Int doorPos)
    {
        List<Vector2Int> validPositions = new List<Vector2Int>();

        // Generate all possible valid positions on empty tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // Only add empty tiles to valid positions
                if (GetTile(pos).type == TileType.Empty)
                {
                    // Check distance from door
                    if (Vector2Int.Distance(pos, doorPos) >= minDistance)
                    {
                        validPositions.Add(pos);
                    }
                }
            }
        }

        Vector2Int[] keyPositions = new Vector2Int[3];
        System.Random rand = new System.Random();

        // Try to place 3 keys
        for (int i = 0; i < 3; i++)
        {
            bool validPos = false;

            while (!validPos && validPositions.Count > 0)
            {
                // Randomly select a valid position
                Vector2Int selectedPos = validPositions[rand.Next(validPositions.Count)];

                // Ensure key is far enough from other keys
                bool isFarEnough = true;
                for (int j = 0; j < i; j++)
                {
                    if (Vector2Int.Distance(selectedPos, keyPositions[j]) < minDistance)
                    {
                        isFarEnough = false;
                        break;
                    }
                }

                if (isFarEnough)
                {
                    validPos = true;
                    keyPositions[i] = selectedPos;
                    validPositions.Remove(selectedPos); // Remove the placed key position
                    RemoveInvalidPositions(selectedPos, minDistance, ref validPositions); // Remove all positions in range of the key
                    SetTile(selectedPos, (TileType)(rand.Next(3) + 2)); // Randomly set the key type
                }
            }
        }

        // Throw error if cannot spawn all 3 keys
        if (keyPositions.Length < 3)
        {
            throw new System.InvalidOperationException("Unable to spawn all 3 keys, try reducing minDistance or increase map size");
        }

        return keyPositions;
    }

    // Remove all positions within the minimum distance from a given position from the valid positions list
    void RemoveInvalidPositions(Vector2Int pos, float minDistance, ref List<Vector2Int> validPositions)
    {
        // Calculate the bounds of the square that contains the circle (radius = minDistance)
        int startX = Mathf.Max(0, pos.x - Mathf.CeilToInt(minDistance));
        int endX = Mathf.Min(width - 1, pos.x + Mathf.CeilToInt(minDistance));
        int startY = Mathf.Max(0, pos.y - Mathf.CeilToInt(minDistance));
        int endY = Mathf.Min(height - 1, pos.y + Mathf.CeilToInt(minDistance));

        // Loop over the bounding box
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector2Int checkPos = new Vector2Int(x, y);
                // Only remove the position if it's within the circular area
                if (Vector2Int.Distance(pos, checkPos) < minDistance)
                {
                    validPositions.Remove(checkPos);
                }
            }
        }
    }
}
