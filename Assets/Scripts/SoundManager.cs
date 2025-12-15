using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private float maxVolume = 5.0f;

    [SerializeField]
    public float currentVolume = 0.0f;
    
    [SerializeField]
    private float volumeIncrement = 1.0f;

    public CircleCollider2D soundCollider;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float delta = volumeIncrement * Time.deltaTime;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
            Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                currentVolume = Mathf.Clamp(currentVolume + delta, 0f, maxVolume);
            } else
            {
                currentVolume = Mathf.Clamp(currentVolume - delta, 0f, maxVolume);
            }

        soundCollider.radius = currentVolume;
    }
}
