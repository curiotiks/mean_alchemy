using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;

public class SupabaseAuth : MonoBehaviour
{
    private static SupabaseAuth instance;
    public TMP_InputField studyCodeInput;
    public TMP_Dropdown classCodeInput;
    public string supabaseUrl = "https://pllursracuxqllyzgcvr.supabase.co";
    public string anonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InBsbHVyc3JhY3V4cWxseXpnY3ZyIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM4MjE4NTUsImV4cCI6MjA1OTM5Nzg1NX0.-xr1Tu0HePRdRjfgXgBikrLCBClY3iIxOoznJaqJiJs";
    public string startSessionFunctionURL = "https://pllursracuxqllyzgcvr.supabase.co/functions/v1/start_session";

    [Header("Scene Flow")]
    [Tooltip("Scene to load after a successful login + session start. Defaults to TheLab via SceneNames.")]
    public string nextSceneName = SceneNames.TheLab;
    [Tooltip("If true, automatically changes scene after session creation succeeds.")]
    public bool autoChangeScene = true;

    [System.Serializable]
    public class StudyCodePayload
    {
        public string study_code;
        public string class_code;
    }

    [HideInInspector] public string AccessToken = "";
    [HideInInspector] public string SessionId = "";
    public string CurrentStudyCode = "";

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void OnAuthenticateClicked()
    {
        string studyCode = studyCodeInput.text.Trim().ToLower();
        CurrentStudyCode = studyCode;
        string email = $"{studyCode}@study.local";
        string password = $"{studyCode}123!";
        string classCode = classCodeInput.options[classCodeInput.value].text;
        StartCoroutine(AuthenticateFlow(email, password, studyCode, classCode));
    }

    private IEnumerator AuthenticateFlow(string email, string password, string studyCode, string classCode)
    {
        // -- Attempt Login
        // ---- Construct the login request
        string loginUrl = $"{supabaseUrl}/auth/v1/token?grant_type=password";
        string loginPayload = $"{{\"email\":\"{email}\",\"password\":\"{password}\"}}";
        byte[] loginBody = Encoding.UTF8.GetBytes(loginPayload);

        using (var loginRequest = new UnityWebRequest(loginUrl, "POST"))
        {
            loginRequest.uploadHandler = new UploadHandlerRaw(loginBody);
            loginRequest.downloadHandler = new DownloadHandlerBuffer();
            loginRequest.disposeUploadHandlerOnDispose = true;
            loginRequest.disposeDownloadHandlerOnDispose = true;
            loginRequest.SetRequestHeader("Content-Type", "application/json");
            loginRequest.SetRequestHeader("apikey", anonKey);

            // ---- Send the request
            yield return loginRequest.SendWebRequest();

            if (loginRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Login successful.");
                yield return CreateSession(loginRequest.downloadHandler.text, studyCode);
            }
            else
            {
                yield return CheckStudyCode(studyCode, (exists) =>
                {
                    if (exists)
                    {
                        Debug.Log("🟢 Study code verified. Attempting signup...");
                        StartCoroutine(SignUp(email, password, studyCode, classCode));
                    }
                    else
                    {
                        Debug.LogError("❌ Invalid study code. Cannot sign up.");
                    }
                });
            }
        }
    }

    private IEnumerator CheckStudyCode(string studyCode, System.Action<bool> callback)
    {
        string checkUrl = $"{supabaseUrl}/rest/v1/study_codes?study_code=eq.{studyCode}";
        using (var checkRequest = UnityWebRequest.Get(checkUrl))
        {
            checkRequest.disposeDownloadHandlerOnDispose = true;
            checkRequest.SetRequestHeader("apikey", anonKey);

            yield return checkRequest.SendWebRequest();

            if (checkRequest.result == UnityWebRequest.Result.Success)
            {
                bool exists = checkRequest.downloadHandler.text.Length > 2;
                Debug.Log(exists ? "✅ Study code exists." : "❌ Study code does not exist.");
                callback(exists);
            }
            else
            {
                Debug.LogError("❌ Error checking study code: " + checkRequest.error);
                callback(false);
            }
        }
    }


    private IEnumerator SignUp(string email, string password, string studyCode, string classCode)
    {
        string signupUrl = $"{supabaseUrl}/auth/v1/signup";
        string signupPayload = $"{{\"email\":\"{email}\",\"password\":\"{password}\"}}";
        byte[] signupBody = Encoding.UTF8.GetBytes(signupPayload);

        using (var signupRequest = new UnityWebRequest(signupUrl, "POST"))
        {
            signupRequest.uploadHandler = new UploadHandlerRaw(signupBody);
            signupRequest.downloadHandler = new DownloadHandlerBuffer();
            signupRequest.disposeUploadHandlerOnDispose = true;
            signupRequest.disposeDownloadHandlerOnDispose = true;
            signupRequest.SetRequestHeader("Content-Type", "application/json");
            signupRequest.SetRequestHeader("apikey", anonKey);

            yield return signupRequest.SendWebRequest();

            if (signupRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ SignUp successful.");

                SupabaseAuthResponse parsed = JsonUtility.FromJson<SupabaseAuthResponse>(signupRequest.downloadHandler.text);
                AccessToken = parsed.access_token;
                Debug.Log("uid: " + parsed.user.id);

                string patchUrl = $"{supabaseUrl}/rest/v1/study_codes?study_code=eq.{studyCode}";
                string patchPayload = $"{{\"uid\":\"{parsed.user.id}\",\"class_code\":\"{classCode}\"}}";
                byte[] patchBody = Encoding.UTF8.GetBytes(patchPayload);

                using (var patchRequest = new UnityWebRequest(patchUrl, "PATCH"))
                {
                    patchRequest.uploadHandler = new UploadHandlerRaw(patchBody);
                    patchRequest.downloadHandler = new DownloadHandlerBuffer();
                    patchRequest.disposeUploadHandlerOnDispose = true;
                    patchRequest.disposeDownloadHandlerOnDispose = true;

                    patchRequest.SetRequestHeader("Content-Type", "application/json");
                    patchRequest.SetRequestHeader("apikey", anonKey);
                    patchRequest.SetRequestHeader("Authorization", "Bearer " + AccessToken);
                    patchRequest.SetRequestHeader("Prefer", "return=representation");

                    yield return patchRequest.SendWebRequest();

                    if (patchRequest.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("✅ study_codes table patched with UID");
                    }
                    else
                    {
                        Debug.LogError("❌ Failed to patch study_codes: " + patchRequest.downloadHandler.text);
                    }
                }

                yield return CreateSession(signupRequest.downloadHandler.text, studyCode);
            }
            else
            {
                Debug.LogError("❌ SignUp failed: " + signupRequest.downloadHandler.text);
            }
        }
    }

