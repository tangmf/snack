using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KeyPickup : MonoBehaviour
{
    [Tooltip("Unique id for this key (e.g. 'red', 'blue' or 'key1')")]
    public string keyId = "key";

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
        // play pickup sound / VFX here
        Destroy(gameObject);
    }
}