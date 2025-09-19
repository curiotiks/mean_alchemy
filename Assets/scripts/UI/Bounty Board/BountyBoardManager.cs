using BountyItemData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using TrollBridge;
using UnityEngine.Assertions.Must;
using UnityEngine.Networking;

public class BountyBoardManager : MonoBehaviour
{
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
        // Initialize the cards holder panels
        SceneManager.sceneLoaded += OnSceneLoaded;
        CardHolderParent = GameObject.FindGameObjectWithTag("cardholderparent");
        cardsHolderPanels = CardHolderParent.GetComponentsInChildren<CardsHolderPanel>().ToList();
        foreach (CardsHolderPanel panel in cardsHolderPanels)
        {
            panel.ClearTheCards();
        }
        if (CardsPrefab == null)
        {
            Debug.LogError("CardsPrefab is not assigned in the inspector.");
        }

        //InitializeCards();
    }

    public void InitializeCards()
    {
        List<BountyItem> bountyItemsFromJSON = LoadBountyDataFromJSON();

        if (bountyItemsFromJSON.Count == 0)
        {
            Debug.LogWarning("No bounty items found in JSON.");
            return;
        }

        CardHolderParent = GameObject.FindGameObjectWithTag("cardholderparent");
        if (CardHolderParent)
        {
            cardsHolderPanels = CardHolderParent.GetComponentsInChildren<CardsHolderPanel>().ToList();
        }
        if (cardsHolderPanels != null && CardsPrefab && CardHolderParent)
        {
            foreach (CardsHolderPanel panel in cardsHolderPanels)
            {
                int cardCOunt = 0;
                foreach (BountyItem bounty in bountyItemsFromJSON)
                {
                    // Debug.Log($"Trying to add '{bounty.difficulty}' to panel '{panel.name}' with difficulty '{panel.cardDifficulty}'");
                    // For each panel, check if the bounty item difficulty matches the panel's difficulty
                    if (bounty.difficulty.Equals(panel.cardDifficulty.ToString(), StringComparison.OrdinalIgnoreCase) && (cardCOunt < panel.MaxCards))
                    {
                        // Debug.Log($"Adding '{bounty.difficulty}' to panel '{panel.name}'");
                        // Debug.Log($"Trying to add card '{bounty.name}' to panel '{panel.name}' (Child count before: {panel.transform.childCount})");
                        GameObject tempCard = Instantiate(CardsPrefab, panel.transform);
                        // Debug.Log($"Card '{tempCard.name}' added. New child count: {panel.transform.childCount}");
                        tempCard.GetComponent<BountyCard>().setCardInfo(bounty);
                        panel.AddCardsToThisRow(new List<GameObject> { tempCard });
                        cardCOunt++;
                    }
                    else
                    {
                        // Debug.Log($"Skipping '{bounty.difficulty}' for panel '{panel.name}' due to difficulty mismatch or max cards reached.");
                    }
                }
                cardCOunt = 0;
            }

        }
        else
        {
            Debug.Log("NO changes in UI but Data Recieved");
        }

    }

    #region JSON operations

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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Debug.LogWarning("Scene : " + SceneManager.GetActiveScene().name);
        CardHolderParent = GameObject.FindGameObjectWithTag("cardholderparent");
        selectedCardPanel = GameObject.FindGameObjectWithTag("selectedcardholder");
        if (selectedCardPanel != null && currentBounty!=null && SceneManager.GetActiveScene().name!= "BountyBoard")
        {
            GameObject _gameObject = Instantiate(CardsPrefab.gameObject, selectedCardPanel.transform);
            _gameObject.transform.localScale = Vector3.one * .6f;
            //_gameObject.GetComponent<BountyCard>().setCardInfo(currentBounty.getCardInfo());
            //_gameObject.transform.SetParent(selectedCardPanel.transform);

            _gameObject.GetComponent<BountyCard>().bountyItem = new BountyItem(currentBounty.bountyItem.name,currentBounty.bountyItem.image
                ,currentBounty.bountyItem.mean,currentBounty.bountyItem.sd,
                currentBounty.bountyItem.difficulty,currentBounty.bountyItem.rewardList);
            BountyCard bcard = _gameObject.GetComponent<BountyCard>();

            bcard.cardImage.sprite = currentBounty.bountyItem.image;
            bcard.cardName.text = currentBounty.bountyItem.name;
            bcard.cardMean.text = "Mean: " + currentBounty.bountyItem.mean.ToString();
            bcard.cardSD.text = "SD: " + currentBounty.bountyItem.sd.ToString();

            bcard.abandonButton.enabled = true;
            bcard.abandonButton.gameObject.SetActive(true);
            bcard.acceptButton.enabled = false;
            bcard.abandonButton.onClick.RemoveAllListeners();
            bcard.abandonButton.onClick.AddListener(() =>
            {
                HardLoadScene("BountyBoard");
            });
            //_gameObject.GetComponent<BountyCard>().CustomMethodForBountyBoard();
            selectedCardPanel.SetActive(true);
        }
        if (CardHolderParent)
        {
            cardsHolderPanels = CardHolderParent.GetComponentsInChildren<CardsHolderPanel>().ToList();

            foreach (CardsHolderPanel panel in cardsHolderPanels)
            {
                panel.ClearTheCards();
            }
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

        InitializeCards();
        //InitializeCards();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
