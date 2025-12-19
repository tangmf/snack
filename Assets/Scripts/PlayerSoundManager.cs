using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private float maxVolume = 10.0f;

    [SerializeField]
    public float currentVolume = 0.0f;
    
    [SerializeField]
    private float volumeIncrementPerSecond = 5.0f;

    [SerializeField]
    private float volumeDecrementPerSecond = 15.0f;

    [SerializeField]
    private float noiseMultiplier  = 1.0f;

    public CircleCollider2D soundCollider;
    public GameObject player;
    
    // Start is called before the first frame update
    void Start()
    {
        KeyManager.Instance.OnKeyCollected += OnKeyCollected;
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null && player.GetComponent<PlayerController>().isMoving)
            {
                currentVolume = Mathf.Clamp(currentVolume + volumeIncrementPerSecond * noiseMultiplier * Time.deltaTime, 0f, maxVolume * noiseMultiplier);
            } else
            {
                currentVolume = Mathf.Clamp(currentVolume - volumeDecrementPerSecond * Time.deltaTime, 0f, maxVolume * noiseMultiplier);
            }

        soundCollider.radius = currentVolume;
    }

    void OnDisable()
    {
        if (KeyManager.Instance != null)
            KeyManager.Instance.OnKeyCollected -= OnKeyCollected;
    }

    void OnKeyCollected(string id)
    {
        UpdateNoiseScaling();
    }

    void UpdateNoiseScaling()
    {
        int keys = KeyManager.Instance.CollectedCount;
        noiseMultiplier = 1f + keys * 0.35f;
    }
}
