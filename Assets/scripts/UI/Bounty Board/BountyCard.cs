using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using BountyItemData;
using UnityEngine.UI;

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

    //public void CustomMethodForBountyBoard()
    //{

    //    abandonButton.enabled = true;
    //    acceptButton.enabled = false;
    //    abandonButton.onClick.RemoveAllListeners();
    //    abandonButton.onClick.AddListener(() =>
    //    {
    //        BountyBoardManager.instance.HardLoadScene("BountyBoard");
    //    });
    //}

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


        // Set up the button listeners
        mainSelectionButton.onClick.RemoveAllListeners();
        mainSelectionButton.onClick.AddListener(() =>
        {
            //Debug.Log($"Showing {bountyItem.name}");
            isSelected = true;
            HandleCardSelection();
        });

        closeButton.onClick.RemoveAllListeners();
        closeButton.gameObject.SetActive(false);
        //closeButton.enabled = isSelected;
        //closeButton.interactable = isSelected;
        closeButton.onClick.AddListener(() =>
        {
            isSelected = false;
            HandleCloseSelection();
        });

        acceptButton.onClick.RemoveAllListeners();
        acceptButton.gameObject.SetActive(false);
        //acceptButton.enabled = isSelected;
        //acceptButton.interactable = isSelected;
        acceptButton.onClick.AddListener(() =>
        {
            Debug.Log("Bounty Accepted: " + bountyItem.name);
            isSelected = false;
            HandleCardAcceptance();
        });

        abandonButton.onClick.RemoveAllListeners();
        abandonButton.gameObject.SetActive(false);
        //abandonButton.enabled = isSelected;
        //abandonButton.interactable = isSelected;
        abandonButton.onClick.AddListener(() =>
        {
            Debug.Log("Bounty Abandoned: " + bountyItem.name);
            isSelected = false;
            HandleAbandonSelection();
        });
    }


    private void HandleCardSelection()
    {
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

    private void HandleCloseSelection()
    {
        selectedCardPanel.SetActive(false);
        this.transform.SetParent(cardHolderParent);
        this.transform.localScale = Vector3.one;
        closeButton.gameObject.SetActive(isSelected);
        acceptButton.gameObject.SetActive(isSelected);
        abandonButton.gameObject.SetActive(false);
        LogKey(EV_CLOSE);
    }

    private void HandleCardAcceptance()
    {
        HandleCloseSelection();
        Destroy(BountyBoardManager.instance.currentBounty?.gameObject);
        BountyBoardManager.instance.currentBounty = null;
        GameObject tempBountyCard = Instantiate(this, selectedCardPanel.transform).gameObject;

        tempBountyCard.transform.SetParent(BountyBoardManager.instance.transform);
        tempBountyCard.GetComponent<Canvas>().enabled = false;

        BountyBoardManager.instance.currentBounty = tempBountyCard.GetComponent<BountyCard>();

        //BountyBoardManager.instance.currentBounty = tempBountyCard.GetComponent<BountyCard>();
        //BountyBoardManager.instance.currentBounty.abandonButton.enabled = true;
        //BountyBoardManager.instance.currentBounty.acceptButton.enabled = false;
        //BountyBoardManager.instance.currentBounty.abandonButton.onClick.RemoveAllListeners();
        //BountyBoardManager.instance.currentBounty.abandonButton.onClick.AddListener(() =>
        //{
        //    BountyBoardManager.instance.HardLoadScene("BountyBoard");
        //});
        LogKey(EV_ACCEPT);
    }

    private void HandleAbandonSelection()
    {
        HandleCloseSelection();
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
