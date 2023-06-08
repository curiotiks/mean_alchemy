using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BountyBoard_Manager : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public GridLayoutGroup gridLayoutGroup;
    public GameObject bountyBoardItemPrefab;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        //for test purpose
        //create random bounty item five times
        for (int i = 0; i < 10; i++)
        {
            BountyItem bountyItem = new BountyItem("Test", null, 10, 1,
                    new Dictionary<RewardType, int> { 
                            { RewardType.Gold, Random.Range(1, 100) }, 
                            { RewardType.Exp, 100 }, 
                            { RewardType.Reputation, 100 } });
            addBountyItem(bountyItem);
        } 
    }

    public void addBountyItem(BountyItem bountyItem)
    {
        GameObject bountyBoardItem = Instantiate(bountyBoardItemPrefab, gridLayoutGroup.transform);
        TextMeshProUGUI title = bountyBoardItem.transform.Find("title").GetComponent<TextMeshProUGUI>();
        title.text = bountyItem.name;
        Image image = bountyBoardItem.transform.Find("image").GetComponent<Image>();
        image.sprite = bountyItem.image;
        TextMeshProUGUI reward = bountyBoardItem.transform.Find("reward").GetComponent<TextMeshProUGUI>();
        reward.text = bountyItem.rewardList[RewardType.Exp].ToString();

        //add listener to button
        Button button = bountyBoardItem.GetComponent<Button>();
        button.onClick.AddListener( () =>{
            //open a new scene and send the bountyItem to the scene 
            //once the new scene loaded completely, send the bountyItem to the scene
            GameObject go = new GameObject();
            go.name = "CombatManager_Temp";
            go.AddComponent<CombatManager>().setBountyItem(bountyItem);
            DontDestroyOnLoad(go);

            SceneManager.LoadScene("Combat", LoadSceneMode.Single);
        });
    } 

    public void removeBountyItem(GameObject bountyBoardItem)
    {
        Destroy(bountyBoardItem); 
    }

}
