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

    public Slider healthbar;
    
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        canJump = true; // Start with ability to jump

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
    }

    void Update()
    {
        HandleInput();
        HandleAttack();
        
        // Debug ground state
        Debug.Log($"Is Grounded: {isGrounded}, Can Jump: {canJump}");
    }
    
    void FixedUpdate()
    {
        HandleMovement();
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
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthBar();
        Debug.Log($"Player healed {healAmount}. Health: {currentHealth}/{maxHealth}");
    }
    
    void Die()
    {
        Debug.Log("Player died!");
        gameObject.SetActive(false);
    }
    
    public float GetCurrentHealth() { return currentHealth; }
    public float GetMaxHealth() { return maxHealth; }
    public float GetHealthPercentage() { return currentHealth / maxHealth; }
    
    void OnDrawGizmosSelected()
    {
        // Draw attack range
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    public void UpdateHealthBar()
    {
        if (healthbar != null)
        {
            healthbar.value = GetHealthPercentage();
        }
    }
}