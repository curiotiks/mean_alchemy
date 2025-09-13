// Assets/Editor/MissingScriptFinder.cs
#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

public static class MissingScriptFinder
{
    [MenuItem("Tools/Missing Scripts/Find In Active Scene")]
    public static void FindInActiveScene()
    {
        int objCount = 0, missCount = 0;
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!go.scene.IsValid() || !go.scene.isLoaded) continue; // only loaded scene
            objCount++;

            var comps = go.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null)
                {
                    missCount++;
                    Debug.LogWarning($"[Missing Script] {GetPath(go)} (component index {i})", go);
                    // Optional: ping in Hierarchy
                    EditorGUIUtility.PingObject(go);
                }
            }
        }

        Debug.Log($"Missing Script scan complete. Objects checked: {objCount}, Missing components: {missCount}");
    }

    [MenuItem("Tools/Missing Scripts/Scan All Prefabs In Project")]
    public static void ScanAllPrefabs()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab");
        int broken = 0;
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

#if UNITY_2021_3_OR_NEWER
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab);
            if (count > 0)
            {
                broken++;
                Debug.LogWarning($"[Missing Script in Prefab] {path}  (components missing: {count})", prefab);
            }
#else
            // Fallback for older versions: manual walk
            bool anyMissing = false;
            foreach (var t in prefab.GetComponentsInChildren<Transform>(true))
            {
                var comps = t.GetComponents<Component>();
                foreach (var c in comps) if (c == null) { anyMissing = true; break; }
                if (anyMissing) break;
            }
            if (anyMissing)
            {
                broken++;
                Debug.LogWarning($"[Missing Script in Prefab] {path}", prefab);
            }
#endif
        }
        Debug.Log($"Prefab scan complete. Prefabs with missing scripts: {broken}/{guids.Length}");
    }

    [MenuItem("Tools/Missing Scripts/Remove From Selection")]
    public static void RemoveFromSelection()
    {
        foreach (var obj in Selection.gameObjects)
        {
#if UNITY_2021_3_OR_NEWER
            Undo.RegisterFullObjectHierarchyUndo(obj, "Remove Missing Scripts");
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
#else
            // Manual remove for older versions
            foreach (var t in obj.GetComponentsInChildren<Transform>(true))
            {
                var comps = t.GetComponents<Component>();
                Undo.RegisterCompleteObjectUndo(t.gameObject, "Remove Missing Scripts");
                for (int i = comps.Length - 1; i >= 0; i--)
                {
                    if (comps[i] == null)
                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                }
            }
#endif
        }
        Debug.Log("Removed missing scripts from selection (and children).");
    }

    static string GetPath(GameObject go)
    {
        var p = go.name;
        var t = go.transform.parent;
        while (t != null) { p = t.name + "/" + p; t = t.parent; }
        return p;
    }
}
#endif