using System.Collections;
using UnityEngine;
using Pathfinding;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class SoundMonster : MonoBehaviour
{
    // Inspector fields
    [Header("Targets / Idle spots")]
    private SoundManager targetSoundManager;
    public Transform target;
    public Transform[] idleSpots;

    [Header("Movement")]
    public float huntingSpeed = 5f;
    public float wanderingSpeed = 3f;
    public float nextWaypointDistance = 0.1f;

    [Header("Idle timings")]
    public float idleTimeAtOneSpot = 3f;
    public float timeUntilForgetPlayer = 3f;

    [Header("Animator / Change clip")]
    public Animator animator;
    // Int param used in Animator: 0 = wander/idle, 1 = hunt
    public string animatorStateParam = "State";
    public string changeStateName = "Change";
    public string wanderStateName = "Wander";
    public AnimationClip changeClip;
    public float changeAnimationDuration = 0.0f;

    // Visual transform to rotate/face movement; defaults to this.transform if not set
    public Transform visual;

    // Internal state
    enum State { Idle = 0, Hunting = 1, Wandering = 2 }
    State state = State.Idle;

    Path path;
    int currentWaypoint = 0;

    Seeker seeker;
    Rigidbody2D rb;

    Coroutine idleCoroutine = null;
    Coroutine changeCoroutine = null;

    // --- Unity callbacks ---
    void Awake()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (visual == null) visual = transform;
        if (changeClip != null && changeAnimationDuration <= 0f) changeAnimationDuration = changeClip.length;
    }

    void Start()
    {
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
        StartIdleCycle();
    }

    // --- State / animator helpers ---
    void SetState(State newState)
    {
        if (state == newState) return;
        state = newState;
        UpdateAnimator();
        if (state != State.Idle) CancelIdleCycle();
    }

    void UpdateAnimator()
    {
        if (animator == null) return;
        // Animator uses only two values: 0 = wander/idle, 1 = hunt
        int animState = (state == State.Hunting) ? 1 : 0;
        animator.SetInteger(animatorStateParam, animState);
    }

    // --- Idle / wander handling ---
    void StartIdleCycle()
    {
        CancelIdleCycle();
        idleCoroutine = StartCoroutine(WaitThenMoveToRandomIdleSpot(idleTimeAtOneSpot));
    }

    void CancelIdleCycle()
    {
        if (idleCoroutine != null) { StopCoroutine(idleCoroutine); idleCoroutine = null; }
    }

    IEnumerator WaitThenMoveToRandomIdleSpot(float waitFor)
    {
        int prev = (int)state;
        SetState(State.Idle);

        // choose spot now so it's stable while waiting
        Transform newSpot = (idleSpots != null && idleSpots.Length > 0) ? idleSpots[Random.Range(0, idleSpots.Length)] : null;

        yield return new WaitForSeconds(waitFor);

        // abort if hunting started during wait
        if (targetSoundManager != null || state == State.Hunting)
        {
            idleCoroutine = null;
            yield break;
        }

        // if we came from hunting, play reverse change -> wander
        if (prev == (int)State.Hunting && changeClip != null)
        {
            if (changeCoroutine != null) { StopCoroutine(changeCoroutine); changeCoroutine = null; }
            changeCoroutine = StartCoroutine(PlayChangeThenWander(newSpot));
            idleCoroutine = null;
            yield break;
        }

        // normal: go to new idle spot
        if (newSpot != null)
        {
            target = newSpot;
            SetState(State.Wandering);
        }

        idleCoroutine = null;
    }

    void waitThenForgetPlayer()
    {
        CancelIdleCycle();
        idleCoroutine = StartCoroutine(WaitThenMoveToRandomIdleSpot(timeUntilForgetPlayer));
    }

    // --- Hunting / change transitions ---
    public void StartHunting()
    {
        // interrupt idle
        CancelIdleCycle();

        SetState(State.Hunting);

        if (changeCoroutine != null) { StopCoroutine(changeCoroutine); changeCoroutine = null; }
        changeCoroutine = StartCoroutine(PlayChangeThenHunt());
    }

    IEnumerator PlayChangeThenHunt()
    {
        if (animator != null && !string.IsNullOrEmpty(changeStateName) && animator.HasState(0, Animator.StringToHash(changeStateName)))
        {
            animator.speed = 1f;
            animator.Play(changeStateName, 0, 0f);
        }

        yield return new WaitForSeconds(changeAnimationDuration);

        changeCoroutine = null;
        if (animator != null) animator.speed = 1f;

        // Force animator into hunting to avoid stuck states
        if (animator != null)
        {
            animator.SetInteger(animatorStateParam, 1);
            if (animator.HasState(0, Animator.StringToHash("Hunt")))
                animator.Play("Hunt");
        }
        else UpdateAnimator();
    }

    IEnumerator PlayChangeThenWander(Transform newIdleSpot)
    {
        if (changeClip != null && animator != null)
        {
            var graph = PlayableGraph.Create($"ChangeReverse_{name}");
            var output = AnimationPlayableOutput.Create(graph, "ChangeReverse", animator);
            var clipPlayable = AnimationClipPlayable.Create(graph, changeClip);
            clipPlayable.SetDuration(changeClip.length);
            clipPlayable.SetTime(changeClip.length);
            clipPlayable.SetSpeed(-1.0);
            output.SetSourcePlayable(clipPlayable);
            graph.Play();

            yield return new WaitForSeconds(changeAnimationDuration);

            graph.Destroy();
            changeCoroutine = null;

            // enter wandering
            target = newIdleSpot;
            SetState(State.Wandering);

            // force animator to wander state/param to avoid Exit Time issues
            if (animator != null)
            {
                animator.SetInteger(animatorStateParam, 0);
                if (animator.HasState(0, Animator.StringToHash(wanderStateName)))
                    animator.Play(wanderStateName);
            }

            yield break;
        }

        // fallback: play forward change state then wander
        if (animator != null && !string.IsNullOrEmpty(changeStateName) && animator.HasState(0, Animator.StringToHash(changeStateName)))
        {
            animator.Play(changeStateName, 0, 0f);
            yield return new WaitForSeconds(changeAnimationDuration);
        }

        changeCoroutine = null;
        target = newIdleSpot;
        SetState(State.Wandering);

        if (animator != null)
        {
            animator.SetInteger(animatorStateParam, 0);
            if (animator.HasState(0, Animator.StringToHash(wanderStateName)))
                animator.Play(wanderStateName);
        }
    }

    // --- Pathfinding / movement ---
    void UpdatePath()
    {
        if (seeker == null || !seeker.IsDone()) return;

        // hunting priority
        if (targetSoundManager != null && target != null)
        {
            // begin hunting (interrupt idling)
            StartHunting();
            path = null;
            if (seeker.IsDone()) seeker.StartPath(rb.position, target.position, OnPathComplete);
            return;
        }

        if (state == State.Wandering && target != null)
        {
            if (seeker.IsDone()) seeker.StartPath(rb.position, target.position, OnPathComplete);
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

    void Update()
    {
        // keep immediate response if needed
        if (seeker == null || !seeker.IsDone()) return;

        if (state == State.Wandering && target != null && seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
        else if (targetSoundManager != null && target != null && seeker.IsDone())
        {
            SetState(State.Hunting);
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    void FixedUpdate()
    {
        if (!(targetSoundManager != null || state == State.Wandering)) return;
        if (path == null || path.vectorPath == null || path.vectorPath.Count == 0) return;

        Vector2 currentPos = rb.position;
        Vector2 finalPoint = (Vector2)path.vectorPath[path.vectorPath.Count - 1];

        if (currentWaypoint >= path.vectorPath.Count ||
            (state == State.Wandering && Vector2.Distance(currentPos, finalPoint) < 0.5f))
        {
            StopMovementAndIdle();
            return;
        }

        // skip waypoints that are effectively our position
        while (currentWaypoint < path.vectorPath.Count)
        {
            Vector2 wp = (Vector2)path.vectorPath[currentWaypoint];
            if (Vector2.Distance(currentPos, wp) < nextWaypointDistance) { currentWaypoint++; continue; }
            break;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            StopMovementAndIdle();
            return;
        }

        Vector2 targetPoint = (Vector2)path.vectorPath[currentWaypoint];
        Vector2 direction = (targetPoint - currentPos);
        float dirMag = direction.magnitude;
        if (dirMag > 0.0001f) direction /= dirMag;
        else direction = Vector2.zero;

        float speed = (state == State.Hunting) ? huntingSpeed : wanderingSpeed;
        Vector2 nextPos = currentPos + direction * speed * Time.fixedDeltaTime;

        rb.MovePosition(nextPos);

        if (Vector2.Distance(currentPos, targetPoint) < nextWaypointDistance) currentWaypoint++;

        // face movement
        if (visual != null && direction.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            visual.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
        }
    }

    void StopMovementAndIdle()
    {
        path = null;
        target = null;
        if (rb != null) { rb.velocity = Vector2.zero; rb.angularVelocity = 0f; }
        StartIdleCycle();
    }

    // --- Triggers ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Sound")) return;
        SoundManager sm = collision.GetComponent<SoundManager>();
        if (sm == null) return;

        targetSoundManager = sm;
        target = collision.transform;

        StartHunting();
        path = null;
        if (seeker != null && seeker.IsDone()) seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Sound")) return;
        SoundManager sm = collision.GetComponent<SoundManager>();
        if (sm == null) return;
        if (targetSoundManager != sm) return;

        targetSoundManager = null;
        waitThenForgetPlayer();
    }
}