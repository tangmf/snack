using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign a Light2D (Point) configured as a cone. If empty the script will try to find one in children.")]
    public Light2D coneLight2D;

    [Header("Position & orientation")]
    [Tooltip("Local offset from player centre")]
    public Vector3 localOffset = new Vector3(0f, 0.3f, 0f);
    [Tooltip("Extra rotation offset in degrees")]
    public float rotationOffset = 0f;
    [Tooltip("Smooth speed for rotation")]
    public float rotationSmooth = 20f;

    [Header("Cone parameters")]
    [Tooltip("Outer angle (degrees) of the cone")]
    public float outerAngle = 60f;
    [Tooltip("Inner angle (degrees) for soft edge")]
    public float innerAngle = 40f;
    [Tooltip("Outer radius (distance)")]
    public float outerRadius = 3f;
    [Tooltip("Inner radius (soft start distance)")]
    public float innerRadius = 0.5f;
    [Tooltip("Light intensity multiplier")]
    public float intensity = 1f;
    [Tooltip("Scale multiplier applied to radii")]
    public float radiusScale = 1f;

    [Header("Controls")]
    public KeyCode toggleKey = KeyCode.Space;
    public bool startOn = true;

    bool isOn;

    [Header("Sound")]
    public AudioClip soundClip;
    public float soundVolume = 1.0f;

    void Awake()
    {
        isOn = startOn;

        if (coneLight2D == null)
            coneLight2D = GetComponentInChildren<Light2D>();

        if (coneLight2D == null)
        {
            Debug.LogError("TorchController: No Light2D found. Add a Light2D (Point) as child or assign it in inspector.");
            enabled = false;
            return;
        }

        // Ensure light is point type so angle/radius fields apply
        coneLight2D.lightType = Light2D.LightType.Point;

        ApplySettings();
        ApplyEnabledState(immediate: true);
    }

    void Update()
    {
        // Toggle on key press
        if (Input.GetKeyDown(toggleKey))
        {
            isOn = !isOn;
            PlayTorchSound();
            ApplyEnabledState(immediate: false);
        }

        // Update position & rotation every frame
        UpdateTransform();
    }

    void ApplyEnabledState(bool immediate = false)
    {
        // Enable/disable the Light2D GameObject for best performance
        if (coneLight2D != null)
            coneLight2D.gameObject.SetActive(isOn);
    }

    void ApplySettings()
    {
        if (coneLight2D == null) return;

        // Configure point-light cone parameters
        coneLight2D.pointLightOuterAngle = Mathf.Clamp(outerAngle, 0f, 360f);
        coneLight2D.pointLightInnerAngle = Mathf.Clamp(innerAngle, 0f, coneLight2D.pointLightOuterAngle);
        coneLight2D.pointLightOuterRadius = Mathf.Max(0f, outerRadius) * radiusScale;
        coneLight2D.pointLightInnerRadius = Mathf.Max(0f, innerRadius) * radiusScale;
        coneLight2D.intensity = intensity;
    }

    void UpdateTransform()
    {
        if (coneLight2D == null) return;

        // Position relative to player
        coneLight2D.transform.localPosition = localOffset;

        // Aim direction: follow player rotation (transform.up) if available,
        // otherwise fall back to mouse direction.
        Vector2 aimDir = transform.up;
        if (aimDir.sqrMagnitude < 0.0001f)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            aimDir = (mouseWorld - transform.position).normalized;
        }

        float targetAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg + rotationOffset;
        // Light2D cone orientation expects the light's forward; sprite default orientation may differ.
        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle - 90f);

        // Smoothly rotate the light to the target
        coneLight2D.transform.rotation = Quaternion.Slerp(coneLight2D.transform.rotation, targetRot, Time.deltaTime * rotationSmooth);
    }

    // Public API
    public void SetTorchOn(bool on)
    {
        isOn = on;
        ApplyEnabledState(immediate: true);
    }

    public bool IsTorchOn() => isOn;

    public void SetConeParameters(float newOuterAngle, float newInnerAngle, float newOuterRadius, float newInnerRadius, float newIntensity, float newRadiusScale = 1f)
    {
        outerAngle = newOuterAngle;
        innerAngle = newInnerAngle;
        outerRadius = newOuterRadius;
        innerRadius = newInnerRadius;
        intensity = newIntensity;
        radiusScale = newRadiusScale;
        ApplySettings();
    }

    void PlayTorchSound()
    {
        if (AudioManager.instance == null || soundClip == null)
            return;

        AudioManager.instance.PlayAudioClip(
            soundClip,
            transform,
            soundVolume
        );
    }
}