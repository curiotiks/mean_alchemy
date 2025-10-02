using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class LeakDetectionBootstrap : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    void Awake()
    {
        // Enables leak detection with full stack traces
        NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
        Debug.Log("✅ Native Leak Detection enabled with stack traces.");
    }
#endif
}