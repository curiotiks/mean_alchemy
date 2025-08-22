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
    public TextMeshProUGUI y_axis;

    [Header("TRANSMUTE")]
    [SerializeField] GameObject TransmutePanel;
    [SerializeField] GameObject DataElementsHolder;
    [SerializeField] List<GameObject> DataElements;
    [SerializeField] GameObject DataElementPrefab;
    [SerializeField] Button TransmuteButton;
    [SerializeField] Button ConfirmTransmuteButton;
    [SerializeField] Button CancelTransmuteButton;


    void Start() { 
        //find all vertical layout group
        //vertical layout group is placed in each numbered block
        y_axis.text = max_stacked_count.ToString();
        foreach(var x in GetComponentsInChildren<VerticalLayoutGroup>()){
            vertical_layout_group_list.Add(x);
        } 
        //validate if the number of vertical layout group is 10
        if(vertical_layout_group_list.Count != 10){
            Debug.Log("vertical layout group count is not 10");
        }

        DataElements.Clear();

        TransmuteButton.onClick.RemoveAllListeners();
        TransmuteButton.onClick.AddListener(setUpTransmutePanel);

        ConfirmTransmuteButton.onClick.RemoveAllListeners();
        ConfirmTransmuteButton.onClick.AddListener(confirmTransmute);

        CancelTransmuteButton.onClick.RemoveAllListeners();
        CancelTransmuteButton.onClick.AddListener(cancelTransmute);

        instantiateVerticalLayoutGroups();
    } 

    public void instantiateVerticalLayoutGroups(){
        int num_for_name = 1;
        foreach (VerticalLayoutGroup x in vertical_layout_group_list){
            //find imge from numbers_panel buttons
            GameObject img_obj = GameObject.Find("btn_num_" + num_for_name);
            // Debug.Log("img_obj" + img_obj);
            
            //add a new gameobject to the vertical layout group
            for (int i = 0; i < max_stacked_count; i++){
                GameObject new_obj = new GameObject();
                new_obj.name = "componentVerticalLayout_"+num_for_name + "_" + i;
                new_obj.AddComponent<RectTransform>();
                new_obj.transform.SetParent(x.transform);
                new_obj.AddComponent<Image>();
                new_obj.GetComponent<Image>().sprite = img_obj.GetComponent<Image>().sprite;
                //set image transparent
                new_obj.GetComponent<Image>().color = new Color(1,1,1,0);
                new_obj.AddComponent<Button>();
                Button btn = new_obj.GetComponent<Button>();
                btn.interactable = false;
                btn.onClick.AddListener(() => {
                Table_Control_Panel.instance.updateInput(int.Parse(new_obj.name.Split('_')[1]), false);
               
                });
            }
            num_for_name++;
        }
    }


    public void drawPlot(int num, bool isAdded){
        // Debug.Log("drawPlot: "+num+" "+isAdded);
        //find the vertical layout group
        VerticalLayoutGroup vlg = vertical_layout_group_list[num-1];
        //if isAdded is true, find the last child which the image is not transparent is disabled and set the image component tranparent and enable the button.
        if(isAdded){
            for (int i = vlg.transform.childCount - 1; i >= 0; i--){
                Transform child = vlg.transform.GetChild(i);
                if (child.GetComponent<Image>().color.a == 0){
                    child.GetComponent<Image>().color = new Color(1,1,1,1);
                    child.GetComponent<Button>().interactable = true;
                    break;
                }
            }
        }
        else{
            //if isAdded is false, find the last child which the image is not transparent and set the image component tranparent and disable the button.
            for (int i = 0; i < vlg.transform.childCount; i++){
                Transform child = vlg.transform.GetChild(i);
                if (child.GetComponent<Image>().color.a != 0){
                    child.GetComponent<Image>().color = new Color(1,1,1,0);
                    child.GetComponent<Button>().interactable = false;
                    break;
                }
            }
        }
    }

    public void resetPlot(){
        // foreach(var x in plot_item_list){
        //     RectTransform rt = x.GetComponent<RectTransform>();
        //     rt.sizeDelta = new Vector2(rt.sizeDelta.x, 0);
        // }
    }

    private void setUpTransmutePanel()
    {
        if(!TransmutePanel)
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
}