    private IEnumerator CreateSession(string json, string studyCode)
    {
        SupabaseAuthResponse parsed = JsonUtility.FromJson<SupabaseAuthResponse>(json);
        AccessToken = parsed.access_token;
        Debug.Log("🔑 AccessToken: " + AccessToken);

        // ✅ Include study_code in request body
        string sessionPayload = $"{{\"study_code\":\"{studyCode}\"}}";
        Debug.Log("📤 Sending session start payload: " + sessionPayload);

        using (var sessionRequest = new UnityWebRequest(startSessionFunctionURL, "POST"))
        {
            sessionRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(sessionPayload));
            sessionRequest.downloadHandler = new DownloadHandlerBuffer();
            sessionRequest.disposeUploadHandlerOnDispose = true;
            sessionRequest.disposeDownloadHandlerOnDispose = true;
            sessionRequest.SetRequestHeader("Content-Type", "application/json");
            sessionRequest.SetRequestHeader("Authorization", "Bearer " + AccessToken);

            yield return sessionRequest.SendWebRequest();

            if (sessionRequest.result == UnityWebRequest.Result.Success)
            {
                var sessionResponse = JsonUtility.FromJson<SessionResponse>(sessionRequest.downloadHandler.text);
                SessionId = sessionResponse.session_id;
                Debug.Log("🟢 Session started. ID: " + SessionId);
                LoadNextSceneIfEnabled();
            }
            else
            {
                Debug.LogError("❌ Session start failed: " + sessionRequest.downloadHandler.text);
            }
        }
    }

    public void QuitGame()
    {
        Debug.Log("🚪 QuitGame called. Logging and exiting...");

        GameLogger logger = FindObjectOfType<GameLogger>();
        if (logger != null)
        {
            string quitPayload = "exit_game";
            logger.LogEventByKey(quitPayload);
        }
        else
        {
            Debug.LogWarning("⚠️ GameLogger not found. No event logged for Quit.");
        }

        // 🔁 Patch game_sessions with end timestamp
        string sessionUrl = $"{supabaseUrl}/rest/v1/game_sessions?session_id=eq.{SessionId}";
        string endPayload = $"{{\"ended_at\":\"{System.DateTime.UtcNow.ToString("o")}\"}}";

        StartCoroutine(SendSessionEndPatch(sessionUrl, endPayload));
    }

    private IEnumerator SendSessionEndPatch(string url, string jsonPayload)
    {
        byte[] body = Encoding.UTF8.GetBytes(jsonPayload);
        using (var patchRequest = new UnityWebRequest(url, "PATCH"))
        {
            patchRequest.uploadHandler = new UploadHandlerRaw(body);
            patchRequest.downloadHandler = new DownloadHandlerBuffer();
            patchRequest.disposeUploadHandlerOnDispose = true;
            patchRequest.disposeDownloadHandlerOnDispose = true;
            patchRequest.SetRequestHeader("Content-Type", "application/json");
            patchRequest.SetRequestHeader("apikey", anonKey);
            patchRequest.SetRequestHeader("Authorization", "Bearer " + AccessToken);
            patchRequest.SetRequestHeader("Prefer", "return=representation");

            yield return patchRequest.SendWebRequest();

            if (patchRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Session ended successfully.");
                Debug.Log("SessionId being patched: " + SessionId);
                Debug.Log("📦 Supabase response: " + patchRequest.downloadHandler.text);
                // Application.Quit(); // Temporarily disabled for testing
                // #if UNITY_EDITOR
                // UnityEditor.EditorApplication.isPlaying = false;
                // #endif
            }
            else
            {
                Debug.LogError("❌ Failed to patch session end.");
                Debug.LogError("📦 Supabase response: " + patchRequest.downloadHandler.text);
            }
        }
    }

    private void LoadNextSceneIfEnabled()
    {
        if (!autoChangeScene)
        {
            Debug.Log("ℹ️ Auto scene change disabled; staying on login scene.");
            return;
        }
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("⚠️ nextSceneName is empty; cannot change scenes.");
            return;
        }
        Debug.Log($"➡️ Loading scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    [System.Serializable] class SupabaseUser { public string id; public string email; }
    [System.Serializable] class SupabaseAuthResponse
    {
        public string access_token;
        public string token_type;
        public int expires_in;
        public string refresh_token;
        public SupabaseUser user;
    }
    [System.Serializable] class SessionResponse { public string session_id; }
}