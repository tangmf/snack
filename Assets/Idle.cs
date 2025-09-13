using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : StateMachineBehaviour
{
    public float moveDistance = 1f;   // how far to sway left/right
    public float cycleDuration = 2f;  // full cycle time (left → right → left)

    private Vector3 startPos;
    private float timer;

    // Called when entering idle state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        startPos = animator.transform.position; // store starting position
        timer = 0f;
    }

    // Called every frame while in idle state
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator == null) return;

        timer += Time.deltaTime;

        // Convert timer into angle (0 → 2π over cycleDuration)
        float angle = (timer / cycleDuration) * Mathf.PI * 2f;

        // sine wave oscillation (-1 → 1)
        float offset = Mathf.Sin(angle) * moveDistance;

        // Apply to position (sway left and right)
        animator.transform.position = startPos + Vector3.right * offset;
    }
}
