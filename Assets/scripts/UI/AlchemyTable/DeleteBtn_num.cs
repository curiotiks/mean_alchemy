using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DeleteBtn_num : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Button btn;
    [SerializeField] private int buttonNumber; // 1-based column number parsed from name

    [Header("FX")]
    [SerializeField] private RectTransform iconToShake; // if null, will use this transform
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeDistance = 6f;
    [SerializeField] private int shakeVibrato = 10; // number of left-right wiggles

    private ButtonLoggerConnector[] _connectors; // cached connectors on this object/children
    private bool _hasChips;
    private bool _suspendRefreshForClick;

    private void Awake()
    {
        if (btn == null) btn = GetComponent<Button>();
        // cache any ButtonLoggerConnector components found on this object or children
        _connectors = GetComponentsInChildren<ButtonLoggerConnector>(true);
    }

    private void Start()
    {
        // Parse the 1-based number from name (e.g., "Deletebtn_num_05") if not explicitly set
        if (buttonNumber == 0)
        {
            var parts = gameObject.name.Split('_');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int parsed))
                buttonNumber = parsed;
        }

        if (btn)
        {
            // Do NOT RemoveAllListeners — we want ButtonLoggerConnector to keep its onClick
            btn.onClick.RemoveListener(OnValidClick);
            btn.onClick.AddListener(OnValidClick);
        }
        else
        {
            Debug.LogWarning($"DeleteBtn_num on '{name}': No Button component found.");
        }

        RefreshState();
    }

    private void Update()
    {
        if (_suspendRefreshForClick) return;
        RefreshState();
    }

    private void RefreshState()
    {
        int countForNum = 0;
        if (Table_Control_Panel.instance != null)
            countForNum = Table_Control_Panel.instance.numbers_list.FindAll(v => v == buttonNumber).Count;

        _hasChips = countForNum > 0;

        // Disable the Button when empty so onClick won't fire
        if (btn != null)
            btn.interactable = _hasChips;

        // Toggle connector logging to match state
        if (_connectors != null)
        {
            foreach (var c in _connectors)
            {
                if (c == null) continue;
                // Disable the connector component entirely when empty to block any logging path
                c.enabled = _hasChips;
                // Also mirror the flag for clarity (in case the connector checks it internally)
                c.logOnButtonClick = _hasChips;
            }
        }
    }

    // Called only when the button is interactable (i.e., there are chips)
    private void OnValidClick()
    {
        _suspendRefreshForClick = true;
        if (Table_Control_Panel.instance != null)
            Table_Control_Panel.instance.RemoveAllElements(buttonNumber);
        StartCoroutine(ResumeRefreshNextFrame());
    }

    private IEnumerator ResumeRefreshNextFrame()
    {
        // Wait one full frame so ButtonLoggerConnector can process onClick before we disable logging
        yield return null;
        _suspendRefreshForClick = false;
        RefreshState();
    }

    // If someone clicks while empty (e.g., via keyboard/gamepad focus), give feedback and consume.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_hasChips)
        {
            StartCoroutine(ShakeTrash());
            eventData.Use();
        }
    }

    private IEnumerator ShakeTrash()
    {
        var rt = iconToShake != null ? iconToShake : (RectTransform)transform;
        if (rt == null) yield break;

        Vector2 original = rt.anchoredPosition;
        int vib = Mathf.Max(1, shakeVibrato);
        float step = Mathf.Max(0.01f, shakeDuration / vib);
        for (int i = 0; i < vib; i++)
        {
            float dir = (i % 2 == 0) ? 1f : -1f;
            rt.anchoredPosition = original + new Vector2(dir * shakeDistance, 0f);
            yield return new WaitForSeconds(step * 0.5f);
            rt.anchoredPosition = original;
            yield return new WaitForSeconds(step * 0.5f);
        }
        rt.anchoredPosition = original;
    }
}
