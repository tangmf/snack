using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMonster : MonoBehaviour
{

    private SoundManager currentTarget;
    private Vector3 targetLocation; // add this

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Started");
    }

    // Update is called once per frame
    void Update()
    {
        // Chase current target if exists
        if (currentTarget != null) {
            // Chase logic here
        } else {
            // Idle logic here
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If sound
        print("Collided with: ");

        if (collision.CompareTag("Sound"))
        {
            SoundManager sm = collision.GetComponent<SoundManager>();
            targetLocation = collision.gameObject.transform.position;
            if (sm != null)
            {
                // Check if sound is louder than prev
                if (currentTarget == null || currentTarget.currentVolume < sm.currentVolume)
                {
                    currentTarget = sm;
                }
            }
            // Start chasing player
            // Destroy(gameObject); // destroy bullet on hit
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // If sound
        if (collision.CompareTag("Sound"))
        {
            SoundManager sm = collision.GetComponent<SoundManager>();
            if (sm != null)
            {
                if (currentTarget == sm)
                {
                    currentTarget = null;
                }
            }
        }
    }

}
