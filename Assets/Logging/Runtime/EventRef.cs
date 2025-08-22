using UnityEngine;

/// <summary>
/// Reference to a specific event payload, defined by category and key.
/// Used in components like ButtonLoggerConnector to pick events via dropdowns.
/// </summary>
[System.Serializable]
public struct EventRef
{
    [Tooltip("Category of the event (e.g., UI, Combat, Transmute)")]
    public string category;

    [Tooltip("Key of the event within the selected category")]
    public string key;
}
