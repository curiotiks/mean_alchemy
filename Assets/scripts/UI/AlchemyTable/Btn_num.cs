using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Btn_num : MonoBehaviour, IClickLoggingGate, UnityEngine.EventSystems.IPointerDownHandler
{
    public int elementButtonCount = 0;
    public Table_Control_Panel table_control_panel;
    public Table_Plot_Panel table_plot_panel;
    public TextMeshProUGUI text_ui, item_count_text;
    //[HideInInspector]
    [SerializeField] private Button btn;

    private int buttonNumber; // 1-based column this button controls
    private ButtonLoggerConnector[] _connectors; // cached connectors on this object/children
    private bool _suspendRefreshForClick; // mirrors the trash fix to avoid race with connector

    #region Logging
    [Header("Logging")]
    [Tooltip("Catalog used to populate the EventRef dropdown below")] 
    public EventPayloadCatalog catalog;

    [Tooltip("Event to log when a stone is successfully added to this column")] 
    public EventRef addStoneEvent;
    #endregion

    private bool lastAddSucceeded = true;

    void Awake()
    {
        #region set buttons text refering to the gameobject name

        string name = gameObject.name;
        int num = -1;
        try{
            num = int.Parse(name.Split('_')[name.Split('_').Length-1]);
        }catch{
            Debug.Log("parsing error: "+name);
            return;
        }
        text_ui = GetComponentInChildren<TextMeshProUGUI>(true) as TextMeshProUGUI;
        text_ui.enableAutoSizing = true;
        text_ui.text = num+"";
        buttonNumber = num;
        _connectors = GetComponentsInChildren<ButtonLoggerConnector>(true);
        if (item_count_text) item_count_text.enabled = true;
        #endregion

        btn = GetComponent<Button>();
        btn.onClick.AddListener( delegate{buttonHandler(num);} ); // This could be updated to: btn.onClick.AddListener(() => buttonHandler(num));
        UpdateItemText(0);
    }

    private void Start()
    {
        table_plot_panel = FindObjectOfType<Table_Plot_Panel>();
        RefreshConnectorState();
    }

    public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
    {
        // Pre-check capacity so the connector knows whether to log this click
        bool canAdd = (table_control_panel == null) || table_control_panel.CanAddToColumn(buttonNumber);
        _suspendRefreshForClick = true; // prevent any later refresh from flipping state this frame
        if (_connectors != null)
        {
            foreach (var c in _connectors)
                if (c != null)
                    c.logOnButtonClick = canAdd;
        }
    }

    void buttonHandler(int num)
    {
        if (num < 1) return;

        // Ask the data model if we can add (source of truth)
        if (table_control_panel != null && !table_control_panel.CanAddToColumn(num))
        {
            lastAddSucceeded = false;
            int cap = table_plot_panel != null ? table_plot_panel.max_stacked_count : 0;
            Debug.LogWarning($"Column {num} is at max capacity ({cap}). Ignoring click.");
            return;
        }

        lastAddSucceeded = true;
        int newCount = table_control_panel != null ? table_control_panel.updateInput(num) : elementButtonCount + 1;
        UpdateItemText(newCount);
        // Allow state to refresh next frame (mirrors trash-can fix)
        StartCoroutine(_ResumeConnectorNextFrame());
    }

    public void UpdateItemText(int num)
    {
        elementButtonCount = num;
        if (!item_count_text)
            return;
        //item_count_text.enabled = (num <= 0 ? false : true);
        item_count_text.SetText(num <= 0 ? "Empty" : $"{num} / {table_plot_panel?.max_stacked_count}");
    }

    public bool CanLogClick() => lastAddSucceeded;

    private System.Collections.IEnumerator _ResumeConnectorNextFrame()
    {
        yield return null; // wait 1 frame so connector can process
        _suspendRefreshForClick = false;
        RefreshConnectorState();
    }

    private void RefreshConnectorState()
    {
        // Keep connector enable/disable aligned with capacity (optional: also toggle Button.interactable)
        bool canAdd = (table_control_panel == null) || table_control_panel.CanAddToColumn(buttonNumber);
        if (_connectors != null)
        {
            foreach (var c in _connectors)
                if (c != null)
                    c.logOnButtonClick = canAdd;
        }
        if (btn != null)
            btn.interactable = canAdd;
    }

    private void Update()
    {
        if (_suspendRefreshForClick) return;
        RefreshConnectorState();
    }

    private void OnValidate()
    {
        if (catalog == null)
        {
            var gl = FindObjectOfType<GameLogger>();
            if (gl != null && gl.catalog != null) catalog = gl.catalog;
            else
            {
                var fallback = Resources.Load<EventPayloadCatalog>("EventPayloadCatalog");
                if (fallback != null) catalog = fallback;
            }
        }
    }
}
