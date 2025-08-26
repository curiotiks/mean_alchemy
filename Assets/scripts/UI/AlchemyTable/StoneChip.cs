using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A single visual chip in a column. Handles click-to-remove by notifying the panel via callback.
/// No tweening; purely static so you can add frame-based sprite animation later if desired.
/// </summary>
public class StoneChip : MonoBehaviour, IPointerClickHandler
{
    public int value;           // Stone value or column number
    public int columnIndex;     // 0-based column index
    public EventRef removeEvent;

    private System.Action<StoneChip> _onRemoveRequested;

    public void Init(int value, int columnIndex, EventRef removeEvent, System.Action<StoneChip> onRemoveRequested)
    {
        this.value = value;
        this.columnIndex = columnIndex;
        this.removeEvent = removeEvent;
        _onRemoveRequested = onRemoveRequested;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _onRemoveRequested?.Invoke(this);
    }

    public void DestroyImmediateSafe()
    {
        if (this != null && gameObject != null)
            Destroy(gameObject);
    }
}