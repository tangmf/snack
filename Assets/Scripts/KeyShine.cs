using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class KeyShine : MonoBehaviour
{
    [Header("Pulse")]
    public float pulseSpeed = 3f;
    public float pulseScale = 0.15f;     // relative scale amount
    public float baseAlpha = 0.5f;
    public float pulseAlpha = 1f;

    [Header("Overlay")]
    public Sprite overlaySprite;         // optional: assign a different sprite for the shine
    public Color tint = Color.white;
    public float overlayScale = 1.2f;    // multiplier relative to source sprite

    SpriteRenderer sourceSR;
    SpriteRenderer shineSR;
    Transform shineT;
    Color shineColor;

    void Awake()
    {
        sourceSR = GetComponent<SpriteRenderer>();
        if (sourceSR == null)
        {
            enabled = false;
            return;
        }

        // create overlay child
        var go = new GameObject("ShineOverlay");
        go.transform.SetParent(transform, false);
        shineT = go.transform;

        shineSR = go.AddComponent<SpriteRenderer>();
        shineSR.sprite = overlaySprite != null ? overlaySprite : sourceSR.sprite;
        shineSR.sortingLayerID = sourceSR.sortingLayerID;
        shineSR.sortingOrder = sourceSR.sortingOrder + 1; // render above key
        shineSR.flipX = sourceSR.flipX;
        shineSR.flipY = sourceSR.flipY;
        // use same material so lighting/sprite settings match; you can assign a custom additive material if desired
        shineSR.sharedMaterial = sourceSR.sharedMaterial;

        // initial color
        shineColor = tint;
        shineColor.a = baseAlpha;
        shineSR.color = shineColor;

        // scale to slightly larger than source
        shineT.localScale = Vector3.one * overlayScale;
    }

    void Update()
    {
        if (shineSR == null || sourceSR == null) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0..1
        // scale pulse
        float s = 1f + Mathf.Lerp(-pulseScale, pulseScale, t);
        shineT.localScale = Vector3.one * overlayScale * s;

        // alpha pulse
        float a = Mathf.Lerp(baseAlpha, pulseAlpha, t);
        shineColor.a = a;
        shineSR.color = shineColor;

        // optional tiny rotation for extra bling
        shineT.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * (pulseSpeed * 0.25f)) * 8f);
    }
}