using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Player Stats")]
    [SerializeField]
    private float maxHealth = 100f;
    private float currentHealth;

    [SerializeField]
    private float attackDamage = 25f;
    [SerializeField]
    private float attackRange = 2f;

    [SerializeField]
    private float attackCooldown = 1f;

    [Header("Movement Stats")]
    [SerializeField]
    private float moveSpeed = 5f;

    [Header("Components")]
    public Transform attackPoint;
    public LayerMask enemyLayers;

    private Rigidbody2D rb;
    private Vector2 movement;

    private float lastAttackTime;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (attackPoint == null)
        {
            Debug.LogError("Attack Point is not assigned in the inspector.");
        }
    }

    void Update()
    {
        HandleInput();
        HandleMeleeAttack();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
    }
    
    void HandleInput()
    {
        // WASD movement input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        // Normalize diagonal movement
        movement = movement.normalized;
    }
    
    void HandleMovement()
    {
        // Move the player using Rigidbody2D
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
    
    void HandleMeleeAttack()
    {
        // Melee attack with left mouse button or spacebar
        if ((Input.GetMouseButtonDown(LeftMouseButton) || Input.GetKeyDown(KeyCode.Space)) && CanAttack())
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
            // Try to damage enemy
            var enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
                Debug.Log($"Hit {enemy.name} for {attackDamage} damage!");
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
    }
    
    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        Debug.Log($"Player healed {healAmount}. Health: {currentHealth}/{maxHealth}");
    }
    
    void Die()
    {
        Debug.Log("Player died!");
        // Add death logic here (restart level, show game over screen, etc.)
        gameObject.SetActive(false);
    }
    
    // Getter for UI or other systems
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw melee attack range in scene view
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}