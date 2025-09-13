using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerDetection : MonoBehaviour
{
    public float detectionRange = 3f;      // detection distance on x-axis
    public bool playerDetected = false;    // flag if player is close
    public string playerTag = "Player";    // tag to detect
    public Animator animator;
    private Transform detectedPlayer = null;

    private void Update()
    {
        playerDetected = false; // reset each frame
        detectedPlayer = null;

        // Find all objects with the player tag
        GameObject playerObj = GameObject.FindWithTag(playerTag);
        if (playerObj == null) return;

        // Check x-axis distance only
        float distanceX = Mathf.Abs(playerObj.transform.position.x - transform.position.x);
        if (distanceX <= detectionRange)
        {
            playerDetected = true;
            detectedPlayer = playerObj.transform;
            animator.SetBool("isPlayerDetected", true);
            Debug.Log("Player detected on x-axis!");
        }
        else
        {
            animator.SetBool("isPlayerDetected", false);
        }
    }

    // Optional: draw detection range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 start = transform.position + Vector3.left * detectionRange;
        Vector3 end = transform.position + Vector3.right * detectionRange;
        Gizmos.DrawLine(start, end);
    }
}
