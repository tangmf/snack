namespace Jacob_Yakesh.MonsterChaseLogic.MonsterLogic
{
    using UnityEngine;
    using Pathfinding;
    using System.Collections;
    using PlayerLogic;

    public class AiAgent : MonoBehaviour
    {
        private AIPath path;

    [Header("Feature Toggles")]
        [Tooltip("Enable or disable the monster's reaction to player footsteps.")]
        [SerializeField] private bool enableFootstepsListening = true;

        [Tooltip("Enable or disable the monster's field of view detection.")]
        [SerializeField] private bool enableMonsterFov = true;

        [Tooltip("Enable or disable the monster losing track of the player after a certain time.")]
        [SerializeField] private bool enableLosingTrack = true;

        [Tooltip("Enable or disable teleportation logic during chases.")]
        [SerializeField] private bool enableChaseTeleportation = true;

        [Tooltip("Allow or prevent the monster from switching targets based on the player's position.")]
        [SerializeField] private bool allowTargetSwitching = true;

        [Tooltip("Enable or disable teleportation between alternative targets while idling.")]
        [SerializeField] private bool enableIdleTeleportation = true;

        [Tooltip("Enable or disable sound effects for the monster.")]
        [SerializeField] private bool enableSoundEffects = true;

        [Header("Speed Settings")]
        [Tooltip("Normal movement speed of the AI.")]
        [SerializeField] private float baseSpeed = 1.1f;

        [Tooltip("Speed multiplier applied when the monster is chasing the player.")]
        [SerializeField] private float chaseSpeedMultiplier = 1.3f;

        [Header("Target Settings")]
        [Tooltip("Reference to the player object.")]
        [SerializeField] private Transform player;

        [Tooltip("Alternative targets the monster moves to when not chasing the player.")]
        [SerializeField] private Transform[] alternativeTargets;

        [Tooltip("Idle duration when the monster reaches a random target.")]
        [SerializeField] private float targetIdleTime = 15f;

        [Tooltip("Cooldown time in seconds before switching targets again.")]
        [SerializeField] private float switchCooldown = 10f;

        [Header("FOV Settings")]
        [Tooltip("Field of view angle for detecting the player.")]
        [SerializeField] private float fovAngle = 120;

        [Tooltip("Maximum detection range of the monster's field of view.")]
        [SerializeField] private float fovRange = 4f;

        [Header("Audio Settings")]
        [Tooltip("Sound played when the monster starts tracking the player.")]
        [SerializeField] private AudioClip startTrackingSound;

        [Tooltip("Sound played when the monster stops tracking the player.")]
        [SerializeField] private AudioClip stopTrackingSound;

        [Tooltip("Sound played when the monster is idling at a target.")]
        [SerializeField] private AudioClip idleSoundEffect;

        [Tooltip("Sound of the monster's footsteps while moving.")]
        [SerializeField] private AudioClip footstepsSound;

        [Tooltip("Sound played when the monster teleports.")]
        [SerializeField] private AudioClip teleportSound;

        [Tooltip("Maximum distance at which the player's footsteps can be heard.")]
        [SerializeField] private float maxFootstepsDistance = 10f;

        [Header("Detection Settings")]
        [Tooltip("Detection range for a sprinting player.")]
        [SerializeField] private float sprintDetectionRange = 7f;

        [Tooltip("Duration for which the AI ignores the player after losing them.")]
        [SerializeField] private float ignorePlayerDuration = 5f;

        [Header("Chase Settings")]
        [Tooltip("Time before the monster loses track of the player when they're out of sight.")]
        [SerializeField] private float lostPlayerDuration = 8f;

        [Tooltip("Randomized duration range before the monster gives up on chasing.")]
        [SerializeField] private Vector2 chaseGiveUpTimeRange = new Vector2(25f, 40f);

        [Header("Teleportation Settings")]
        [Tooltip("Probability of teleporting when the player is audible but not visible.")]
        [SerializeField] private float teleportChance = 0.3f;

        [Tooltip("Delay in seconds before retrying a failed teleport attempt.")]
        [SerializeField] private float teleportRetryDelay = 5f;

        [Tooltip("Probability of teleporting from one alternative target to another.")]
        [SerializeField] private float teleportToAltTargetChance = 0.4f;

        [Header("Child Settings")]
        [Tooltip("Reference to the monster's sprite object for animations.")]
        [SerializeField] private Transform spriteObject;

        private AudioSource audioSource; // Main audio source for the AI
        private AudioSource footstepsAudioSource; // Separate audio source for footsteps
        private Transform currentTarget; // Current target the AI is moving towards
        private bool trackingPlayer; // Whether the AI is currently tracking the player
        private bool idlingAtTarget; // Whether the AI is idling at a random target
        private float idleTimer; // Timer for idling duration
        private Animator childAnimator; // Animator for handling movement animations
        private Player playerScript; // Script attached to the player for state checks
        private float lostPlayerTimer; // Timer for how long the AI remembers the player after losing sight
        private float ignorePlayerTimer; // Timer for ignoring the player after losing them
        private bool isTeleporting = false; // Flag to prevent actions during teleportation
        private float hearingTimer = 0f; // Timer for how long the player has been audible but unseen
        private int teleportState = 0; // State of teleportation logic (0 = No attempt, 1 = Can teleport, 2 = Teleport used)
        private float teleportRetryTimer = 0f; // Timer to track retry delay for teleportation
        private bool hasCaughtPlayer = false; // Flag to track if the player has been caught
        private float chaseTimer = 0f; // Timer to track how long the AI has been chasing the player
        private float chaseGiveUpTime; // Randomized time threshold for giving up
        private float timeSinceLastSwitch = 0f; // Tracks time since the last target switch

        private void Awake()
        {
            // Initialize child animator, audio sources, and player script references
            if (spriteObject != null)
            {
                childAnimator = spriteObject.GetComponent<Animator>();
            }
            audioSource = GetComponent<AudioSource>();
            footstepsAudioSource = gameObject.AddComponent<AudioSource>();

            if (player != null)
            {
                playerScript = player.GetComponent<Player>();
            }
        }

        private void Start()
        {
            // Initialize AI settings
            path = GetComponent<AIPath>();
            trackingPlayer = false;
            path.maxSpeed = baseSpeed;
            SwitchToRandomTarget(); // Start by targeting a random alternative target
            lostPlayerTimer = lostPlayerDuration;
            ignorePlayerTimer = 0;
            chaseGiveUpTime = Random.Range(chaseGiveUpTimeRange.x, chaseGiveUpTimeRange.y); // Randomize the give-up time
        }

        private void Update()
        {
            if (isTeleporting) return; // Skip updates if teleporting

            if (idlingAtTarget)
            {
                HandleIdling(); // Handle idle behavior
                return;
            }

            path.destination = currentTarget.position; // Continuously update the AI's destination

            UpdateMovementAnimation(); // Update walking animations based on movement
            UpdateFootstepsSound(); // Play or adjust footsteps sound based on movement

            if (ignorePlayerTimer > 0)
            {
                ignorePlayerTimer -= Time.deltaTime; // Reduce ignore timer if active
            }

            // Switch to player target if player is sprinting and within detection range, only if footsteps listening is enabled
            if (enableFootstepsListening && ignorePlayerTimer <= 0 && playerScript != null && playerScript.IsSprinting && Vector2.Distance(transform.position, player.position) < sprintDetectionRange)
            {
                SwitchToPlayerTarget();
            }



            // Switch to player target if within field of view
            if (ignorePlayerTimer <= 0 && IsTargetInsideFov(player) && !trackingPlayer && enableMonsterFov)
            {
                SwitchToPlayerTarget();
            }

            if (trackingPlayer)
            {
                HandleChasingPlayer(); // Behavior while chasing the player
            }
            else if (Vector3.Distance(transform.position, currentTarget.position) < 1f)
            {
                StartIdlingAtTarget(); // Idle when reaching a random target
            }

            timeSinceLastSwitch += Time.deltaTime;

            // Check if the player is too far from the current target
            if (allowTargetSwitching && !trackingPlayer && currentTarget != null)
            {
                float distanceToCurrentTarget = Vector2.Distance(player.position, currentTarget.position);
                if (distanceToCurrentTarget > fovRange * 3 && timeSinceLastSwitch >= switchCooldown) // Adjust the threshold as needed
                {
                    SwitchToRandomTarget();
                    timeSinceLastSwitch = 0f; // Reset the timer after switching
                }
            }

        }

        private void HandleChasingPlayer()
    {
        // Handle logic for chasing the player
        if (!IsTargetInsideFov(player))
        {
            if (IsPlayerAudible() && enableChaseTeleportation)
            {
                hearingTimer += Time.deltaTime;

                if (hearingTimer >= 4f && teleportState == 0 && !isTeleporting)
                {
                    float randomValue = Random.value;
                    Debug.Log($"Random value: {randomValue}, Teleport chance: {teleportChance}");

                    if (randomValue < teleportChance)
                    {
                        Debug.Log("Teleportation triggered.");
                        teleportState = 1; // Ready to teleport
                    }
                    else
                    {
                        Debug.Log("Teleportation failed.");
                        teleportState = 2; // Mark teleportation as attempted
                        teleportRetryTimer = teleportRetryDelay; // Start retry timer
                    }
                }
            }
            else
            {
                hearingTimer = 0f; // Reset hearing timer when player is not audible
            }

            if (teleportState == 1 && !isTeleporting)
            {
                StartCoroutine(PauseAndTeleport());
                return;
            }
        }
        else
        {
            hearingTimer = 0f; // Reset when the player is back in FOV
            teleportState = 0; // Reset teleportation state
        }

        // Retry teleportation if the timer has elapsed
        if (teleportState == 2 && teleportRetryTimer > 0)
        {
            teleportRetryTimer -= Time.deltaTime;

            if (teleportRetryTimer <= 0)
            {
                teleportState = 0; // Reset state to attempt teleport again
            }
        }

        if (IsTargetInsideFov(player) || (playerScript != null && playerScript.IsSprinting))
        {
            lostPlayerTimer = lostPlayerDuration; // Reset the lost player timer
            chaseTimer += Time.deltaTime; // Increment the chase timer

            // Check if chase time exceeds the give-up threshold
            if (chaseTimer >= chaseGiveUpTime)
            {
                Debug.Log("Monster gave up chasing the player!");
                trackingPlayer = false;
                chaseTimer = 0f; // Reset the chase timer
                SwitchToRandomTarget(); // Switch to an alternative target
                chaseGiveUpTime = Random.Range(chaseGiveUpTimeRange.x, chaseGiveUpTimeRange.y); // Randomize the next give-up time
                if (enableSoundEffects)
                {
                    audioSource.PlayOneShot(stopTrackingSound);
                }
                return;
            }
        }
        else
        {
            lostPlayerTimer -= Time.deltaTime;
            if (lostPlayerTimer <= 0 && enableLosingTrack)
            {
                trackingPlayer = false;
                teleportState = 0; // Reset teleport state when monster loses the player
                SwitchToRandomTarget();
                if (enableSoundEffects)
                {
                    audioSource.PlayOneShot(stopTrackingSound);
                }
                ignorePlayerTimer = ignorePlayerDuration;
                chaseTimer = 0f; // Reset the chase timer
                hasCaughtPlayer = false;
            }
        }

        // Check if the AI reaches or catches the player
        if (Vector3.Distance(transform.position, player.position) < 1f && !hasCaughtPlayer)  // You can adjust the threshold
        {
            hasCaughtPlayer = true;
            Debug.Log("Monster caught the player!");
            // Optionally trigger any other events when the monster catches the player
        }
    }

        private bool IsPlayerAudible()
        {   
            if (playerScript == null || !playerScript.IsSprinting) return false;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            return distanceToPlayer <= sprintDetectionRange; // Audible if within range
        }

    private IEnumerator PauseAndTeleport()
    {
        // Handles teleportation logic with a brief pause
        isTeleporting = true;
        path.canMove = false; // Temporarily pause movement for a smooth teleport
        StopFootstepsSound(); // Stop footsteps sound during teleport

        // Find the closest alternative target to the player
        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Transform target in alternativeTargets)
        {
            float distance = Vector2.Distance(player.position, target.position);
            if (distance < closestDistance)
            {
                closestTarget = target;
                closestDistance = distance;
            }
        }

        if (closestTarget != null)
        {
            yield return new WaitForSeconds(0.5f); // Optional short delay for teleporting

            // Play the teleport sound
            if (teleportSound != null && enableSoundEffects)
            {
                audioSource.PlayOneShot(teleportSound);
            }

            transform.position = closestTarget.position; // Teleport to the closest target
            Debug.Log($"Teleportation successful! Teleported to: {closestTarget.name}");

            isTeleporting = false;
            path.canMove = true; // Resume movement
            trackingPlayer = true; // Ensure the monster continues chasing the player
            path.maxSpeed = baseSpeed * chaseSpeedMultiplier; // Resume chase speed
            teleportState = 2; // Mark teleportation as successful
            yield break;
        }

        // If teleport fails, start retry delay
        Debug.Log("Teleportation failed! Retrying in 10 seconds...");
        isTeleporting = false;
        path.canMove = true;
        teleportRetryTimer = teleportRetryDelay;
    }

        private void SwitchToPlayerTarget()
        {
            // Switch to chasing the player
            if (!trackingPlayer && enableSoundEffects)
            {
                audioSource.PlayOneShot(startTrackingSound);
            }

            trackingPlayer = true;
            currentTarget = player;
            path.maxSpeed = baseSpeed * chaseSpeedMultiplier;

            lostPlayerTimer = lostPlayerDuration;
            ignorePlayerTimer = 0;
        }

    // Switches the AI agent's target to one of the two closest alternative targets to the player's position.
    private void SwitchToRandomTarget()
    {
        if (alternativeTargets.Length == 0)
        {
            Debug.LogWarning("No alternative targets are assigned.");
            return;
        }

        // Find the two closest alternative targets to the player's position
        Transform[] closestTargets = new Transform[2];
        float[] closestDistances = { float.MaxValue, float.MaxValue };

        foreach (Transform target in alternativeTargets)
        {
            float distance = Vector2.Distance(player.position, target.position);

            if (distance < closestDistances[0]) // New closest
            {
                closestDistances[1] = closestDistances[0];
                closestTargets[1] = closestTargets[0];

                closestDistances[0] = distance;
                closestTargets[0] = target;
            }
            else if (distance < closestDistances[1]) // Second closest
            {
                closestDistances[1] = distance;
                closestTargets[1] = target;
            }
        }

        // Choose randomly between the two closest targets
        int selectedTargetIndex = Random.Range(0, 2);
        Transform newTarget = closestTargets[selectedTargetIndex];

        if (newTarget != null)
        {
            currentTarget = newTarget;
            path.maxSpeed = baseSpeed; // Reset speed to idle speed
            ignorePlayerTimer = ignorePlayerDuration; // Reset ignore timer
            teleportState = 0; // Reset teleport state
            Debug.Log($"Switched to target: {currentTarget.name}");
        }
        else
        {
            Debug.LogWarning("Failed to find a valid alternative target.");
        }
    }


        // Starts idling behavior when the AI agent reaches a target.
        private void StartIdlingAtTarget()
        {
            idlingAtTarget = true;
            idleTimer = targetIdleTime;
            childAnimator.SetFloat("wander", 0);
            childAnimator.SetFloat("wander", 0);

            StopFootstepsSound();
            if (enableSoundEffects)
            {
                        audioSource.PlayOneShot(idleSoundEffect);
            } 
        }

        private void TeleportToNearestAltTargetToPlayer()
    {
        // Find the closest alternative target to the player
        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Transform target in alternativeTargets)
        {
            float distance = Vector2.Distance(player.position, target.position);
            if (distance < closestDistance)
            {
                closestTarget = target;
                closestDistance = distance;
            }
        }

        if (closestTarget != null)
        {
            // Teleport the AI to the closest target
            transform.position = closestTarget.position;
            Debug.Log($"Teleported to nearest target: {closestTarget.name}");

            // Play teleportation sound
            if (teleportSound != null && enableSoundEffects)
            {
                audioSource.PlayOneShot(teleportSound);
            }

            // Choose a new random target different from the teleported one
            int newTargetIndex;
            do
            {
                newTargetIndex = Random.Range(0, alternativeTargets.Length);
            } while (alternativeTargets[newTargetIndex] == closestTarget);

            currentTarget = alternativeTargets[newTargetIndex];
            Debug.Log($"Switched to new random target: {alternativeTargets[newTargetIndex].name}");
        }
    }


        // Handles the AI agent's behavior while idling.
    private void HandleIdling()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            idlingAtTarget = false;

            // 40% chance to teleport to the nearest target to the player
            if (Random.value < teleportToAltTargetChance && enableIdleTeleportation)
            {
                TeleportToNearestAltTargetToPlayer();
            }
            else
            {
                SwitchToRandomTarget();
            }
        }
        else
        {
            childAnimator.SetFloat("wander", 0);
            childAnimator.SetFloat("wander", 0);
            StopFootstepsSound();
            if (enableSoundEffects)
            {
                PlayIdleSound();
            }
        }
    }

        // Plays the idle sound effect if not already playing.
        private void PlayIdleSound()
        {
            if (!audioSource.isPlaying || audioSource.clip != idleSoundEffect && enableSoundEffects)
            {
                audioSource.clip = idleSoundEffect;
                audioSource.loop = false;
                audioSource.Play();
            }
        }

        // Updates the movement animation based on the AI's velocity.
        private void UpdateMovementAnimation()
        {
            Vector2 movementDirection = path.desiredVelocity.normalized;
            if (movementDirection.sqrMagnitude > 0.1f)
            {
                childAnimator.SetFloat("hunt", movementDirection.x);
                childAnimator.SetFloat("hunt", movementDirection.y);
            }
            else
            {
                childAnimator.SetFloat("wander", 0);
                childAnimator.SetFloat("wander", 0);
            }
        }

        // Adjusts the volume of the footsteps sound based on distance to the player.
        private void UpdateFootstepsSound()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            float volume = Mathf.Clamp01(1 - (distanceToPlayer / maxFootstepsDistance));

            if (path.desiredVelocity.magnitude > 0.1f && enableSoundEffects)
            {
                PlayFootstepsSound(volume);
            }
            else
            {
                StopFootstepsSound();
            }
        }

        // Plays the footsteps sound with a specified volume.
        private void PlayFootstepsSound(float volume)
        {
            if (!footstepsAudioSource.isPlaying || footstepsAudioSource.clip != footstepsSound)
            {
                footstepsAudioSource.clip = footstepsSound;
                footstepsAudioSource.loop = true;
                footstepsAudioSource.Play();
            }
            footstepsAudioSource.volume = volume;
        }

        // Stops the footsteps sound if it's playing.
        private void StopFootstepsSound()
        {
            if (footstepsAudioSource.isPlaying && footstepsAudioSource.clip == footstepsSound)
            {
                footstepsAudioSource.Stop();
            }
        }

        // Checks if a specified target is within the AI's field of view.
        public bool IsTargetInsideFov(Transform target)
        {
            Vector2 directionToTarget = (target.position - transform.position).normalized;
            float angleToTarget = Vector2.Angle(GetLookDirection(), directionToTarget);

            if (angleToTarget < fovAngle / 2)
            {
                float distance = Vector2.Distance(transform.position, target.position);
                if (distance < fovRange)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, fovRange);
                    if (hit.collider != null && hit.collider.transform == target)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        // Gets the AI's current forward-facing direction.
        private Vector2 GetLookDirection()
        {
            return transform.up;
        }
        // Draws the AI's FOV and detection information in the editor.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
    #if UNITY_EDITOR
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, fovRange);
    #endif

            Vector3 leftBoundary = DirectionFromAngle(-fovAngle / 2) * fovRange;
            Vector3 rightBoundary = DirectionFromAngle(fovAngle / 2) * fovRange;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

            if (IsTargetInsideFov(player))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, player.position);
            }
        }
        // Calculates a direction vector from an angle in degrees relative to the AI's rotation.
        private Vector2 DirectionFromAngle(float angleInDegrees)
        {
            float angleWithDirection = angleInDegrees - transform.eulerAngles.z;
            return new Vector2(Mathf.Sin(angleWithDirection * Mathf.Deg2Rad), Mathf.Cos(angleWithDirection * Mathf.Deg2Rad));
        }
    }
}