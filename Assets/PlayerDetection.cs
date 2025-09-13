using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerDetection : MonoBehaviour
{
    public bool playerDetected = false;
    public Transform detectPoint;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector2 rayOrigin = detectPoint.position;
        Vector2 rayDirection = Vector2.left; // Example: casting downwards
        float rayDistance = 1.0f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection);
        Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red, 0.1f);
        // If it hits something...
        if (hit)
        {
            if(hit.collider.CompareTag("Player"))
            {
                playerDetected = true;
            }
            else
            {
                playerDetected = false;
            }
        }
    }
}
