using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    // will be set automatically if present
    Rigidbody2D rb;
    // smoothing for rotation applied via Rigidbody2D
    public float rotationSmooth = 18f;
    float targetRotation = 0f;

    // input / state
    Vector2 moveInput;
    public bool isMoving = false;

    [Header("Footstep Audio")]
    public AudioClip[] footstepClips;
    public float footstepInterval = 0.4f;
    public float footstepVolume = 0.8f;

    float footstepTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            // add one if missing (helps avoid setup mistakes)
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            // allow controlled rotation via MoveRotation (we'll zero angular velocity each frame)
            rb.constraints = RigidbodyConstraints2D.None;
        }
        else
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.None;
        }

        // smooth physics-driven visuals
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Ensure there's a collider on the player (CircleCollider2D is a good default)
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<CircleCollider2D>();
        }
    }

    void Update()
    {
        // 1) read input (WASD)
        float ix = 0f;
        float iy = 0f;
        if (Input.GetKey(KeyCode.A)) ix -= 1f;
        if (Input.GetKey(KeyCode.D)) ix += 1f;
        if (Input.GetKey(KeyCode.W)) iy += 1f;
        if (Input.GetKey(KeyCode.S)) iy -= 1f;

        moveInput = new Vector2(ix, iy);
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
        isMoving = moveInput.sqrMagnitude > 0f;

        // 2) face mouse for torch aiming / visuals
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 faceDir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(faceDir.y, faceDir.x) * Mathf.Rad2Deg;
        // calculate desired rotation but don't set transform directly (use physics in FixedUpdate)
        targetRotation = angle - 90f;

        // 3) footstep audio
        HandleFootsteps();
    }

    void FixedUpdate()
    {
        // Move using Rigidbody2D to respect physics / colliders / tilemap colliders
        if (rb != null && moveInput.sqrMagnitude > 0f)
        {
            Vector2 nextPos = rb.position + moveInput * speed * Time.fixedDeltaTime;
            rb.MovePosition(nextPos);
        }

        // Smoothly rotate the rigidbody to face targetRotation (prevents jitter)
        if (rb != null)
        {
            float newRot = Mathf.LerpAngle(rb.rotation, targetRotation, rotationSmooth * Time.fixedDeltaTime);
            rb.MoveRotation(newRot);
            // prevent any physics-driven rotation from accumulating
            rb.angularVelocity = 0f;
        }
    }

    // Optional: example collision callback
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Finish")) return;
        Debug.Log("Level Complete!");

        // Switch to win scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Win");
    }

    void HandleFootsteps()
    {
        if (!isMoving)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            PlayFootstep();
            footstepTimer = footstepInterval;
        }
    }

    void PlayFootstep()
    {
        if (AudioManager.instance == null || footstepClips.Length == 0)
            return;

        AudioManager.instance.PlayRandomAudioClip(
            footstepClips,
            transform,
            footstepVolume
        );
    }
}

