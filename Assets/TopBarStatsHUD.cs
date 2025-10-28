using UnityEngine;
using TMPro;
using System;
using System.Reflection;
using System.Linq;

public class TopBarStatsHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text reputationText;
    [SerializeField] private TMP_Text meanText;
    [SerializeField] private TMP_Text sdText;

    [Header("Labels / Format")]
    [SerializeField] private string repPrefix = "Reputation: ";
    [SerializeField] private string meanPrefix = "Mean: ";
    [SerializeField] private string sdPrefix = "SD: ";
    [SerializeField] private string floatFormat = "0.##";
    [Tooltip("Seconds between refresh attempts. Lower = more responsive, higher = fewer CPU/GC costs.")]
    [SerializeField] private float refreshInterval = 0.25f;
    [SerializeField] private bool debugLogs = false;

    [Header("Stats Source (leave empty to auto-detect)")]
    [SerializeField] private MonoBehaviour explicitStatsSource;

    [Header("Advanced: Candidate Names (override only if needed)")]
    [SerializeField] private string[] meanNames = { "ConfirmedMean", "Mean", "currentMean", "mean" };
    [SerializeField] private string[] sdNames   = { "ConfirmedSD", "SD", "currentSD", "sd", "StdDev", "stdDev" };

    // Reputation source (Wallet or legacy Money)
    private Component repSource;
    private MemberInfo cachedRepMember;
    [SerializeField] private string[] repNames = { "Reputation", "Balance", "Amount", "Coins", "Value" };

    // Cached auto-detected stats source + members
    private MonoBehaviour statsSource;
    private MemberInfo cachedMeanMember;
    private MemberInfo cachedSdMember;

    // Strong reference to TransmuteManager if it exists across scenes (DontDestroyOnLoad)
    private MonoBehaviour cachedTransmuteManager; // keep as MonoBehaviour to avoid hard compile dep if namespace differs
    private bool tmSubscribed = false;

    // Last displayed values to avoid unnecessary string allocations
    private int   lastRep = int.MinValue;
    private float lastMean = float.NaN;
    private float lastSd   = float.NaN;

    private void OnEnable()
    {
        // Reset last values so first tick forces a full draw
        lastRep = int.MinValue;
        lastMean = float.NaN;
        lastSd = float.NaN;

        TryBindTransmuteManager();

        InvokeRepeating(nameof(Refresh), 0f, Mathf.Max(0.05f, refreshInterval));
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Refresh));

        // Best-effort unsubscribe if we subscribed
        if (tmSubscribed && cachedTransmuteManager != null)
        {
            try
            {
                var t = cachedTransmuteManager.GetType();
                var evt = t.GetEvent("OnConfirmedStatsChanged", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (evt != null)
                {
                    var handlerMethod = typeof(TopBarStatsHUD).GetMethod("OnConfirmedStatsChangedHandler", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (handlerMethod != null)
                    {
                        var del = Delegate.CreateDelegate(evt.EventHandlerType, this, handlerMethod);
                        evt.RemoveEventHandler(null, del);
                    }
                }
            }
            catch { }
            tmSubscribed = false;
        }
    }

    /// <summary>
    /// Allows other systems to pin a known stats source at runtime (e.g., TransmuteManager).
    /// </summary>
    public void SetStatsSource(MonoBehaviour src)
    {
        explicitStatsSource = src;
        statsSource = null;           // force rebuild of cached members
        cachedMeanMember = null;
        cachedSdMember = null;
        // Force an immediate refresh so UI updates right away
        Refresh();
    }

    [ContextMenu("TopBarHUD/Force Rebind Stats Source")]
    private void ForceRebind()
    {
        statsSource = null;
        cachedMeanMember = null;
        cachedSdMember = null;
        cachedTransmuteManager = null;
        tmSubscribed = false;
        TryBindTransmuteManager();
        Refresh();
    }

    private void TryBindTransmuteManager()
    {
        if (cachedTransmuteManager != null) return;

        // Try to find by type name to avoid namespace coupling
        var allBehaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
        for (int i = 0; i < allBehaviours.Length; i++)
        {
            var b = allBehaviours[i];
            if (b == null) continue;
            var t = b.GetType();
            if (t.Name == "TransmuteManager")
            {
                cachedTransmuteManager = b;
                SetStatsSource(cachedTransmuteManager);
                if (debugLogs) Debug.Log("[TopBarStatsHUD] Bound TransmuteManager: " + b.name + " (" + t.FullName + ")");

                // Attempt to subscribe to static event OnConfirmedStatsChanged(mean, sd) if present
                TrySubscribeToConfirmedStatsChanged(t, b);
                break;
            }
        }
    }

    private void TrySubscribeToConfirmedStatsChanged(Type transmuteType, MonoBehaviour instance)
    {
        if (tmSubscribed) return;
        try
        {
            // Look for: public static event Action<float,float> OnConfirmedStatsChanged;
            var evt = transmuteType.GetEvent("OnConfirmedStatsChanged", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (evt != null)
            {
                // Create a delegate Action<float,float> -> our handler
                var handlerMethod = typeof(TopBarStatsHUD).GetMethod("OnConfirmedStatsChangedHandler", BindingFlags.Instance | BindingFlags.NonPublic);
                if (handlerMethod != null)
                {
                    var actionType = evt.EventHandlerType; // should be Action<float,float>
                    var del = Delegate.CreateDelegate(actionType, this, handlerMethod);
                    evt.AddEventHandler(null, del); // static event, target is null
                    tmSubscribed = true;
                }
            }
        }
        catch { /* best-effort; ignore if signature mismatches */ }
    }

    // This must match Action<float,float>
    private void OnConfirmedStatsChangedHandler(float mean, float sd)
    {
        if (debugLogs) Debug.Log($"[TopBarStatsHUD] OnConfirmedStatsChanged mean={mean} sd={sd}");
        // Update immediately and cache values to suppress extra string churn in next Refresh()
        if (meanText != null)
        {
            lastMean = mean;
            meanText.text = $"{meanPrefix}{mean.ToString(floatFormat)}";
        }
        if (sdText != null)
        {
            lastSd = sd;
            sdText.text = $"{sdPrefix}{sd.ToString(floatFormat)}";
        }
    }

    private void Refresh()
    {
        // Ensure we bind to a DontDestroyOnLoad TransmuteManager if it appears later
        if (cachedTransmuteManager == null)
            TryBindTransmuteManager();

        // Reputation (supports Wallet or legacy Money via reflection)
        int rep = GetReputation();
        if (debugLogs && reputationText != null) Debug.Log($"[TopBarStatsHUD] Rep={rep}");
        if (reputationText != null && rep != lastRep)
        {
            lastRep = rep;
            reputationText.text = $"{repPrefix}{rep}";
        }

        // Mean/SD
        float mean, sd;
        if (TryGetStats(out mean, out sd))
        {
            if (debugLogs) Debug.Log($"[TopBarStatsHUD] Stats OK from {(explicitStatsSource ?? statsSource)?.GetType().Name ?? "none"}: mean={mean} sd={sd}");
            if (!ApproximatelyEqual(mean, lastMean) && meanText != null)
            {
                lastMean = mean;
                meanText.text = $"{meanPrefix}{mean.ToString(floatFormat)}";
            }
            if (!ApproximatelyEqual(sd, lastSd) && sdText != null)
            {
                lastSd = sd;
                sdText.text = $"{sdPrefix}{sd.ToString(floatFormat)}";
            }
        }
        else
        {
            if (debugLogs) Debug.Log("[TopBarStatsHUD] No stats source found.");
            // If we had values before, clear once
            if (meanText != null && !float.IsNaN(lastMean))
            {
                lastMean = float.NaN;
                meanText.text = $"{meanPrefix}—";
            }
            if (sdText != null && !float.IsNaN(lastSd))
            {
                lastSd = float.NaN;
                sdText.text = $"{sdPrefix}—";
            }
        }
    }

    private int GetReputation()
    {
        // Find and cache a Wallet (preferred) or legacy Money component
        if (repSource == null)
        {
            var walletType = FindTypeByName("Wallet");
            if (walletType != null)
                repSource = (Component)FindObjectOfType(walletType);

            if (repSource == null)
            {
                var moneyType = FindTypeByName("Money");
                if (moneyType != null)
                    repSource = (Component)FindObjectOfType(moneyType);
            }
            cachedRepMember = null; // force re-resolve on first use
        }

        if (repSource == null) return 0;

        // Resolve (and cache) a member that looks like the reputation amount
        if (cachedRepMember == null)
            cachedRepMember = ResolveMember(repSource.GetType(), repNames, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (ReadInt(repSource, cachedRepMember, out var val))
            return val;

        return 0;
    }

    private static Type FindTypeByName(string name)
    {
        var t = Type.GetType(name);
        if (t != null) return t;
        try
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(x => x.Name == name);
        }
        catch { return null; }
    }

    private static bool ReadInt(object src, MemberInfo member, out int value)
    {
        value = 0;
        if (member == null || src == null) return false;
        object v = null;
        if (member is PropertyInfo pi)
        {
            try { v = pi.GetValue(src, null); } catch { return false; }
        }
        else if (member is FieldInfo fi)
        {
            try { v = fi.GetValue(src); } catch { return false; }
        }
        if (v == null) return false;
        if (v is int vi) { value = vi; return true; }
        if (v is float vf) { value = Mathf.RoundToInt(vf); return true; }
        if (int.TryParse(v.ToString(), out value)) return true;
        return false;
    }

    private bool TryGetStats(out float mean, out float sd)
    {
        mean = 0f; sd = 0f;

        // Prefer explicit source when provided
        if (explicitStatsSource != null)
        {
            if (TryReadFromSource(explicitStatsSource, ref cachedMeanMember, meanNames, ref mean,
                                   ref cachedSdMember, sdNames, ref sd))
            {
                return true;
            }
        }

        // Use cached autodetected source if still valid
        if (statsSource != null)
        {
            if (TryReadFromSource(statsSource, ref cachedMeanMember, meanNames, ref mean,
                                   ref cachedSdMember, sdNames, ref sd))
            {
                return true;
            }
            else
            {
                // cache became invalid (scene change etc.)
                statsSource = null;
                cachedMeanMember = null;
                cachedSdMember = null;
            }
        }

        // Auto-detect: scan all active & inactive behaviours (once; cache the first match)
        var behaviours = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var b in behaviours)
        {
            if (b == null) continue;
            MemberInfo meanMember = null, sdMember = null;
            float m = 0f, s = 0f;
            if (TryReadFromSource(b, ref meanMember, meanNames, ref m,
                                     ref sdMember,   sdNames,   ref s))
            {
                statsSource = b;
                cachedMeanMember = meanMember;
                cachedSdMember = sdMember;
                mean = m; sd = s;
                return true;
            }
        }

        return false;
    }

    private static bool TryReadFromSource(MonoBehaviour src,
                                          ref MemberInfo meanMember, string[] meanCandidates, ref float mean,
                                          ref MemberInfo sdMember,   string[] sdCandidates,   ref float sd)
    {
        if (src == null) return false;
        var type = src.GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Resolve & cache member for Mean
        if (meanMember == null)
            meanMember = ResolveMember(type, meanCandidates, flags);
        // Resolve & cache member for SD
        if (sdMember == null)
            sdMember = ResolveMember(type, sdCandidates, flags);

        bool haveMean = ReadFloat(src, meanMember, out mean);
        bool haveSd   = ReadFloat(src, sdMember,   out sd);
        return haveMean && haveSd;
    }

    private static MemberInfo ResolveMember(Type type, string[] candidates, BindingFlags flags)
    {
        // Prefer property, then field
        foreach (var name in candidates)
        {
            var p = type.GetProperty(name, flags);
            if (p != null && p.CanRead) return p;
        }
        foreach (var name in candidates)
        {
            var f = type.GetField(name, flags);
            if (f != null) return f;
        }
        return null;
    }

    private static bool ReadFloat(object src, MemberInfo member, out float value)
    {
        value = 0f;
        if (member == null || src == null) return false;

        object v = null;
        if (member is PropertyInfo pi)
        {
            try { v = pi.GetValue(src, null); } catch { return false; }
        }
        else if (member is FieldInfo fi)
        {
            try { v = fi.GetValue(src); } catch { return false; }
        }

        return TryConvertToFloat(v, out value);
    }

    private static bool TryConvertToFloat(object v, out float f)
    {
        f = 0f;
        if (v == null) return false;
        if (v is float vf) { f = vf; return true; }
        if (v is double vd) { f = (float)vd; return true; }
        if (v is int vi) { f = vi; return true; }
        if (v is long vl) { f = vl; return true; }
        if (float.TryParse(v.ToString(), out f)) return true;
        return false;
    }

    private static bool ApproximatelyEqual(float a, float b)
    {
        // Handle NaN comparisons
        if (float.IsNaN(a) || float.IsNaN(b)) return false;
        return Mathf.Abs(a - b) <= 0.0001f;
    }
}