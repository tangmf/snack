using UnityEngine;

public class Attack : StateMachineBehaviour
{
    private Transform player;
    private PlayerDetection detection;
    private float attackTimer;

    public float moveSpeed = 2f;
    public float attackRange = 1f;
    public float attackCooldown = 0.3f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        detection = animator.GetComponent<PlayerDetection>();
        if (detection != null)
        {
            player = detection.detectedPlayer; // assumes PlayerDetection stores the Transform of the player
        }
        attackTimer = 0f;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        // Follow the player
        Vector2 direction = (player.position - animator.transform.position).normalized;
        animator.transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);

        // Update attack cooldown
        attackTimer -= Time.deltaTime;

        // Check if within range
        float distance = Vector2.Distance(animator.transform.position, player.position);
        if (distance <= attackRange && attackTimer <= 0f)
        {
            // Trigger attack animation
            animator.SetTrigger("Attack");

            Debug.Log("Enemy attacks player!");

            // Reset cooldown
            attackTimer = attackCooldown;

            // TODO: Deal damage (e.g., call a method on player health script)
            // player.GetComponent<PlayerHealth>()?.TakeDamage(1);
        }
    }
}
