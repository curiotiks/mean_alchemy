using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BountyItemData;
using UnityEngine.UI;

public class BountyCard : MonoBehaviour
{
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
        cardImage.sprite = bountyItem.image;
        cardName.text = bountyItem.name;
        cardMean.text = "Mean: " + bountyItem.mean.ToString();
        cardSD.text = "SD: " + bountyItem.sd.ToString();

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
    }

    private void HandleCloseSelection()
    {
        selectedCardPanel.SetActive(false);
        this.transform.SetParent(cardHolderParent);
        this.transform.localScale = Vector3.one;
        closeButton.gameObject.SetActive(isSelected);
        acceptButton.gameObject.SetActive(isSelected);
        abandonButton.gameObject.SetActive(false);
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
    }

    private void HandleAbandonSelection()
    {
        HandleCloseSelection();
        cardSelectedStatus.enabled = false;
        Destroy(BountyBoardManager.instance.currentBounty.gameObject);
        BountyBoardManager.instance.currentBounty = null;
    }
}
