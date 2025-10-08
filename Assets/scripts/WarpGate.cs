using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class WarpGate : MonoBehaviour
{
    public enum GateMode
    {
        AlwaysActive,
        AlwaysInactive,
        RequiresBountyAndFamiliar
    }

    [Header("Behaviour")]
    [SerializeField] private GateMode mode = GateMode.RequiresBountyAndFamiliar; // set per warp in Inspector

    [Header("Wiring")]
    [SerializeField] private WarpIndicator indicator; // child visual; optional auto-wire

    [Header("Visual Hiding")]
    [Tooltip("If true, the indicator (and any Extra Visual Roots) will be fully SetActive(false) when locked.")]
    [SerializeField] private bool hideInactiveFully = true;
    [Tooltip("Optional additional GameObjects to toggle along with the indicator (e.g., halo child, parent frame, etc.)")]
    [SerializeField] private GameObject[] extraVisualRoots;

    [Header("Warp Destination")] 
    [SerializeField] private string sceneToLoad = "Combat";

    [Header("Blocked Message")] 
    [TextArea]
    [SerializeField] private string blockedMessage =
        "You need a selected bounty AND a powered familiar (use the table).";

    private bool gateOpen;
    // Cached last-known reasons (for debugging)
    private bool lastHasBounty;
    private bool lastHasFamiliar;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        if (!indicator) indicator = GetComponentInChildren<WarpIndicator>(true);
        if ((extraVisualRoots == null || extraVisualRoots.Length == 0) && indicator != null)
        {
            // Try to include the indicator's parent so both parent/child visuals hide together
            var list = new System.Collections.Generic.List<GameObject>();
            var p = indicator.transform.parent;
            if (p != null) list.Add(p.gameObject);
            extraVisualRoots = list.ToArray();
        }
    }

    void Awake()
    {
        if (!indicator) indicator = GetComponentInChildren<WarpIndicator>(true);
    }

    void OnEnable()
    {
        EvaluateGate();
    }

    /// <summary>
    /// Re-evaluate whether this gate should be open and update the indicator.
    /// Call this after bounty selection/abandon and after familiar transmutation.
    /// </summary>
    public void EvaluateGate()
    {
        // Compute sources once from single truth
        lastHasBounty   = HasBounty();
        lastHasFamiliar = FamiliarState.Powered; // <<< single source of truth

        switch (mode)
        {
            case GateMode.AlwaysActive:
                gateOpen = true;
                break;
            case GateMode.AlwaysInactive:
                gateOpen = false;
                break;
            case GateMode.RequiresBountyAndFamiliar:
                gateOpen = lastHasBounty && lastHasFamiliar;
                break;
        }

#if UNITY_EDITOR
        Debug.Log($"[WarpGate:{name}] EvaluateGate -> mode={mode}, gateOpen={gateOpen}, hasBounty={lastHasBounty}, hasFamiliar={lastHasFamiliar}, FamiliarState.Powered={FamiliarState.Powered}");
#endif

        if (indicator)
            indicator.SetActiveVisual(gateOpen);

        if (hideInactiveFully)
        {
            if (indicator) indicator.gameObject.SetActive(gateOpen);
            if (extraVisualRoots != null)
            {
                foreach (var go in extraVisualRoots)
                {
                    if (go) go.SetActive(gateOpen);
                }
            }
        }
    }

    /// <summary>
    /// Refresh all gates in the scene. Call this when bounty/familiar state changes.
    /// </summary>
    public static void RefreshAllGates()
    {
        var gates = GameObject.FindObjectsOfType<WarpGate>(includeInactive: true);
        foreach (var g in gates) g.EvaluateGate();
    }

    [ContextMenu("Evaluate Gate Now")] private void ContextEvaluate() => EvaluateGate();

    private bool HasBounty()
    {
        return BountyBoardManager.instance != null &&
               BountyBoardManager.instance.currentBounty != null;
    }

    private bool HasFamiliar()
    {
        return FamiliarState.Powered;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

#if UNITY_EDITOR
        Debug.Log($"[WarpGate:{name}] OnTriggerEnter2D by {other.name} -> gateOpen={gateOpen}, hasBounty={lastHasBounty}, hasFamiliar={lastHasFamiliar}");
#endif

        if (!gateOpen)
        {
            ShowAlert(blockedMessage);
            // Optional: log
            try
            {
                GameLogger.Instance?.LogEvent(
                    "warp_blocked",
                    $"gate={name},mode={mode},hasBounty={HasBounty()},hasFamiliar={HasFamiliar()}"
                );
            }
            catch {}
            return;
        }

        // Optional: log success
        try
        {
            GameLogger.Instance?.LogEvent(
                "warp_enter",
                $"gate={name},mode={mode}"
            );
        }
        catch {}

        // Load target (swap to your loading shim if desired)
        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void Alert(string msg);
    private void ShowAlert(string msg) => Alert(msg);
#else
    private void ShowAlert(string msg)
    {
        Debug.LogWarning(msg);
        // If you prefer an in-game popup, call it here.
    }
#endif
}
