using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Btn_num : MonoBehaviour, IClickLoggingGate
{
    public int elementButtonCount = 0;
    public Table_Control_Panel table_control_panel;
    public Table_Plot_Panel table_plot_panel;
    public TextMeshProUGUI text_ui, item_count_text;
    //[HideInInspector]
    [SerializeField] private Button btn;

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
        if (item_count_text) item_count_text.enabled = true;
        #endregion

        btn = GetComponent<Button>();
        btn.onClick.AddListener( delegate{buttonHandler(num);} ); // This could be updated to: btn.onClick.AddListener(() => buttonHandler(num));
        UpdateItemText(0);
        
    }

    private void Start()
    {
        table_plot_panel = FindObjectOfType<Table_Plot_Panel>();    
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

        // Log only after a successful add
        var logger = GameLogger.Instance != null ? GameLogger.Instance : FindObjectOfType<GameLogger>();
        if (logger != null && !string.IsNullOrEmpty(addStoneEvent.category) && !string.IsNullOrEmpty(addStoneEvent.key))
        {
            logger.LogEvent(addStoneEvent);
        }
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
