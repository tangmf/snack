namespace Jacob_Yakesh.MonsterChaseLogic.PlayerLogic
{
    using System.Collections;
    using UnityEngine;
    public class Player : MonoBehaviour
    {
        public float speed = 1f;
        [SerializeField] private AudioClip footstepsSound; // Sound for footsteps
        private AudioSource audioSource;
        public bool IsSprinting => Input.GetKey(KeyCode.LeftShift);
        private Animator animator;

        // Fade settings
        [SerializeField] private float fadeDuration = 0.5f; // Duration of fade in/out
        private Coroutine fadeCoroutine;

        private Vector2 lastDirection = Vector2.down; // Default facing direction is down

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = footstepsSound; // Assign the footsteps sound clip here
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            Vector3 movement = Vector3.zero;

            // WASD and Arrow key movement
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                movement += Vector3.left;
            }
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                movement += Vector3.right;
            }
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                movement += Vector3.up;
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                movement += Vector3.down;
            }

            // Adjust speed when Shift key is held
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? speed * 1.5f : speed;

            // Apply movement
            transform.Translate(movement * currentSpeed * Time.deltaTime);

            // Update animator parameters
            if (movement.magnitude > 0)
            {
                lastDirection = new Vector2(movement.x, movement.y).normalized; // Store last movement direction
                animator.SetFloat("moveX", lastDirection.x);
                animator.SetFloat("moveY", lastDirection.y);
            }
            animator.SetBool("isMoving", movement.magnitude > 0);

            // Handle footsteps sound
            if (movement.magnitude > 0)
            {
                PlayFootstepsSound();
            }
            else
            {
                StopFootstepsSound();
            }
        }

        public Vector2 GetFacingDirection()
        {
            return lastDirection; // Return the last recorded movement direction
        }

        private void PlayFootstepsSound()
        {
            if (!audioSource.isPlaying) // Check if not already playing
            {
                audioSource.loop = true; // Ensure the sound loops
                audioSource.volume = 0; // Start with volume 0
                audioSource.Play();

                if (fadeCoroutine != null)
                    StopCoroutine(fadeCoroutine); // Stop any existing fade coroutine
                
                fadeCoroutine = StartCoroutine(FadeInFootsteps());
            }
        }

        private void StopFootstepsSound()
        {
            if (audioSource.isPlaying)
            {
                if (fadeCoroutine != null)
                    StopCoroutine(fadeCoroutine); // Stop any existing fade coroutine
                
                fadeCoroutine = StartCoroutine(FadeOutFootsteps());
            }
        }

        private IEnumerator FadeInFootsteps()
        {
            float targetVolume = 0.3f; // Set your desired target volume

            while (audioSource.volume < targetVolume)
            {
                audioSource.volume += Time.deltaTime / fadeDuration;
                yield return null; // Wait for the next frame
            }

            audioSource.volume = targetVolume; // Ensure we set to target volume
        }

        private IEnumerator FadeOutFootsteps()
        {
            while (audioSource.volume > 0)
            {
                audioSource.volume -= Time.deltaTime / fadeDuration;
                yield return null; // Wait for the next frame
            }
            
            audioSource.Stop(); // Stop playing sound after fading out
            audioSource.volume = 1f; // Reset volume for next play
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Walls"))
            {
                Debug.Log("You hit a wall!");
            }
        }
    }

}
