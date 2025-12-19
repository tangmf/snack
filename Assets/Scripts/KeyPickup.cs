using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KeyPickup : MonoBehaviour
{
    public string keyId = "key";
    public AudioClip[] pickupSounds;

    bool playerInRange = false;

    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            Collect();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    void Collect()
    {
        KeyManager.Instance?.CollectKey(keyId);

        PlayPickupSound();
        Destroy(gameObject);
    }

    void PlayPickupSound()
    {
        if (pickupSounds != null && pickupSounds.Length > 0)
        {
            AudioManager.instance.PlayRandomAudioClip(
                pickupSounds,
                transform,
                1.0f
            );
        }
    }
}
