using System.Collections;
using UnityEngine;
using Pathfinding;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Seeker), typeof(Rigidbody2D))]
public class LightMonster : MonoBehaviour
{
    [Header("Player reference")]
    public Transform player;

    [Header("Movement")]
    public float huntingSpeed = 5f;
    public float hoveringSpeed = 3f;
    public float nextWaypointDistance = 0.2f;
    public float followRadius = 3f;

    [Header("Light detection")]
    public float timeToSpotPlayer = 2f;
    private float lightExposureTimer = 0f;
    public LayerMask obstacleMask;
    public Light2D playerTorch;

    [Header("Animator / SFX")]
    public Animator animator;
    public string animatorStateParam = "State";
    public AudioClip[] idleSounds;
    public AudioClip[] attackSounds;
    public float soundVolume = 1.0f;

    [Header("Visual")]
    public Transform visual;

    private enum State { Hovering, Hunting }
    private State state = State.Hovering;

    private Vector2 hoverTarget;

    private Seeker seeker;
    private Rigidbody2D rb;
    private Path path;
    private int currentWaypoint = 0;

    private Coroutine idleSoundCoroutine;
    private Coroutine huntingSoundCoroutine;

    [Header("Pathfinding")]
    public float pathUpdateInterval = 0.2f;
    private float pathUpdateTimer = 0f;

    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        if (visual == null) visual = transform;
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        StartIdleSoundLoop();
        UpdatePath(); // initial path
    }

    private void Update()
    {
        if (player == null) return;

        // Light detection
        if (IsExposedToLight())
        {
            lightExposureTimer += Time.deltaTime;
            if (lightExposureTimer >= timeToSpotPlayer && state != State.Hunting)
                StartHunting();
        }
        else
        {
            lightExposureTimer -= Mathf.Max(0f, lightExposureTimer) * 0.5f * Time.deltaTime;
        }

        // Path update timer
        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0f)
        {
            pathUpdateTimer = pathUpdateInterval;
            UpdatePath();
        }
    }

    private void FixedUpdate()
    {
        if (path == null || path.vectorPath.Count == 0) return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            path = null;
            UpdatePath();
            return;
        }

        Vector2 targetPoint = path.vectorPath[currentWaypoint];
        float speed = (state == State.Hunting) ? huntingSpeed : hoveringSpeed;

        // Move towards waypoint
        rb.position = Vector2.MoveTowards(rb.position, targetPoint, speed * Time.fixedDeltaTime);

        // Waypoint reached
        if (Vector2.Distance(rb.position, targetPoint) < nextWaypointDistance)
            currentWaypoint++;

        // Rotate visual
        if (visual != null)
        {
            Vector2 dir = targetPoint - rb.position;
            if (dir.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                visual.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
            }
        }
    }

    private void UpdatePath()
    {
        if (seeker == null || !seeker.IsDone() || player == null) return;

        Vector2 targetPos;

        if (state == State.Hunting)
        {
            targetPos = player.position;
        }
        else // Hovering
        {
            // Generate a new hover target if path ended or target reached
            if (path == null || path.vectorPath.Count <= 1 || Vector2.Distance(rb.position, hoverTarget) < 0.5f)
            {
                PickNewHoverTarget();
            }
            targetPos = hoverTarget;
        }

        seeker.StartPath(rb.position, targetPos, OnPathComplete);
    }

    private void PickNewHoverTarget()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        hoverTarget = (Vector2)player.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * followRadius;
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error && p.vectorPath.Count > 0)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    private bool IsExposedToLight()
    {
        if (playerTorch == null) return false;

        Vector2 dir = (Vector2)transform.position - (Vector2)playerTorch.transform.position;
        float dist = dir.magnitude;

        if (dist > playerTorch.pointLightOuterRadius) return false;

        if (playerTorch.lightType == Light2D.LightType.Sprite)
        {
            float angleToMonster = Vector2.Angle(playerTorch.transform.up, dir);
            if (angleToMonster > playerTorch.pointLightInnerAngle / 2f) return false;
        }

        if (Physics2D.Raycast(playerTorch.transform.position, dir.normalized, dist, obstacleMask))
            return false;

        return true;
    }

    private void StartHunting()
    {
        SetState(State.Hunting);
        StopIdleSoundLoop();
        StartHuntingSoundLoop();
        UpdatePath();
    }

    private void SetState(State newState)
    {
        state = newState;
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetInteger(animatorStateParam, (state == State.Hunting) ? 1 : 0);
    }

    // --- Sounds ---
    private void StartIdleSoundLoop()
    {
        if (idleSoundCoroutine != null) StopCoroutine(idleSoundCoroutine);
        idleSoundCoroutine = StartCoroutine(IdleSoundsLoop());
    }

    private void StopIdleSoundLoop()
    {
        if (idleSoundCoroutine != null) { StopCoroutine(idleSoundCoroutine); idleSoundCoroutine = null; }
    }

    private void StartHuntingSoundLoop()
    {
        if (huntingSoundCoroutine != null) StopCoroutine(huntingSoundCoroutine);
        huntingSoundCoroutine = StartCoroutine(HuntingSoundsLoop());
    }

    private IEnumerator IdleSoundsLoop()
    {
        while (state == State.Hovering)
        {
            if (idleSounds.Length > 0 && AudioManager.instance != null)
                AudioManager.instance.PlayRandomAudioClip(idleSounds, transform, soundVolume);
            yield return new WaitForSeconds(Random.Range(2f, 5f));
        }
    }

    private IEnumerator HuntingSoundsLoop()
    {
        while (state == State.Hunting)
        {
            if (attackSounds.Length > 0 && AudioManager.instance != null)
                AudioManager.instance.PlayRandomAudioClip(attackSounds, transform, soundVolume);
            yield return new WaitForSeconds(Random.Range(1f, 4f));
        }
    }
}
