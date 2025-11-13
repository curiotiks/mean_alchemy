using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using BountyItemData;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class BountyCard : MonoBehaviour
{
    // Built-in logging (no inspector wiring required)
    private const string LOG_CATEGORY = "Bounty Board";
    private const string EV_VIEW     = "view_card";
    private const string EV_CLOSE    = "close_card";
    private const string EV_ACCEPT   = "select_bounty";
    private const string EV_ABANDON  = "abandon_bounty";

    [Header("Fill UI ELEMEMTS HERE")]
    [SerializeField] public Image cardImage;
    [SerializeField] public TMPro.TextMeshProUGUI cardName;
    [SerializeField] public TMPro.TextMeshProUGUI cardMean;
    [SerializeField] public TMPro.TextMeshProUGUI cardSD;
    [SerializeField] public TMPro.TextMeshProUGUI cardSelectedStatus;
    [SerializeField] bool isSelected = false;
    [SerializeField] Button mainSelectionButton;
    [SerializeField] Button closeButton;
    [SerializeField] public Button acceptButton;
    [SerializeField] public Button abandonButton;

    [Header("Transform Details")]
    [SerializeField] GameObject selectedCardPanel;
    [SerializeField] Transform cardHolderParent;

    [SerializeField] public BountyItem bountyItem;

    private void Start()
    {
        // Initialize the bounty item here if needed
        //bountyItem = new BountyItem("Test", null, 10, 1, "Easy" ,new
        //    List<RewardEntry>());
        if(selectedCardPanel)
            selectedCardPanel.SetActive(false); 
        if (selectedCardPanel == null)
            selectedCardPanel = GameObject.FindGameObjectWithTag("selectedcardholder");
    }

    public BountyItem getCardInfo()
    {
        return bountyItem;
    }

    public void setCardInfo(BountyItem bountyItem)
    {
        this.bountyItem = bountyItem;
        isSelected = false;
        InitializeCard();
    }

    private void InitializeCard()
    {
        // Resolve sprite: prefer runtime sprite on the item; otherwise try Resources by path
        Sprite spriteToUse = bountyItem != null ? bountyItem.image : null;
        if (spriteToUse == null && bountyItem != null && !string.IsNullOrEmpty(bountyItem.imagePath))
        {
            // Expect path like "Sprites/Dragon" under Assets/Resources/
            spriteToUse = Resources.Load<Sprite>(bountyItem.imagePath);
            if (spriteToUse == null)
            {
                // If this is a sliced sprite sheet, try LoadAll and take the first sub-sprite
                var all = Resources.LoadAll<Sprite>(bountyItem.imagePath);
                if (all != null && all.Length > 0)
                    spriteToUse = all[0];
            }
#if UNITY_EDITOR
            if (spriteToUse == null)
                Debug.LogWarning($"BountyCard: Sprite not found at Resources path '{bountyItem.imagePath}'.");
#endif
            // Cache for later scene restore paths that read bountyItem.image
            if (spriteToUse != null && bountyItem != null)
                bountyItem.image = spriteToUse;
        }

        if (cardImage != null)
            cardImage.sprite = spriteToUse;

        if (cardName != null) cardName.text = bountyItem.name;
        if (cardMean != null) cardMean.text = "Mean: " + bountyItem.mean.ToString();
        if (cardSD   != null) cardSD.text   = "SD: "   + bountyItem.sd.ToString();

        //Small check to display card status
        cardSelectedStatus.enabled = false;
        if (BountyBoardManager.instance.currentBounty!=null)
        {
            //Debug.Log($"HERE ---> {BountyBoardManager.instance.currentBounty.name},{bountyItem.name}");
            cardSelectedStatus.enabled =(BountyBoardManager.instance.currentBounty.bountyItem.name == bountyItem.name);
        }

        //Getting placeholders and parent
        selectedCardPanel = GameObject.FindGameObjectWithTag("selectedcardholder");
        cardHolderParent = transform.parent;


        // --- Button listeners: wipe any persistent Inspector hooks first, then add code listeners ---

        // Main select
        if (mainSelectionButton)
        {
            if (mainSelectionButton.onClick.GetPersistentEventCount() > 0)
                Debug.LogWarning($"[BountyCard] mainSelectionButton had {mainSelectionButton.onClick.GetPersistentEventCount()} persistent listeners. Replacing to avoid unintended scene loads.");
            mainSelectionButton.onClick = new Button.ButtonClickedEvent(); // wipes persistent listeners
            mainSelectionButton.onClick.AddListener(() =>
            {
                isSelected = true;
                HandleCardSelection();
            });
        }

        // Close
        if (closeButton)
        {
            if (closeButton.onClick.GetPersistentEventCount() > 0)
                Debug.LogWarning($"[BountyCard] closeButton had {closeButton.onClick.GetPersistentEventCount()} persistent listeners. Replacing.");
            closeButton.gameObject.SetActive(false);
            closeButton.onClick = new Button.ButtonClickedEvent();
            closeButton.onClick.AddListener(() =>
            {
                isSelected = false;
                HandleCloseSelection();
            });
        }

        // Accept
        if (acceptButton)
        {
            if (acceptButton.onClick.GetPersistentEventCount() > 0)
                Debug.LogWarning($"[BountyCard] acceptButton had {acceptButton.onClick.GetPersistentEventCount()} persistent listeners. Replacing.");
            acceptButton.gameObject.SetActive(false);
            acceptButton.onClick = new Button.ButtonClickedEvent();
            acceptButton.onClick.AddListener(() =>
            {
                Debug.Log("Bounty Accepted: " + bountyItem.name);
                isSelected = false;
                HandleCardAcceptance();
            });
        }

        // Abandon
        if (abandonButton)
        {
            if (abandonButton.onClick.GetPersistentEventCount() > 0)
                Debug.LogWarning($"[BountyCard] abandonButton had {abandonButton.onClick.GetPersistentEventCount()} persistent listeners. Replacing.");
            abandonButton.gameObject.SetActive(false);
            abandonButton.onClick = new Button.ButtonClickedEvent();
            abandonButton.onClick.AddListener(() =>
            {
                Debug.Log("Bounty Abandoned: " + bountyItem.name);
                isSelected = false;
                HandleAbandonSelection();
            });
        }
    }


    private void HandleCardSelection()
    {
        if (selectedCardPanel == null)
        {
            Debug.LogWarning("[BountyCard] No selectedCardPanel found in this scene (tag 'selectedcardholder'). Skipping move-to-panel UI.");
            // Still show appropriate buttons without moving the card in hierarchy
            closeButton?.gameObject.SetActive(isSelected);
            if (BountyBoardManager.instance.currentBounty)
            {
                if (BountyBoardManager.instance.currentBounty.bountyItem.name == bountyItem.name)
                {
                    abandonButton?.gameObject.SetActive(true);
                    acceptButton?.gameObject.SetActive(false);
                }
                else
                {
                    abandonButton?.gameObject.SetActive(false);
                    acceptButton?.gameObject.SetActive(true);
                }
            }
            else
            {
                acceptButton?.gameObject.SetActive(isSelected);
                abandonButton?.gameObject.SetActive(!isSelected);
            }
            if (isSelected) LogKey(EV_VIEW);
            return;
        }
        selectedCardPanel.SetActive(isSelected);
        this.transform.SetParent(selectedCardPanel.transform);
        this.transform.localPosition = Vector3.zero;
        this.transform.localScale = Vector3.one * 2f;
        closeButton.gameObject.SetActive(isSelected);

        if (BountyBoardManager.instance.currentBounty)
        {
#if UNITY_EDITOR 
            Debug.Log($"{BountyBoardManager.instance.currentBounty.name},{bountyItem.name}");
#endif
            if (BountyBoardManager.instance.currentBounty.bountyItem.name == bountyItem.name)
            {

                abandonButton.gameObject.SetActive(true);
                acceptButton.gameObject.SetActive(false);
            }
            else
            {
                abandonButton.gameObject.SetActive(false);
                acceptButton.gameObject.SetActive(true);
            }
        }
        else
        {
            acceptButton.gameObject.SetActive(isSelected);
            abandonButton.gameObject.SetActive(!isSelected);
        }
        if (isSelected)
            LogKey(EV_VIEW);
    }

    private void HandleCloseSelection(bool log = true)
    {
        selectedCardPanel.SetActive(false);
        this.transform.SetParent(cardHolderParent);
        this.transform.localScale = Vector3.one;
        closeButton.gameObject.SetActive(isSelected);
        acceptButton.gameObject.SetActive(isSelected);
        abandonButton.gameObject.SetActive(false);
        if (log)
            LogKey(EV_CLOSE);
    }

    private void HandleCardAcceptance()
    {
        HandleCloseSelection(log: false);
        Destroy(BountyBoardManager.instance.currentBounty?.gameObject);
        BountyBoardManager.instance.currentBounty = null;
        GameObject tempBountyCard = Instantiate(this, selectedCardPanel.transform).gameObject;

        tempBountyCard.transform.SetParent(BountyBoardManager.instance.transform);
        tempBountyCard.GetComponent<Canvas>().enabled = false;

        BountyBoardManager.instance.currentBounty = tempBountyCard.GetComponent<BountyCard>();

        LogKey(EV_ACCEPT);
        // Log movement back to the Lab using the Location category
        var logger = GameLogger.Instance != null ? GameLogger.Instance : GameObject.FindObjectOfType<GameLogger>();
        if (logger != null)
        {
            logger.LogEvent("Location", "lab", null);
        }
        // Automatically return to Lab after accepting bounty
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.TheLab);
    }

    private void HandleAbandonSelection()
    {
        HandleCloseSelection(log: false);
        cardSelectedStatus.enabled = false;
        Destroy(BountyBoardManager.instance.currentBounty.gameObject);
        BountyBoardManager.instance.currentBounty = null;
        LogKey(EV_ABANDON);
    }

    private Dictionary<string, object> BuildBountyPayload()
    {
        var payload = new Dictionary<string, object>();
        if (bountyItem != null)
        {
            payload["bounty_name"] = bountyItem.name;
            payload["bounty_mean"] = bountyItem.mean;
            payload["bounty_sd"] = bountyItem.sd;
            payload["bounty_difficulty"] = bountyItem.difficulty;
            if (!string.IsNullOrEmpty(bountyItem.imagePath))
                payload["image_path"] = bountyItem.imagePath;
        }
        payload["is_selected"] = isSelected;
        return payload;
    }

    private void LogKey(string key, Dictionary<string, object> extra = null)
    {
        var logger = GameLogger.Instance != null ? GameLogger.Instance : GameObject.FindObjectOfType<GameLogger>();
        if (logger == null) return; // silently skip if logger not present

        var payload = BuildBountyPayload();
        if (extra != null)
        {
            foreach (var kv in extra)
                if (!payload.ContainsKey(kv.Key)) payload[kv.Key] = kv.Value;
        }
        logger.LogEvent(LOG_CATEGORY, key, payload);
    }
}
