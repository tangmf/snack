using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KeyPickup : MonoBehaviour
{
    [Tooltip("Unique id for this key (e.g. 'red', 'blue' or 'key1')")]
    public string keyId = "key";
    public AudioClip[] pickupSounds;

    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        // ensure trigger so player can press F while overlapping
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Input.GetKeyDown(KeyCode.F))
        {
            Collect();
        }
    }

    public void Collect()
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