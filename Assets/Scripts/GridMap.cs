using UnityEngine;

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

        // sample obstacles (可删)
        for (int x = 5; x < 15; x++)
            SetTile(new Vector2Int(x, 10), TileType.Wall);
    }

}
