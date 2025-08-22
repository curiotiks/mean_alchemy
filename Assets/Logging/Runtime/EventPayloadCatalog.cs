using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Catalog of event payloads organized into categories.
/// Each category contains a list of payload entries.
/// </summary>
[CreateAssetMenu(menuName = "Logging/Event Payload Catalog", fileName = "EventPayloadCatalog")]
public class EventPayloadCatalog : ScriptableObject
{
    [System.Serializable]
    public class PayloadEntry
    {
        [Tooltip("Unique key for this event inside its category")]
        public string key;

        [Tooltip("Action value to send in the log payload")]
        public string action;

        [Tooltip("Target value to send in the log payload")]
        public string target;

        [Tooltip("Optional description for clarity in the editor")]
        [TextArea]
        public string description;
    }

    [System.Serializable]
    public class Category
    {
        [Tooltip("Category name (e.g., UI, Combat, Transmute)")]
        public string name;

        [Tooltip("List of events under this category")]
        public List<PayloadEntry> entries = new List<PayloadEntry>();
    }

    [Tooltip("All categories of event payloads")]
    public List<Category> categories = new List<Category>();
}
