using BountyItemData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BountyBoardManager : MonoBehaviour
{
    /// <summary>
    /// Central controller for the Bounty Board scene. Responsible for:
    ///  - Loading bounty data (Resources / StreamingAssets / PersistentData)
    ///  - Spawning bounty card prefabs into difficulty lanes
    ///  - Restoring currently selected bounty into non-board scenes
    ///  - Handling scene transitions and re-initialization
    /// </summary>

    private const string TAG_CARD_HOLDER_PARENT = "cardholderparent";
    private const string TAG_SELECTED_CARD_HOLDER = "selectedcardholder";

    public static BountyBoardManager instance;

    public enum BountyDataSource { Resources, StreamingAssets, PersistentData }

    [Header("Bounty Data Source")]
    [Tooltip("Where to load BountyItems.json from. Resources = Assets/Resources/<Resource Path>. StreamingAssets = Assets/StreamingAssets/<Relative Path>. PersistentData = Application.persistentDataPath/<File Name>.")]
    [SerializeField] private BountyDataSource dataSource = BountyDataSource.Resources;

    [Tooltip("For Resources: path under Assets/Resources without extension. Example: Data/BountyItems")] 
    [SerializeField] private string resourcesPath = "Data/BountyItems";

    [Tooltip("For StreamingAssets: relative path under Assets/StreamingAssets, e.g., Data/BountyItems.json")] 
    [SerializeField] private string streamingAssetsRelativePath = "Data/BountyItems.json";

    [Tooltip("For PersistentData: file name used under Application.persistentDataPath")] 
    [SerializeField] private string persistentFileName = "BountyItems.json";

    [field: SerializeField] public bool shouldDestroyOnLoad { get; private set; }

    [Header("JSON Handler (Legacy Persistent Save Support)")]
    [SerializeField] string bountyFileName = "BountyItems.json"; // used for PersistentData
    [SerializeField] string BountyFilePath; // resolved at runtime for PersistentData

    [SerializeField] GameObject CardHolderParent;
    [SerializeField] GameObject selectedCardPanel;
    [SerializeField] GameObject CardsPrefab;
    [SerializeField] private GameObject blackout; // BG Image under Selected Card Panel
    [SerializeField] private string blackoutChildName = "BG Image"; // name of blackout under selectedCardPanel
    private bool overlayShown = false;

    [Header("ON GOING BOUNTY")]
    public BountyCard currentBounty = null;

    [SerializeField] List<CardsHolderPanel> cardsHolderPanels;

    private void Awake()
    {
        if (dataSource == BountyDataSource.PersistentData)
            BountyFilePath = Path.Combine(Application.persistentDataPath, string.IsNullOrEmpty(persistentFileName) ? bountyFileName : persistentFileName);
        if (instance == null)
        {
            instance = this;
            if (shouldDestroyOnLoad == false)
                DontDestroyOnLoad(gameObject);
            InitializeCards();
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        CardHolderParent = GameObject.FindGameObjectWithTag(TAG_CARD_HOLDER_PARENT);
        if (CardHolderParent)
        {
            cardsHolderPanels = CardHolderParent.GetComponentsInChildren<CardsHolderPanel>().ToList();
            foreach (var panel in cardsHolderPanels) panel.ClearTheCards();
        }
        if (CardsPrefab == null)
        {
            Debug.LogError("CardsPrefab is not assigned in the inspector.");
        }
    }

    /// <summary>
    /// Rebinds scene references and rebuilds UI for the *current* scene.
    /// </summary>
    private void RebindAndBuildForActiveScene()
    {
        var active = SceneManager.GetActiveScene();
        OnSceneLoaded(active, LoadSceneMode.Single);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // When returning to this object that persists across scenes, immediately sync to the current scene
        RebindAndBuildForActiveScene();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Builds the board by reading bounty data and instantiating card prefabs into each difficulty panel.
    /// Panels are discovered via the tag defined in TAG_CARD_HOLDER_PARENT.
    /// </summary>
    public void InitializeCards()
    {
        List<BountyItem> bountyItemsFromJSON = LoadBountyDataFromJSON();

        if (bountyItemsFromJSON.Count == 0)
        {
            Debug.LogWarning("No bounty items found in JSON.");
            return;
        }

        CardHolderParent = GameObject.FindGameObjectWithTag(TAG_CARD_HOLDER_PARENT);
        if (CardHolderParent)
        {
            cardsHolderPanels = CardHolderParent.GetComponentsInChildren<CardsHolderPanel>().ToList();
        }
        if (cardsHolderPanels != null && CardsPrefab && CardHolderParent)
        {
            foreach (CardsHolderPanel panel in cardsHolderPanels)
            {
                int cardCount = 0;
                foreach (BountyItem bounty in bountyItemsFromJSON)
                {
                    // For each panel, check if the bounty item difficulty matches the panel's difficulty
                    if (bounty.difficulty.Equals(panel.cardDifficulty.ToString(), StringComparison.OrdinalIgnoreCase) && (cardCount < panel.MaxCards))
                    {
                        GameObject tempCard = Instantiate(CardsPrefab, panel.transform);
                        tempCard.GetComponent<BountyCard>().setCardInfo(bounty);
                        panel.AddCardsToThisRow(new List<GameObject> { tempCard });
                        cardCount++;
                        // If the spawned card has a Button on its root, clicking it should show the modal blackout
                        var btn = tempCard.GetComponent<UnityEngine.UI.Button>();
                        if (btn != null)
                        {
                            btn.onClick.AddListener(ShowBlackout);
                        }
                        else
                        {
                            // try to find a common child button called "View" or "Open" if present
                            var view = tempCard.transform.Find("View")?.GetComponent<UnityEngine.UI.Button>()
                                       ?? tempCard.transform.Find("Open")?.GetComponent<UnityEngine.UI.Button>();
                            if (view != null) view.onClick.AddListener(ShowBlackout);
                        }
                    }
                }
                cardCount = 0;
            }

        }
        else
        {
            Debug.Log("NO changes in UI but Data Recieved");
        }

    }

    private void ResolveBlackout()
    {
        if (blackout == null)
        {
            if (selectedCardPanel != null)
            {
                var bg = selectedCardPanel.transform.Find(blackoutChildName);
                if (bg != null) blackout = bg.gameObject;
            }
            if (blackout == null)
            {
                var bgByName = GameObject.Find(blackoutChildName);
                if (bgByName != null) blackout = bgByName;
            }
        }
    }

    private void ShowBlackout()
    {
        ResolveBlackout();
        if (blackout != null && !blackout.activeSelf)
            blackout.SetActive(true);
    }

    private void HideBlackout()
    {
        ResolveBlackout();
        if (blackout != null && blackout.activeSelf)
            blackout.SetActive(false);
    }

    private bool AnyModalContentActive()
    {
        if (selectedCardPanel == null) return false;
        for (int i = 0; i < selectedCardPanel.transform.childCount; i++)
        {
            var child = selectedCardPanel.transform.GetChild(i).gameObject;
            if (child == null) continue;
            if (blackout != null && child == blackout) continue; // skip the BG Image itself
            if (child.activeInHierarchy) return true; // any visible modal content
        }
        return false;
    }

    private void EnsureOverlaySync()
    {
        if (SceneManager.GetActiveScene().name != "BountyBoard") return;
        ResolveBlackout();
        bool shouldShow = AnyModalContentActive();
        if (shouldShow && !overlayShown)
        {
            ShowBlackout();
            overlayShown = true;
        }
        else if (!shouldShow && overlayShown)
        {
            HideBlackout();
            overlayShown = false;
        }
    }

    private void Update()
    {
        // Poll lightly each frame; cost is negligible (handful of children)
        EnsureOverlaySync();
    }

    #region JSON operations

    /// <summary>
    /// Appends a single <see cref="BountyItem"/> to a JSON array under Application.persistentDataPath.
    /// This does not modify project assets; intended for legacy/local persistence only.
    /// </summary>
    public void SaveBountyItemToJson(BountyItem item)
    {
        // Only persist to PersistentData to avoid modifying project assets at runtime
        string fullPath = Path.Combine(Application.persistentDataPath, string.IsNullOrEmpty(persistentFileName) ? bountyFileName : persistentFileName);

        string jsonData = JsonUtility.ToJson(item);
        if (File.Exists(fullPath))
        {
            string existingContent = File.ReadAllText(fullPath).TrimEnd(']');
            existingContent += existingContent.Length > 2 ? "," : "";
            File.WriteAllText(fullPath, $"{existingContent}{jsonData}]");
        }
        else
        {
            File.WriteAllText(fullPath, $"[{jsonData}]");
        }
#if UNITY_EDITOR
        Debug.Log($"Bounty saved at: {fullPath}");
#endif
    }


    /// <summary>
    /// Loads bounty data according to the selected <see cref="BountyDataSource"/>.
    /// Resources → Assets/Resources/{resourcesPath}.json
    /// StreamingAssets → Assets/StreamingAssets/{streamingAssetsRelativePath}
    /// PersistentData → Application.persistentDataPath/{persistentFileName}
    /// </summary>
    public List<BountyItem> LoadBountyDataFromJSON()
    {
        List<BountyItem> items = new List<BountyItem>();
        string json = null;

        try
        {
            switch (dataSource)
            {
                case BountyDataSource.Resources:
                    {
                        if (string.IsNullOrEmpty(resourcesPath))
                        {
                            Debug.LogError("Resources path not set for Bounty data.");
                            return items;
                        }
                        TextAsset ta = Resources.Load<TextAsset>(resourcesPath);
                        if (ta == null)
                        {
                            Debug.LogError($"Bounty JSON not found in Resources at '{resourcesPath}'. Place 'BountyItems.json' under Assets/Resources/{resourcesPath}.json");
                            return items;
                        }
                        json = ta.text;
                        break;
                    }
                case BountyDataSource.StreamingAssets:
                    {
                        string fullPath = Path.Combine(Application.streamingAssetsPath, streamingAssetsRelativePath);
                        if (fullPath.StartsWith("http"))
                        {
                            // Android/WebGL path — use UnityWebRequest synchronously via SendWebRequest + wait
                            var req = UnityWebRequest.Get(fullPath);
                            var op = req.SendWebRequest();
                            while (!op.isDone) { }
                            if (req.result != UnityWebRequest.Result.Success)
                            {
                                Debug.LogError($"Failed to read StreamingAssets JSON at '{fullPath}': {req.error}");
                                return items;
                            }
                            json = req.downloadHandler.text;
                        }
                        else
                        {
                            if (!File.Exists(fullPath))
                            {
                                Debug.LogError($"Bounty JSON not found in StreamingAssets at '{fullPath}'.");
                                return items;
                            }
                            json = File.ReadAllText(fullPath);
                        }
                        break;
                    }
                case BountyDataSource.PersistentData:
                    {
                        string fullPath = string.IsNullOrEmpty(BountyFilePath)
                            ? Path.Combine(Application.persistentDataPath, string.IsNullOrEmpty(persistentFileName) ? bountyFileName : persistentFileName)
                            : BountyFilePath;
                        if (!File.Exists(fullPath))
                        {
                            Debug.LogWarning($"No bounty data found @ {fullPath}");
                            return items;
                        }
                        json = File.ReadAllText(fullPath);
                        break;
                    }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading bounty JSON: {ex.Message}");
            return items;
        }

        if (string.IsNullOrWhiteSpace(json))
            return items;

        // Expect an array JSON (e.g., [ {..}, {..} ])
        // If your file is an array, use a wrapper Scriptable format or decode as array
        try
        {
            // If your existing code uses a wrapper, keep it. Otherwise try array parsing helper
            // Using wrapper class as before:
            json = $"{json}"; // keep as-is; your wrapper expects an array
            BountyItemWrapper wrapper = JsonUtility.FromJson<BountyItemWrapper>(json);
            return wrapper?.bountyItems ?? items;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse bounty JSON: {ex.Message}");
            return items;
        }
    }

    #endregion


    /// <summary>
    /// Synchronously loads a scene by name. Clears cached panel references; the Start/sceneLoaded flow
    /// will repopulate them and rebuild cards.
    /// </summary>
    public void HardLoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is null or empty.");
            return;
        }
        if (cardsHolderPanels!=null)
        {
            cardsHolderPanels.Clear();
            cardsHolderPanels = null;
        }
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// Clears the selected bounty without changing scenes and refreshes warp gates.
    /// </summary>
    public void ClearCurrentBounty(bool log = true)
    {
        if (currentBounty != null && log)
        {
            try
            {
                GameLogger.Instance?.LogEvent("bounty_abandoned_from_lab",
                    $"key={currentBounty.bountyItem?.name ?? "unknown"}");
            }
            catch {}
        }

        currentBounty = null;
        WarpGate.RefreshAllGates();
#if UNITY_EDITOR
        Debug.Log("[BountyBoardManager] currentBounty cleared (Lab abandon).");
#endif
    }

    /// <summary>
    /// SceneLoaded callback: rebinds panel references, restores the selected bounty into non-board scenes,
    /// then (re)initializes the board.
    /// </summary>
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Debug.LogWarning("Scene : " + SceneManager.GetActiveScene().name);
        CardHolderParent = GameObject.FindGameObjectWithTag(TAG_CARD_HOLDER_PARENT);
        selectedCardPanel = GameObject.FindGameObjectWithTag(TAG_SELECTED_CARD_HOLDER);
        Debug.Log($"[BountyBoard] SceneLoaded start → CardHolderParent={(CardHolderParent != null)}, selectedCardPanel={(selectedCardPanel != null)}, panelActive={(selectedCardPanel ? selectedCardPanel.activeSelf : false)}");

        // Ensure the Selected Card Panel parent is active when entering the BountyBoard scene,
        // so tag lookups continue to work and the modal UI can be shown.
        if (scene.name == "BountyBoard" && selectedCardPanel != null)
        {
            if (!selectedCardPanel.activeSelf)
            {
                Debug.LogWarning("[BountyBoard] SelectedCardPanel was inactive at load; forcing active.");
                selectedCardPanel.SetActive(true);
            }
        }

        // Make sure the blackout child ("BG Image") starts hidden in the BountyBoard scene.
        if (scene.name == "BountyBoard")
        {
            var bgObj = GameObject.Find("BG Image");
            if (bgObj != null && bgObj.activeSelf)
                bgObj.SetActive(false);
        }

        Debug.Log($"[BountyBoard] selectedCardPanel active = {(selectedCardPanel != null && selectedCardPanel.activeSelf)}, scene = {scene.name}");

        // If this scene has a full-screen blackout image (used by popups/menus), force it off on entry
        ResolveBlackout();
        HideBlackout();
        overlayShown = false;

        if (scene.name == "BountyBoard" && selectedCardPanel != null)
        {
            // Keep parent active for tag lookups; hide its visual children until a card is opened
            for (int i = 0; i < selectedCardPanel.transform.childCount; i++)
            {
                var child = selectedCardPanel.transform.GetChild(i).gameObject;
                if (child != null && child.name != "BG Image")
                {
                    child.SetActive(false);
                }
            }
        }

        // ensure blackout reference is set for this scene
        ResolveBlackout();

        if (selectedCardPanel != null && currentBounty != null && SceneManager.GetActiveScene().name != "BountyBoard")
        {
            GameObject instantiatedCard = Instantiate(CardsPrefab.gameObject, selectedCardPanel.transform);
            instantiatedCard.transform.localScale = Vector3.one * .6f;

            var src = currentBounty.bountyItem;
            instantiatedCard.GetComponent<BountyCard>().bountyItem = new BountyItem(
                src.name,
                src.imagePath,   // constructor now expects imagePath (string)
                src.mean,
                src.sd,
                src.difficulty,
                src.rewardList
            );

            BountyCard bcard = instantiatedCard.GetComponent<BountyCard>();

            // Resolve sprite from cached image or Resources path
            Sprite resolved = src.image;
            if (resolved == null && !string.IsNullOrEmpty(src.imagePath))
            {
                resolved = Resources.Load<Sprite>(src.imagePath);
                if (resolved == null)
                {
                    var all = Resources.LoadAll<Sprite>(src.imagePath);
                    if (all != null && all.Length > 0) resolved = all[0];
                }
            }
            if (bcard.cardImage != null) bcard.cardImage.sprite = resolved;
            if (bcard.cardName  != null) bcard.cardName.text  = src.name;
            if (bcard.cardMean  != null) bcard.cardMean.text  = "Mean: " + src.mean.ToString();
            if (bcard.cardSD    != null) bcard.cardSD.text    = "SD: "   + src.sd.ToString();
            // cache into the new item too (helps later scene passes)
            bcard.bountyItem.image = resolved;

            bcard.abandonButton.enabled = true;
            bcard.abandonButton.gameObject.SetActive(true);
            bcard.acceptButton.enabled = false;

            // Replace any persistent listeners to avoid unexpected scene loads
            bcard.abandonButton.onClick = new Button.ButtonClickedEvent();
            bcard.abandonButton.onClick.AddListener(() =>
            {
                // Log + clear selection
                ClearCurrentBounty(false);
                try
                {
                    GameLogger.Instance?.LogEvent("bounty_abandoned_from_lab",
                        $"key={src.name}");
                }
                catch {}

                // Hide the badge panel in the Lab and destroy this instantiated card
                if (selectedCardPanel) selectedCardPanel.SetActive(false);
                if (instantiatedCard) Destroy(instantiatedCard);
                HideBlackout();
            });
        }

        if (CardHolderParent)
        {
            cardsHolderPanels = CardHolderParent.GetComponentsInChildren<CardsHolderPanel>().ToList();

            foreach (CardsHolderPanel panel in cardsHolderPanels)
            {
                panel.ClearTheCards();
            }
            // Required: assign the card prefab in Inspector
            if (CardsPrefab == null)
            {
                Debug.LogError("CardsPrefab is not assigned in the inspector.");
                return;
            }
        }
        else
        {
            Debug.LogWarning("CardHolderParent not found in the scene.");
        }

        if (CardHolderParent == null || CardsPrefab == null)
        {
            return; // nothing to build in this scene
        }

        InitializeCards();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
