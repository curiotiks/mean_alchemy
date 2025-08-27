using UnityEngine;
using UnityEngine.UI;

// Ensure only one StoneChip, and always has RectTransform for UI layout
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
/// <summary>
/// A single visual chip in a column. Handles removal by notifying the panel via a public
/// button-click method. No tweening and no internal logging; logging is handled by
/// ButtonLoggerConnector attached to the prefab.
/// </summary>
public class StoneChip : MonoBehaviour
{
    public int value;           // Stone value or column number
    public int columnIndex;     // 0-based column index

    private System.Action<StoneChip> _onRemoveRequested;
    private Button _button;

    public void Init(int value, int columnIndex, System.Action<StoneChip> onRemoveRequested)
    {
        this.value = value;
        this.columnIndex = columnIndex;
        _onRemoveRequested = onRemoveRequested;

        // Find a Button on this object or any child (prefabs may place it on a nested object)
        _button = GetComponent<Button>();
        if (_button == null)
            _button = GetComponentInChildren<Button>(true);

        if (_button != null)
        {
            _button.onClick.RemoveListener(OnButtonClicked);
            _button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning($"StoneChip.Init: No Button found on '{name}'. Add a Button to the chip prefab so ButtonLoggerConnector can log and deletion can trigger.");
        }
        Debug.Log($"StoneChip.Init set up: value={this.value}, col={this.columnIndex}, button={_button != null}");
    }

    /// <summary>
    /// Called by the UI Button (and therefore by ButtonLoggerConnector) when the chip is clicked.
    /// </summary>
    public void OnButtonClicked()
    {
        Debug.Log($"StoneChip clicked: value={value}, col={columnIndex}");
        _onRemoveRequested?.Invoke(this);
    }

    public void DestroyImmediateSafe()
    {
        if (this != null && gameObject != null)
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnButtonClicked);
            _button.onClick.AddListener(OnButtonClicked);
        }
    }
}