using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SoundMonster : MonoBehaviour
{

    private SoundManager targetSoundManager;
    public Transform target;

    public Transform[] idleSpots;

    private static int STATUS_IDLE = 0; // not moving
    private static int STATUS_HUNTING = 1; // chasing player
    private static int STATUS_WANDERING = 2; // moving to random idle spot

    private int status = STATUS_IDLE;
    public float idleTimeAtOneSpot = 3f;
    public float timeUntilForgetPlayer = 3f;

    private Coroutine idleCoroutine = null;

    public Transform soundMonster;

    public float huntingSpeed = 5f;
    public float wanderingSpeed = 3f;
    public float nextWaypointDistance = 3f;


    Path path;
    int currentWaypoint = 0;
    // bool reachedEndOfPath = false;

    Seeker seeker;
    Rigidbody2D rb;

    // After losing track of player
    void waitThenForgetPlayer() {
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        idleCoroutine = StartCoroutine(waitThenMoveToRandomIdleSpot(timeUntilForgetPlayer));
    }

    // After reaching idle spot, waits for a while then moves to another idle spot
    void waitThenMoveToAnotherIdleSpot() {
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        idleCoroutine = StartCoroutine(waitThenMoveToRandomIdleSpot(idleTimeAtOneSpot));
    }

    IEnumerator waitThenMoveToRandomIdleSpot(float waitFor) {
        Debug.Log("Idling...");
        status = STATUS_IDLE;
        // Choose random idle spot
        Debug.Log("RANDOM: " + Random.Range(0, idleSpots.Length));
        Transform newIdleSpot = idleSpots[Random.Range(0, idleSpots.Length)];
        // Idle for X seconds
        yield return new WaitForSeconds(waitFor);

        // Move to random idle spot
        target = newIdleSpot;
        Debug.Log("Moving to idle spot at " + newIdleSpot.position);
        status = STATUS_WANDERING;
    }

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating("UpdatePath", 0f, 0.5f);
        //seeker.StartPath(rb.position, target.position, OnPathComplete);
        waitThenMoveToAnotherIdleSpot();
    }

    void UpdatePath()
    {
        if (status == STATUS_WANDERING && seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        } else if (targetSoundManager != null && seeker.IsDone())
        {
            status = STATUS_HUNTING;
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Path and Status: " + (path != null) + ", " + status);
        // Chase current target if exists or wander to idle spot if wandering
        if (targetSoundManager != null || status == STATUS_WANDERING) {
            // Chase logic here
            if (path == null)
                return;
            
            // If reached destination or
            // If wandering and close enough, consider it reached, else will be stuck
            if (currentWaypoint >= path.vectorPath.Count || (status == STATUS_WANDERING && Vector2.Distance(rb.position, path.vectorPath[path.vectorPath.Count - 1]) < 0.5f))
            {
                Debug.Log("Reached destination");
                path = null;
                target = null;
                waitThenMoveToAnotherIdleSpot();
                //reachedEndOfPath = true;
                return;
            }
            // else
            // {
            //     //reachedEndOfPath = false;
            // }

            // Walking logic
            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
            Vector2 pos = soundMonster.position;
            float speed = (status == STATUS_HUNTING) ? huntingSpeed : wanderingSpeed;
            Vector2 nextPos = pos + direction * speed * Time.deltaTime;
            soundMonster.position = nextPos;
            float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

            if (distance < nextWaypointDistance)
            {
                currentWaypoint++;
            }

            // Flip sprite based on movement direction
            if (direction.x > 0.1f)
            {
                soundMonster.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (direction.x < -0.1f)
            {
                soundMonster.localScale = new Vector3(-1f, 1f, 1f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If sound
        print("Collided with: ");

        // After colliding with a collider with sound tag, chase it
        if (collision.CompareTag("Sound"))
        {
            SoundManager sm = collision.GetComponent<SoundManager>();
            target = collision.gameObject.transform;
            if (sm != null)
            {
                //if (targetSoundManager == null)
                //{
                    // Stop idling and start chasing whatever made that noise
                    StopCoroutine(idleCoroutine);
                    idleCoroutine = null;
                    targetSoundManager = sm;
                    status = STATUS_HUNTING;
                //}
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
                if (targetSoundManager == sm)
                {
                    targetSoundManager = null;
                    waitThenForgetPlayer();
                }
            }
        }
    }

}
