using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableManager : MonoBehaviour
{
    [Header("Consumable Settings")]
    [SerializeField]
    private GameObject consumablePrefab; // Assign a prefab in inspector
    [SerializeField]
    private float healAmount = 20f;
    [SerializeField]
    private float spawnInterval = 10f; // Spawn every 10 seconds
    [SerializeField]
    private Transform[] spawnPoints; // Array of spawn locations
    [SerializeField]
    private int maxConsumablesInScene = 3;

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

        // Create a simple consumable prefab if none assigned
        if (consumablePrefab == null && autoCreateConsumable)
        {
            CreateSimpleConsumablePrefab();
        }

        // Create spawn points if none assigned
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            CreateDefaultSpawnPoints();
        }

        // Start spawning consumables
        StartCoroutine(SpawnConsumablesRoutine());
        
        Debug.Log("ConsumableManager initialized!");
    }

    void CreateSimpleConsumablePrefab()
    {
        // Create a simple cube as consumable
        GameObject tempConsumable = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tempConsumable.name = "HealthConsumable";
        tempConsumable.transform.localScale = Vector3.one * 0.5f;
        
        // Make it green for health
        Renderer renderer = tempConsumable.GetComponent<Renderer>();
        renderer.material.color = Color.green;
        
        // Add the consumable component
        tempConsumable.AddComponent<HealthConsumable>();
        
        // Set as trigger for collision detection
        Collider collider = tempConsumable.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // For 2D games, add 2D components
        tempConsumable.AddComponent<BoxCollider2D>().isTrigger = true;
        
        // Make it a prefab reference
        consumablePrefab = tempConsumable;
        tempConsumable.SetActive(false);
        
        Debug.Log("Auto-created simple consumable prefab!");
    }

    void CreateDefaultSpawnPoints()
    {
        // Create some default spawn points around the origin
        GameObject spawnPointParent = new GameObject("SpawnPoints");
        spawnPointParent.transform.SetParent(transform);
        
        List<Transform> points = new List<Transform>();
        
        // Create 5 spawn points in different locations
        Vector3[] positions = {
            new Vector3(-5f, 2f, 0f),
            new Vector3(5f, 2f, 0f),
            new Vector3(0f, 5f, 0f),
            new Vector3(-3f, -1f, 0f),
            new Vector3(3f, -1f, 0f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
            spawnPoint.transform.SetParent(spawnPointParent.transform);
            spawnPoint.transform.position = positions[i];
            points.Add(spawnPoint.transform);
        }

        spawnPoints = points.ToArray();
        Debug.Log($"Created {spawnPoints.Length} default spawn points!");
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
                SpawnConsumable();
            }
        }
    }

    void SpawnConsumable()
    {
        if (consumablePrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("ConsumableManager: No prefab or spawn points available!");
            return;
        }

        // Choose random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // Instantiate consumable
        GameObject newConsumable = Instantiate(consumablePrefab, spawnPoint.position, Quaternion.identity);
        newConsumable.SetActive(true);
        
        // Add to active list
        activeConsumables.Add(newConsumable);
        
        // Set up the health consumable component
        HealthConsumable healthComp = newConsumable.GetComponent<HealthConsumable>();
        if (healthComp == null)
        {
            healthComp = newConsumable.AddComponent<HealthConsumable>();
        }
        healthComp.Initialize(healAmount, playerManager);
        
        Debug.Log($"Spawned health consumable at {spawnPoint.position} - Heal Amount: {healAmount}");
    }

    // Public method to spawn consumable manually (for testing)
    public void SpawnConsumableNow()
    {
        SpawnConsumable();
    }

    void Update()
    {
        // Optional: Press C to spawn consumable for testing
        if (Input.GetKeyDown(KeyCode.C))
        {
            SpawnConsumableNow();
            Debug.Log("Manual consumable spawn!");
        }
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
        
        Debug.Log($"COLLISION DETECTED: Player touched health consumable!");
        
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
        Debug.Log("Consumable destroyed!");
        Destroy(gameObject);
    }
}