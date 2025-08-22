#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws an EventRef as two dependent dropdowns: Category -> Event (key).
/// The drawer looks for an EventPayloadCatalog reference on the same serialized object
/// under a field named "catalog". If not found, it will fallback to Resources.Load<EventPayloadCatalog>("EventPayloadCatalog").
/// If no catalog is available, it will show plain text fields as a graceful fallback.
/// </summary>
[CustomPropertyDrawer(typeof(EventRef))]
public class EventRefDrawer : PropertyDrawer
{
    private const float LinePadding = 2f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Two lines (Category + Event) with small padding
        var line = EditorGUIUtility.singleLineHeight;
        return line * 2f + LinePadding * 3f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Locate the catalog reference on the same object (field name: "catalog")
        var so = property.serializedObject;
        var catalogProp = so.FindProperty("catalog");
        EventPayloadCatalog catalog = catalogProp != null ? catalogProp.objectReferenceValue as EventPayloadCatalog : null;

        // Fallback: try Resources
        if (catalog == null)
        {
            catalog = Resources.Load<EventPayloadCatalog>("EventPayloadCatalog");
        }

        // References to the underlying string fields
        var categoryProp = property.FindPropertyRelative("category");
        var keyProp = property.FindPropertyRelative("key");

        // Layout rects
        var line = EditorGUIUtility.singleLineHeight;
        var y = position.y;
        var fullWidth = position;
        fullWidth.height = line;

        // CATEGORY ROW
        var categoryLabel = new Rect(position.x, y, 80f, line);
        var categoryField = new Rect(position.x + 85f, y, position.width - 85f, line);
        y += line + LinePadding;

        // EVENT ROW
        var eventLabel = new Rect(position.x, y, 80f, line);
        var eventField = new Rect(position.x + 85f, y, position.width - 85f, line);

        if (catalog == null || catalog.categories == null || catalog.categories.Count == 0)
        {
            // Graceful fallback: plain text fields
            EditorGUI.LabelField(categoryLabel, "Category");
            categoryProp.stringValue = EditorGUI.TextField(categoryField, categoryProp.stringValue);

            EditorGUI.LabelField(eventLabel, "Event");
            keyProp.stringValue = EditorGUI.TextField(eventField, keyProp.stringValue);

            EditorGUI.EndProperty();
            return;
        }

        // Build category list
        var categories = catalog.categories.Where(c => !string.IsNullOrEmpty(c.name)).ToList();
        var categoryNames = categories.Select(c => c.name).ToArray();

        // Current category index
        int currentCategoryIndex = Mathf.Max(0, System.Array.IndexOf(categoryNames, categoryProp.stringValue));
        if (currentCategoryIndex < 0) currentCategoryIndex = 0;

        // Draw category popup
        EditorGUI.LabelField(categoryLabel, "Category");
        int newCategoryIndex = EditorGUI.Popup(categoryField, currentCategoryIndex, categoryNames);
        if (newCategoryIndex != currentCategoryIndex)
        {
            currentCategoryIndex = newCategoryIndex;
            categoryProp.stringValue = categoryNames[currentCategoryIndex];
            // Reset key when category changes
            keyProp.stringValue = string.Empty;
        }
        else
        {
            // Ensure the string matches a valid value (helpful on first paint)
            categoryProp.stringValue = categoryNames[currentCategoryIndex];
        }

        // Now draw the event (key) popup for the selected category
        var selectedCategory = categories[currentCategoryIndex];
        var entries = (selectedCategory.entries ?? new System.Collections.Generic.List<EventPayloadCatalog.PayloadEntry>())
            .Where(e => !string.IsNullOrEmpty(e.key)).ToList();
        var keyNames = entries.Select(e => e.key).ToArray();

        EditorGUI.LabelField(eventLabel, "Event");

        if (keyNames.Length == 0)
        {
            // No entries available — let user type
            keyProp.stringValue = EditorGUI.TextField(eventField, keyProp.stringValue);
        }
        else
        {
            int currentKeyIndex = Mathf.Max(0, System.Array.IndexOf(keyNames, keyProp.stringValue));
            if (currentKeyIndex < 0) currentKeyIndex = 0;

            int newKeyIndex = EditorGUI.Popup(eventField, currentKeyIndex, keyNames);
            if (newKeyIndex != currentKeyIndex)
            {
                keyProp.stringValue = keyNames[newKeyIndex];
            }
            else if (string.IsNullOrEmpty(keyProp.stringValue))
            {
                // Initialize on first draw if empty
                keyProp.stringValue = keyNames[currentKeyIndex];
            }
        }

        EditorGUI.EndProperty();
    }
}
#endif
