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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        canJump = true;

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
    }

    void Update()
    {
        HandleInput();
        HandleAttack();

        Debug.Log($"Is Grounded: {isGrounded}, Can Jump: {canJump}");
    }

    void FixedUpdate()
    {
        HandleMovement();
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
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();

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

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red; // Color of the circle
            Gizmos.DrawWireSphere(attackPoint.position, attackRange); // Draw circle in Scene view
        }
    }


    public void UpdateHealthBar()
    {
        if (healthbar != null)
            healthbar.value = GetHealthPercentage();
    }
}
