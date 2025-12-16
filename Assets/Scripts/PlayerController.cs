using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public GridMap grid;

    public bool keyCircle, keySquare, keyTriangle;
    public bool isMoving = false;

    void Update()
    {
        // Debug.Log($"A={Input.GetKey(KeyCode.A)} D={Input.GetKey(KeyCode.D)} W={Input.GetKey(KeyCode.W)} S={Input.GetKey(KeyCode.S)}");

        if (grid == null) return;

        // 1) movement input (WASD)
        float ix = 0f;
        float iy = 0f;
        if (Input.GetKey(KeyCode.A)) ix -= 1f;
        if (Input.GetKey(KeyCode.D)) ix += 1f;
        if (Input.GetKey(KeyCode.W)) iy += 1f;
        if (Input.GetKey(KeyCode.S)) iy -= 1f;

        // Debug.Log($"A={Input.GetKey(KeyCode.A)} D={Input.GetKey(KeyCode.D)} W={Input.GetKey(KeyCode.W)} S={Input.GetKey(KeyCode.S)}");

        Vector2 move = new Vector2(ix, iy);
        isMoving = move.sqrMagnitude > 0f;
        if (move.sqrMagnitude > 1f) move.Normalize();

        Vector2 pos = transform.position;
        Vector2 nextPos = pos + move * speed * Time.deltaTime;

        // 2) grid collision
        Vector2Int cell = grid.WorldToGrid(nextPos);
        if (!grid.BlocksMovement(cell))
            transform.position = nextPos;

        // 3) face mouse (for torch later)
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 faceDir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;

        // simple visual: rotate player to face mouse (optional)
        float angle = Mathf.Atan2(faceDir.y, faceDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 4) pickup key by current cell
        Vector2Int curCell = grid.WorldToGrid(transform.position);
        var tile = grid.GetTile(curCell);

        if (tile.type == GridMap.TileType.KeyCircle) { keyCircle = true; grid.SetTile(curCell, GridMap.TileType.Empty); }
        if (tile.type == GridMap.TileType.KeySquare) { keySquare = true; grid.SetTile(curCell, GridMap.TileType.Empty); }
        if (tile.type == GridMap.TileType.KeyTriangle) { keyTriangle = true; grid.SetTile(curCell, GridMap.TileType.Empty); }
    }
}

