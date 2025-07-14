using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(SceneChanger))]
public class SceneChangerEditor : Editor
{
    private string[] sceneOptions;

    private void OnEnable()
    {
        // Reflectively get all public constants in the SceneNames class
        sceneOptions = typeof(SceneNames)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => f.GetValue(null).ToString())
            .ToArray();
    }

    public override void OnInspectorGUI()
    {
        var changer = (SceneChanger)target;

        // Get current index from current destScene value
        int currentIndex = Array.IndexOf(sceneOptions, changer.destScene);
        if (currentIndex == -1) currentIndex = 0;

        // Show dropdown
        EditorGUILayout.LabelField("Scene Changer", EditorStyles.boldLabel);
        int newIndex = EditorGUILayout.Popup("Destination Scene", currentIndex, sceneOptions);
        changer.destScene = sceneOptions[newIndex];
    }
}