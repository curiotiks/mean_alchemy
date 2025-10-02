using UnityEngine;

// Attach to the GameObject with the SpriteRenderer (your glow circle)
[RequireComponent(typeof(SpriteRenderer))]
public class WarpIndicator : MonoBehaviour
{
    [Header("Scale Pulse")]
    [Tooltip("Base scale multiplier (1 = original scale).")]
    public float baseScale = 1f;
    [Tooltip("How much to grow/shrink around the base (0.1 = ±10%).")]
    public float scaleAmplitude = 0.12f;
    [Tooltip("Pulses per second.")]
    public float pulseHz = 1.5f;

    [Header("Alpha Pulse")]
    [Range(0f, 1f)] public float baseAlpha = 0.6f;
    [Range(0f, 1f)] public float alphaAmplitude = 0.25f;

    SpriteRenderer sr;
    Vector3 initialLocalScale;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        initialLocalScale = transform.localScale;
    }

    void Update()
    {
        // Sine wave 0..1..0..-1 rhythm
        float t = Time.time * Mathf.PI * 2f * pulseHz;
        float s = Mathf.Sin(t);

        // Scale pulse
        float scale = baseScale * (1f + s * scaleAmplitude);
        transform.localScale = initialLocalScale * scale;

        // Alpha pulse
        var c = sr.color;
        c.a = Mathf.Clamp01(baseAlpha + s * alphaAmplitude);
        sr.color = c;
    }
}