using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public Slider healthBar;
    public float healthPoints;
    public bool dead = false;

    [Header("Respawn Settings")]
    [SerializeField] private bool canRespawn = true; // Can this object respawn?
    [SerializeField] private float respawnDelay = 3f; // Delay before respawning
    
    [Header("Spawn Area Settings")]
    [SerializeField] private float spawnHeight = 0f; // Same Y position for all spawns
    [SerializeField] private float minX = -10f; // Minimum X position
    [SerializeField] private float maxX = 10f;  // Maximum X position
    [SerializeField] private float spawnZ = 0f; // Z position (for 2D usually 0)

    private Vector3 originalPosition; // Store original spawn position
    private float maxHealthPoints; // Store max health for respawning

    // Start is called before the first frame update
    void Start()
    {
        // Store original values
        originalPosition = transform.position;
        maxHealthPoints = healthPoints;
        
        MaxHealth();

        // Set spawn height to current Y position if not manually set
        if (spawnHeight == 0f)
        {
            spawnHeight = originalPosition.y;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Damage(10);
        }
    }

    void RespawnAtRandomLocation()
    {
        Debug.Log($"Respawning {gameObject.name}...");
        
        // Generate random X position within the specified range
        float randomX = Random.Range(minX, maxX);
        
        // Create spawn position with fixed Y and Z
        Vector3 respawnPosition = new Vector3(randomX, spawnHeight, spawnZ);
        
        // Set new position
        transform.position = respawnPosition;
        
        // Reset health and state
        healthPoints = maxHealthPoints;
        dead = false;
        
        // Update health bar
        if (healthBar != null)
            healthBar.value = healthPoints;
        
        // Reactivate the object
        gameObject.SetActive(true);
        
        // Reset any Rigidbody velocity
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        Debug.Log($"{gameObject.name} respawned at random location: {respawnPosition}!");
    }

    public void Damage(float dmg)
    {
        if (healthPoints > 0)
        {
            healthPoints -= dmg;

            if (healthPoints <= 0)
            {
                healthPoints = 0;
                if (!dead)
                {
                    Die();
                    dead = true;
                }
            }
            else
            {
                // damaged
            }
        }

        if (healthBar != null)
            healthBar.value = healthPoints;
    }

    public void Heal(float amt)
    {
        healthPoints += amt;
        if (healthBar != null && healthPoints >= healthBar.maxValue)
        {
            healthPoints = healthBar.maxValue;
        }

        if (healthBar != null)
            healthBar.value = healthPoints;
    }

    public void MaxHealth()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = healthPoints;
            healthBar.value = healthPoints;
        }
    }

    public void AddHealth(float health)
    {
        if (healthBar != null)
            healthBar.maxValue += health;
    }

    public void Die()
    {
        // Check if this is the player
        if (gameObject.CompareTag("Player"))
        {
            // Player death logic
            Transform playerTransform = transform;
            GameObject spawnPoint = GameObject.Find("SpawnPoint");
            
            if (spawnPoint != null)
            {
                playerTransform.position = spawnPoint.transform.position;
            }
            
            Respawn(); // Respawn the player immediately
        }
        else
        {
            // Enemy death logic
            if (canRespawn)
            {
                Debug.Log($"{gameObject.name} died! Will respawn in {respawnDelay} seconds...");
                StartCoroutine(RespawnAfterDelay());
            }
            else
            {
                Debug.Log($"{gameObject.name} died and will not respawn.");
                Destroy(gameObject);
            }
        }
    }

    IEnumerator RespawnAfterDelay()
    {
        // Hide the object while waiting to respawn
        gameObject.SetActive(false);
        
        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);
        
        // Respawn the enemy
        RespawnAtRandomLocation();
    }



    // Public method to respawn immediately (for player or testing)
    public void Respawn()
    {
        if (gameObject.CompareTag("Player"))
        {
            // Player respawn logic
            healthPoints = maxHealthPoints;
            dead = false;
            
            if (healthBar != null)
                healthBar.value = healthPoints;
                
            Debug.Log("Player respawned!");
        }
        else
        {
            // Enemy respawn logic
            RespawnAtRandomLocation();
        }
    }

    // Visualize spawn area in Scene view
    void OnDrawGizmosSelected()
    {
        // Draw spawn area as horizontal line
        Gizmos.color = Color.yellow;
        
        Vector3 leftPoint = new Vector3(minX, spawnHeight, spawnZ);
        Vector3 rightPoint = new Vector3(maxX, spawnHeight, spawnZ);
        
        Gizmos.DrawLine(leftPoint, rightPoint);
        
        // Draw markers at endpoints
        Gizmos.DrawWireCube(leftPoint, Vector3.one * 0.5f);
        Gizmos.DrawWireCube(rightPoint, Vector3.one * 0.5f);
        
        // Draw original position
        Gizmos.color = Color.blue;
        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(originalPosition, 0.3f);
        }
    }
}