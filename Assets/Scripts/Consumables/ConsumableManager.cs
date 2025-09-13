using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableManager : MonoBehaviour
{
    [Header("Consumable Settings")]
    [SerializeField]
    private GameObject[] consumablePrefabs; // Array of different consumable prefabs
    [SerializeField]
    private float healAmount = 20f;
    [SerializeField]
    private float spawnInterval = 10f; // Spawn every 10 seconds
    [SerializeField]
    private int maxConsumablesInScene = 3;

    [Header("Spawn Area Settings")]
    [SerializeField]
    private float spawnHeight = 0f; // Same Y position for all spawns
    [SerializeField]
    private float minX = -10f; // Minimum X position
    [SerializeField]
    private float maxX = 10f;  // Maximum X position
    [SerializeField]
    private float spawnZ = 0f; // Z position (for 2D usually 0)

    [Header("Auto-Create Consumable")]
    [SerializeField]
    private bool autoCreateConsumable = true; // Auto-create if no prefab assigned

    private List<GameObject> activeConsumables = new List<GameObject>();
    private PlayerManager playerManager;

    void Start()
    {
        // Find the player
        playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("ConsumableManager: No PlayerManager found in scene!");
            return;
        }

        // Create consumable prefabs if none assigned
        if ((consumablePrefabs == null || consumablePrefabs.Length == 0) && autoCreateConsumable)
        {
            CreateConsumablePrefabs();
        }

        // Start spawning consumables
        StartCoroutine(SpawnConsumablesRoutine());
        
        Debug.Log("ConsumableManager initialized!");
        Debug.Log($"Spawn area: X({minX} to {maxX}), Y({spawnHeight}), Z({spawnZ})");
    }

    void CreateConsumablePrefabs()
    {
        // Create two different consumable prefabs
        GameObject[] tempPrefabs = new GameObject[2];
        
        // Create first consumable (Green cube - Health)
        tempPrefabs[0] = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tempPrefabs[0].name = "HealthConsumable_Prefab";
        tempPrefabs[0].transform.localScale = Vector3.one * 0.5f;
        
        Renderer renderer1 = tempPrefabs[0].GetComponent<Renderer>();
        if (renderer1 != null && renderer1.material != null)
        {
            renderer1.material.color = Color.green;
        }
        
        tempPrefabs[0].AddComponent<HealthConsumable>();
        
        // Remove 3D collider and add 2D collider
        Collider collider3D1 = tempPrefabs[0].GetComponent<Collider>();
        if (collider3D1 != null) DestroyImmediate(collider3D1);
        
        BoxCollider2D collider2D1 = tempPrefabs[0].AddComponent<BoxCollider2D>();
        collider2D1.isTrigger = true;
        
        // Create second consumable (Blue sphere - Different type)
        tempPrefabs[1] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tempPrefabs[1].name = "MegaHealthConsumable_Prefab";
        tempPrefabs[1].transform.localScale = Vector3.one * 0.6f;
        
        Renderer renderer2 = tempPrefabs[1].GetComponent<Renderer>();
        if (renderer2 != null && renderer2.material != null)
        {
            renderer2.material.color = Color.blue;
        }
        
        tempPrefabs[1].AddComponent<HealthConsumable>();
        
        // Remove 3D collider and add 2D collider
        Collider collider3D2 = tempPrefabs[1].GetComponent<Collider>();
        if (collider3D2 != null) DestroyImmediate(collider3D2);
        
        // Use CircleCollider2D instead of SphereCollider2D
        CircleCollider2D collider2D2 = tempPrefabs[1].AddComponent<CircleCollider2D>();
        collider2D2.isTrigger = true;
        
        // Hide both prefabs
        for (int i = 0; i < tempPrefabs.Length; i++)
        {
            tempPrefabs[i].SetActive(false);
            tempPrefabs[i].transform.position = new Vector3(1000, 1000, 1000);
            tempPrefabs[i].transform.SetParent(transform);
        }
        
        consumablePrefabs = tempPrefabs;
        Debug.Log("Auto-created 2 consumable prefabs (Green cube, Blue sphere)!");
    }

    IEnumerator SpawnConsumablesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            // Remove null references from destroyed consumables
            activeConsumables.RemoveAll(item => item == null);
            
            // Spawn new consumable if under limit
            if (activeConsumables.Count < maxConsumablesInScene)
            {
                SpawnConsumableAtRandomLocation();
            }
        }
    }

    void SpawnConsumableAtRandomLocation()
    {
        if (consumablePrefabs == null || consumablePrefabs.Length == 0)
        {
            Debug.LogWarning("ConsumableManager: No consumable prefabs available!");
            return;
        }

        // Generate random X position within the specified range
        float randomX = Random.Range(minX, maxX);
        
        // Create spawn position with fixed Y and Z
        Vector3 spawnPosition = new Vector3(randomX, spawnHeight, spawnZ);
        
        // Choose random consumable prefab
        GameObject selectedPrefab = consumablePrefabs[Random.Range(0, consumablePrefabs.Length)];
        
        // Instantiate consumable
        GameObject newConsumable = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        newConsumable.SetActive(true);
        
        // Rename for clarity
        newConsumable.name = $"{selectedPrefab.name.Replace("_Prefab", "")}_{activeConsumables.Count}";
        
        // Add to active list
        activeConsumables.Add(newConsumable);
        
        // Set up the health consumable component
        HealthConsumable healthComp = newConsumable.GetComponent<HealthConsumable>();
        if (healthComp == null)
        {
            healthComp = newConsumable.AddComponent<HealthConsumable>();
        }
        
        // Different heal amounts based on prefab type
        float currentHealAmount = healAmount;
        if (selectedPrefab.name.Contains("Mega"))
        {
            currentHealAmount = healAmount * 2; // Blue sphere heals more
        }
        
        healthComp.Initialize(currentHealAmount, playerManager);
        
        Debug.Log($"Spawned '{newConsumable.name}' at ({randomX:F1}, {spawnHeight}, {spawnZ}) - Heal: {currentHealAmount}");
    }

    // Public method to spawn consumable manually (for testing)
    public void SpawnConsumableNow()
    {
        SpawnConsumableAtRandomLocation();
    }

    // Visualize spawn area in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        
        // Draw spawn line
        Vector3 leftPoint = new Vector3(minX, spawnHeight, spawnZ);
        Vector3 rightPoint = new Vector3(maxX, spawnHeight, spawnZ);
        
        Gizmos.DrawLine(leftPoint, rightPoint);
        
        // Draw markers at endpoints
        Gizmos.DrawWireCube(leftPoint, Vector3.one * 0.5f);
        Gizmos.DrawWireCube(rightPoint, Vector3.one * 0.5f);
        
        // Draw text info (if you have Gizmos text available)
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(new Vector3((minX + maxX) / 2, spawnHeight, spawnZ), new Vector3(maxX - minX, 0.1f, 0.1f));
    }
}

