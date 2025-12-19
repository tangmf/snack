using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DoorManager : MonoBehaviour
{
    public Tilemap wallTilemap;
    public Tilemap floorTilemap;
    public Tile floorTile;

    // Hardcoded door position for simplicity
    public Vector3Int doorPosition = new Vector3Int(-7, 94, 0);

    void Start()
    {
        KeyManager.Instance.OnKeyCollected += CheckDoorOpen;
    }

    void OnDisable()
    {
        if (KeyManager.Instance != null)
            KeyManager.Instance.OnKeyCollected -= CheckDoorOpen;
    }

    void CheckDoorOpen(string id)
    {
        // Example: Open door if 3 keys collected
        if (KeyManager.Instance.CollectedCount >= 3)
        {
            OpenDoor();
        }
    }

    void OpenDoor()
    {
        // Change the tiles in the Tilemap to open the door
        wallTilemap.SetTile(doorPosition, null);
        floorTilemap.SetTile(doorPosition, floorTile);

        // 
    }
}
