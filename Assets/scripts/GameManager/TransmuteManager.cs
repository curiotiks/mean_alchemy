using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    [Header("Confirmed Stats (read-only at runtime)")]
    [Tooltip("Last confirmed mean from the most recent successful transmutation.")]
    [SerializeField] private float confirmedMean = 0f;

    [Tooltip("Last confirmed standard deviation from the most recent successful transmutation.")]
    [SerializeField] private float confirmedSD = 0f;

    /// <summary>Public read-only accessors for HUDs/other systems.</summary>
    public float ConfirmedMean => confirmedMean;
    public float ConfirmedSD   => confirmedSD;

    /// <summary>Event fired whenever the confirmed stats are updated.</summary>
    public static event Action<float, float> OnConfirmedStatsChanged;

    /// <summary>Helper to fetch the latest confirmed stats in one call.</summary>
    public void GetConfirmedStats(out float mean, out float sd)
    {
        mean = confirmedMean;
        sd = confirmedSD;
    }

    [Header("Transmute Requirements (UI)")]
    [Tooltip("Minimum stones required in the current mix before Transmute is allowed.")]
    [SerializeField] private int minStonesRequired = 20;
    [Tooltip("Button that triggers Transmute (Confirm). Will be disabled until requirement met).")]
    [SerializeField] private Button transmuteButton;
    [Tooltip("Label shown when requirement is not met, e.g., 'Need at least 20 stones.'")]
    [SerializeField] private TMP_Text requirementText;
    [Tooltip("How often (seconds) to check the table and update the UI.")]
    [SerializeField] private float uiCheckInterval = 0.2f;

    private float _nextUiCheckTime = 0f;
    private bool  _lastOkState = false;

    private int CurrentStoneCount()
    {
        var table = Table_Control_Panel.instance;
        if (table != null && table.numbers_list != null)
            return table.numbers_list.Count;
        return 0;
    }

    private bool HasEnoughStones()
    {
        return CurrentStoneCount() >= minStonesRequired;
    }

    private void UpdateTransmuteAvailability()
    {
        bool ok = HasEnoughStones();
        if (ok != _lastOkState)
        {
            _lastOkState = ok;
            if (transmuteButton != null)
                transmuteButton.interactable = ok;
        }

        if (requirementText != null)
        {
            if (!ok)
            {
                requirementText.text = $"Need at least {minStonesRequired} stones.";
                if (!requirementText.gameObject.activeSelf) requirementText.gameObject.SetActive(true);
            }
            else
            {
                if (requirementText.gameObject.activeSelf) requirementText.gameObject.SetActive(false);
            }
        }
    }

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

        _lastOkState = !HasEnoughStones(); // force first refresh to run path
        UpdateTransmuteAvailability();
        _nextUiCheckTime = Time.time + uiCheckInterval;
    }

    private void Update()
    {
        if (Time.time >= _nextUiCheckTime)
        {
            _nextUiCheckTime = Time.time + uiCheckInterval;
            UpdateTransmuteAvailability();
        }
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
        // Minimum requirement guard for WebGL/classroom use
        if (!HasEnoughStones())
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Transmute blocked: need at least {minStonesRequired} stones (have {CurrentStoneCount()}).");
#endif
            UpdateTransmuteAvailability();
            return;
        }

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
            // Cache confirmed stats for HUD/other systems
            confirmedMean = TempFamiliarItem.mean;
            confirmedSD   = TempFamiliarItem.sd;
            OnConfirmedStatsChanged?.Invoke(confirmedMean, confirmedSD);
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

            // Mark familiar as powered (centralized state) and refresh warp gates
            FamiliarState.SetPowered(true);
            WarpGate.RefreshAllGates();
#if UNITY_EDITOR
            Debug.Log("[TransmuteManager] Familiar powered -> gates refreshed (FamiliarState)");
#endif
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
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
    /// <summary>
    /// Clears the powered-familiar flag and refreshes any WarpGates. Call when the familiar is reset.
    /// </summary>
    public static void ClearPoweredFamiliar()
    {
        FamiliarState.SetPowered(false);
        // Clear cached confirmed stats when familiar is cleared
        if (Instance != null)
        {
            Instance.confirmedMean = 0f;
            Instance.confirmedSD   = 0f;
            OnConfirmedStatsChanged?.Invoke(0f, 0f);
        }
        WarpGate.RefreshAllGates();
#if UNITY_EDITOR
        Debug.Log("[TransmuteManager] Familiar cleared -> gates refreshed (FamiliarState)");
#endif
    }
}
