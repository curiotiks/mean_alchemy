using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Table_Plot_Panel : MonoBehaviour
{ 
    public RectTransform plot_holder; 
    float chart_height;
    float an_increment_height;
    private List<Table_Plot_Item> plot_item_list;

    void Start() {
        chart_height = Screen.height + plot_holder.sizeDelta.y;
        an_increment_height = chart_height / (float) Table_Control_Panel.max_height_count; 
        an_increment_height *= 0.8f;
        //plot bar items --> list
        plot_item_list = new List<Table_Plot_Item>();
        foreach(var x in plot_holder.GetComponentsInChildren<Table_Plot_Item>(true)){
            plot_item_list.Add(x);
        }

    } 

    //num ranging from 1 to 10
    public void drawPlot(int num, bool isAdded){
        RectTransform rt = plot_item_list[num-1].GetComponent<RectTransform>();
        float originalSize = rt.sizeDelta.y;
        if(isAdded){
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + an_increment_height);
        }else{ 
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y - an_increment_height);
        }
    }

    public void resetPlot(){
        foreach(var x in plot_item_list){
            RectTransform rt = x.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, 0);
        }
    }
}
