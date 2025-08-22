
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// Orchestrates the creation ("transmutation") of a <see cref="FamiliarItem"/> from the current
/// state of the Table_Control_Panel, and persists the result locally. 
/// 
/// External systems (e.g., Supabase logging) can subscribe to <see cref="OnTransmuted"/>
/// to be notified whenever a familiar is saved, so they can mirror the write to a server.
/// </summary>
public sealed class TransmuteManager : MonoBehaviour
{
    /// <summary>Singleton instance. Will destroy duplicates on Awake.</summary>
    public static TransmuteManager Instance { get; private set; }

    [Header("Persistence")]
    [Tooltip("Optional override for the local JSON filename (without extension). Leave blank to use default.")]
    [SerializeField] private string fileName = string.Empty;

    [SerializeField] private string filePath;

    /// <summary>
    /// The most recently prepared (but not yet persisted) item. 
    /// Note: static for convenience; not serialized by Unity.
    /// </summary>
    public static FamiliarItem TempFamiliarItem { get; private set; }

    /// <summary>
    /// Event fired after a familiar is successfully saved locally. External loggers
    /// (e.g., a Supabase sender) can subscribe to mirror the write to a server.
    /// </summary>
    public static event Action<FamiliarItem> OnTransmuted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        var baseName = string.IsNullOrWhiteSpace(fileName) ? "FamiliarItemsData_LOCAL" : fileName.Trim();
        filePath = Path.Combine(Application.persistentDataPath, $"{baseName}.json");
    }

    /// <summary>
    /// Builds a new <see cref="FamiliarItem"/> snapshot from the current table, but does not persist it.
    /// Call <see cref="TransmuteAddNew"/> to save it locally (and trigger any external subscribers).
    /// </summary>
    /// <param name="name">Display name for the familiar.</param>
    /// <param name="iconID">Icon resource key for UI.</param>
    public void TransmuteMakeNew(string name = "No name", string iconID = "defaultFamiliarIcon")
    {
        var table = Table_Control_Panel.instance;
        var nums = table.numbers_list; // raw stones as entered by the player

        TempFamiliarItem = new FamiliarItem(
            0,                                             // id assigned server-side (if applicable)
            name,
            string.Join('_', nums.ToArray()),              // store raw distribution as underscore-separated string
            DateTime.Now.ToString("o"),                    // ISO 8601 timestamp
            iconID,
            table.mean,
            table.sd,
            table.skew
        );
    }

    /// <summary>Clears the staged familiar without persisting.</summary>
    public void TransmuteEraseNew() => TempFamiliarItem = null;

    /// <summary>
    /// Persists the currently staged familiar locally. If successful, raises <see cref="OnTransmuted"/>
    /// so that any external sender can mirror the write (e.g., to Supabase).
    /// </summary>
    public void TransmuteAddNew()
    {
        if (TempFamiliarItem == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("TransmuteAddNew called but TempFamiliarItem is null.");
#endif
            return;
        }

        try
        {
            AppendToJsonArray(filePath, JsonUtility.ToJson(TempFamiliarItem));
#if UNITY_EDITOR
            Debug.Log($"Saved FamiliarItem to {filePath}");
#endif
            OnTransmuted?.Invoke(TempFamiliarItem);
            // Also send to server via GameLogger if present
            var logger = FindObjectOfType<GameLogger>();
            if (logger != null)
            {
                logger.LogTransmutation(TempFamiliarItem);
            }
            else
            {
                Debug.LogWarning("GameLogger not found; transmutation was not sent to server.");
            }
        }
        catch (Exception ex)
        {
#if UNITY_EDITOR
            Debug.LogError($"Failed to save FamiliarItem: {ex}");
#endif
        }
    }

    /// <summary>
    /// Appends a JSON object to a JSON array file on disk. Creates the file if it doesn't exist.
    /// This sidesteps JsonUtility's lack of native array append support.
    /// </summary>
    private static void AppendToJsonArray(string path, string jsonObject)
    {
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "[" + jsonObject + "]");
            return;
        }

        string existing = File.ReadAllText(path).Trim();

        if (existing.Length == 0)
        {
            File.WriteAllText(path, "[" + jsonObject + "]");
            return;
        }

        if (existing.EndsWith("]"))
        {
            if (existing.Length > 2)
            {
                existing = existing.Substring(0, existing.Length - 1) + "," + jsonObject + "]";
            }
            else
            {
                existing = "[" + jsonObject + "]";
            }
        }
        else
        {
            // File is malformed; rebuild a minimal valid array.
            existing = "[" + jsonObject + "]";
        }

        File.WriteAllText(path, existing);
    }
}
