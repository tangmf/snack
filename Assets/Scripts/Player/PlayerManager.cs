using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField]
    private float maxHealth = 100f;
    private float currentHealth;

    [SerializeField]
    private float attackDamage = 25f;

    [SerializeField]
    private float attackRange = 2f;

    [SerializeField]
    private float attackCooldown = 0.5f;

    [Header("Movement Stats")]
    [SerializeField]
    private float moveSpeed = 8f;
    [SerializeField]
    private float jumpForce = 15f;

    [Header("Respawn Settings")]
    [SerializeField]
    private Vector3 spawnPosition; // Store the starting position
    [SerializeField]
    private float respawnDelay = 0f; // Delay before respawning

    [Header("Ground Check")]
    [SerializeField]
    private string groundTag = "Ground"; // Tag for ground objects

    [Header("Components")]
    public Transform attackPoint;
    public LayerMask enemyLayers;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private bool canJump; // Only allow jump when this is true
    private float lastAttackTime;
    private bool isDead = false;

    public Slider healthbar;
    
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        canJump = true; // Start with ability to jump

        // Store the starting position for respawning
        spawnPosition = transform.position;

        // Physics settings for platformer
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 3f; // Normal gravity for platformer
        }

        // Create attack point if not assigned
        if (attackPoint == null)
        {
            GameObject attackObj = new GameObject("AttackPoint");
            attackObj.transform.SetParent(transform);
            attackObj.transform.localPosition = new Vector3(1f, 0f, 0f);
            attackPoint = attackObj.transform;
        }

        Debug.Log($"Player spawn position set to: {spawnPosition}");
    }

    void Update()
    {
        if (!isDead)
        {
            HandleInput();
            HandleAttack();
        }
        
        // Debug ground state
        Debug.Log($"Is Grounded: {isGrounded}, Can Jump: {canJump}");
    }
    
    void FixedUpdate()
    {
        if (!isDead)
        {
            HandleMovement();
        }
    }

    void HandleInput()
    {
        // Horizontal movement (A/D or Arrow keys)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Jump (W, Space, or Up Arrow) - only if can jump
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) 
            && canJump)
        {
            Jump();
        }
    }

    void HandleMovement()
    {
        // Horizontal movement
        Vector2 velocity = rb.velocity;
        velocity.x = horizontalInput * moveSpeed;
        rb.velocity = velocity;

        // Flip sprite based on movement direction
        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        canJump = false;   // Disable jumping until touching ground
        isGrounded = false; // Player is now in air
        Debug.Log("Player jumped! Jump disabled until landing.");
    }

    // Detect when player touches ground
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if we hit the ground
        if (collision.gameObject.CompareTag(groundTag) || collision.gameObject.name.Contains("Ground"))
        {
            // Make sure we're landing on top (not hitting from the side)
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.7f) // Hit from above
                {
                    isGrounded = true;
                    canJump = true;  // Re-enable jumping
                    Debug.Log("Player landed! Jump re-enabled.");
                    break;
                }
            }
        }
    }

    // Detect when player leaves ground
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag) || collision.gameObject.name.Contains("Ground"))
        {
            isGrounded = false;
            Debug.Log("Player left ground.");
        }
    }

    void HandleAttack()
    {
        // Regular melee attack (Left mouse or X key)
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.X)) && CanAttack())
        {
            PerformMeleeAttack();
        }
    }
    
    bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }
    
    void PerformMeleeAttack()
    {
        if (attackPoint == null) return;
        
        lastAttackTime = Time.time;
        
        // Detect enemies in melee range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log($"Hit {enemy.name} for {attackDamage} damage!");
            
            // Add knockback to enemies if they have Rigidbody2D
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                enemyRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
            }
        }
        
        Debug.Log($"Melee attack! Hit {hitEnemies.Length} enemies.");
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return; // Don't take damage if already dead
        
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
        UpdateHealthBar();
    }
    
    public void Heal(float healAmount)
    {
        if (isDead) return; // Don't heal if dead
        
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthBar();
        Debug.Log($"Player healed {healAmount}. Health: {currentHealth}/{maxHealth}");
    }
    
    void Die()
    {
        if (isDead) return; // Prevent multiple death calls
        
        isDead = true;
        Debug.Log("Player died! Respawning in " + respawnDelay + " seconds...");
        
        // Stop player movement
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        // Start respawn coroutine
        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);
        
        // Respawn the player
        Respawn();
    }

    void Respawn()
    {
        Debug.Log($"Player respawning at: {spawnPosition}");
        
        // Reset position
        transform.position = spawnPosition;
        
        // Reset health
        currentHealth = maxHealth;
        UpdateHealthBar();
        
        // Reset physics
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        // Reset states
        isDead = false;
        canJump = true;
        isGrounded = false;
        
        // Make sure player is active
        gameObject.SetActive(true);
        
        Debug.Log("Player respawned successfully!");
    }

    // Public method to manually set spawn position
    public void SetSpawnPosition(Vector3 newSpawnPosition)
    {
        spawnPosition = newSpawnPosition;
        Debug.Log($"Spawn position updated to: {spawnPosition}");
    }

    // Public method to manually respawn (for testing)
    public void ForceRespawn()
    {
        Die();
    }
    
    public float GetCurrentHealth() { return currentHealth; }
    public float GetMaxHealth() { return maxHealth; }
    public float GetHealthPercentage() { return currentHealth / maxHealth; }
    public bool IsDead() { return isDead; }
    
    void OnDrawGizmosSelected()
    {
        // Draw attack range
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        // Draw spawn position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnPosition, 0.5f);
    }

    public void UpdateHealthBar()
    {
        if (healthbar != null)
        {
            healthbar.value = GetHealthPercentage();
        }
    }
}