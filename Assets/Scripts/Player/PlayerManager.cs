using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Attack Stats")]
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 0.4f; // 0.4s cooldown

    [Header("Movement Stats")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 15f;

    [Header("Respawn Settings")]
    [SerializeField]
    private Vector3 spawnPosition; // Store the starting position
    [SerializeField]
    private float respawnDelay = 0f; // Delay before respawning

    [Header("Ground Check")]
    [SerializeField] private string groundTag = "Ground";

    [Header("Components")]
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public Animator animator;
    public Slider healthbar;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private bool canJump;
    private float lastAttackTime;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        canJump = true;

        // Store the starting position for respawning
        spawnPosition = transform.position;

        // Physics settings for platformer
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 3f;
        }

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
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            && canJump)
        {
            Jump();
        }
    }

    void HandleMovement()
    {
        Vector2 velocity = rb.velocity;
        velocity.x = horizontalInput * moveSpeed;
        rb.velocity = velocity;

        if (horizontalInput > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (horizontalInput < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        canJump = false;
        isGrounded = false;
        Debug.Log("Player jumped! Jump disabled until landing.");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag) || collision.gameObject.name.Contains("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.7f)
                {
                    isGrounded = true;
                    canJump = true;
                    Debug.Log("Player landed! Jump re-enabled.");
                    break;
                }
            }
        }
    }

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
        // Attack input: Spacebar, Left Mouse, or X
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.X))
            )
        {
            if (animator != null)
                animator.SetTrigger("Attack");
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

        // Trigger attack animation
        

        // Detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Damage via HealthManager
            HealthManager hm = enemy.GetComponent<HealthManager>();
            if (hm != null)
            {
                hm.Damage((int)attackDamage);
                Debug.Log($"Hit {enemy.name} for {attackDamage} damage!");
            }

            // Optional knockback
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemyRb.AddForce(knockbackDir * 10f, ForceMode2D.Impulse);
            }
        }

        Debug.Log($"Melee attack performed! Hit {hitEnemies.Length} enemies.");
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; // Don't take damage if already dead
        
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();

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
        if (attackPoint != null)
        {
            Gizmos.color = Color.red; // Color of the circle
            Gizmos.DrawWireSphere(attackPoint.position, attackRange); // Draw circle in Scene view
        }

        // Draw spawn position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnPosition, 0.5f);
    }
    

    public void UpdateHealthBar()
    {
        if (healthbar != null)
            healthbar.value = GetHealthPercentage();
    }
}
