using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class KeySpawner : MonoBehaviour
{
    [Header("References")]
    public Tilemap floorTilemap;                // tilemap representing walkable floor (use HasTile)
    public Tilemap itemsTilemap;                // optional tilemap used to place item GameObjects (for z-order)
    public Transform player;
    public Transform door;                      // optional; leave null if none

    [Header("Prefabs")]
    [Tooltip("Distinct key prefabs (one will be spawned per prefab). Each prefab should have KeyPickup attached.")]
    public List<GameObject> keyPrefabs;

    [Header("Spawn rules")]
    public LayerMask obstacleLayers;            // walls / colliders to avoid
    public float avoidRadiusPlayer = 3f;
    public float avoidRadiusDoor = 3f;
    public float avoidRadiusBetweenKeys = 2f;
    public int maxAttemptsPerKey = 200;

    [Header("Debug")]
    public bool debugSpawn = false;

    List<Vector2> spawnedPositions = new List<Vector2>();

    void Start()
    {
        SpawnAllKeys();
    }

    public void SpawnAllKeys()
    {
        spawnedPositions.Clear();

        if (floorTilemap == null)
        {
            Debug.LogError("KeySpawner: floorTilemap is not assigned.");
            return;
        }

        if (keyPrefabs == null || keyPrefabs.Count == 0)
        {
            Debug.LogWarning("KeySpawner: no keyPrefabs assigned.");
            return;
        }

        // Build candidate list of walkable cells
        List<Vector3Int> candidates = new List<Vector3Int>();
        BoundsInt bounds = floorTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                if (!floorTilemap.HasTile(cell)) continue;

                // If itemsTilemap provided, skip cells that already have an item tile
                if (itemsTilemap != null && itemsTilemap.HasTile(cell)) continue;

                // compute world position (align to itemsTilemap if available)
                Vector3 worldPos = (itemsTilemap != null) ? itemsTilemap.GetCellCenterWorld(cell) : floorTilemap.GetCellCenterWorld(cell);

                // small obstacle check
                if (Physics2D.OverlapCircle(worldPos, 0.2f, obstacleLayers)) continue;

                // avoid player/door proximity now so we don't waste attempts later
                if (player != null && Vector2.Distance(player.position, worldPos) < avoidRadiusPlayer) continue;
                if (door != null && Vector2.Distance(door.position, worldPos) < avoidRadiusDoor) continue;

                candidates.Add(cell);
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("KeySpawner: no candidate cells found for spawning keys. Check floorTilemap, bounds and obstacleLayers.");
            return;
        }

        // Try to place each prefab
        TilemapRenderer itemsRenderer = (itemsTilemap != null) ? itemsTilemap.GetComponent<TilemapRenderer>() : null;

        for (int i = 0; i < keyPrefabs.Count; i++)
        {
            var prefab = keyPrefabs[i];
            if (prefab == null)
            {
                Debug.LogWarning($"KeySpawner: keyPrefabs[{i}] is null, skipping.");
                continue;
            }

            bool placed = false;
            int attempts = 0;

            // copy of candidates index list for random picks
            List<int> remaining = new List<int>(candidates.Count);
            for (int k = 0; k < candidates.Count; k++) remaining.Add(k);

            while (remaining.Count > 0 && attempts < maxAttemptsPerKey)
            {
                attempts++;
                int idxInRemaining = Random.Range(0, remaining.Count);
                int candidateIndex = remaining[idxInRemaining];
                remaining.RemoveAt(idxInRemaining);

                Vector3Int cell = candidates[candidateIndex];
                Vector3 worldPos = (itemsTilemap != null) ? itemsTilemap.GetCellCenterWorld(cell) : floorTilemap.GetCellCenterWorld(cell);

                // avoid other spawned keys
                bool tooClose = false;
                foreach (var p in spawnedPositions)
                {
                    if (Vector2.Distance(p, worldPos) < avoidRadiusBetweenKeys) { tooClose = true; break; }
                }
                if (tooClose) continue;

                // final obstacle re-check (in case dynamic)
                if (Physics2D.OverlapCircle(worldPos, 0.2f, obstacleLayers)) continue;

                // instantiate
                Transform parent = (itemsTilemap != null) ? itemsTilemap.transform : transform;
                var go = Instantiate(prefab, worldPos, Quaternion.identity, parent);

                // align sorting so key renders above tilemap
                var sr = go.GetComponentInChildren<SpriteRenderer>();
                if (sr != null && itemsRenderer != null)
                {
                    sr.sortingLayerID = itemsRenderer.sortingLayerID;
                    sr.sortingOrder = itemsRenderer.sortingOrder + 1;
                }

                spawnedPositions.Add(worldPos);
                placed = true;
                if (debugSpawn) Debug.Log($"KeySpawner: placed {prefab.name} at {worldPos} after {attempts} attempts.");
                break;
            }

            if (!placed)
            {
                Debug.LogWarning($"KeySpawner: Failed to place key prefab {prefab.name} after {attempts} attempts.");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(player.position, avoidRadiusPlayer);
        }
        if (door != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(door.position, avoidRadiusDoor);
        }
        Gizmos.color = Color.magenta;
        foreach (var p in spawnedPositions) Gizmos.DrawWireSphere(p, avoidRadiusBetweenKeys);
    }
}