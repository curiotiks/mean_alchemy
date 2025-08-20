using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameLogger : MonoBehaviour
{
    private static GameLogger instance; // Singleton instance; ensures only one logger exists
    public SupabaseAuth authScript;  // Drag the SupabaseAuth GameObject into this field
    public string supabaseUrl = "https://pllursracuxqllyzgcvr.supabase.co";
    public string anonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InBsbHVyc3JhY3V4cWxseXpnY3ZyIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM4MjE4NTUsImV4cCI6MjA1OTM5Nzg1NX0.-xr1Tu0HePRdRjfgXgBikrLCBClY3iIxOoznJaqJiJs";  // Same as the one in SupabaseAuth
    public string logTable = "game_events";

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
        LoadPayloadTemplates();
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
}