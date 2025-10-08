using UnityEngine;

/// <summary>
/// Pulsing visual for a warp indicator. Safe if called during Awake order and if the SpriteRenderer lives on a child.
/// </summary>
public class WarpIndicator : MonoBehaviour
{
    [Header("Renderer (optional)")]
    [Tooltip("If set, this SpriteRenderer will be used. If null, the component will search on self, then children.")]
    [SerializeField] private SpriteRenderer targetRenderer;

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

    private bool isActive = true;
    private Color initialColor;
    private static readonly Color InactiveGray = new Color(0.6f, 0.6f, 0.6f, 0.4f);

    private SpriteRenderer sr;
    private Vector3 initialLocalScale;
    private bool _initialized;

    private void Awake()
    {
        TryInit();
    }

    private void TryInit()
    {
        if (_initialized) return;

        sr = targetRenderer; // honor explicit assignment first
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>(true);

        if (sr == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[WarpIndicator:{name}] No SpriteRenderer found on self or children; visual updates will be skipped.");
#endif
            return; // stay uninitialized; public calls will early-return safely
        }

        initialLocalScale = transform.localScale;
        initialColor = sr.color; // cache RGB; alpha will be driven by baseAlpha
        _initialized = true;
    }

    /// <summary>Enable/disable pulsing. When disabled, the indicator turns gray and stops animating.</summary>
    public void SetActiveVisual(bool on)
    {
        isActive = on;
        TryInit();
        if (sr == null) return;

        if (!isActive)
        {
            // Freeze scale and set gray color
            transform.localScale = initialLocalScale * baseScale;
            sr.color = InactiveGray;
        }
        else
        {
            // Restore base color with base alpha; Update() will keep pulsing
            var c = initialColor;
            c.a = Mathf.Clamp01(baseAlpha);
            sr.color = c;
        }
    }

    /// <summary>Optionally change the active RGB tint (alpha still controlled by pulse).</summary>
    public void SetActiveTint(Color rgb)
    {
        TryInit();
        if (sr == null) return;
        initialColor = new Color(rgb.r, rgb.g, rgb.b, Mathf.Clamp01(baseAlpha));
        if (isActive)
        {
            sr.color = initialColor;
        }
    }

    private void Update()
    {
        TryInit();
        if (!isActive || sr == null)
            return;

        float t = Time.time * Mathf.PI * 2f * pulseHz;
        float s = Mathf.Sin(t);

        // Scale pulse
        float scale = baseScale * (1f + s * scaleAmplitude);
        transform.localScale = initialLocalScale * scale;

        // Alpha pulse (keep RGB from initialColor)
        var c = initialColor;
        c.a = Mathf.Clamp01(baseAlpha + s * alphaAmplitude);
        sr.color = c;
    }

    private void OnValidate()
    {
        baseScale = Mathf.Max(0.001f, baseScale);
        scaleAmplitude = Mathf.Max(0f, scaleAmplitude);
        pulseHz = Mathf.Max(0f, pulseHz);
        alphaAmplitude = Mathf.Clamp01(alphaAmplitude);
        baseAlpha = Mathf.Clamp01(baseAlpha);
    }
}