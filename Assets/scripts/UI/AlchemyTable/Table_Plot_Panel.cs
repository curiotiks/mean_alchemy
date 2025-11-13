using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Table_Plot_Panel : MonoBehaviour
{
    private List<VerticalLayoutGroup> vertical_layout_group_list = new List<VerticalLayoutGroup>();
    public int max_stacked_count = 10;

    [Header("Columns Root (optional)")]
    [Tooltip("If set, columns will be discovered only under this transform. If null, a child named 'Columns' will be used if found; otherwise all VerticalLayoutGroups under this object are used.")]
    [SerializeField] private RectTransform columnsRoot;

    [Header("Chip Prefab & Columns")]
    [SerializeField] private GameObject chipPrefab; // Prefab with Image (+ optional Button) + StoneChip
    [Tooltip("Optional: Per-stone prefab variants. Index 0 => stone 1, index 9 => stone 10.")]
    [SerializeField] private GameObject[] chipPrefabs; // size 10 recommended

    public TextMeshProUGUI y_axis;

    [Header("TRANSMUTE")]
    [SerializeField] GameObject TransmutePanel;
    [SerializeField] GameObject DataElementsHolder;
    [SerializeField] List<GameObject> DataElements;
    [SerializeField] GameObject DataElementPrefab;
    [SerializeField] Button TransmuteButton;
    [SerializeField] Button ConfirmTransmuteButton;
    [SerializeField] Button CancelTransmuteButton;


    [Header("Logging")]
    [SerializeField] private EventPayloadCatalog catalog;
    [SerializeField] private EventRef chipRemovedEvent; // Catalog event for chip removal

    // Runtime stacks: one list of chips per column
    private List<List<StoneChip>> columnChips = new List<List<StoneChip>>();


    void Start()
    {
        // Discover column parents (prefer the explicit Columns root or a child named "Columns")
        vertical_layout_group_list.Clear();
        Transform searchRoot = transform;
        if (columnsRoot != null) searchRoot = columnsRoot;
        else
        {
            var child = transform.Find("Columns");
            if (child != null) searchRoot = child;
        }

        // Prefer only direct children of the root that have a VerticalLayoutGroup
        for (int i = 0; i < searchRoot.childCount; i++)
        {
            var t = searchRoot.GetChild(i);
            var vlg = t.GetComponent<VerticalLayoutGroup>();
            if (vlg != null) vertical_layout_group_list.Add(vlg);
        }

        // Fallback: scan all descendants if none found as direct children (legacy scenes)
        if (vertical_layout_group_list.Count == 0)
        {
            foreach (var x in searchRoot.GetComponentsInChildren<VerticalLayoutGroup>(true))
                vertical_layout_group_list.Add(x);
        }

        // Diagnostics
        if (vertical_layout_group_list.Count != 10)
        {
            Debug.LogWarning($"Table_Plot_Panel: Expected 10 columns, found {vertical_layout_group_list.Count}. Root='{searchRoot.name}'.");
        }
        else
        {
            string names = string.Join(", ", vertical_layout_group_list.ConvertAll(v => v.transform.name));
            Debug.Log($"Table_Plot_Panel: Columns detected: {names}");
        }

        y_axis.text = max_stacked_count.ToString();

        DataElements.Clear();

        TransmuteButton.onClick.RemoveAllListeners();
        TransmuteButton.onClick.AddListener(setUpTransmutePanel);

        ConfirmTransmuteButton.onClick.RemoveAllListeners();
        ConfirmTransmuteButton.onClick.AddListener(confirmTransmute);

        CancelTransmuteButton.onClick.RemoveAllListeners();
        CancelTransmuteButton.onClick.AddListener(cancelTransmute);

        // Initialize per-column chip stacks to match the found column groups
        columnChips.Clear();
        for (int i = 0; i < vertical_layout_group_list.Count; i++)
            columnChips.Add(new List<StoneChip>());
    }

    public void drawPlot(int num, bool isAdded)
    {
        // num is 1-based in current callers; convert to 0-based index
        int col = Mathf.Clamp(num - 1, 0, vertical_layout_group_list.Count - 1);
        if (isAdded)
        {
            AddChip(col, num);
        }
        else
        {
            RemoveLastChip(col);
        }
    }

    private void AddChip(int columnIndex, int stoneValue)
    {
        // Ok if chipPrefab is null when variants are provided; we'll choose below
        if (columnIndex < 0 || columnIndex >= vertical_layout_group_list.Count) return;

        var colList = columnChips[columnIndex];
        if (max_stacked_count > 0 && colList.Count >= max_stacked_count)
        {
            Debug.LogWarning($"Column {columnIndex + 1} is at max capacity ({max_stacked_count}).");
            return;
        }

        Transform parent = vertical_layout_group_list[columnIndex].transform;
        if (parent == null)
        {
            Debug.LogError($"Table_Plot_Panel: Column parent at index {columnIndex} is null.");
            return;
        }

        // Prefer a per-stone variant if provided; fallback to the single chipPrefab
        GameObject prefabToUse = chipPrefab;
        if (chipPrefabs != null && chipPrefabs.Length > 0)
        {
            int idx = stoneValue - 1; // stone values are 1..10
            if (idx >= 0 && idx < chipPrefabs.Length && chipPrefabs[idx] != null)
            {
                prefabToUse = chipPrefabs[idx];
            }
        }

        if (prefabToUse == null)
        {
            Debug.LogError("Table_Plot_Panel: No prefab assigned (chipPrefabs entry and chipPrefab are both null).");
            return;
        }

        GameObject go = Instantiate(prefabToUse, parent);
        // Place newest chip at the top (first child) so animation plays on the newest
        go.transform.SetSiblingIndex(0);
        var chip = go.GetComponent<StoneChip>();
        if (chip == null) chip = go.AddComponent<StoneChip>();

        chip.Init(stoneValue, columnIndex, HandleChipRemoveRequest);
        colList.Add(chip);
    }

    private void RemoveLastChip(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= columnChips.Count) return;
        var colList = columnChips[columnIndex];
        if (colList.Count == 0) return;

        var chip = colList[colList.Count - 1];
        colList.RemoveAt(colList.Count - 1);

        chip.DestroyImmediateSafe();
    }

    // Currently unused; kept for future direct-removal flows
    private void RemoveChipInstance(StoneChip chip)
    {
        if (chip == null) return;
        int columnIndex = chip.columnIndex;
        if (columnIndex < 0 || columnIndex >= columnChips.Count)
        {
            // Fallback: just destroy if we can't resolve the column safely
            chip.DestroyImmediateSafe();
            return;
        }
        var colList = columnChips[columnIndex];
        int idx = colList.IndexOf(chip);
        if (idx >= 0)
        {
            colList.RemoveAt(idx);
        }
        chip.DestroyImmediateSafe();
    }

    private void HandleChipRemoveRequest(StoneChip chip)
    {
        // Defer to the data model; it will call drawPlot(num,false) which invokes RemoveLastChip
        if (Table_Control_Panel.instance != null)
            Table_Control_Panel.instance.updateInput(chip.columnIndex + 1, false);
        // Do NOT call RemoveChipInstance here; that causes a second removal.
    }

    public void resetPlot()
    {
        // foreach(var x in plot_item_list){
        //     RectTransform rt = x.GetComponent<RectTransform>();
        //     rt.sizeDelta = new Vector2(rt.sizeDelta.x, 0);
        // }
    }

    private void setUpTransmutePanel()
    {
        if (!TransmutePanel)
        {
            return;
        }

        TransmuteManager.Instance.TransmuteMakeNew();
        if (TransmuteManager.TempFamiliarItem == null)
        {
            Debug.LogWarning("TempFamiliarItem was not created by TransmuteMakeNew().");
            return;
        }
        //Update UI here
        // setUpDataElements("Name :", TransmuteManager.TempFamiliarItem.name);
        // setUpDataElements("ID :", TransmuteManager.TempFamiliarItem.id.ToString());
        // setUpDataElements("Description :", TransmuteManager.TempFamiliarItem.description, false);
        setUpDataElements("Mean :", TransmuteManager.TempFamiliarItem.mean.ToString(), false);
        setUpDataElements("SD :", TransmuteManager.TempFamiliarItem.sd.ToString(), false);
        setUpDataElements("Skew :", TransmuteManager.TempFamiliarItem.skew.ToString(), false);
        TransmutePanel.SetActive(true);

    }

    private void setUpDataElements(string key, string value, bool isInteractable = true)
    {
        GameObject tempDataElement;
        tempDataElement = Instantiate(DataElementPrefab) as GameObject;

        tempDataElement.GetComponentInChildren<TMP_Text>().text = key;
        tempDataElement.GetComponentInChildren<TMP_InputField>().text = value;
        tempDataElement.GetComponentInChildren<TMP_InputField>().interactable = isInteractable;

        tempDataElement.transform.SetParent(DataElementsHolder.transform, false);
        DataElements.Add(tempDataElement);
    }

    private void confirmTransmute()
    {
        TransmuteManager.Instance.TransmuteAddNew();
        TransmutePanel.SetActive(false);
        foreach (Transform child in DataElementsHolder.transform)
        {
            DataElements.Remove(child.gameObject);
            GameObject.Destroy(child.gameObject);
        }
        Debug.Log("Transmute confirmed. Returning to Alchemy Table.");
        Debug.Log(SceneNames.AlchemyTable);

        // Log movement back to the Lab using the EventPayloadCatalog (Location -> lab)
        if (catalog != null)
        {
            var logger = GameLogger.Instance != null 
                ? GameLogger.Instance 
                : GameObject.FindObjectOfType<GameLogger>();

            if (logger != null)
            {
                logger.LogEvent("Location", "lab", null);
            }
            else
            {
                Debug.LogWarning("Table_Plot_Panel: GameLogger instance not found; could not log return-to-lab event.");
            }
        }
        else
        {
            Debug.LogWarning("Table_Plot_Panel: EventPayloadCatalog not assigned; cannot log return-to-lab event.");
        }

        SceneManager.LoadScene(SceneNames.TheLab); // Always return to Alchemy Table after transmute

    }

    private void cancelTransmute()
    {
        TransmuteManager.Instance.TransmuteEraseNew();
        TransmutePanel.SetActive(false);
        foreach (Transform child in DataElementsHolder.transform)
        {
            DataElements.Remove(child.gameObject);
            GameObject.Destroy(child.gameObject);
        }
    }

    // private string ResolveSceneName()
    // {
    //     switch (destination)
    //     {
    //         case QuitTarget.AlchemyTable: return SceneNames.AlchemyTable;
    //         case QuitTarget.TheLab: return SceneNames.TheLab;
    //         case QuitTarget.BountyBoard: return SceneNames.BountyBoard;
    //         case QuitTarget.CombatArena: return SceneNames.CombatArena;
    //         case QuitTarget.WorldMap: return SceneNames.WorldMap;
    //         default: return SceneNames.AlchemyTable;
    //     }
    // }
}
