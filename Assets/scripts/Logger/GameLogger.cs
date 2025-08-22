using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameLogger : MonoBehaviour
{
    private static GameLogger instance; // Singleton instance; ensures only one logger exists
    public static GameLogger Instance => instance;
    public SupabaseAuth authScript;  // Drag the SupabaseAuth GameObject into this field
    public string supabaseUrl = "https://pllursracuxqllyzgcvr.supabase.co";
    public string anonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InBsbHVyc3JhY3V4cWxseXpnY3ZyIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM4MjE4NTUsImV4cCI6MjA1OTM5Nzg1NX0.-xr1Tu0HePRdRjfgXgBikrLCBClY3iIxOoznJaqJiJs";  // Same as the one in SupabaseAuth
    public string logTable = "game_events";

    [Header("Catalog")]
    [Tooltip("ScriptableObject catalog that defines logging categories and entries")] 
    public EventPayloadCatalog catalog;

    // In-memory index: category -> key -> entry
    private Dictionary<string, Dictionary<string, EventPayloadCatalog.PayloadEntry>> _byCategory;

    private Dictionary<string, Dictionary<string, string>> payloadTemplates;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
        // Ensure catalog is set at runtime too
        if (catalog == null)
        {
            var fallback = Resources.Load<EventPayloadCatalog>("EventPayloadCatalog");
            if (fallback != null)
            {
                catalog = fallback;
            }
        }
        LoadPayloadTemplates();
        BuildCatalogIndex();
    }

    private void OnValidate()
    {
        // In the editor, auto-assign a catalog if missing
        if (catalog == null)
        {
            var fallback = Resources.Load<EventPayloadCatalog>("EventPayloadCatalog");
            if (fallback != null)
            {
                catalog = fallback;
            }
        }
    }

    public static EventPayloadCatalog SharedCatalog
    {
        get
        {
            if (Instance != null && Instance.catalog != null) return Instance.catalog;
            return Resources.Load<EventPayloadCatalog>("EventPayloadCatalog");
        }
    }

    void LoadPayloadTemplates()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("EventPayloads");
        if (jsonFile == null)
        {
            Debug.LogError("❌ EventPayloads.json not found in Resources.");
            payloadTemplates = new Dictionary<string, Dictionary<string, string>>();
            return;
        }

        payloadTemplates = JsonUtility.FromJson<Wrapper>(jsonFile.text).ToDictionary();
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<Entry> entries;

        [System.Serializable]
        public class Entry
        {
            public string key;
            public string action;
            public string target;
        }

        public Dictionary<string, Dictionary<string, string>> ToDictionary()
        {
            var dict = new Dictionary<string, Dictionary<string, string>>();
            foreach (var e in entries)
            {
                dict[e.key] = new Dictionary<string, string>
                {
                    { "action", e.action },
                    { "target", e.target }
                };
            }
            return dict;
        }
    }

    private void BuildCatalogIndex()
    {
        _byCategory = new Dictionary<string, Dictionary<string, EventPayloadCatalog.PayloadEntry>>();

        // Prefer explicitly assigned catalog; fallback to Resources if missing
        var sourceCatalog = catalog != null ? catalog : Resources.Load<EventPayloadCatalog>("EventPayloadCatalog");
        if (sourceCatalog == null || sourceCatalog.categories == null) return;

        foreach (var cat in sourceCatalog.categories)
        {
            if (cat == null || string.IsNullOrEmpty(cat.name)) continue;
            var inner = new Dictionary<string, EventPayloadCatalog.PayloadEntry>();
            if (cat.entries != null)
            {
                foreach (var e in cat.entries)
                {
                    if (e == null || string.IsNullOrEmpty(e.key)) continue;
                    inner[e.key] = e;
                }
            }
            _byCategory[cat.name] = inner;
        }
    }

    private bool TryGetCatalogEntry(string category, string key, out EventPayloadCatalog.PayloadEntry entry)
    {
        entry = null;
        if (_byCategory == null)
        {
            BuildCatalogIndex();
            if (_byCategory == null) return false;
        }
        if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(key)) return false;
        if (!_byCategory.TryGetValue(category, out var inner) || inner == null) return false;
        return inner.TryGetValue(key, out entry);
    }

    [System.Serializable]
    private class CatalogEnvelope
    {
        public string action;
        public string target;
    }

    void Start()
    {
        if (authScript == null)
        {
            authScript = FindObjectOfType<SupabaseAuth>();
            if (authScript == null)
            {
                Debug.LogError("❌ GameLogger: SupabaseAuth not found in scene.");
            }
            else
            {
                Debug.Log("🔗 GameLogger: SupabaseAuth linked at Start.");
            }
        }
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    public void LogEventByKey(string key)
    {
        if (!payloadTemplates.ContainsKey(key))
        {
            Debug.LogError($"❌ No payload found for key: {key}");
            return;
        }

        Dictionary<string, string> basePayload = payloadTemplates[key];
        string jsonPayload = JsonUtility.ToJson(new SerializablePayload(basePayload));
        StartCoroutine(SendLog(jsonPayload));
    }

    [System.Serializable]
    private class SerializablePayload
    {
        public string action;
        public string target;

        public SerializablePayload(Dictionary<string, string> dict)
        {
            dict.TryGetValue("action", out action);
            dict.TryGetValue("target", out target);
        }
    }

    [System.Serializable]
    private class TransmutationEventData
    {
        public string numbers_string;
        public List<int> numbers;
        public float mean;
        public float sd;
        public float skew;
    }

    [System.Serializable]
    private class TransmutationEnvelope
    {
        public string action = "transmute";
        public string target = "alchemy_table";
        public TransmutationEventData data;
    }

    private static List<int> ParseNumbersFromUnderscoreString(string s)
    {
        var list = new List<int>();
        if (string.IsNullOrEmpty(s)) return list;
        var parts = s.Split('_');
        for (int i = 0; i < parts.Length; i++)
        {
            if (int.TryParse(parts[i], out int val)) list.Add(val);
        }
        return list;
    }

    private IEnumerator SendLog(string jsonPayload)
    {
        Debug.Log($"🔍 authScript: {(authScript != null ? "Found" : "Missing")}");
        if (authScript == null)
        {
            authScript = FindObjectOfType<SupabaseAuth>();
            if (authScript == null)
            {
                Debug.LogError("❌ GameLogger: SupabaseAuth could not be found at runtime.");
                yield break;
            }
        }

        if (string.IsNullOrEmpty(authScript.SessionId))
        {
            Debug.LogError("⚠️ Cannot log: Session ID not available.");
            yield break;
        }

        string timestamp = System.DateTime.UtcNow.ToString("o"); // ISO 8601 format

        string dynamicPayload = jsonPayload
            .Replace("<<timestamp>>", timestamp)
            .Replace("<<study_code>>", authScript.CurrentStudyCode.Trim().ToLower())
            .Replace("<<session_id>>", authScript.SessionId);
        
        string fullPayload = $@"
            {{
            ""session_id"": ""{authScript.SessionId}"",
            ""timestamp"":  ""{timestamp}"",
            ""event_data"": {dynamicPayload}
            }}";

        Debug.Log("📦 Full payload: " + dynamicPayload);

        byte[] body = Encoding.UTF8.GetBytes(fullPayload);

        string url = $"{supabaseUrl}/rest/v1/{logTable}";
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", anonKey);
        request.SetRequestHeader("Authorization", "Bearer " + authScript.AccessToken);
        request.SetRequestHeader("Prefer", "return=representation");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("📝 Event logged: " + jsonPayload);
        }
        else
        {
            Debug.LogError("❌ Failed to log event: " + request.downloadHandler.text);
        }
    }

    public void LogTransmutation(FamiliarItem item)
    {
        if (item == null)
        {
            Debug.LogError("❌ LogTransmutation called with null item.");
            return;
        }

        var env = new TransmutationEnvelope
        {
            data = new TransmutationEventData
            {
                numbers_string = item.description, // underscore-separated raw input
                numbers = ParseNumbersFromUnderscoreString(item.description),
                mean = item.mean,
                sd = item.sd,
                skew = item.skew
            }
        };

        string jsonPayload = JsonUtility.ToJson(env);
        StartCoroutine(SendLog(jsonPayload));
    }

    /// <summary>
    /// Log an event defined in the EventPayloadCatalog by category and key.
    /// </summary>
    public void LogEvent(string category, string key)
    {
        if (!TryGetCatalogEntry(category, key, out var entry))
        {
            Debug.LogWarning($"GameLogger: No catalog entry for {category}/{key}. Check your EventPayloadCatalog.");
            return;
        }

        var env = new CatalogEnvelope { action = entry.action, target = entry.target };
        string jsonPayload = JsonUtility.ToJson(env);
        StartCoroutine(SendLog(jsonPayload));
    }

    /// <summary>
    /// Convenience overload to log using an EventRef (as used by UI connectors).
    /// </summary>
    public void LogEvent(EventRef ev)
    {
        LogEvent(ev.category, ev.key);
    }
}