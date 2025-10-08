using UnityEngine;

/// <summary>
/// Single source of truth for whether the player currently has a powered Familiar.
/// Purely in-memory (resets on app start). Keep this minimal and dependency-free.
/// </summary>
public static class FamiliarState
{
    /// <summary>In-memory truth for this session; defaults to false on app start.</summary>
    public static bool Powered { get; private set; } = false;

    /// <summary>Set powered state.</summary>
    public static void SetPowered(bool value)
    {
        if (Powered == value) return;
#if UNITY_EDITOR
        TraceSet(value);
#endif
        Powered = value;
    }

    /// <summary>No-op for compatibility with older call sites.</summary>
    public static void LoadFromPrefs() { /* intentionally empty: now in-memory only */ }

    /// <summary>Dev helper to force clear state.</summary>
    public static void ResetForDebug() { Powered = false; }

    /// <summary>
    /// Ensure the flag resets at the start of a play session / app launch, even if Domain Reload is disabled.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetOnLoad()
    {
        Powered = false;
#if UNITY_EDITOR
        Debug.Log("[FamiliarState] ResetOnLoad -> Powered = false");
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only: trace who sets the flag to help find unintended callers in the Lab scene.
    /// </summary>
    private static void TraceSet(bool value)
    {
        var st = new System.Diagnostics.StackTrace(2, true);
        Debug.Log($"[FamiliarState] SetPowered({value}) called.\n{st}");
    }
#endif
}