// Separate component for individual consumables
public class HealthConsumable : MonoBehaviour
{
    private float healAmount;
    private PlayerManager playerManager;
    private bool hasBeenCollected = false;

    public void Initialize(float heal, PlayerManager player)
    {
        healAmount = heal;
        playerManager = player;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player collided
        if (other.CompareTag("Player") && !hasBeenCollected)
        {
            CollectConsumable(other.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // For 3D collisions
        if (other.CompareTag("Player") && !hasBeenCollected)
        {
            CollectConsumable(other.gameObject);
        }
    }

    void CollectConsumable(GameObject player)
    {
        hasBeenCollected = true;
        
        Debug.Log($"COLLISION DETECTED: Player touched {gameObject.name}!");
        
        // Get player manager from colliding object
        PlayerManager pm = player.GetComponent<PlayerManager>();
        if (pm == null)
        {
            pm = playerManager; // Use the reference we stored
        }

        if (pm != null)
        {
            float healthBefore = pm.GetCurrentHealth();
            pm.Heal(healAmount);
            float healthAfter = pm.GetCurrentHealth();
            
            Debug.Log($"HEALTH INCREASED: {healthBefore} -> {healthAfter} (+{healAmount})");
        }
        else
        {
            Debug.LogError("No PlayerManager found on colliding object!");
        }

        // Destroy the consumable
        Debug.Log($"Consumable {gameObject.name} destroyed!");
        Destroy(gameObject);
    }
}