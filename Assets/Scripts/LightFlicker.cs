using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    [Header("Light Reference")]
    public Light2D light2D;

    [Header("Flicker Timing")]
    [Tooltip("Average time between flickers (seconds)")]
    public float flickerInterval = 3f;

    [Tooltip("How long a flicker lasts (seconds)")]
    public float flickerDuration = 0.5f;

    [Header("Intensity")]
    [Tooltip("Minimum intensity during flicker")]
    public float minIntensity = 0.0f;

    [Tooltip("Maximum intensity during flicker")]
    public float maxIntensity = 1f;

    private float baseIntensity;
    private float timer;

    void Start()
    {
        if (light2D == null)
            light2D = GetComponent<Light2D>();

        baseIntensity = light2D.intensity;
        ResetTimer();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            StartCoroutine(Flicker());
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        // Adds randomness so flicker is not predictable
        timer = Random.Range(flickerInterval * 0.7f, flickerInterval * 1.3f);
    }

    System.Collections.IEnumerator Flicker()
    {
        float elapsed = 0f;

        while (elapsed < flickerDuration)
        {
            light2D.intensity = Random.Range(minIntensity, maxIntensity);
            elapsed += Time.deltaTime;
            yield return null;
        }

        light2D.intensity = baseIntensity;
    }
}
