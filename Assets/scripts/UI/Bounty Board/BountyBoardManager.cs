using BountyItemData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using TrollBridge;
using UnityEngine.Assertions.Must;

public class BountyBoardManager : MonoBehaviour
{
    public static BountyBoardManager instance;

    [field: SerializeField] public bool shouldDestroyOnLoad { get; private set; }

    [Header("JSON Handler")]
    [SerializeField] string bountyFileName = "BountyItems";
    [SerializeField] string BountyFilePath/* => Path.Combine(Application.persistentDataPath, bountyFileName)*/;

    [SerializeField] GameObject CardHolderParent;
    [SerializeField] GameObject selectedCardPanel;
    [SerializeField] GameObject CardsPrefab;

    [Header("ON GOING BOUNTY")]
    public BountyCard currentBounty = null;

    [SerializeField] List<CardsHolderPanel> cardsHolderPanels;

    private void Awake()
    {
        BountyFilePath = Path.Combine(Application.persistentDataPath, bountyFileName);
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
                    Debug.Log($"Trying to add '{bounty.difficulty}' to panel '{panel.name}' with difficulty '{panel.cardDifficulty}'");
                    // For each panel, check if the bounty item difficulty matches the panel's difficulty
                    if (bounty.difficulty.Equals(panel.cardDifficulty.ToString(), StringComparison.OrdinalIgnoreCase) && (cardCOunt < panel.MaxCards))
                    {
                        Debug.Log($"Adding '{bounty.difficulty}' to panel '{panel.name}'");
                        Debug.Log($"Trying to add card '{bounty.name}' to panel '{panel.name}' (Child count before: {panel.transform.childCount})");
                        GameObject tempCard = Instantiate(CardsPrefab, panel.transform);
                        Debug.Log($"Card '{tempCard.name}' added. New child count: {panel.transform.childCount}");
                        tempCard.GetComponent<BountyCard>().setCardInfo(bounty);
                        panel.AddCardsToThisRow(new List<GameObject> { tempCard });
                        cardCOunt++;
                    }
                    else
                    {
                        Debug.Log($"Skipping '{bounty.difficulty}' for panel '{panel.name}' due to difficulty mismatch or max cards reached.");
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
        string jsonData = JsonUtility.ToJson(item);
        if (File.Exists(BountyFilePath))
        {
            string existingContent = File.ReadAllText(BountyFilePath).TrimEnd(']');
            existingContent += existingContent.Length > 2 ? "," : "";
            File.WriteAllText(BountyFilePath, $"{existingContent}{jsonData}]");
        }
        else
        {
            File.WriteAllText(BountyFilePath, $"[{jsonData}]");
        }
#if UNITY_EDITOR
        Debug.Log($"Bounty saved at: {BountyFilePath}");
#endif
    }


    public List<BountyItem> LoadBountyDataFromJSON()
    {
        List<BountyItem> items = new List<BountyItem>();
        if (!File.Exists(BountyFilePath))
        {
            Debug.LogWarning($"No bounty data found @ {BountyFilePath}");
            return items;
        }

        string json = File.ReadAllText(BountyFilePath);
        // Handle JSON array deserialization
        json = $"{json}"; // Wrap array in an object
        Debug.Log($"your Bounty Json : {json}");
        BountyItemWrapper wrapper = JsonUtility.FromJson<BountyItemWrapper>(json);
        return wrapper?.bountyItems ?? items;
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
        Debug.LogWarning("Scene Loaded: " + SceneManager.GetActiveScene().name);
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



